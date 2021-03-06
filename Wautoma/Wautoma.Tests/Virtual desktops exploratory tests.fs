module Tests.``Virtual desktops exploratory tests``

open System
open System.Diagnostics
open Wautoma.VirtualDesktops
open Wautoma.UIAutomation.Processes
open Xunit
open Swensen.Unquote

[<Fact>]
let ``listing all virtual desktops using VD API`` () =
    let manager = createVirtualDesktopsManager ()
    let desktops = manager.ListDesktops() |> Seq.toArray
    test <@ desktops |> Seq.length = 4 @>
//    test <@ desktops.[0].Name = "Main" @>
//    test <@ desktops.[1].Name = "Desktop 2" @>

[<Fact>]
let ``listing all virtual desktop GUIDs`` () =
    let desktopIdsFromReg = virtualDesktopIdsFromRegistry ()

    let desktopIdsFromApi =
        createVirtualDesktopsManager().ListDesktops()
        |> Seq.map (fun d -> d.Id)
        |> Seq.toArray

    test <@ desktopIdsFromReg = desktopIdsFromApi @>

//[<Fact>]
let ``getting virtual desktop names from the registry`` () =
    let desktopNames = virtualDesktopNames ()

    let desktopId: VirtualDesktopId =
        Guid.Parse("{430FBCBF-F402-4399-AF83-6BAE450B7EAD}")

    test <@ desktopNames.[desktopId] = "Main" @>

//[<Fact>]
let ``Can switch to another desktop`` () =
    let manager = createVirtualDesktopsManager ()

    let differentDesktop =
        manager.ListDesktops()
        |> Seq.toList
        |> List.item 1

    differentDesktop.SwitchTo()

    test <@ true @>

//[<Fact>]
let ``pinning windows works`` () =
    let ``process`` = (allProcessesWithName "foobar2000").[0]
    let hwnd = ``process``.MainWindowHandle
    //    virtualDesktopsManager.PinApplication hwnd
    virtualDesktopManagerWrapper().PinWindow hwnd
