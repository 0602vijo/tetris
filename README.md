# introduction

this project uses [`raylib-cs`](https://github.com/raylib-cs/raylib-cs) and [`rlImGui-cs`](https://github.com/raylib-extras/rlImGui-cs)

## features

implementation of a modern multiplayer competitive tetris featuring

* wallkicks
* random bag system
* t-spins
* (kind-of) working multiplayer

## controls

C - holds currents piece

Z/X - rotates piece left/right

Arrow Up - rotates piece right

Space - drops piece while placing it

Arrow Left/Right - moves piece left/right

Arrow Down - moves piece down 1 tile or to bottom depending on setting

## building

uses .net9

### dotnet cli

`dotnet build -c release`

### visual studio

open `Raylib Tetris.sln`

set configuration to `"Release"`

right-click `Solution 'Raylib Tetris'`

click `Rebuild Solution`

## running

### dotnet cli

`dotnet run -c release`

### visual studio

set configuration to `"Release"`

press `F5`

### executable

navigate to `bin/Release/net9.0/`

run `Raylib Tetris.exe`
