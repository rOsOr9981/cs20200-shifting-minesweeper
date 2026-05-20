module Game

open System
open Board

type GameResult = Continue | GameOver | Won

let handleReveal (state: GameState) (r: int, c: int) (rng: Random) : GameState * GameResult =
    if Set.contains (r, c) state.Revealed then
        printfn "That cell is already revealed!"
        (state, Continue)
    elif Set.contains (r, c) state.Mines then
        (state, GameOver)
    else
        let revealed = revealCells state (r, c)
        let newCount = revealed.RevealCount + 1
        let counted = { revealed with RevealCount = newCount }

        let shifted =
            if newCount % 5 = 0 then
                printfn ""
                printfn "EARTHQUAKE! All flags are lost and mines have shifted."
                shiftMines counted rng
            else counted

        let nonMineCells = SIZE * SIZE - Set.count shifted.Mines
        if Set.count shifted.Revealed >= nonMineCells then
            (shifted, Won)
        else
            (shifted, Continue)

let handleFlag (state: GameState) (r: int, c: int) : GameState =
    if Set.contains (r, c) state.Revealed then
        printfn "Cannot flag a revealed cell!"
        state
    else
        let newState = toggleFlag state (r, c)
        if Set.contains (r, c) newState.Flags then
            printfn "Flagged (%d, %d)." (r + 1) (c + 1)
        else
            printfn "Unflagged (%d, %d)." (r + 1) (c + 1)
        newState
