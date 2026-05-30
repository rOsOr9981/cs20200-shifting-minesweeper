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

    let initGame (_: Random) =
        // Mines are placed lazily on the first reveal (see ensureMinesPlaced)
        // so the first reveal is guaranteed safe by construction — the clicked
        // cell is excluded from the candidate pool when mines are generated.
        { Mines = Set.empty
          Revealed = Set.empty
          Flags = Set.empty
          RevealCount = 0 }

    /// Place NUM_MINES mines on the first reveal of the game. The clicked cell
    /// AND its 8 neighbors are excluded from the mine pool so that the clicked
    /// cell has zero adjacent mines, triggering the flood fill and opening a
    /// satisfyingly large empty area on the very first click.
    let private ensureMinesPlaced (state: GameState) (safeR: int, safeC: int) (rng: Random) =
        if not (Set.isEmpty state.Mines) then state
        else
            let safeZone =
                seq {
                    for dr in -1 .. 1 do
                        for dc in -1 .. 1 do
                            yield (safeR + dr, safeC + dc)
                }
                |> Set.ofSeq
            let mutable mines = Set.empty
            while Set.count mines < NUM_MINES do
                let r = rng.Next(SIZE)
                let c = rng.Next(SIZE)
                if not (Set.contains (r, c) safeZone) then
                    mines <- Set.add (r, c) mines
            { state with Mines = mines }

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
        elif Set.count state.Flags >= NUM_MINES then
            // Cap flag count at the number of mines: prevents flagging more
            // cells than there could possibly be mines.
            state
        else
            { state with Flags = Set.add (r, c) state.Flags }

    let shiftMines (state: GameState) (rng: Random) =
        // Mines move only orthogonally (up/down/left/right), one cell, and
        // never into revealed cells. Cells that were flagged just before the
        // shift are valid targets because flags are cleared as part of the shift.
        //
        // Mine count must be preserved: every mine ends up at a distinct cell.
        // We try a random sequential assignment; if it fails (two mines forced
        // into the same cell), we retry with a different random ordering. With
        // 10 mines on an 81-cell board, a valid assignment is found almost
        // always on the first try.
        let dirs = [| (-1, 0); (1, 0); (0, -1); (0, 1) |]

        let tryAttempt () =
            let order =
                state.Mines |> Set.toArray |> Array.sortBy (fun _ -> rng.Next())
            let mutable placed = Set.empty
            let mutable success = true
            let mutable i = 0
            while success && i < order.Length do
                let (r, c) = order.[i]
                let valid =
                    dirs
                    |> Array.choose (fun (dr, dc) ->
                        let nr, nc = r + dr, c + dc
                        if nr >= 0 && nr < SIZE && nc >= 0 && nc < SIZE
                           && not (Set.contains (nr, nc) state.Revealed)
                           && not (Set.contains (nr, nc) placed)
                        then Some (nr, nc)
                        else None)
                let chosen =
                    if valid.Length > 0 then Some valid.[rng.Next(valid.Length)]
                    elif not (Set.contains (r, c) placed) then Some (r, c)
                    else None
                match chosen with
                | Some p -> placed <- Set.add p placed
                | None -> success <- false
                i <- i + 1
            if success && Set.count placed = order.Length then Some placed
            else None

        let rec attempt n =
            if n >= 100 then state.Mines  // fallback: no movement this round
            else
                match tryAttempt () with
                | Some result -> result
                | None -> attempt (n + 1)

        { state with Mines = attempt 0; Flags = Set.empty }

    /// Attempt to reveal a cell. Returns Revealed, HitMine, or NoOp.
    /// Flagged cells cannot be revealed — the player must unflag first.
    /// On the very first reveal of a game, mines are generated lazily with the
    /// clicked cell excluded, so the first reveal is always safe.
    /// When the resulting RevealCount is a multiple of 5, an earthquake fires
    /// and mines are shifted before returning.
    let handleReveal (state: GameState) (r: int, c: int) (rng: Random) =
        if Set.contains (r, c) state.Revealed then NoOp
        elif Set.contains (r, c) state.Flags then NoOp
        else
            let state = ensureMinesPlaced state (r, c) rng
            if Set.contains (r, c) state.Mines then HitMine
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
