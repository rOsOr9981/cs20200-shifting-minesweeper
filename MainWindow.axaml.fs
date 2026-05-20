namespace ShiftingMinesweeper

open System
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Primitives
open Avalonia.Input
open Avalonia.Layout
open Avalonia.Markup.Xaml
open Avalonia.Media
open Avalonia.Threading
open Board

type MainWindow() as this =
    inherit Window()

    let rng = Random()
    let mutable state = initGame rng
    let mutable gameOver = false
    let mutable won = false
    let mutable animating = false

    let cells : Border[,] = Array2D.zeroCreate SIZE SIZE
    let cellTexts : TextBlock[,] = Array2D.zeroCreate SIZE SIZE

    // Classic Minesweeper colors for the adjacency numbers 1-8
    let numberBrush n =
        let c =
            match n with
            | 1 -> Color.FromRgb(0x29uy, 0x6Fuy, 0xCBuy)  // blue
            | 2 -> Color.FromRgb(0x2Euy, 0xA4uy, 0x3Auy)  // green
            | 3 -> Color.FromRgb(0xD9uy, 0x3Auy, 0x3Auy)  // red
            | 4 -> Color.FromRgb(0x1Auy, 0x35uy, 0x66uy)  // dark blue
            | 5 -> Color.FromRgb(0x80uy, 0x0Cuy, 0x0Cuy)  // dark red
            | 6 -> Color.FromRgb(0x12uy, 0x99uy, 0xA8uy)  // teal
            | 7 -> Color.FromRgb(0x33uy, 0x33uy, 0x33uy)  // black
            | _ -> Color.FromRgb(0x66uy, 0x66uy, 0x66uy)  // gray
        SolidColorBrush(c) :> IBrush

    let hiddenBrush     = SolidColorBrush(Color.FromRgb(0x4Cuy, 0x4Cuy, 0x52uy)) :> IBrush
    let hiddenHoverBrush= SolidColorBrush(Color.FromRgb(0x60uy, 0x60uy, 0x66uy)) :> IBrush
    let revealedBrush   = SolidColorBrush(Color.FromRgb(0xECuy, 0xECuy, 0xECuy)) :> IBrush
    let mineBrush       = SolidColorBrush(Color.FromRgb(0x99uy, 0x12uy, 0x12uy)) :> IBrush
    let flagFg          = SolidColorBrush(Color.FromRgb(0xFFuy, 0x6Buy, 0x6Buy)) :> IBrush

    do this.InitializeComponent()

    member private this.InitializeComponent() =
        AvaloniaXamlLoader.Load(this)
        let boardGrid = this.FindControl<UniformGrid>("BoardGrid")
        let restartBtn = this.FindControl<Button>("RestartButton")

        // Build cells
        for r in 0 .. SIZE - 1 do
            for c in 0 .. SIZE - 1 do
                let txt =
                    TextBlock(
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 20.0,
                        FontWeight = FontWeight.Bold)
                let cell =
                    Border(
                        Background = hiddenBrush,
                        Margin = Thickness(1.0),
                        CornerRadius = CornerRadius(3.0),
                        Child = txt)
                let row, col = r, c
                cell.PointerEntered.Add(fun _ ->
                    if not (Set.contains (row, col) state.Revealed)
                       && not gameOver && not won then
                        cell.Background <- hiddenHoverBrush)
                cell.PointerExited.Add(fun _ ->
                    if not (Set.contains (row, col) state.Revealed)
                       && not gameOver && not won then
                        cell.Background <- hiddenBrush)
                cell.PointerPressed.Add(fun e ->
                    if not gameOver && not won && not animating then
                        let point = e.GetCurrentPoint(cell)
                        if point.Properties.IsRightButtonPressed then
                            this.HandleRightClick(row, col)
                        elif point.Properties.IsLeftButtonPressed then
                            this.HandleLeftClick(row, col))
                cells.[r, c] <- cell
                cellTexts.[r, c] <- txt
                boardGrid.Children.Add(cell) |> ignore

        restartBtn.Click.Add(fun _ -> this.Restart())
        this.RefreshAll()

    member private this.RefreshCell(r: int, c: int) =
        let cell = cells.[r, c]
        let txt = cellTexts.[r, c]
        if gameOver && Set.contains (r, c) state.Mines then
            cell.Background <- mineBrush
            txt.Text <- "*"
            txt.Foreground <- SolidColorBrush(Colors.White)
        elif Set.contains (r, c) state.Revealed then
            cell.Background <- revealedBrush
            let n = countAdjacentMines state.Mines (r, c)
            if n = 0 then
                txt.Text <- ""
            else
                txt.Text <- string n
                txt.Foreground <- numberBrush n
        elif Set.contains (r, c) state.Flags then
            cell.Background <- hiddenBrush
            txt.Text <- "F"
            txt.Foreground <- flagFg
        else
            cell.Background <- hiddenBrush
            txt.Text <- ""

    member private this.RefreshAll() =
        for r in 0 .. SIZE - 1 do
            for c in 0 .. SIZE - 1 do
                this.RefreshCell(r, c)
        this.RefreshStatus()

    member private this.RefreshStatus() =
        let qc = this.FindControl<TextBlock>("QuakeCounter")
        let fc = this.FindControl<TextBlock>("FlagCounter")
        let sl = this.FindControl<TextBlock>("StatusLabel")
        qc.Text <- string (5 - (state.RevealCount % 5))
        fc.Text <- sprintf "%d / %d" (Set.count state.Flags) NUM_MINES
        if gameOver then
            sl.Text <- "GAME OVER"
            sl.Foreground <- SolidColorBrush(Colors.Red)
        elif won then
            sl.Text <- "YOU WIN!"
            sl.Foreground <- SolidColorBrush(Colors.LimeGreen)
        else
            sl.Text <- "Playing"
            sl.Foreground <- SolidColorBrush(Color.FromRgb(0x6Buy, 0xCBuy, 0x77uy))

    member private this.HandleLeftClick(r: int, c: int) =
        match handleReveal state (r, c) rng with
        | NoOp -> ()
        | HitMine ->
            gameOver <- true
            this.RefreshAll()
        | Revealed(newState, earthquake) ->
            state <- newState
            if earthquake then
                this.PlayEarthquakeAndRefresh()
            else
                this.RefreshAll()
                if isWon state then
                    won <- true
                    this.RefreshStatus()

    member private this.HandleRightClick(r: int, c: int) =
        state <- toggleFlag state (r, c)
        this.RefreshCell(r, c)
        this.RefreshStatus()

    member private this.Restart() =
        state <- initGame rng
        gameOver <- false
        won <- false
        animating <- false
        let board = this.FindControl<Border>("BoardContainer")
        board.RenderTransform <- null
        let overlay = this.FindControl<Border>("FlashOverlay")
        overlay.Opacity <- 0.0
        let eqText = this.FindControl<TextBlock>("EarthquakeText")
        eqText.Opacity <- 0.0
        this.RefreshAll()

    member private this.PlayEarthquakeAndRefresh() =
        animating <- true
        let board = this.FindControl<Border>("BoardContainer")
        let overlay = this.FindControl<Border>("FlashOverlay")
        let eqText = this.FindControl<TextBlock>("EarthquakeText")
        let transform = TranslateTransform()
        board.RenderTransform <- transform
        eqText.Opacity <- 1.0

        // Shake offsets played one per timer tick
        let shakes = [| -16.0; 14.0; -12.0; 10.0; -7.0; 5.0; -3.0; 2.0; 0.0 |]
        // Flash opacities for the yellow overlay
        let flashes = [| 0.85; 0.0; 0.7; 0.0; 0.5; 0.0; 0.3; 0.0 |]
        let totalFrames = max shakes.Length flashes.Length + 4
        let mutable frame = 0

        let timer = DispatcherTimer()
        timer.Interval <- TimeSpan.FromMilliseconds(48.0)
        timer.Tick.Add(fun _ ->
            if frame < shakes.Length then transform.X <- shakes.[frame]
            if frame < flashes.Length then overlay.Opacity <- flashes.[frame]
            // Fade the EARTHQUAKE label out near the end
            if frame >= totalFrames - 4 then
                eqText.Opacity <- eqText.Opacity * 0.6
            frame <- frame + 1
            if frame >= totalFrames then
                timer.Stop()
                transform.X <- 0.0
                overlay.Opacity <- 0.0
                eqText.Opacity <- 0.0
                this.RefreshAll()
                if isWon state then
                    won <- true
                    this.RefreshStatus()
                animating <- false)
        timer.Start()
