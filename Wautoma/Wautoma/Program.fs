open System
open System.Windows.Forms
open Wautoma.UIStuff
open Wautoma.MyHotkeys
open Wautoma.UIAutomation.Processes

[<EntryPoint; STAThread>]
let main _ =
    let allWautomaProcesses = allProcessesWithName "Wautoma"

    // do not allow multiple instances of Wautoma to run
    if allWautomaProcesses |> Array.length > 1 then
        1
    else
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(false)

        let form, _ = createUIElements hotkeys
        Application.Run(form)
        0
