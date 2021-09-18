module Tests.``Virtual desktops exploratory tests``

open System
open Microsoft.Win32
open Wautoma.VirtualDesktops
open Xunit
open Swensen.Unquote

[<Fact>]
let ``listing all virtual desktops using VD API`` () =
    let manager = createVirtualDesktopsManager ()
    let desktops = manager.ListDesktops()
    test <@ desktops |> Seq.length = 4 @>

[<Fact>]
let ``listing all virtual desktop GUIDs`` () =
    let desktopIdsFromReg = virtualDesktopIdsFromRegistry ()

    let desktopIdsFromApi =
        createVirtualDesktopsManager().ListDesktops()
        |> Seq.map (fun d -> d.Id)
        |> Seq.toArray

    test <@ desktopIdsFromReg = desktopIdsFromApi @>

[<Fact>]
let ``getting virtual desktop names from the registry`` () =
    let desktopNames = virtualDesktopNames ()

    let desktopId: VirtualDesktopId =
        Guid.Parse("{430FBCBF-F402-4399-AF83-6BAE450B7EAD}")

    test <@ desktopNames.[desktopId] = "Main" @>
