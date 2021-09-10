module Wautoma.Async

open System.Threading
open Wautoma.Logging


let executeInBackground
    (logActivity: LoggingFunc)
    (action: LoggingFunc -> unit)
    : unit =
    let run () = action logActivity
    let thread: Thread = Thread(ThreadStart(run))
    thread.Start()
