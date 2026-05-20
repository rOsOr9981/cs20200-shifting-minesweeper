namespace ShiftingMinesweeper

open System
open System.Collections.Generic

module Board =
    let SIZE = 9
    let NUM_MINES = 10

    type GameState = {
        Mines: Set<int * int>
        Revealed: Set<int * int>
        Flags: Set<int * int>
        RevealCount: int
    }

    type RevealResult =
        | Revealed of GameState * earthquake: bool
        | HitMine
        | NoOp

    let countAdjacentMines (mines: Set<int * int>) (r: int, c: int) =
        let dirs = [(-1,-1);(-1,0);(-1,1);(0,-1);(0,1);(1,-1);(1,0);(1,1)]
        dirs |> List.sumBy (fun (dr, dc) ->
            let nr, nc = r + dr, c + dc
            if nr >= 0 && nr < SIZE && nc >= 0 && nc < SIZE && Set.contains (nr, nc) mines
            then 1 else 0)

    let initGame (rng: Random) =
        let mutable mines = Set.empty
        while Set.count mines < NUM_MINES do
            let r = rng.Next(SIZE)
            let c = rng.Next(SIZE)
            mines <- Set.add (r, c) mines
        { Mines = mines; Revealed = Set.empty; Flags = Set.empty; RevealCount = 0 }

    let revealCells (state: GameState) (startR: int, startC: int) =
        if Set.contains (startR, startC) state.Mines then state
        else
            let queue = Queue<int * int>()
            queue.Enqueue((startR, startC))
            let mutable revealed = state.Revealed
            while queue.Count > 0 do
                let (r, c) = queue.Dequeue()
                if r >= 0 && r < SIZE && c >= 0 && c < SIZE
                   && not (Set.contains (r, c) revealed)
                   && not (Set.contains (r, c) state.Mines) then
                    revealed <- Set.add (r, c) revealed
                    if countAdjacentMines state.Mines (r, c) = 0 then
                        for (dr, dc) in [(-1,0);(1,0);(0,-1);(0,1);(-1,-1);(-1,1);(1,-1);(1,1)] do
                            queue.Enqueue((r + dr, c + dc))
            { state with Revealed = revealed }

    let toggleFlag (state: GameState) (r: int, c: int) =
        if Set.contains (r, c) state.Revealed then state
        elif Set.contains (r, c) state.Flags then
            { state with Flags = Set.remove (r, c) state.Flags }
        else
            { state with Flags = Set.add (r, c) state.Flags }

    let shiftMines (state: GameState) (rng: Random) =
        // Mines move only orthogonally (up/down/left/right), one cell, and
        // never into revealed cells. Cells that were flagged just before the
        // shift are valid targets because flags are cleared as part of the shift.
        let dirs = [| (-1, 0); (1, 0); (0, -1); (0, 1) |]
        let mines = state.Mines |> Set.toArray |> Array.sortBy (fun _ -> rng.Next())
        let mutable placed = Set.empty
        for (r, c) in mines do
            let valid =
                dirs
                |> Array.choose (fun (dr, dc) ->
                    let nr, nc = r + dr, c + dc
                    if nr >= 0 && nr < SIZE && nc >= 0 && nc < SIZE
                       && not (Set.contains (nr, nc) state.Revealed)
                       && not (Set.contains (nr, nc) placed)
                    then Some (nr, nc)
                    else None)
            let newPos =
                if valid.Length > 0 then valid.[rng.Next(valid.Length)]
                elif not (Set.contains (r, c) placed) then (r, c)
                else
                    // Rare fallback: original spot taken by another mine.
                    let fb =
                        dirs
                        |> Array.choose (fun (dr, dc) ->
                            let nr, nc = r + dr, c + dc
                            if nr >= 0 && nr < SIZE && nc >= 0 && nc < SIZE
                               && not (Set.contains (nr, nc) state.Revealed)
                            then Some (nr, nc) else None)
                    if fb.Length > 0 then fb.[rng.Next(fb.Length)] else (r, c)
            placed <- Set.add newPos placed
        { state with Mines = placed; Flags = Set.empty }

    /// Attempt to reveal a cell. Returns Revealed, HitMine, or NoOp.
    /// When the resulting RevealCount is a multiple of 5, an earthquake fires
    /// and mines are shifted before returning.
    let handleReveal (state: GameState) (r: int, c: int) (rng: Random) =
        if Set.contains (r, c) state.Revealed then NoOp
        elif Set.contains (r, c) state.Mines then HitMine
        else
            let revealed = revealCells state (r, c)
            let newCount = revealed.RevealCount + 1
            let counted = { revealed with RevealCount = newCount }
            let earthquake = newCount % 5 = 0
            let final =
                if earthquake then shiftMines counted rng
                else counted
            Revealed(final, earthquake)

    let isWon (state: GameState) =
        let nonMineCells = SIZE * SIZE - Set.count state.Mines
        Set.count state.Revealed >= nonMineCells
