module Tests.``Keyboard shortcuts parsing``

open Wautoma.KeysNames
open Wautoma.KeysTypes
open Xunit
open Swensen.Unquote
open FsCheck


let chooseFromValidKeyCombo () =
    gen {
        let! modifiers = Arb.from<ModifierKeys>.Generator

        let! keyCodeIndex = Gen.choose (0, keysNamesToKeyCodes.Count - 1)

        let keyCodeUint =
            keysNamesToKeyCodes.Values
            |> Seq.item keyCodeIndex
            |> uint

        let keyCode: VirtualKeyCode = keyCodeUint

        return
            { Modifiers = modifiers
              KeyCode = Some keyCode }
    }

[<Fact>]
let ``KeyCombo parsing and text serialization works`` () =
    let keyComboStringParsesToItself (keyCombo: KeyCombo) =
        let keyComboStr = keyCombo.ToString()
        let parsedKeyCombo = KeyCombo.Parse(keyComboStr)
        parsedKeyCombo = keyCombo

    let arbitraryKeyCombo =
        chooseFromValidKeyCombo () |> Arb.fromGen

    keyComboStringParsesToItself
    |> Prop.forAll arbitraryKeyCombo
    |> Check.QuickThrowOnFailure

[<Fact>]
let ``handles empty key combo`` () =
    test <@ KeyCombo.Empty = KeyCombo.Parse("") @>

[<Theory>]
[<InlineData(0x08, "Backspace")>]
[<InlineData(0x13, "Pause")>]
[<InlineData(0x14, "Caps Lock")>]
[<InlineData(0x1b, "Esc")>]
[<InlineData(0x24, "Home")>]
[<InlineData(0x30, "0")>]
[<InlineData(0x41, "A")>]
[<InlineData(0x6e, "NumDecimal")>]
[<InlineData(0x7e, "F15")>]
[<InlineData(0x87, "F24")>]
[<InlineData(0x90, "Num Lock")>]
[<InlineData(0xa0, "Left Shift")>]
[<InlineData(0xa5, "Right Menu")>]
[<InlineData(0xff, "")>]
let ``key codes and names are correct`` code name =
    test <@ keysNames.[code] = name @>
