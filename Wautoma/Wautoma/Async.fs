module Wautoma.Async

open System.Threading
open Wautoma.Logging


let safeAction action logActivity =
    try
        action logActivity
    with
    | ex -> logActivity (ex.ToString())


let executeInBackground (logActivity: LoggingFunc) (action: Logged) : unit =
    let run () = safeAction action logActivity
    let thread: Thread = Thread(ThreadStart(run))
    thread.Start()
