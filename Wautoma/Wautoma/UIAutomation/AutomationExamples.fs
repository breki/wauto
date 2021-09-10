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


let pause () = Thread.Sleep(250)

let moveToGmail (loggingFunc: LoggingFunc) : unit =
    let chromeMaybe =
        allMainWindows ()
        |> Seq.tryFind (nameEndsWith "Google Chrome")

    match chromeMaybe with
    | Some chrome ->
        loggingFunc "Found Chrome"
        chrome.SetFocus()
        pause ()
        "+(^A)" |> sendKeys loggingFunc
        pause ()
        "gmail" |> sendKeys loggingFunc
        pause ()
        "{ENTER}" |> sendKeys loggingFunc
    | None -> ()
