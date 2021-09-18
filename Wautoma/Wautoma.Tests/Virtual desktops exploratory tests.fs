module Tests.``Virtual desktops exploratory tests``

open Wautoma.VirtualDesktops
open Xunit
open Swensen.Unquote

[<Fact>]
let ``trying out virtual desktops`` () =
    let manager = createVirtualDesktopsManager ()
    let desktops = manager.ListDesktops()
    test <@ desktops |> Seq.length = 4 @>
