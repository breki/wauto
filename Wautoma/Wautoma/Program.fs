open System
open System.Threading
open System.Windows.Forms
open Wautoma.KeysTypes
open Wautoma.UIStuff
open Wautoma.Async
open Wautoma.UIAutomation.AutomationExamples


let hotkeys: Hotkeys =
    [ { Keys = KeyCombo.Parse("Win+Shift+X")
        Action = moveToGmail
        Description = "Open Gmail" } ]
    |> List.map (fun x -> x.Keys, x)
    |> Map.ofSeq


[<EntryPoint; STAThread>]
let main _ =
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault(false)

    let form, loggingTextBox = createUIElements hotkeys

    // todo igor: this should be returned by createUIElements
    let logActivity msg =
        loggingTextBox |> logActivityIntoTextBox msg

    let logHelloWorld logger =
        Thread.Sleep 2000
        logger "Hello World!"

    logHelloWorld |> executeInBackground logActivity

    Application.Run(form)
    0
