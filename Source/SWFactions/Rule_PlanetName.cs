using Verse;
using Verse.Grammar;

namespace SWFactions;

public class Rule_PlanetName : Rule
{
    private readonly int selectionWeight = 1;

    public override float BaseSelectionWeight => selectionWeight;

    public override string Generate()
    {
        return Find.World.info.name;
    }

    public override string ToString()
    {
        return keyword + "->(worldname)";
    }
}