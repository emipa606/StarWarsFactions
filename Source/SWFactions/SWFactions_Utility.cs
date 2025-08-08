using Verse;

namespace SWSaber;

public static class Utility
{
    private static bool modCheck;
    private static bool loadedForcePowers;

    private static bool loadedLightsabers;

    public static bool AreForcePowersLoaded()
    {
        if (!modCheck)
        {
            ModCheck();
        }

        return loadedForcePowers;
    }

    public static bool AreLightsabersLoaded()
    {
        if (!modCheck)
        {
            ModCheck();
        }

        return loadedLightsabers;
    }

    private static void ModCheck()
    {
        loadedForcePowers = false;
        loadedLightsabers = false;
        //loadedFactions = false;
        foreach (var resolvedMod in LoadedModManager.RunningMods)
        {
            if (loadedForcePowers && loadedLightsabers)
            {
                break; //Save some loading
            }

            if (resolvedMod.Name.Contains("Star Wars - The Force"))
            {
                Log.Message("Lightsabers :: Star Wars - The Force Detected.");
                loadedForcePowers = true;
            }

            if (!resolvedMod.Name.Contains("Star Wars - Fully Functional Lightsabers"))
            {
                continue;
            }

            Log.Message("Lightsabers :: Star Wars - Lightsabers.");
            loadedLightsabers = true;
        }

        modCheck = true;
    }
}