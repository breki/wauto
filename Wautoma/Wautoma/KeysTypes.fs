module Wautoma.KeysTypes

open System
open System.Text

[<Flags>]
type ModifierKeys =
    | None = 0
    | Alt = 1
    | Control = 2
    | Shift = 4
    | WindowsKey = 8


type VirtualKeyCode = uint

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

        let keyCodeToString keyCode =
            match keyCode with
            | 83u -> Some "S"
            | _ -> None

        let keyCodeStr =
            this.KeyCode |> Option.bind keyCodeToString

        if keyCodeStr |> Option.isSome then
            s.Append(keyCodeStr) |> ignore

        s.ToString()
