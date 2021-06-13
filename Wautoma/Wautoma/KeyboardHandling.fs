module Wautoma.KeyboardHandling

open Wautoma.UIStuff
open Wautoma.NativeApi

type KeyboardHandler(loggingFunc: LoggingFunc) =
    let mutable hookHandle : nativeint option = None

    let loggingFunc = loggingFunc

    let keyboardHookFunc nCode wParam lParam =
        match hookHandle with
        | Some hookHandle -> CallNextHookEx(hookHandle, nCode, wParam, lParam)
        | None -> invalidOp "Hook handle not set."

    let keyboardHook = LowLevelKeyboardProc(keyboardHookFunc)

    member this.Start() =
        let hMod = LoadLibrary("User32")

        hookHandle <-
            SetWindowsHookEx(WH_KEYBOARD_LL, keyboardHook, hMod, 0u)
            |> Some

    member this.Stop() : unit =
        match hookHandle with
        | Some hookHandle -> UnhookWindowsHookEx(hookHandle) |> ignore
        | None -> ()
