using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace SWFactions
{
    public class ImperialTax_Data : WorldComponent
    {
        public static readonly float empireTaxRate = 0.2f;
        public static readonly int empireLandRightsTax = 100;
        public int empireDebt;
        public int empireFunds;
        public bool metEmpire;
        public int nextTaxCollectionTicks = -1;
        public bool receivedMessage;
        public bool taxedColony;

        public ImperialTax_Data(World world) : base(world)
        {
        }

        public void ResolveDeclarationOfHostility(Pawn imperial)
        {
            imperial.Faction.SetRelationDirect(Faction.OfPlayer, FactionRelationKind.Hostile);
            //List<Pawn> imperialsOnSite = imperial.Map.mapPawns.AllPawnsSpawned.FindAll((x) => x.Faction == imperial.Faction);
            //if (imperialsOnSite != null && imperialsOnSite.Count > 0)
            //    {
            //    if (imperial?.GetLord() is Lord imperialLord) imperial.Map.lordManager.RemoveLord(imperialLord);
            //        LordMaker.MakeNewLord(imperial.Faction, new LordJob_AssaultColony(imperial.Faction, false, false, false, false, false), imperial.Map, imperialsOnSite);
            //    }
        }


        public void ResolveGalacticEmpireTaxDeal(Pawn imperial)
        {
            var unused = imperial.MapHeld;
            if (!taxedColony)
            {
                return;
            }

            if (nextTaxCollectionTicks == -1)
            {
                //nextTaxCollectionTicks += 2000; //GenDate.DaysPerSeason * GenDate.TicksPerYear;
                nextTaxCollectionTicks += GenDate.DaysPerSeason * GenDate.TicksPerYear;
                ReceiveTaxes(imperial, empireLandRightsTax);
            }
            else if (Find.TickManager.TicksGame > nextTaxCollectionTicks)
            {
                var totalCurrency = DetermineSilverAvailable(imperial);
                var resolvedTax = Math.Max(empireLandRightsTax, (int) (totalCurrency * 0.2));
                ReceiveTaxes(imperial, resolvedTax);
            }
        }

        public int DetermineSilverAvailable(Pawn imperial)
        {
            var result = 0;
            var currencies = imperial.Map.listerThings.ThingsOfDef(ThingDefOf.Silver);
            if (currencies == null || currencies.Count <= 0)
            {
                return result;
            }

            foreach (var currency in currencies)
            {
                result += currency.stackCount;
            }

            return result;
        }

        public void ReceiveTaxes(Pawn imperial, int amountOwed)
        {
            var amountUnpaid = amountOwed + empireDebt;
            var currencies = imperial.Map.listerThings.ThingsOfDef(ThingDefOf.Silver);
            if (currencies is {Count: > 0})
            {
                foreach (var currency in currencies.InRandomOrder())
                {
                    if (amountUnpaid <= 0)
                    {
                        break;
                    }

                    var num = Math.Min(amountUnpaid, currency.stackCount);
                    currency.SplitOff(num).Destroy();
                    amountUnpaid -= num;
                    empireFunds += num;
                }
            }

            if (amountUnpaid > 0)
            {
                empireDebt = amountUnpaid;
                var amountPaid = amountOwed - amountUnpaid;
                if (amountPaid < 0)
                {
                    amountPaid = 0;
                }

                Messages.Message(
                    "PJ_ImperialTaxes_Owed".Translate(imperial.Map.info.parent.Label, amountPaid, empireDebt),
                    MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message("PJ_ImperialTaxes_Paid".Translate(imperial.Map.info.parent.Label, amountOwed),
                    MessageTypeDefOf.PositiveEvent);
            }
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            try
            {
                if (Find.TickManager == null)
                {
                    return;
                }

                if (Find.TickManager.TicksGame % 1000 != 0)
                {
                    return;
                }

                if (Find.Maps != null)
                {
                    Find.Maps.ForEach(delegate(Map map)
                    {
                        var pawns = map.mapPawns.AllPawnsSpawned.Where(p => p.Faction != null).ToList();
                        var imperial = pawns.FirstOrDefault(p =>
                            p.Name != null && p.Faction.def.defName.EqualsIgnoreCase("PJ_GalacticEmpire"));
                        if (imperial == null)
                        {
                            return;
                        }

                        ResolveMeetingGalacticEmpire(imperial);
                        ResolveGalacticEmpireTaxDeal(imperial);
                    });
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref metEmpire, "metEmpire");
            Scribe_Values.Look(ref receivedMessage, "receivedEmpire");
            Scribe_Values.Look(ref taxedColony, "taxedColony");
            Scribe_Values.Look(ref empireFunds, "empireFunds");
            Scribe_Values.Look(ref empireDebt, "empireDebt");
            Scribe_Values.Look(ref nextTaxCollectionTicks, "lastTaxCollectionTicks", -1);
        }

        #region Meeting

        public void ResolveMeetingGalacticEmpire(Pawn imperial)
        {
            if (!metEmpire)
            {
                metEmpire = true;

                Find.MusicManagerPlay.disabled = true;
                Find.MusicManagerPlay.ForceSilenceFor(10f);
                Find.MusicManagerPlay.disabled = false;
                SoundDef.Named("PJ_ImperialMarchBanjo").PlayOneShotOnCamera();
            }
            else if (metEmpire && !receivedMessage)
            {
                receivedMessage = true;
                SendFirstMeetingDialog(imperial);
            }
        }

        public void SendFirstMeetingDialog(Pawn imperial)
        {
            var playerSettlement = imperial.Map.info.parent as Settlement;

            var text = "PJ_ImperialGreeting".Translate(imperial.Name.ToStringFull, imperial.kindDef.label,
                playerSettlement?.Label, empireLandRightsTax.ToString(), empireTaxRate.ToStringPercent());
            var diaNode = new DiaNode(text);
            var diaOption = new DiaOption("PJ_ImperialGreeting_Accept".Translate())
            {
                action = delegate { taxedColony = true; },
                resolveTree = true
            };
            diaNode.options.Add(diaOption);

            var text2 = "PJ_ImperialGreeting_Rejected".Translate(imperial.LabelShort);
            var diaNode2 = new DiaNode(text2);
            var diaOption2 = new DiaOption("OK".Translate())
            {
                resolveTree = true
            };
            diaNode2.options.Add(diaOption2);
            var diaOption3 = new DiaOption("PJ_ImperialGreeting_Reject".Translate())
            {
                action = delegate { ResolveDeclarationOfHostility(imperial); },
                link = diaNode2
            };
            diaNode.options.Add(diaOption3);
            string title = "PJ_ImperialGreeting_Title".Translate();
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, title));
        }

        #endregion Meeting
    }
}