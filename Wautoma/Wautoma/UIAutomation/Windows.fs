module Wautoma.UIAutomation.Windows

open System
open System.Runtime.InteropServices
open System.Windows.Automation
open Wautoma.Logging
open Wautoma.NativeApi
open Wautoma.VirtualDesktops

let allChildren =
    TreeScope.Children, Condition.TrueCondition

let allMainWindows () =
    seq {
        for el in AutomationElement.RootElement.FindAll allChildren do
            yield el
    }


let windowOfProcess processId =
    let automationElementProcess (el: AutomationElement) = el.Current.ProcessId

    allMainWindows ()
    |> Seq.tryFind (fun el -> el |> automationElementProcess = processId)

let allWindowsHandlesForProcess processId =
    let rec run current acc =
        let found =
            FindWindowEx(IntPtr.Zero, current, null, null)

        if found = IntPtr.Zero then
            acc
        else
            let mutable windowProcessId: Int32 = 0

            GetWindowThreadProcessId(found, &windowProcessId)
            |> ignore

            if windowProcessId = processId then
                (found :: acc)
            else
                acc
            |> run found

    run IntPtr.Zero []

let nameStartsWith text (el: AutomationElement) =
    let name =
        el.GetCurrentPropertyValue(AutomationElement.NameProperty)
        |> string

    name.StartsWith(text)

let nameEndsWith text (el: AutomationElement) =
    let name =
        el.GetCurrentPropertyValue(AutomationElement.NameProperty)
        |> string

    name.EndsWith(text)

let nameIs text (el: AutomationElement) =
    let name =
        el.GetCurrentPropertyValue(AutomationElement.NameProperty)
        |> string

    name = text

let nameContains text (el: AutomationElement) =
    let name =
        el.GetCurrentPropertyValue(AutomationElement.NameProperty)
        |> string

    name.Contains(text)

let classNameIs text (el: AutomationElement) =
    let name =
        el.GetCurrentPropertyValue(AutomationElement.ClassNameProperty)
        |> string

    name = text

let getWindowPattern (el: AutomationElement) : WindowPattern option =
    try
        let windowPattern =
            el.GetCurrentPattern(WindowPattern.Pattern) :?> WindowPattern

        if windowPattern.WaitForInputIdle(10000) then
            //            log "some windowPattern"
            Some windowPattern
        else
            //            log "None windowPattern"
            None
    with
    | :? InvalidOperationException -> None


let getWindowPlacement handle =
    let mutable placement =
        WINDOWPLACEMENT(length = Marshal.SizeOf(typeof<WINDOWPLACEMENT>))

    if GetWindowPlacement(handle, &placement) then
        //        log "getWindowPlacement got something"
        Some placement
    else
        //        log "getWindowPlacement got nothing"
        None


let elementWindowHandle (el: AutomationElement) =
    let winHandleInt =
        el.GetCurrentPropertyValue(AutomationElement.NativeWindowHandleProperty)
        :?> int

    winHandleInt |> IntPtr.op_Explicit


let getAutoElementWindowPlacement
    (el: AutomationElement)
    : WINDOWPLACEMENT option =
    el |> elementWindowHandle |> getWindowPlacement

let getWindowPlacementShowCommandAndFlags
    (windowPlacement: WINDOWPLACEMENT)
    : ShowWindowCommand * WindowPlacementFlags =
    LanguagePrimitives.EnumOfValue windowPlacement.showCmd,
    LanguagePrimitives.EnumOfValue windowPlacement.flags

let setWindowState (windowPattern: WindowPattern) state : unit =
    let interactionState =
        windowPattern.Current.WindowInteractionState

    if interactionState = WindowInteractionState.ReadyForUserInteraction then
        windowPattern.SetWindowVisualState(state)
    else
        ()

let maximizeWindow (windowPattern: WindowPattern) =
    log "maximizeWindow()"
    setWindowState windowPattern WindowVisualState.Maximized

let setWindowToNormal (windowPattern: WindowPattern) =
    log "setWindowToNormal()"
    setWindowState windowPattern WindowVisualState.Normal

let activate (el: AutomationElement) : AutomationElement =
    log "activate()"
    
    getAutoElementWindowPlacement el
    |> Option.map getWindowPlacementShowCommandAndFlags
    |> function
        | Some (cmd, flags) when cmd = ShowWindowCommand.Minimized ->
            log $"got %A{cmd} and %A{flags}"

            let windowFunc =
                if flags &&& WindowPlacementFlags.RestoreToMaximized = WindowPlacementFlags.RestoreToMaximized then
                    log "maximizeWindow"
                    maximizeWindow
                else
                    log "setWindowToNormal"
                    setWindowToNormal

            el
            |> getWindowPattern
            |> Option.map windowFunc
            |> ignore

            el
        | Some (cmd, flags) ->
            log $"got %A{cmd} and %A{flags}"
            el
        | None ->
            log "got None"
            el

let focus (el: AutomationElement) : AutomationElement =
    log "focus()"
    
    try
        el.SetFocus()
    with
    | :? InvalidOperationException as ex ->
        try
            $"Could not set focus on window %s{el.Current.Name}, "
            + $"reason: %s{ex.Message}"
            |> log
        with
        | :? ElementNotAvailableException -> ()

        let handle = el |> elementWindowHandle
        SetForegroundWindow handle |> ignore
        SetFocus handle |> ignore

    | :? ElementNotAvailableException -> ()

    el


let makeSticky (el: AutomationElement) : AutomationElement =
    log "--> makeSticky()"

    let handle = el |> elementWindowHandle
    virtualDesktopManagerWrapper().PinWindow handle

    log "<-- makeSticky() "

    el
