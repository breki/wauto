module Tests.``Keyboard shortcuts parsing``

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
