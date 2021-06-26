module Wautoma.KeyboardHandling

open System
open Wautoma.Logging
open Wautoma.NativeApi

type KeyboardHandler(loggingFunc: LoggingFunc) =
    let mutable hookHandle : nativeint option = None

    let loggingFunc = loggingFunc

    // todo now: extend the hook function
    let keyboardHookFunc nCode (wParam: nativeint) lParam =
        match hookHandle with
        | Some hookHandle ->
            if nCode >= 0 then
                let keyboardMessage : NativeKeyboardMessage =
                    enum (int32 wParam)

                ignore ()
            else
                ignore ()

            CallNextHookEx(hookHandle, nCode, wParam, lParam)
        | None -> invalidOp "Hook handle not set."

    let keyboardHook = LowLevelKeyboardProc(keyboardHookFunc)

    member this.Start() =
        let hMod = LoadLibrary("User32")

        hookHandle <-
            SetWindowsHookEx(WH_KEYBOARD_LL, keyboardHook, hMod, 0u)
            |> Some

    member this.Stop() : unit =
        match hookHandle with
        | Some hookHandleToUnhook ->
            UnhookWindowsHookEx(hookHandleToUnhook) |> ignore
            hookHandle <- None
        | None -> ()

    interface IDisposable with
        member this.Dispose() : unit =
            this.Dispose(true)
            GC.SuppressFinalize(this)

    member this.Dispose disposing =
        match disposing with
        | true -> this.Stop()
        | false -> ()
