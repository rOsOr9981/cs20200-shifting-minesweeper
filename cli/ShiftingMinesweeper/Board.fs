module Board

open System
open System.Collections.Generic

let SIZE = 9
let NUM_MINES = 10

type GameState = {
    Mines: Set<int * int>
    Revealed: Set<int * int>
    Flags: Set<int * int>
    RevealCount: int
}

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
    let dirs = [| (-1, 0); (1, 0); (0, -1); (0, 1) |]
    // Process mines in random order to avoid directional bias
    let mines = state.Mines |> Set.toArray |> Array.sortBy (fun _ -> rng.Next())
    let mutable placed = Set.empty
    for (r, c) in mines do
        // Valid moves: in bounds, not revealed, not already claimed by a moved mine
        let valid =
            dirs
            |> Array.choose (fun (dr, dc) ->
                let nr, nc = r + dr, c + dc
                if nr >= 0 && nr < SIZE && nc >= 0 && nc < SIZE
                   && not (Set.contains (nr, nc) state.Revealed)
                   && not (Set.contains (nr, nc) placed)
                then Some (nr, nc)
                else None)
        // If original position is available and no valid moves, stay there
        let newPos =
            if valid.Length > 0 then
                valid.[rng.Next(valid.Length)]
            elif not (Set.contains (r, c) placed) then
                (r, c)
            else
                // Fallback: any non-revealed adjacent cell (ignoring mine conflicts)
                let fallback =
                    dirs
                    |> Array.choose (fun (dr, dc) ->
                        let nr, nc = r + dr, c + dc
                        if nr >= 0 && nr < SIZE && nc >= 0 && nc < SIZE
                           && not (Set.contains (nr, nc) state.Revealed)
                        then Some (nr, nc)
                        else None)
                if fallback.Length > 0 then fallback.[rng.Next(fallback.Length)]
                else (r, c)
        placed <- Set.add newPos placed
    { state with Mines = placed; Flags = Set.empty }

let printBoard (state: GameState) (gameOver: bool) =
    printf "   "
    for c in 0..SIZE-1 do
        printf " %d " (c + 1)
    printfn ""
    for r in 0..SIZE-1 do
        printf "%d  " (r + 1)
        for c in 0..SIZE-1 do
            let cell =
                if gameOver && Set.contains (r, c) state.Mines then "[*]"
                elif Set.contains (r, c) state.Revealed then
                    let n = countAdjacentMines state.Mines (r, c)
                    if n = 0 then "   " else sprintf "[%d]" n
                elif Set.contains (r, c) state.Flags then "[F]"
                else "[ ]"
            printf "%s" cell
        printfn ""
