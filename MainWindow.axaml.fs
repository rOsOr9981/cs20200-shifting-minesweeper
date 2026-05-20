namespace ShiftingMinesweeper

open System
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Primitives
open Avalonia.Controls.Shapes
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
    let cellBombs : Control[,] = Array2D.zeroCreate SIZE SIZE

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
            | 7 -> Color.FromRgb(0x33uy, 0x33uy, 0x33uy)  // near-black
            | _ -> Color.FromRgb(0x66uy, 0x66uy, 0x66uy)  // gray (8)
        SolidColorBrush(c) :> IBrush

    let hiddenBrush      = SolidColorBrush(Color.FromRgb(0x4Cuy, 0x4Cuy, 0x52uy)) :> IBrush
    let hiddenHoverBrush = SolidColorBrush(Color.FromRgb(0x60uy, 0x60uy, 0x66uy)) :> IBrush
    let revealedBrush    = SolidColorBrush(Color.FromRgb(0xECuy, 0xECuy, 0xECuy)) :> IBrush
    let mineBrush        = SolidColorBrush(Color.FromRgb(0xB3uy, 0x1Cuy, 0x1Cuy)) :> IBrush
    let flagFg           = SolidColorBrush(Color.FromRgb(0xFFuy, 0x6Buy, 0x6Buy)) :> IBrush

    // Build a small vector "bomb" glyph (black ball + highlight + yellow spark)
    // using basic shapes so it renders identically on every platform without
    // relying on an emoji font being installed.
    let createBombGlyph () : Control =
        let grid =
            Grid(
                Width = 30.0,
                Height = 30.0,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center)

        let body =
            Ellipse(
                Width = 22.0,
                Height = 22.0,
                Fill = SolidColorBrush(Color.FromRgb(0x1Auy, 0x1Auy, 0x1Auy)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center)
        grid.Children.Add(body) |> ignore

        let highlight =
            Ellipse(
                Width = 5.0,
                Height = 5.0,
                Fill = SolidColorBrush(Color.FromArgb(200uy, 255uy, 255uy, 255uy)),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = Thickness(7.0, 6.0, 0.0, 0.0))
        grid.Children.Add(highlight) |> ignore

        let spark =
            Ellipse(
                Width = 6.0,
                Height = 6.0,
                Fill = SolidColorBrush(Color.FromRgb(0xFFuy, 0xD9uy, 0x3Duy)),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = Thickness(0.0, 1.0, 1.0, 0.0))
        grid.Children.Add(spark) |> ignore

        grid :> Control

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
                let bomb = createBombGlyph ()
                bomb.IsVisible <- false

                let inner = Grid()
                inner.Children.Add(txt) |> ignore
                inner.Children.Add(bomb) |> ignore

                let cell =
                    Border(
                        Background = hiddenBrush,
                        Margin = Thickness(1.0),
                        CornerRadius = CornerRadius(3.0),
                        Child = inner)
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
                cellBombs.[r, c] <- bomb
                boardGrid.Children.Add(cell) |> ignore

        restartBtn.Click.Add(fun _ -> this.Restart())
        this.RefreshAll()

    member private this.RefreshCell(r: int, c: int) =
        let cell = cells.[r, c]
        let txt = cellTexts.[r, c]
        let bomb = cellBombs.[r, c]
        if gameOver && Set.contains (r, c) state.Mines then
            cell.Background <- mineBrush
            txt.IsVisible <- false
            bomb.IsVisible <- true
        else
            bomb.IsVisible <- false
            txt.IsVisible <- true
            if Set.contains (r, c) state.Revealed then
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
        this.RefreshAll()

    // EARTHQUAKE animation: shake only — no color flash, no flashing text.
    // The status bar's "REVEALS TO QUAKE" counter resetting to 5 is the
    // additional non-visual cue.
    member private this.PlayEarthquakeAndRefresh() =
        animating <- true
        let board = this.FindControl<Border>("BoardContainer")
        let transform = TranslateTransform()
        board.RenderTransform <- transform

        // Shake offsets played one per timer tick (~50ms)
        let shakes = [| -16.0; 14.0; -12.0; 10.0; -7.0; 5.0; -3.0; 2.0; 0.0 |]
        let mutable frame = 0

        let timer = DispatcherTimer()
        timer.Interval <- TimeSpan.FromMilliseconds(50.0)
        timer.Tick.Add(fun _ ->
            if frame < shakes.Length then
                transform.X <- shakes.[frame]
            frame <- frame + 1
            if frame >= shakes.Length then
                timer.Stop()
                transform.X <- 0.0
                this.RefreshAll()
                if isWon state then
                    won <- true
                    this.RefreshStatus()
                animating <- false)
        timer.Start()
