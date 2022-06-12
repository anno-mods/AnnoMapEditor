# Community Map Editor for Anno 1800

This project is not affiliated in any way with Ubisoft.

Anno 1800 is a trademark of Ubisoft Entertainment in the US and/or other countries.
Anno is a trademark of Ubisoft GmbH in the US and/or other countries.

![](./doc/screenshot-1.jpg)

The current version of the map editor is only useful if you know how to mod and work with extracted RDA files.

## Setup

You need to install [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) to run the application.
It should prompt you to download it.

Once started, the editor will detect your game path automatically.
If it didn't find it, you'll need to set the path manually to your game or a folder with all RDA `data/` extracted.

## Features

Currently supported features:
- viewing only currently, no editing
- `.a7tinfo` and [FileDBReader](https://github.com/anno-mods/FileDBReader) `.xml`
- open map templates directly from the game

## Roadmap

- editing (add, remove, change islands, resize session, ...)

- save as `.a7tinfo`, FileDBReader `.xml`
- export as ready-to-play mod

## Notes

This project uses:
- [RDAExplorer](https://github.com/jakobharder/RDAExplorer) (custom .NET6 build without WinForms)
- [FileDBReader](https://github.com/anno-mods/FileDBReader)
