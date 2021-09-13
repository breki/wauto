﻿module Wautoma.KeyboardHandling

open System
open System.Runtime.InteropServices
open Wautoma.KeysTypes
open Wautoma.Logging
open Wautoma.NativeApi
open Wautoma.Async

type KeyboardHandler(hotkeys: Hotkeys, loggingFunc: LoggingFunc) =
    let mutable hookHandle: nativeint option = None
    let mutable currentlyPressedKeys: KeyCombo = KeyCombo.Empty

    let hotkeys = hotkeys
    let loggingFunc = loggingFunc

    let keyboardHookFunc nCode (wParam: nativeint) lParam =
        match hookHandle with
        | Some hookHandle ->
            let forwardToNextHook =
                if nCode >= 0 then
                    let keyboardMessage: NativeKeyboardMessage =
                        enum (int32 wParam)

                    let kbHookStruct: KBDLLHOOKSTRUCT =
                        Marshal.PtrToStructure(lParam, typeof<KBDLLHOOKSTRUCT>)
                        :?> KBDLLHOOKSTRUCT

                    let virtualKeyCode: VirtualKeyCode = kbHookStruct.vkCode

                    let modifierKey =
                        virtualKeyCodeToModifierKeys virtualKeyCode

                    let (|KEY_DOWN|KEY_UP|UNKNOWN|) keyboardMessage =
                        match keyboardMessage with
                        | NativeKeyboardMessage.WM_KEYDOWN -> KEY_DOWN
                        | NativeKeyboardMessage.WM_SYSKEYDOWN -> KEY_DOWN
                        | NativeKeyboardMessage.WM_KEYUP -> KEY_UP
                        | NativeKeyboardMessage.WM_SYSKEYUP -> KEY_UP
                        | _ -> UNKNOWN

                    let newKeystroke =
                        match keyboardMessage with
                        | KEY_DOWN ->
                            if modifierKey = ModifierKeys.None then
                                currentlyPressedKeys <-
                                    currentlyPressedKeys.WithPressedMainKey
                                        virtualKeyCode
                            else
                                currentlyPressedKeys <-
                                    currentlyPressedKeys.WithPressedModifier
                                        modifierKey

                            true
                        | KEY_UP ->
                            if modifierKey = ModifierKeys.None then
                                currentlyPressedKeys <-
                                    currentlyPressedKeys.WithUnpressedMainKey()
                            else
                                currentlyPressedKeys <-
                                    currentlyPressedKeys.WithUnpressedModifier
                                        modifierKey

                            true
                        | UNKNOWN ->
                            "UNKOWN" |> loggingFunc
                            false

                    if newKeystroke then
                        match hotkeys.TryFind currentlyPressedKeys with
                        | Some hotkey ->
                            executeInBackground loggingFunc hotkey.Action
                            false
                        | None -> true
                    else
                        false
                else
                    true

            if forwardToNextHook then
                CallNextHookEx(hookHandle, nCode, wParam, lParam)
            else
                IntPtr(1)

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
