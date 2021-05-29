// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.Windows.Forms

type StartupForm() = 
    inherit Form()

[<EntryPoint; STAThread>]
let main _ =
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault(false)

    use form = new StartupForm()
    Application.Run(form)    
    0 // return an integer exit code
