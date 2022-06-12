# Community Editor for Anno 1800 Maps

The current version of the map editor is only useful if you know how to mod and work with extracted RDA files.

## Setup

You need to install [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) to run the application.
It should prompt you to download it.

Once started, you'll need to open a map template file and ideally set the path to your extracted RDA `data/` folder.
`data5.rda` includes the base game islands, but I recommend to extract all RDA files into one folder.

## Features

Currently supported features:
- viewing only currently, no editing
- `.a7tinfo` and [FileDBReader](https://github.com/anno-mods/FileDBReader) `.xml`

## Roadmap

- editing (add, remove, change islands, resize session, ...)
- open map templates directly from the game
- save as `.a7tinfo`, FileDBReader `.xml`
- export as ready-to-play mod
