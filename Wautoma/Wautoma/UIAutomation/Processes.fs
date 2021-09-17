module Wautoma.UIAutomation.Processes

open System.Diagnostics

let allProcesses () = Process.GetProcesses()


let allProcessesWithName processName =
    allProcesses ()
    |> Array.filter (fun p -> p.ProcessName = processName)
