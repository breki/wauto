module Wautoma.Logging

type LoggingFunc = string -> unit


type Logged = LoggingFunc -> unit
