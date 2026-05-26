# Shifting Minesweeper

A 9×9 Minesweeper game with a dynamic twist, built with **F# + .NET 10 + Avalonia 12**.

You play on a 9×9 grid with 10 hidden mines. Every 5th reveal triggers an **EARTHQUAKE** — all flags are lost and every mine shifts one step in a random direction. Adapt to the ever-changing board to survive and win.

## Setup & Execution

The only thing you need is the **.NET 10 SDK**. Everything else (Avalonia, the F# compiler, etc.) is downloaded automatically the first time you run the game.

If you have already installed .NET 10 and just want the short version: clone the repo and run `run.bat` on Windows or `./run.sh` on macOS/Linux. The detailed step-by-step instructions below assume you have done none of that before.

### Step 1 — Install the .NET 10 SDK

The .NET 10 SDK is a free program from Microsoft that lets your computer compile and run F# code. You only need to do this once.

#### On Windows

1. Open this page in your web browser:
   <https://dotnet.microsoft.com/download/dotnet/10.0>
2. Under **".NET 10.0"**, find the **SDK** column and click the **Windows** **x64** installer (most modern PCs are x64; if you have a very new ARM PC use **Arm64**).
3. Run the downloaded `.exe` file and click **Install**. When it finishes, click **Close**.
4. To check it worked, press the **Windows key**, type `cmd`, and open **Command Prompt**. Type:
   ```
   dotnet --version
   ```
   and press **Enter**. You should see something like `10.0.100`. If you see "command not recognized," close and reopen Command Prompt and try again.

#### On macOS

1. Open this page in Safari or another browser:
   <https://dotnet.microsoft.com/download/dotnet/10.0>
2. Under **".NET 10.0"**, find the **SDK** column and click the **macOS** installer:
   - If you have an **Apple Silicon Mac** (M1, M2, M3, M4 — anything from late 2020 onward), pick **Arm64**.
   - If you have an **Intel Mac**, pick **x64**.
   - Not sure which one? Click the Apple menu → "About This Mac". If it says "Chip: Apple M…" it is Apple Silicon. If it says "Processor: Intel…" it is Intel.
3. Open the downloaded `.pkg` file. macOS will ask for your password and walk you through the installer — just keep clicking **Continue** and **Install**.
4. To check it worked, open the **Terminal** app (press **Cmd + Space**, type `terminal`, hit Enter). Type:
   ```
   dotnet --version
   ```
   and press **Return**. You should see something like `10.0.100`.

### Step 2 — Get the project files

You have two ways to download the code. Either is fine.

**Easiest (no Git needed):**

1. Open this page in your browser:
   <https://github.com/rOsOr9981/cs20200-shifting-minesweeper>
2. Click the green **`< > Code`** button near the top right.
3. Click **Download ZIP**.
4. Find the downloaded `cs20200-shifting-minesweeper-main.zip` file (usually in your **Downloads** folder) and double-click it to unzip. You will end up with a folder named `cs20200-shifting-minesweeper-main`.

**With Git (if you already have Git installed):**

```
git clone https://github.com/rOsOr9981/cs20200-shifting-minesweeper.git
```

This creates a folder called `cs20200-shifting-minesweeper` in whatever directory your terminal is currently in.

### Step 3 — Run the game

#### On Windows

1. Open the project folder in **File Explorer** (the unzipped folder from Step 2).
2. Double-click **`run.bat`**.
   - If a blue "Windows protected your PC" dialog appears, click **More info** → **Run anyway**. This happens because `.bat` files from the internet are treated as untrusted by Windows; the file is just three lines of plain text and you can open it in Notepad to verify.
3. A black command window will appear and say things like "복원할 프로젝트를 확인하는 중…" / "Restoring NuGet packages…". On the **first run only**, this takes 1–2 minutes while Avalonia downloads. On every later run it is almost instant.
4. A game window titled **"Shifting Minesweeper"** appears. Have fun!

#### On macOS

1. Open the **Terminal** app.
2. Navigate into the project folder. If you unzipped it to your **Downloads** folder, type:
   ```
   cd ~/Downloads/cs20200-shifting-minesweeper-main
   ```
   (If you used `git clone` from your home directory, it's `cd ~/cs20200-shifting-minesweeper` instead. Adjust the path to wherever your project folder is.)
3. The first time only, give the run script permission to execute:
   ```
   chmod +x run.sh
   ```
4. Run the game:
   ```
   ./run.sh
   ```
5. On the **first run only**, Terminal will show "Restoring packages…" and similar messages for 1–2 minutes while Avalonia downloads. After that, the **Shifting Minesweeper** window opens. On every later run it is almost instant.

#### On Linux (bonus)

Same as macOS. Open a terminal, `cd` into the project folder, `chmod +x run.sh`, then `./run.sh`. You need a graphical desktop session, since this is a GUI app.

#### Alternative for any OS — direct `dotnet` command

If `run.bat` / `run.sh` does not work for some reason, you can always run the game manually from inside the project folder:

```
dotnet run
```

This does exactly the same thing the run scripts do.

### Troubleshooting

- **"dotnet: command not found" or "'dotnet' is not recognized"** — The .NET 10 SDK is not installed yet, or your terminal was already open when you installed it. Close and reopen the terminal and try again. If it still fails, repeat Step 1.
- **"permission denied: ./run.sh" on macOS/Linux** — Run `chmod +x run.sh` first (see Step 3, macOS instructions point 3).
- **The first run is very slow** — That is normal. NuGet is downloading Avalonia (~80 MB). It only happens once.
- **A white/blank window appears with no grid** — Close it and run again; this is usually a one-off rendering hiccup on the very first launch after install.
- **You see Korean text "복원할 프로젝트를 확인하는 중…"** in the terminal — That is just .NET printing its progress in Korean because of your system locale. It is normal output, not an error.

### Running the original CLI version instead

The repository also contains the original terminal-based version under [`cli/`](./cli/). It runs without a window, just inside the terminal. To launch it:

```
dotnet run --project cli/ShiftingMinesweeper.fsproj
```

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
