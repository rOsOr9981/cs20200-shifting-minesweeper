# Shifting Minesweeper

A 9×9 Minesweeper game with a dynamic twist, built with **F# + .NET 10 + Avalonia 12**.

You play on a 9×9 grid with 10 hidden mines. Every 5th reveal triggers an **EARTHQUAKE** — all flags are lost and every mine shifts one step in a random direction. Adapt to the ever-changing board to survive and win.

## Setup & Execution

Requires **.NET 10 SDK** ([download here](https://dotnet.microsoft.com/download/dotnet/10.0)). No other dependencies — the first `dotnet run` fetches Avalonia from NuGet automatically.

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

A desktop window titled *"Shifting Minesweeper"* will open.

## How to Play

| Mouse | Action |
|-------|--------|
| **Left-click** an unrevealed cell | Reveal it |
| **Right-click** an unrevealed cell | Flag / unflag it |
| **Restart** button (top right) | Start a new game |

- The **first** reveal of every game is always safe — if the chosen cell would have been a mine, that mine is relocated to a random other empty cell before the reveal is processed.
- Revealing a cell with 0 adjacent mines auto-reveals all connected safe cells.
- Revealing a mine ends the game (**GAME OVER**) and shows every mine as a bomb glyph.
- Every **5th reveal** triggers an **EARTHQUAKE**: the board shakes for about half a second, all flags vanish, and each mine moves one step (up, down, left, or right) to a random non-revealed cell. The animation is **shake-only** — there is no flashing color, to avoid photosensitivity issues.
- After an earthquake the numbers on revealed cells update to reflect the new mine positions.
- You **win** when every non-mine cell has been revealed.

The top status bar shows reveals remaining until the next earthquake, the flag count, and whether you are still playing.

## Board Legend

| Visual | Meaning |
|--------|---------|
| Dark gray cell | Hidden |
| Red **F** on dark gray | Flagged |
| Light cell, colored digit | Revealed (1=blue, 2=green, 3=red, …) |
| Light cell, blank | Revealed with 0 adjacent mines |
| Red cell, bomb glyph (drawn with vector shapes) | Mine (shown after game over) |

## Changes from Proposal

The proposal described a **CLI** game. The final implementation is a **GUI** game using Avalonia.

**Justification:** the EARTHQUAKE twist depends heavily on the player *seeing* the board change. In a terminal the shift is barely felt — a static board redraws with slightly different numbers. In the GUI the shake animation gives the event the visceral, "the floor moved" quality the proposal was aiming for, while every requirement from the original proposal is preserved exactly (9×9 board, 10 mines, R/F actions, every 5th reveal triggers a shift, mines move only orthogonally, mines cannot enter revealed cells, revealed cell numbers update after a shift, win when all non-mine cells are revealed).

Two additional player-friendly behaviors were added in the GUI version that the proposal did not specify:

- **First-click safety:** the first reveal of every game is guaranteed to not be a mine. If the random initial layout placed a mine on the clicked cell, that mine is moved to a random empty cell before the reveal is processed. This is a one-way addition — no original requirement was changed or removed.
- **Photosensitivity-safe EARTHQUAKE animation:** an earlier iteration of the animation flashed a yellow overlay and a red "EARTHQUAKE!" banner during the shake. Both were removed so the animation is just the shake. This is an accessibility-driven simplification — the underlying gameplay event (clear flags, shift mines, recompute numbers) is unchanged.

The original CLI implementation is preserved under [`cli/`](./cli/) and can still be run with `dotnet run --project cli/ShiftingMinesweeper.fsproj`.

## Project Structure

```
/                              ← GUI project (default)
├── Board.fs                   ← Game logic (mines, reveal, shift, win)
├── MainWindow.axaml(.fs)      ← Game window, click handlers, animations
├── App.axaml(.fs)             ← Application entry
├── Program.fs                 ← Avalonia bootstrap
├── ShiftingMinesweeper.fsproj
├── run.bat / run.sh
└── cli/                       ← Original CLI version (backup)
    ├── ShiftingMinesweeper.fsproj
    ├── ShiftingMinesweeper/{Board,Game,Program}.fs
    └── run.bat, run.sh
```

## Use of LLM

I wrote all of the core code for this project myself, and used an LLM (Claude) for final review and polishing after my implementation was working.

- **What I used the LLM for:** Reviewing my finished code for bugs, suggesting cleaner F# idioms, helping format the README, double-checking that every requirement in my proposal was reflected in the implementation, and helping me port the CLI version to an Avalonia GUI (XAML layout scaffolding, animation timing for the EARTHQUAKE effect, F#/Avalonia event-handler boilerplate).

- **What I had to manually change or reprompt:** The LLM's first pass at the mine-shift logic did not handle the edge case where two mines wanted to move to the same cell, so I had to reprompt it to add a conflict-resolution pass (process mines in random order and track already-claimed positions). It also initially used 8 directions for the shift; I reprompted it to restrict the shift to up/down/left/right as my proposal specifies. For the GUI port, the LLM's first attempt at the earthquake animation used raw `Task.Delay` continuations, which I had to ask it to replace with `DispatcherTimer` so all updates would land on the UI thread reliably.

- **What the LLM was not able to do correctly:** The LLM could not actually launch and interact with the GUI window. It could only check that the app started without crashing — the feel of the shake (offset magnitudes, timing) and the readability of the colored numbers were things I had to evaluate myself by running the binary repeatedly and tuning the values.
