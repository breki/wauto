// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.Threading
open System.Windows.Forms

type AppForm() = 
    inherit Form()

let createUIElements() =
    let form = new AppForm()
    form.Width <- 500
    form.Height <- 500
    
    let loggingTextBox = new TextBox()
    loggingTextBox.Anchor
        <- AnchorStyles.Top ||| AnchorStyles.Right
           ||| AnchorStyles.Bottom ||| AnchorStyles.Left
    loggingTextBox.Multiline <- true
    loggingTextBox.ReadOnly <- true
    loggingTextBox.Dock <- DockStyle.Fill
        
    form.Controls.Add(loggingTextBox)
    (form, loggingTextBox)
    
let logActivityIntoTextBox msg (loggingTextBox: TextBox): unit =
    let logFunc() =
            loggingTextBox.Text <-
                loggingTextBox.Text + Environment.NewLine + msg
    
    loggingTextBox.Invoke(MethodInvoker(logFunc)) |> ignore

[<EntryPoint; STAThread>]
let main _ =
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault(false)

    let form, loggingTextBox = createUIElements()
    
    let logActivity msg = loggingTextBox |> logActivityIntoTextBox msg 
    
    let backgroundTask() =
        let run() =
            Thread.Sleep(2000)
            logActivity "Hello World!"
        let thread: Thread = Thread(ThreadStart(run))
        thread.Start()
    
    backgroundTask()
    
    Application.Run(form)
    0 // return an integer exit code
