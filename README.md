# SolastaDMKit

> **100% vibe coded.** Every line of code in this repository is written by Claude (an AI assistant from Anthropic), under human direction. The human author sets scope, makes architectural decisions, and reviews output — the AI writes the code. No human-authored C# has been committed to this repo.

NWScript-style scripting layer for Solasta: Crown of the Magister. Extends what campaign authors can do with the in-game Dungeon Maker by providing runtime event hooks, a C# scripting host, and APIs for environmental manipulation.

**Status**: Early development (Stage 0 — bootstrap).

## Building

Requires .NET SDK (any recent version; LTS recommended). Solasta install is expected at `C:\Program Files (x86)\Steam\steamapps\common\Slasta_COTM` (override via the `SolastaInstallDir` environment variable).

```
dotnet build
```

Output is written directly to `{SolastaInstallDir}\Mods\SolastaDMKit\`.

## License

MIT — see [LICENSE](LICENSE).
