module Wautoma.UIStuff

open System
open System.Drawing
open System.Reflection
open System.Windows.Forms
open Wautoma.KeyboardHandling
open Wautoma.KeysTypes
open Wautoma.Logging
open Wautoma.Settings
open Wautoma.VirtualDesktops
open Wautoma.UIAutomation.Windows

let wautomaVersion () =
    Assembly.GetExecutingAssembly().GetName().Version

type EventHandlerFunc = obj -> EventArgs -> unit

let logActivityIntoTextBox (loggingTextBox: TextBox) msg : unit =
    let logFunc () =
        loggingTextBox.AppendText(
            DateTime.Now.ToString("[MM-dd HH:mm:ss] ")
            + msg
            + Environment.NewLine
        )

        let maxLines = 200

        if loggingTextBox.Lines.Length > maxLines then
            let croppedLog: string [] =
                Array.sub
                    loggingTextBox.Lines
                    (loggingTextBox.Lines.Length - maxLines)
                    maxLines

            loggingTextBox.Lines <- croppedLog

        if loggingTextBox.Visible then
            loggingTextBox.SelectionStart <- loggingTextBox.Text.Length
            loggingTextBox.ScrollToCaret()

    loggingTextBox.Invoke(MethodInvoker(logFunc))
    |> ignore

type WautomaFormState = bool * FormWindowState

let mutable showWautomaFormForSwitching: unit -> WautomaFormState =
    (fun () -> true, FormWindowState.Normal)

let mutable hideWautomaFormForSwitching: WautomaFormState -> unit =
    (fun _ -> ())

type UnitDelegate = delegate of unit -> unit
type DelegateWithResult<'TResult> = delegate of unit -> 'TResult
type DelegateWithParameter<'TParam> = delegate of 'TParam -> unit

type AppForm(hotkeys: Hotkeys) as this =
    inherit Form()

    let components = new System.ComponentModel.Container()

    let loggingTextBox = new TextBox()

    let logActivity msg =
        logActivityIntoTextBox loggingTextBox msg

    let taskbarIcon = new NotifyIcon(components)

    let keyboardHandler =
        new KeyboardHandler(hotkeys, logActivity)

    let settingsFileName = "Wautoma-settings.json"

    let restoreLogWindowPositionAndSizeFromSettingsFile () =
        let settings = loadSettings settingsFileName

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

    let createLoggingTextBox () =
        loggingTextBox.Anchor <-
            AnchorStyles.Top
            ||| AnchorStyles.Right
            ||| AnchorStyles.Bottom
            ||| AnchorStyles.Left

        loggingTextBox.Multiline <- true
        loggingTextBox.ReadOnly <- true
        loggingTextBox.Dock <- DockStyle.Fill
        loggingTextBox.ScrollBars <- ScrollBars.Vertical

        let moveCursorToEnd _ _ =
            if loggingTextBox.Visible then
                loggingTextBox.SelectionStart <- loggingTextBox.Text.Length
                loggingTextBox.ScrollToCaret()
                loggingTextBox.Refresh()

        EventHandler moveCursorToEnd
        |> loggingTextBox.VisibleChanged.AddHandler

        this.Controls.Add(loggingTextBox)

    let invokeUnitFunc func =
        this.Invoke(UnitDelegate func) |> ignore

    let invokeFuncWithResult (func: unit -> 'TResult) =
        this.Invoke(DelegateWithResult<'TResult> func) :?> 'TResult

    let invokeFuncWithParam (func: 'TParam -> unit) (param: 'TParam) =
        this.Invoke(DelegateWithParameter<'TParam> func, param)
        |> ignore

    let showLogWindow _ _ =
        invokeUnitFunc
            (fun () ->
                this.Show()
                this.WindowState <- FormWindowState.Normal
                this.ShowInTaskbar <- true
                this.Opacity <- 1.

                virtualDesktopManagerWrapper()
                    .PinWindow(this.Handle))

    let hideLogWindow () =
        invokeUnitFunc
            (fun () ->
                this.WindowState <- FormWindowState.Minimized
                this.Hide())

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

    let createTaskbarIcon icon =
        taskbarIcon.Icon <- icon
        taskbarIcon.Visible <- true
        taskbarIcon.Text <- $"Wautoma %A{wautomaVersion ()}"

        MouseEventHandler showLogWindowOnLeftClick
        |> taskbarIcon.MouseClick.AddHandler

        onMenuItemClick
            suspendResumeMenuItem
            (suspendResumeHotkeys suspendResumeMenuItem)

        onMenuItemClick
            debugLoggingMenuItem
            (toggleDebugLogging debugLoggingMenuItem)

        taskbarIcon.ContextMenu <-
            new ContextMenu(
                [| menuItem "Show Log Window" (Some showLogWindow)
                   menuItemsSeparator ()
                   suspendResumeMenuItem
                   debugLoggingMenuItem
                   menuItemsSeparator ()
                   menuItem "Exit" (Some exit) |]
            )

    do
        this.Text <- $"Wautoma %A{wautomaVersion ()}"
        this.StartPosition <- FormStartPosition.Manual
        this.ShowInTaskbar <- false

        let icon = new Icon("icon.ico")
        this.Icon <- icon

        restoreLogWindowPositionAndSizeFromSettingsFile ()

        createLoggingTextBox ()
        createTaskbarIcon icon

        showWautomaFormForSwitching <-
            fun () ->
                log "showWautomaFormForSwitching"

                let existingState =
                    invokeFuncWithResult
                        (fun () ->
                            let existingState = this.Visible, this.WindowState

                            logActivityIntoTextBox
                                this.LoggingTextBox
                                $"ShowOnSwitch form.Visible=%A{this.Visible}"

                            this.Opacity <- 0.
                            this.ShowInTaskbar <- false
                            this.Show()
                            this.WindowState <- FormWindowState.Normal

                            existingState)

                virtualDesktopManagerWrapper()
                    .PinWindow(this.Handle)

                let wautomaMaybe =
                    allMainWindows ()
                    |> Seq.tryFind (nameStartsWith "Wautoma")

                match wautomaMaybe with
                | Some app -> app |> activate |> focus |> ignore
                | None -> ()

                log
                    $"showWautomaFormForSwitching existingState=%A{existingState}"

                existingState

        hideWautomaFormForSwitching <-
            fun (existingState: WautomaFormState) ->
                log
                    $"hideWautomaFormForSwitching existingState=%A{existingState}"

                let hideFunc =
                    fun (existingState: WautomaFormState) ->
                        let showForm, windowState = existingState

                        if showForm then
                            this.WindowState <- windowState
                        else
                            this.Visible <- false

                        this.Opacity <- 1.

                invokeFuncWithParam hideFunc existingState


        keyboardHandler.Start()

    member this.LoggingTextBox = loggingTextBox

    override this.OnLoad _ = hideLogWindow ()

    override this.OnFormClosing e =
        match e.CloseReason with
        | CloseReason.UserClosing ->
            e.Cancel <- true
            hideLogWindow ()

            logActivityIntoTextBox
                this.LoggingTextBox
                $"OnFormClosing form.Visible=%A{this.Visible}"
        | _ -> ()

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

    log <- loggingFunc

    form, loggingFunc
