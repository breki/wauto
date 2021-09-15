module Wautoma.UIStuff

open System
open System.Drawing
open System.Windows.Forms
open Wautoma.KeyboardHandling
open Wautoma.KeysTypes
open Wautoma.Settings

type EventHandlerFunc = obj -> EventArgs -> unit

let logActivityIntoTextBox (loggingTextBox: TextBox) msg : unit =
    let logFunc () =
        loggingTextBox.AppendText(msg + Environment.NewLine)

    loggingTextBox.Invoke(MethodInvoker(logFunc))
    |> ignore


type AppForm(hotkeys: Hotkeys) as this =
    inherit Form()

    let components = new System.ComponentModel.Container()

    let loggingTextBox = new TextBox()

    let logActivity msg =
        logActivityIntoTextBox loggingTextBox msg

    let notifyIcon = new NotifyIcon(components)

    let keyboardHandler =
        new KeyboardHandler(hotkeys, logActivity)

    let settingsFileName = "Wautoma-settings.json"

    let showLogWindow _ _ =
        this.Show()
        this.WindowState <- FormWindowState.Normal
        this.ShowInTaskbar <- true

    let hideLogWindow () =
        this.WindowState <- FormWindowState.Minimized
        this.Hide()

    let suspendResumeHotkeys (menuItem: MenuItem) _ _ =
        if keyboardHandler.IsSuspended then
            keyboardHandler.Resume()
            menuItem.Text <- "Suspend Hotkeys"
        else
            keyboardHandler.Suspend()
            menuItem.Text <- "Resume Hotkeys"

    let saveAppState () =
        loadSettings settingsFileName
        |> setSetting "form.x" this.Location.X
        |> setSetting "form.y" this.Location.Y
        |> setSetting "form.width" this.Width
        |> setSetting "form.height" this.Height
        |> saveSettings settingsFileName

    let onMenuItemClick (menuItem: MenuItem) eventHandlerFunc =
        EventHandler eventHandlerFunc
        |> menuItem.Click.AddHandler

    let createMenuItem text (eventHandlerFunc: EventHandlerFunc option) =
        let menuItem = new MenuItem(text)

        match eventHandlerFunc with
        | Some eventHandlerFunc -> onMenuItemClick menuItem eventHandlerFunc
        | None -> ()

        menuItem

    let suspendResumeMenuItem = createMenuItem "Suspend Hotkeys" None


    do
        let settings = loadSettings settingsFileName

        this.Text <- "Wautoma"
        this.StartPosition <- FormStartPosition.Manual

        this.Location <-
            Point(
                settings |> getSettingInt "form.x" 10,
                settings |> getSettingInt "form.y" 10
            )

        this.Width <- settings |> getSettingInt "form.width" 500
        this.Height <- settings |> getSettingInt "form.height" 500
        this.ShowInTaskbar <- false

        let icon =
            new Icon(@"\src\wauto\Wautoma\Wautoma\icon.ico")

        this.Icon <- icon

        loggingTextBox.Anchor <-
            AnchorStyles.Top
            ||| AnchorStyles.Right
            ||| AnchorStyles.Bottom
            ||| AnchorStyles.Left

        loggingTextBox.Multiline <- true
        loggingTextBox.ReadOnly <- true
        loggingTextBox.Dock <- DockStyle.Fill
        loggingTextBox.ScrollBars <- ScrollBars.Vertical

        this.Controls.Add(loggingTextBox)

        // todo igor: use an embedded icon here
        notifyIcon.Icon <- icon
        notifyIcon.Visible <- true

        EventHandler showLogWindow
        |> notifyIcon.Click.AddHandler

        onMenuItemClick
            suspendResumeMenuItem
            (suspendResumeHotkeys suspendResumeMenuItem)

        notifyIcon.ContextMenu <-
            new ContextMenu(
                [| createMenuItem "Show Log Window" (Some showLogWindow)
                   suspendResumeMenuItem |]
            )

        keyboardHandler.Start()

    member this.LoggingTextBox = loggingTextBox

    override this.OnLoad _ = hideLogWindow ()

    override this.OnClosing e =
        e.Cancel <- true
        hideLogWindow ()

    //        keyboardHandler.Stop()

    override this.Dispose disposing =
        (keyboardHandler :> IDisposable).Dispose()

        if disposing then
            if components <> null then
                components.Dispose()

        base.Dispose(disposing)




let createUIElements hotkeys =
    let form = new AppForm(hotkeys)

    let loggingFunc =
        form.LoggingTextBox |> logActivityIntoTextBox

    form, loggingFunc
