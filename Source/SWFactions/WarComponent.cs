using RimWorld;
using RimWorld.Planet;
using Verse;

namespace SWFactions;

public class WarComponent : WorldComponent
{
    private bool hostilityDeclared;

    public WarComponent(World world) : base(world)
    {
    }

    public override void WorldComponentTick()
    {
        base.WorldComponentTick();
        if (Find.TickManager.TicksGame % 500 != 0 ||
            hostilityDeclared)
        {
            return;
        }

        hostilityDeclared = true;
        if (Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamedSilentFail("PJ_RebelFac"))
                is not { } rebelFaction ||
            Find.FactionManager.FirstFactionOfDef(
                    DefDatabase<FactionDef>.GetNamedSilentFail("PJ_GalacticEmpire"))
                is not { } impFaction)
        {
            return;
        }

        impFaction.ChangeGoodwill_Debug(rebelFaction, -100);
        rebelFaction.ChangeGoodwill_Debug(impFaction, -100);


        Find.MusicManagerPlay.disabled = true;
        Find.MusicManagerPlay.ForceSilenceFor(10f);
        Find.MusicManagerPlay.disabled = false;
        Find.LetterStack.ReceiveLetter(
            "PJ_WarDeclared".Translate(),
            "PJ_WarDeclaredDesc".Translate(rebelFaction.def.label, impFaction.def.label),
            DefDatabase<LetterDef>.GetNamed("PJ_BadUrgent"),
            null,
            rebelFaction
        );
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref hostilityDeclared, "hostilityDeclared");
    }
}