// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
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
    form

[<EntryPoint; STAThread>]
let main _ =
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault(false)

    use form = createUIElements()
    
    Application.Run(form)
    0 // return an integer exit code
