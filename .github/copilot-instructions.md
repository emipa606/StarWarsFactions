# GitHub Copilot Instructions for Star Wars - Factions (Continued)

## Mod Overview and Purpose

**Star Wars - Factions (Continued)** is a RimWorld mod that introduces iconic factions from the Star Wars universe into the game. Featuring the Galactic Empire, Rebel Alliance, and various other characters from Star Wars lore, this mod aims to enhance the gameplay experience with diverse interactions and thematic elements. Originally developed by Xen and Jecrell, this continuation builds upon their foundation, ensuring compatibility with later versions of RimWorld and utilizing improved support for Asset Bundles.

**Note:** This mod is no longer updated, and you have full permission to remix, recreate, and reuse its content according to your needs.

## Key Features and Systems

- **Galactic Empire Interactions**: Engage with the Galactic Empire through unique systems like tax demands, hostility declarations, and first contact dialogs.
- **Rebel Alliance and Scum Factions**: Add depth to your game with various factions that bring unique challenges and opportunities.
- **Asset Bundles**: Enhanced graphical and audio assets are utilized for a more immersive experience, supported by RimWorld 1.6.

## Coding Patterns and Conventions

- **File Structure**: Classes are separated into different files, organized primarily by function: `HarmonyPatches.cs`, `ImperialTax_Data.cs`, `SWFactions_Utility.cs`, and `WarComponent.cs`.
- **Class Naming**: Descriptive class names follow PascalCase convention, indicating their role, such as `ImperialTax_Data` or `WarComponent`.
- **Methods**: Functionality is encapsulated in methods with clear, descriptive naming to provide clarity on their operations.

### C# Files Summary

#### `ImperialTax_Data.cs`
- **Class**: `ImperialTax_Data` extending `WorldComponent`
- **Key Methods**: 
  - `ResolveMeetingGalacticEmpire(Pawn imperial)`
  - `SendFirstMeetingDialog(Pawn imperial)`
  - `ResolveDeclarationOfHostility(Pawn imperial)`
  - `ResolveGalacticEmpireTaxDeal(Pawn imperial)`
  - `DetermineSilverAvailable(Pawn imperial)`
  - `ReceiveTaxes(Pawn imperial, int amountOwed)`

#### `SWFactions_Utility.cs`
- **Class**: `Utility`
- **Purpose**: Contains utility functions used throughout the mod.

#### `WarComponent.cs`
- **Class**: `WarComponent` extending `WorldComponent`
- **Purpose**: Manages war-related components and interactions within the game.

#### `HarmonyPatches.cs`
- **Purpose**: Contains Harmony patches for modifying and extending gameplay mechanics.

## XML Integration

- **Assets and XML**: XML files define assets and game data, such as faction definitions, item properties, and interaction parameters.
- **Pattern**: Ensure compliance with RimWorld's XML structure, using clear attribute naming and logical hierarchy.

## Harmony Patching

- **Purpose**: Harmony patches allow modifications to the game's original C# code without altering the source, making updates and maintenance more manageable.
- **Structure**: Use of techniques such as Prefix, Postfix, and Transpiler methods when designing Harmony patches to implement or alter game features.

## Suggestions for Copilot

- **Autocomplete Functionality**: Ensure accurate method signatures and parameters are used when writing new methods.
- **Smart Code Completion**: Utilize contextually aware code suggestions to quickly implement boilerplate code for commonly used patterns.
- **Refactoring Assistance**: When modifying existing code, leverage Copilot to propose optimized refactoring suggestions.

## Testing and Validation

- **Testing**: Consider adding unit tests where applicable, especially for complex logic in methods like `ResolveDeclarationOfHostility`.
- **Debugging**: Use logging and debugging tools to ensure code is correct and functions as expected in-game.

## Additional Information

For any problems, questions, or further discussion, feel free to join the community on Discord. For more mods or support, consider contributing to the developers.

---

*May the Force be with you in your modding ventures!*
