﻿module Wautoma.UIAutomation.Windows

open System
open System.Runtime.InteropServices
open System.Windows.Automation
open Wautoma.NativeApi

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

let getWindowPattern (el: AutomationElement) : WindowPattern option =
    try
        let windowPattern =
            el.GetCurrentPattern(WindowPattern.Pattern) :?> WindowPattern

        if windowPattern.WaitForInputIdle(10000) then
            Some windowPattern
        else
            None
    with
    | :? InvalidOperationException -> None


let getWindowPlacement handle =
    let mutable placement =
        WINDOWPLACEMENT(length = Marshal.SizeOf(typeof<WINDOWPLACEMENT>))

    if GetWindowPlacement(handle, &placement) then
        Some placement
    else
        None

let getAutoElementWindowPlacement
    (el: AutomationElement)
    : WINDOWPLACEMENT option =
    let winHandleInt =
        el.GetCurrentPropertyValue(AutomationElement.NativeWindowHandleProperty)
        :?> int

    let winHandle = winHandleInt |> IntPtr.op_Explicit

    getWindowPlacement winHandle

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
    setWindowState windowPattern WindowVisualState.Maximized

let setWindowToNormal (windowPattern: WindowPattern) =
    setWindowState windowPattern WindowVisualState.Normal

let activate (el: AutomationElement) : AutomationElement =
    getAutoElementWindowPlacement el
    |> Option.map getWindowPlacementShowCommandAndFlags
    |> function
        | Some (cmd, flags) when cmd = ShowWindowCommand.Minimized ->
            let windowFunc =
                if flags &&& WindowPlacementFlags.RestoreToMaximized = WindowPlacementFlags.RestoreToMaximized then
                    maximizeWindow
                else
                    setWindowToNormal

            el
            |> getWindowPattern
            |> Option.map windowFunc
            |> ignore

            el
        | _ -> el

let focus (el: AutomationElement) : AutomationElement =
    el.SetFocus()
    el