module Wautoma.KeysTypes

open KeysNames
open System
open System.Text
open Wautoma.Logging

[<Flags>]
type ModifierKeys =
    | None = 0
    | Alt = 1
    | Control = 2
    | Shift = 4
    | WindowsKey = 8


type VirtualKeyCode = uint



let keysNamesToKeyCodes =
    keysNames
    |> Array.mapi
        (fun code name ->
            let vkcode: VirtualKeyCode = uint code
            (name, vkcode))
    |> Array.filter (fun (name, _) -> name.Length > 0)
    |> dict


let virtualKeyCodeToModifierKeys (virtualKeyCode: VirtualKeyCode) =
    match virtualKeyCode with
    | 160u -> ModifierKeys.Shift
    | 161u -> ModifierKeys.Shift
    | 162u -> ModifierKeys.Control
    | 163u -> ModifierKeys.Control
    | 164u -> ModifierKeys.Alt
    | 165u -> ModifierKeys.Alt
    | 91u -> ModifierKeys.WindowsKey
    | _ -> ModifierKeys.None


type KeyCombo =
    { Modifiers: ModifierKeys
      KeyCode: VirtualKeyCode option }

    static member Empty =
        { Modifiers = ModifierKeys.None
          KeyCode = None }

    static member Parse(value: string) : KeyCombo =
        let processSplit
            ((modifiers, keycode): ModifierKeys * VirtualKeyCode option)
            (split: string)
            : ModifierKeys * VirtualKeyCode option =

            match split with
            | "Win" -> (modifiers ||| ModifierKeys.WindowsKey), keycode
            | "Shift" -> (modifiers ||| ModifierKeys.Shift), keycode
            | "Ctrl" -> (modifiers ||| ModifierKeys.Control), keycode
            | "Alt" -> (modifiers ||| ModifierKeys.Alt), keycode
            | _ ->
                match keycode with
                | Some _ ->
                    // the keycode was already parsed, so there cannot be
                    // another one
                    let message = sprintf $"Invalid hotkey '%s{value}'."
                    invalidArg "value" message
                | None ->
                    if keysNamesToKeyCodes.ContainsKey split then
                        let keycode = keysNamesToKeyCodes.[split]
                        (modifiers, Some keycode)
                    else
                        let message = sprintf $"Invalid hotkey '%s{value}'."
                        invalidArg "value" message

        let trimmedValue = value.Trim()

        match trimmedValue with
        | "" -> KeyCombo.Empty
        | _ ->
            let splits = value.Split('+')

            let modifiers, keycode =
                splits
                |> Array.rev
                |> Array.fold processSplit (ModifierKeys.None, None)

            { Modifiers = modifiers
              KeyCode = keycode }

    member this.WithPressedModifier modifier =
        let newModifiers = this.Modifiers ||| modifier

        { Modifiers = newModifiers
          KeyCode = this.KeyCode }

    member this.WithUnpressedModifier modifier =
        let newModifiers = this.Modifiers &&& ~~~modifier

        { Modifiers = newModifiers
          KeyCode = this.KeyCode }

    member this.WithPressedMainKey pressedKeyCode =
        { Modifiers = this.Modifiers
          KeyCode = Some pressedKeyCode }

    member this.WithUnpressedMainKey() =
        { Modifiers = this.Modifiers
          KeyCode = None }

    override this.ToString() =
        let s = StringBuilder()

        if (this.Modifiers &&& ModifierKeys.WindowsKey)
           <> ModifierKeys.None then
            s.Append("Win+") |> ignore

        if (this.Modifiers &&& ModifierKeys.Shift)
           <> ModifierKeys.None then
            s.Append("Shift+") |> ignore

        if (this.Modifiers &&& ModifierKeys.Control)
           <> ModifierKeys.None then
            s.Append("Ctrl+") |> ignore

        if (this.Modifiers &&& ModifierKeys.Alt)
           <> ModifierKeys.None then
            s.Append("Alt+") |> ignore

        let keyCodeToString (keyCode: VirtualKeyCode) : string =
            keysNames.[keyCode |> uint |> int]

        let keyCodeStr =
            this.KeyCode |> Option.map keyCodeToString

        match keyCodeStr with
        | Some keyCodeStr -> s.Append(keyCodeStr) |> ignore
        | None -> ()

        s.ToString()


type HotkeyAction = Logged

type Hotkey =
    { Keys: KeyCombo
      Action: HotkeyAction
      Description: str }

type Hotkeys = Map<KeyCombo, Hotkey>
