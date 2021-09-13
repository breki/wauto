module Wautoma.UIAutomation.AutomationExamples

open System
open System.Diagnostics
open System.IO
open System.Threading
open System.Windows.Automation
open System.Windows.Forms
open Wautoma.Logging

let allChildren =
    TreeScope.Children, Condition.TrueCondition

let allMainWindows () =
    seq {
        for el in AutomationElement.RootElement.FindAll allChildren do
            yield el
    }

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

let sendKeys loggingFunc keysStr =
    try
        $"Sending keys %s{keysStr}" |> loggingFunc
        SendKeys.SendWait(keysStr)
    with
    | :? InvalidOperationException as ex ->
        $"InvalidOperationException %A{ex}" |> loggingFunc

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

let unminimizeWindow (windowPattern: WindowPattern) : unit =
    let interactionState =
        windowPattern.Current.WindowInteractionState

    if interactionState = WindowInteractionState.ReadyForUserInteraction then
        windowPattern.SetWindowVisualState(WindowVisualState.Normal)
    else
        ()

let unminimize (el: AutomationElement) : AutomationElement =
    el
    |> getWindowPattern
    |> Option.map unminimizeWindow
    |> ignore

    el

let focus (el: AutomationElement) : AutomationElement =
    el.SetFocus()
    el

let runProgram filename : unit =
    let procStartInfo =
        ProcessStartInfo(
            FileName = filename,
            UseShellExecute = false
        )

    let proc = new Process(StartInfo = procStartInfo)
    proc.Start() |> ignore

let pause (time: int) = Thread.Sleep(time)

let openGmail (loggingFunc: LoggingFunc) : unit =
    let chromeMaybe =
        allMainWindows ()
        |> Seq.tryFind (nameEndsWith "Google Chrome")

    match chromeMaybe with
    | Some chrome ->
        chrome |> unminimize |> focus |> ignore
        pause 250
        "+^A" |> sendKeys loggingFunc
        pause 250
        "gmail" |> sendKeys loggingFunc
        pause 250
        "{ENTER}" |> sendKeys loggingFunc
    | None -> ()

let openNotepadPlusPlus (_: LoggingFunc) : unit =
    let notepadMaybe =
        allMainWindows ()
        |> Seq.tryFind (nameEndsWith "Notepad++")

    match notepadMaybe with
    | Some notepad -> notepad |> unminimize |> focus |> ignore
    | None -> runProgram "notepad++.exe"

let openFm (_: LoggingFunc) : unit =
    let appFormMaybe =
        allMainWindows () |> Seq.tryFind (nameIs "fman")

    match appFormMaybe with
    | Some appForm -> appForm |> unminimize |> focus |> ignore
    | None ->
        let appDataDir =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)

        Path.Combine(
            [| appDataDir
               @"Microsoft\Windows\Start Menu\Programs\fman.lnk" |]
        )
        |> runProgram

let openFoobar2000 (_: LoggingFunc) : unit =
    let appFormMaybe =
        allMainWindows ()
        |> Seq.tryFind (nameEndsWith "[foobar2000]")

    match appFormMaybe with
    | Some appForm -> appForm |> unminimize |> focus |> ignore
    | None ->
        let programFilesDir =
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)

        Path.Combine(
            [| programFilesDir
               @"foobar2000\foobar2000.exe" |]
        )
        |> runProgram

let openWindowsTerminal (_: LoggingFunc) : unit =
    let appFormMaybe =
        allMainWindows ()
        |> Seq.tryFind (nameStartsWith "WinTerm")

    match appFormMaybe with
    | Some appForm -> appForm |> unminimize |> focus |> ignore
    | None -> "wt.exe" |> runProgram
