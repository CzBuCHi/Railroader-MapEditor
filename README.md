# Railroader Mod: Map Editor

### Requirements:

-   [Railloader](https://railroader.stelltis.ch/) 1.8+
-   [Strange Customs](https://railroader.stelltis.ch/mods/strange-customs) 1.7+

## Installation

-   Download `MapEditor-VERSION.zip` from the releases page
-   Install with [Railloader](<[https://www.nexusmods.com/site/mods/21](https://railroader.stelltis.ch/)>)

## Features

### Milestone manager

Quick way to advance / revert progressions:

| Button                | Description                                                |
| --------------------- | ---------------------------------------------------------- |
| Advance               | advance single phase of selected progression               |
| Advance all phases    | advance all phases phases of selected progression          |
| Revert                | revert selected progression and all others depending on it |
| Advance Prerequisites | complete all prerequisites of selected progression         |

### Track node editor

- move and rotate track node around
- create new node from existing
- delete existing
- split existing into separate nodes

### Track segment editor

- edit track segment properites (Group id, Priority, Speed limit, Style and Track class)
- delete edisting
- inject new node in middle of segment
- reverse segment direction (aka swap start and end node)

### Telegraph pole editor

- move and rotate telegraph pole around


## Project Setup

In order to get going with this, follow the following steps:

1. Clone the repo
2. Copy the `Paths.user.example` to `Paths.user`, open the new `Paths.user` and set the `<GameDir>` to your game's directory.
3. Open the Solution
4. You're ready!

### During Development

Make sure you're using the _Debug_ configuration. Every time you build your project, the files will be copied to your Mods folder and you can immediately start the game to test it.

### Publishing

Make sure you're using the _Release_ configuration. The build pipeline will then automatically do a few things:

1. Makes sure it's a proper release build without debug symbols
1. Replaces `$(AssemblyVersion)` in the `Definition.json` with the actual assembly version.
1. Copies all build outputs into a zip file inside `bin` with a ready-to-extract structure inside, named like the project they belonged to and the version of it.
