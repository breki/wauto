module Wautoma.Misc

open System.Threading
open Logging

let wait (duration: int) =
    log $"Waiting for %d{duration} milliseconds..."
    Thread.Sleep(duration)
