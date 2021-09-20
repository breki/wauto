module Wautoma.UIAutomation.Keyboard

open System
open System.Windows.Forms

// some docs on the SendKeys syntax:
// https://www.autoitscript.com/autoit3/docs/functions/Send.htm

let sendKeys loggingFunc keysStr =
    try
        $"Sending keys %s{keysStr}" |> loggingFunc
        SendKeys.SendWait(keysStr)
    with
    | :? InvalidOperationException as ex ->
        $"InvalidOperationException %A{ex}" |> loggingFunc
