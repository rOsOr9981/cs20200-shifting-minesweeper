# CLI Shifting Minesweeper

A command-line Minesweeper game with a dynamic twist, built with F# and .NET 10.

You play on a 9×9 grid with 10 hidden mines. Every 5th reveal triggers an **EARTHQUAKE** — all flags are lost and every mine shifts one step in a random direction. Adapt to the ever-changing board to survive and win.

## Setup & Execution

Requires **.NET 10 SDK** ([download here](https://dotnet.microsoft.com/download/dotnet/10.0)).

**Windows:**
```
run.bat
```

**Unix/macOS:**
```
chmod +x run.sh
./run.sh
```

**Direct method:**
```
dotnet run
```

## How to Play

| Command | Action |
|---------|--------|
| `R <row> <col>` | Reveal a cell (e.g. `R 3 4`) |
| `F <row> <col>` | Flag or unflag a suspected mine (e.g. `F 3 4`) |

- Rows and columns are numbered **1–9**.
- Revealing a mine ends the game (**Game Over**).
- Revealing a safe cell shows how many mines are adjacent. If it is 0, all neighboring cells are revealed automatically.
- Every **5th reveal** triggers an **EARTHQUAKE**: flags vanish and each mine moves one step (up, down, left, or right) to a random non-revealed cell.
- After an earthquake, the numbers on revealed cells update to reflect the new mine positions.
- You **win** when all non-mine cells are revealed.

## Board Legend

| Symbol | Meaning |
|--------|---------|
| `[ ]` | Hidden cell |
| `[F]` | Flagged cell |
| `[1]`–`[8]` | Revealed cell (adjacent mine count) |
| (blank) | Revealed cell with 0 adjacent mines |
| `[*]` | Mine (shown on game over) |

## Example

```
==================================
    CLI SHIFTING MINESWEEPER
==================================
...
    1  2  3  4  5  6  7  8  9
1  [ ][ ][ ][ ][ ][ ][ ][ ][ ]
2  [ ][ ][ ][ ][ ][ ][ ][ ][ ]
...

[Reveals until EARTHQUAKE: 5]  Enter command: R 5 5
...
EARTHQUAKE! All flags are lost and mines have shifted.
```
