open System
open System.Threading
open System.Windows.Forms
open Wautoma.UIStuff
open Wautoma.Async

[<EntryPoint; STAThread>]
let main _ =
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault(false)

    let form, loggingTextBox = createUIElements ()

    // todo igor: this should be returned by createUIElements
    let logActivity msg =
        loggingTextBox |> logActivityIntoTextBox msg

    let logHelloWorld logger =
        Thread.Sleep 2000
        logger "Hello World!"

    logHelloWorld |> executeInBackground logActivity

    Application.Run(form)
    0
