module Tests.``Keyboard shortcuts parsing``

open Wautoma
open Xunit
open Swensen.Unquote

[<Fact>]
let ``Icebreaker`` () =
    test <@ "Backspace" = KeysNames.keysNames.[8] @>
