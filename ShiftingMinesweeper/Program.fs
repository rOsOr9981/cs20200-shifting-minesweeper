module Program

open System
open Board
open Game

[<EntryPoint>]
let main _ =
    let rng = Random()
    let mutable state = initGame rng
    let mutable running = true

    printfn "=================================="
    printfn "    CLI SHIFTING MINESWEEPER"
    printfn "=================================="
    printfn "R <row> <col>  : Reveal a cell"
    printfn "F <row> <col>  : Flag / Unflag a cell"
    printfn "Every 5th reveal triggers EARTHQUAKE!"
    printfn "All flags vanish and mines shift!"
    printfn "=================================="
    printfn ""
    printBoard state false

    while running do
        let reveals = state.RevealCount
        let toNext = 5 - (reveals % 5)
        printf "\n[Reveals until EARTHQUAKE: %d]  Enter command: " toNext
        let input = Console.ReadLine()
        if isNull input then
            running <- false
        else
            let parts =
                input.Trim().ToUpper().Split([|' '; '\t'|], StringSplitOptions.RemoveEmptyEntries)
            if parts.Length = 3 then
                match Int32.TryParse(parts.[1]), Int32.TryParse(parts.[2]) with
                | (true, row), (true, col)
                    when row >= 1 && row <= SIZE && col >= 1 && col <= SIZE ->
                    let r, c = row - 1, col - 1
                    match parts.[0] with
                    | "R" ->
                        let newState, result = handleReveal state (r, c) rng
                        state <- newState
                        match result with
                        | GameOver ->
                            printfn ""
                            printfn "*** BOOM! You hit a mine! GAME OVER ***"
                            printBoard state true
                            running <- false
                        | Won ->
                            printfn ""
                            printfn "*** Congratulations! All mines cleared! YOU WIN! ***"
                            printBoard state false
                            running <- false
                        | Continue ->
                            printBoard state false
                    | "F" ->
                        state <- handleFlag state (r, c)
                        printBoard state false
                    | cmd ->
                        printfn "Unknown command '%s'. Use R or F." cmd
                | _ ->
                    printfn "Invalid coordinates. Row and column must be 1-%d." SIZE
            else
                printfn "Invalid input. Format: R <row> <col>  or  F <row> <col>"

    0
