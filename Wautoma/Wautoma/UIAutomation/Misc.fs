module Wautoma.UIAutomation.Misc

open System.Diagnostics
open System.Threading
open System.Windows.Forms


let hibernate _ =
    Application.SetSuspendState(PowerState.Hibernate, true, true)
    |> ignore

let runProgram filename : unit =
    let procStartInfo =
        ProcessStartInfo(FileName = filename, UseShellExecute = false)

    let proc = new Process(StartInfo = procStartInfo)
    proc.Start() |> ignore

let pause (time: int) = Thread.Sleep(time)
