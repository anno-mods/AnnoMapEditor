# Community Map Editor for Anno 1800

This project is not affiliated in any way with Ubisoft.

Anno 1800 and Anno 117 - Pax Romana are trademarks of Ubisoft Entertainment in the US and/or other countries.
Anno is a trademark of Ubisoft GmbH in the US and/or other countries.

![](./doc/editor-v0.6.gif)

## Setup

You need to install [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime?cid=getdotnetcore) to run the application.
If you don't have it yet it will prompt you to download it.
Select "Run desktop apps, Download x64".

Once started, the Editor will ask for your Anno installation path. Continue to the editor and you are presented with an empty map template.

## Features

- open map templates from the game, or extracted files (`.a7tinfo` and [FileDBReader](https://github.com/anno-mods/FileDBReader) `.xml`)
- create entirely new map templates
- rearrange, delete and add islands, change island pool type
- modify mining slots and fertilities
- undo and redo changes (currently not working with map size changes and group transform)
- save as ready-to-play mod.
- save as map template for manual modding

## How to Use

Open an existing map or create a new one, change it to your liking and safe it as a mod.

Have a look at the [Modding Guide](https://github.com/anno-mods/modding-guide) if you need help how to create more complex map mods or even new sessions.

Sometimes you will see some warnings on the left side:

- Too many small/medium/large pool islands: The game uses every island variation only once.
  If you have too many pool islands, the game will just omit the remaining ones.
- Too many continental islands. More than 1 continental island may result in visual glitches.

You can export the map anyways, but you might encounter unwanted behavior while playing on it.

Keep in mind, third party and pirate slots may turn into small islands depending on game settings.

## Roadmap

- Compatibility with Anno 117 - Pax Romana (currently underway)
- Quality of life enhancements
- Optimized export-as-mod methods

## Changelog

### 0.6

- Complete UI overhaul:
  - New startup window with optimized asset loading
  - Undo and Redo features
  - Zooming (mousewheel) and panning (right click and drag)
  - Visible slots on the map
  - Streamlined fertility selection
- General fixes and optimizations 

### 0.5

- Add, remove, change pool islands

### 0.4

- Move islands
- Save as playable mod (Old World only)

### 0.3

- Save/export to `.a7tinfo` and `.xml` files

### 0.2

- Open maps from game archives
- Auto-detect game data path

## Notes

This project uses:
- [RDAExplorer](https://github.com/jakobharder/RDAExplorer) (custom .NET6 build without WinForms)
- [FileDBReader](https://github.com/anno-mods/FileDBReader)
