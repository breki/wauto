// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.Drawing
open System.Threading
open System.Windows.Forms

type AppForm() as this = 
    inherit Form()
    member this.components = new System.ComponentModel.Container();

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
    
    let notifyIcon = new NotifyIcon(form.components)
    // todo igor: use an embedded icon here 
    notifyIcon.Icon <- new Icon(@"D:\src\wauto\Wautoma\Wautoma\sample.ico");
    notifyIcon.Visible <- true
    
    (form, loggingTextBox)
    
let logActivityIntoTextBox msg (loggingTextBox: TextBox): unit =
    let logFunc() =
            loggingTextBox.Text <-
                loggingTextBox.Text + Environment.NewLine + msg
    
    loggingTextBox.Invoke(MethodInvoker(logFunc)) |> ignore

type loggingFunc = string -> unit

let executeInBackground
    (action: loggingFunc -> unit)
    (logActivity: loggingFunc)
    : unit =
    let run() = action logActivity
    let thread: Thread = Thread(ThreadStart(run))
    thread.Start()
    
    
[<EntryPoint; STAThread>]
let main _ =
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault(false)

    let form, loggingTextBox = createUIElements()
    
    let logActivity msg = loggingTextBox |> logActivityIntoTextBox msg 
    
    let logHelloWorld logger =
        Thread.Sleep 2000
        logger "Hello World!"
    executeInBackground logHelloWorld logActivity
    
    Application.Run(form)
    0 // return an integer exit code
