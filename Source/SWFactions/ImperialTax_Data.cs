using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace SWFactions;

public class ImperialTax_Data : WorldComponent
{
    private static readonly float empireTaxRate = 0.2f;
    private static readonly int empireLandRightsTax = 100;
    private int empireDebt;
    private int empireFunds;
    private bool metEmpire;
    private int nextTaxCollectionTicks = -1;
    private bool receivedMessage;
    private bool taxedColony;

    public ImperialTax_Data(World world) : base(world)
    {
    }

    private static void resolveDeclarationOfHostility(Pawn imperial)
    {
        imperial.Faction.ChangeGoodwill_Debug(Faction.OfPlayer, -100);
    }


    private void resolveGalacticEmpireTaxDeal(Pawn imperial)
    {
        if (!taxedColony)
        {
            return;
        }

        if (nextTaxCollectionTicks == -1)
        {
            nextTaxCollectionTicks += GenDate.DaysPerSeason * GenDate.TicksPerYear;
            receiveTaxes(imperial, empireLandRightsTax);
        }
        else if (Find.TickManager.TicksGame > nextTaxCollectionTicks)
        {
            var totalCurrency = determineSilverAvailable(imperial);
            var resolvedTax = Math.Max(empireLandRightsTax, (int)(totalCurrency * 0.2));
            receiveTaxes(imperial, resolvedTax);
        }
    }

    private static int determineSilverAvailable(Pawn imperial)
    {
        var result = 0;
        var currencies = imperial.Map.listerThings.ThingsOfDef(ThingDefOf.Silver);
        if (currencies is not { Count: > 0 })
        {
            return result;
        }

        foreach (var currency in currencies)
        {
            result += currency.stackCount;
        }

        return result;
    }

    private void receiveTaxes(Pawn imperial, int amountOwed)
    {
        var amountUnpaid = amountOwed + empireDebt;
        var currencies = imperial.Map.listerThings.ThingsOfDef(ThingDefOf.Silver);
        if (currencies is { Count: > 0 })
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

            Messages.Message("PJ_ImperialTaxes_Owed".Translate(imperial.Map.info.parent.Label, amountPaid, empireDebt),
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

                    resolveMeetingGalacticEmpire(imperial);
                    resolveGalacticEmpireTaxDeal(imperial);
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


    private void resolveMeetingGalacticEmpire(Pawn imperial)
    {
        switch (metEmpire)
        {
            case false:
                metEmpire = true;

                Find.MusicManagerPlay.disabled = true;
                Find.MusicManagerPlay.ForceSilenceFor(10f);
                Find.MusicManagerPlay.disabled = false;
                SoundDef.Named("PJ_ImperialMarchBanjo").PlayOneShotOnCamera();
                break;
            case true when !receivedMessage:
                receivedMessage = true;
                sendFirstMeetingDialog(imperial);
                break;
        }
    }

    private void sendFirstMeetingDialog(Pawn imperial)
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
            action = delegate { resolveDeclarationOfHostility(imperial); },
            link = diaNode2
        };
        diaNode.options.Add(diaOption3);
        string title = "PJ_ImperialGreeting_Title".Translate();
        Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, title));
    }
}