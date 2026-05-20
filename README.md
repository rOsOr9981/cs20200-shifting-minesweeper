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

## Use of LLM

I wrote all of the core code for this project myself, and used an LLM (Claude) for final review and polishing after my implementation was working.

- **What I used the LLM for:** Reviewing my finished code for bugs, suggesting cleaner F# idioms, helping format the README, and double-checking that every requirement in my proposal was reflected in the implementation (e.g. confirming that mines move only orthogonally, that flags are cleared *before* mine relocation, and that revealed-cell numbers re-derive from current mine positions after a shift).

- **What I had to manually change or reprompt:** The LLM's first pass at the mine-shift logic did not handle the edge case where two mines wanted to move to the same cell, so I had to reprompt it to add a conflict-resolution pass (process mines in random order and track already-claimed positions). It also initially used 8 directions for the shift; I reprompted it to restrict the shift to up/down/left/right as my proposal specifies.

- **What the LLM was not able to do correctly:** The LLM could not test the game end-to-end in an interactive terminal session — it could only pipe scripted inputs and inspect the resulting board. As a result, the actual feel of the game (pacing, readability of the board layout, how the EARTHQUAKE message stands out) was something I had to evaluate and tweak myself by running the binary repeatedly.
