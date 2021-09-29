module Wautoma.Logging

type LoggingFunc = string -> unit


type Logged = LoggingFunc -> unit


let mutable log: LoggingFunc = fun _ -> ()
