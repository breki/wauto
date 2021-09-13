open System
open System.Threading
open System.Windows.Forms
open Wautoma.KeysTypes
open Wautoma.UIStuff
open Wautoma.Async
open Wautoma.UIAutomation.AutomationExamples


let hotkeys: Hotkeys =
    [ { Keys = KeyCombo.Parse("Win+Shift+X")
        Action = openGmail
        Description = "Open Gmail" }
      { Keys = KeyCombo.Parse("Win+N")
        Action = openNotepadPlusPlus
        Description = "Open Notepad++" }
      { Keys = KeyCombo.Parse("Win+W")
        Action = openFoobar2000
        Description = "Open foobar2000" }
      { Keys = KeyCombo.Parse("Win+E")
        Action = openFm
        Description = "Open fm" } ]
    |> List.map (fun x -> x.Keys, x)
    |> Map.ofSeq


[<EntryPoint; STAThread>]
let main _ =
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault(false)

    let form, logActivity = createUIElements hotkeys

    let logHelloWorld logger =
        Thread.Sleep 2000
        logger "Hello World!"

    logHelloWorld |> executeInBackground logActivity

    Application.Run(form)
    0
