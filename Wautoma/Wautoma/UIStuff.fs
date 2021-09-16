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

    let showLogWindowOnLeftClick obj (args: MouseEventArgs) =
        if args.Button &&& MouseButtons.Left = MouseButtons.Left then
            showLogWindow obj args

    let suspendResumeHotkeys (menuItem: MenuItem) _ _ =
        if keyboardHandler.IsSuspended then
            keyboardHandler.Resume()
            menuItem.Text <- "Suspend Hotkeys"
        else
            keyboardHandler.Suspend()
            menuItem.Text <- "Resume Hotkeys"

    let toggleDebugLogging (menuItem: MenuItem) _ _ =
        if keyboardHandler.DebugLoggingEnabled then
            keyboardHandler.DisableDebugLogging()
            menuItem.Text <- "Enable Debug Logging"
        else
            keyboardHandler.EnableDebugLogging()
            menuItem.Text <- "Disable Debug Logging"


    let saveAppState () =
        // the window should be in normal state before we measure it
        this.Show()
        this.WindowState <- FormWindowState.Normal

        loadSettings settingsFileName
        |> setSetting "form.x" this.Location.X
        |> setSetting "form.y" this.Location.Y
        |> setSetting "form.width" this.Width
        |> setSetting "form.height" this.Height
        |> saveSettings settingsFileName

    let exit _ _ =
        keyboardHandler.Stop()
        saveAppState ()
        Application.Exit()

    let onMenuItemClick (menuItem: MenuItem) eventHandlerFunc =
        EventHandler eventHandlerFunc
        |> menuItem.Click.AddHandler

    let menuItem text (eventHandlerFunc: EventHandlerFunc option) =
        let menuItem = new MenuItem(text)

        match eventHandlerFunc with
        | Some eventHandlerFunc -> onMenuItemClick menuItem eventHandlerFunc
        | None -> ()

        menuItem

    let menuItemsSeparator () = menuItem "-" None

    let suspendResumeMenuItem = menuItem "Suspend Hotkeys" None
    let debugLoggingMenuItem = menuItem "Enable Debug Logging" None

    do
        let settings = loadSettings settingsFileName

        this.Text <- "Wautoma"
        this.StartPosition <- FormStartPosition.Manual

        let x =
            settings |> getSettingInt "form.x" 10 |> max 0

        let y =
            settings |> getSettingInt "form.y" 10 |> max 0

        let width =
            settings
            |> getSettingInt "form.width" 500
            |> max 300

        let height =
            settings
            |> getSettingInt "form.height" 500
            |> max 200

        this.Location <- Point(x, y)
        this.Width <- width
        this.Height <- height
        this.ShowInTaskbar <- false

        let icon = new Icon("icon.ico")

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

        notifyIcon.Icon <- icon
        notifyIcon.Visible <- true

        MouseEventHandler showLogWindowOnLeftClick
        |> notifyIcon.MouseClick.AddHandler

        onMenuItemClick
            suspendResumeMenuItem
            (suspendResumeHotkeys suspendResumeMenuItem)

        onMenuItemClick
            debugLoggingMenuItem
            (toggleDebugLogging debugLoggingMenuItem)

        notifyIcon.ContextMenu <-
            new ContextMenu(
                [| menuItem "Show Log Window" (Some showLogWindow)
                   menuItemsSeparator ()
                   suspendResumeMenuItem
                   debugLoggingMenuItem
                   menuItemsSeparator ()
                   menuItem "Exit" (Some exit) |]
            )

        keyboardHandler.Start()

    member this.LoggingTextBox = loggingTextBox

    override this.OnLoad _ = hideLogWindow ()

    override this.OnClosing e =
        e.Cancel <- true
        hideLogWindow ()

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
