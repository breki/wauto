module Tests.``Keyboard shortcuts to SendKeys string``

open Wautoma.KeysTypes
open Xunit
open Swensen.Unquote
open Wautoma.Experimental.Native
open Wautoma.Experimental.Keystroke


[<Theory>]
[<InlineData("Shift+Ctrl+A", "+^a")>]
[<InlineData("Win+A", "^({ESC}a)")>]
[<InlineData("Ctrl+Win+Right", "^({ESC}{RIGHT})")>]
let ``key codes and names are correct`` keyCombo expectedSendKeysHotkey =
    let parsedKeyCombo = KeyCombo.Parse(keyCombo)
    let sendKeysHotkey = parsedKeyCombo.ToSendKeysString()

    test <@ sendKeysHotkey = expectedSendKeysHotkey @>


//[<Fact>]
let ``test desktop switch hotkey`` () =
    //    pressKey KeyCodes.KEY_A
//    pressKey KeyCodes.VK_CONTROL
//    pressKey KeyCodes.VK_LWIN
//    pressKey KeyCodes.VK_RIGHT
//    releaseKey KeyCodes.VK_RIGHT
//    releaseKey KeyCodes.VK_CONTROL
//    releaseKey KeyCodes.VK_LWIN

    KeyboardInput.tapKey T.LWin

//    sendKeys (fun _ -> ()) "^({ESC}{LEFT})"
