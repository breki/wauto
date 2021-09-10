module Wautoma.UIAutomation.AutomationExamples

open System
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

let nameEndsWith text (el: AutomationElement) =
    let name =
        el.GetCurrentPropertyValue(AutomationElement.NameProperty)
        |> string

    name.EndsWith(text)

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
    | :? InvalidOperationException as ex -> None

let unminimizeWindow (windowPattern: WindowPattern) : unit =
    let interactionState =
        windowPattern.Current.WindowInteractionState

    if interactionState = WindowInteractionState.ReadyForUserInteraction then
        windowPattern.SetWindowVisualState(WindowVisualState.Normal)
    else
        ()

let unminimize (el: AutomationElement) : unit =
    el
    |> getWindowPattern
    |> Option.map unminimizeWindow
    |> ignore

let pause (time: int) = Thread.Sleep(time)

let moveToGmail (loggingFunc: LoggingFunc) : unit =
    let chromeMaybe =
        allMainWindows ()
        |> Seq.tryFind (nameEndsWith "Google Chrome")

    match chromeMaybe with
    | Some chrome ->
        loggingFunc "Found Chrome"
        chrome |> unminimize
        chrome.SetFocus()
        pause 250
        "+^A" |> sendKeys loggingFunc
        pause 250
        "gmail" |> sendKeys loggingFunc
        pause 250
        "{ENTER}" |> sendKeys loggingFunc
    | None -> ()
