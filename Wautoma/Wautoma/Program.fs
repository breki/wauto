open System
open System.Threading
open System.Windows.Forms
open Wautoma.UIStuff
open Wautoma.Async
open Wautoma.MyHotkeys


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
