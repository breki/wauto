module Wautoma.Async

open System.Threading
open Wautoma.Logging


let allKeysDepressedEvent = new ManualResetEvent(false)

let waitForAllKeysToBeDepressed () =
    allKeysDepressedEvent.WaitOne(10 * 1000) |> ignore


let safeAction action logActivity =
    try
        action logActivity
    with
    | ex -> logActivity (ex.ToString())


let executeInBackground (logActivity: LoggingFunc) (action: Logged) : unit =
    let run () = safeAction action logActivity
    let thread: Thread = Thread(ThreadStart(run))
    thread.Start()
