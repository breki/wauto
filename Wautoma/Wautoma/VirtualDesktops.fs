module Wautoma.VirtualDesktops

// copied and adapted from https://github.com/Trundle/Kolonnade
// under Apache License, Version 2.0

open System
open System.Runtime.InteropServices
open Microsoft.Win32

type VirtualDesktopId = Guid

module internal CLSIDs =
    let ImmersiveShell =
        Guid("c2f03a33-21f5-47fa-b4bb-156362a2f239")

    let VirtualDesktopManager =
        Guid("aa509086-5ca9-4c25-8f95-589d3c07b48a")

    let VirtualDesktopManagerInternal =
        Guid("c5e0cdca-7b6e-41b2-9fc4-d93975cc467b")

    let VirtualDesktopNotificationService =
        Guid("a501fdec-4a09-464c-ae4e-1b9c21b84918")

[<ComImport;
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown);
  Guid("6d5140c1-7436-11ce-8034-00aa006009fa")>]
type internal IServiceProvider10 =
    abstract QueryService :
        service: byref<Guid> * rrid: byref<Guid> ->
        [<MarshalAs(UnmanagedType.IUnknown)>] obj

[<ComImport;
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown);
  Guid("92ca9dcd-5622-4bba-a805-5e9f541bd8c9")>]
type internal IObjectArray =
    abstract GetCount : unit -> uint32

    abstract GetAt :
        index: int
        * rrid: byref<Guid>
        * [<MarshalAs(UnmanagedType.IUnknown)>] out: outref<obj> ->
        unit

[<ComImport;
  InterfaceType(ComInterfaceType.InterfaceIsIInspectable);
  Guid("372e1d3b-38d3-42e4-a15b-8ab2b178f513")>]
type internal IApplicationView =
    abstract SetFocus : unit -> int
    abstract SwitchTo : unit -> int
// Other methods omitted

[<ComImport;
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown);
  Guid("1841c6d7-4f9d-42c0-af41-8747538f10e5")>]
type internal IApplicationViewCollection =
    abstract GetViews : byref<IObjectArray> -> int
    abstract GetViewsByZOrder : byref<IObjectArray> -> int
    abstract GetViewsByAppUserModelId : string * byref<IObjectArray> -> int
    abstract GetViewForHwnd : IntPtr * outref<IApplicationView> -> int
// Other methods omitted

[<ComImport;
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown);
  Guid("ff72ffdd-be7e-43fc-9c03-ad81681e88e4")>]
type internal IVirtualDesktop =
    abstract IsViewVisible : obj -> bool
    abstract GetId : unit -> VirtualDesktopId

[<ComImport;
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown);
  Guid("a5cd92ff-29be-454c-8d04-d82879fb3f1b")>]
type internal IVirtualDesktopManager =
    abstract IsWindowOnCurrentVirtualDesktop : IntPtr -> bool

    [<PreserveSig>]
    abstract GetWindowDesktopId : IntPtr * outref<VirtualDesktopId> -> int

[<ComImport;
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown);
  Guid("f31574d6-b682-4cdc-bd56-1827860abec6")>]
// Unfortunately the "Internal" in its name is no joke, it's not documented,
// but see for example https://github.com/MScholtes/PSVirtualDesktop
// and https://github.com/nathannelson97/VirtualDesktopGridSwitcher
type internal IVirtualDesktopManagerInternal =
    abstract GetCount : unit -> int
    abstract MoveViewToDesktop : IApplicationView * IVirtualDesktop -> unit
    abstract CanViewMoveDesktops : IApplicationView -> unit
    abstract GetCurrentDesktop : unit -> IVirtualDesktop
    abstract GetDesktops : unit -> IObjectArray
    abstract GetAdjacentDesktop : IVirtualDesktop -> int -> IVirtualDesktop
    abstract SwitchDesktop : desktop: IVirtualDesktop -> unit
    abstract CreateDesktopW : unit -> IVirtualDesktop
    abstract RemoveDesktop : IVirtualDesktop -> IVirtualDesktop -> unit
    abstract FindDesktop : inref<VirtualDesktopId> -> IVirtualDesktop

[<ComImport;
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown);
  Guid("c179334c-4295-40d3-bea1-c654d965605a")>]
type internal IVirtualDesktopNotification =
    abstract VirtualDesktopCreated : IVirtualDesktop -> unit

    abstract VirtualDesktopDestroyBegin :
        IVirtualDesktop -> IVirtualDesktop -> unit

    abstract VirtualDesktopDestroyFailed :
        IVirtualDesktop -> IVirtualDesktop -> unit

    abstract VirtualDesktopDestroyed :
        IVirtualDesktop -> IVirtualDesktop -> unit

    abstract ViewVirtualDesktopChanged : IApplicationView -> unit

    abstract CurrentVirtualDesktopChanged :
        IVirtualDesktop -> IVirtualDesktop -> unit

[<ComImport;
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown);
  Guid("0cd45e71-d927-4f15-8b0a-8fef525337bf")>]
type internal IVirtualDesktopNotificationService =
    abstract Register : byref<IVirtualDesktopNotification> -> int
    abstract Unregister : int -> unit

type Desktop
    internal
    (
        manager: IVirtualDesktopManagerInternal,
        desktop: IVirtualDesktop,
        applicationViewCollection: IApplicationViewCollection,
        n
    ) =
    member this.N = n
    member this.Id = desktop.GetId()

    member this.MoveWindowTo(hWnd: IntPtr) =
        match applicationViewCollection.GetViewForHwnd(hWnd) with
        | 0, view -> manager.MoveViewToDesktop(view, desktop)
        | _ -> ()

    member this.SwitchTo() = manager.SwitchDesktop(desktop)

type Manager
    internal
    (
        manager: IVirtualDesktopManager,
        managerInternal: IVirtualDesktopManagerInternal,
        applicationViewCollection: IApplicationViewCollection
    ) as self =

    let mutable desktops: Map<VirtualDesktopId, Desktop> = Map.empty

    do
        desktops <-
            self.ListDesktops()
            |> Seq.map (fun (desktop: Desktop) -> desktop.Id, desktop)
            |> Map.ofSeq

    member this.GetCurrentDesktop() =
        let virtualDesktop = managerInternal.GetCurrentDesktop()
        // Should always be present (famous last words)
        desktops.[virtualDesktop.GetId()]

    member this.GetDesktop(window: IntPtr) =
        match manager.GetWindowDesktopId(window) with
        | 0, desktopId when desktopId = VirtualDesktopId.Empty -> None
        | 0, desktopId -> desktops.[desktopId] |> Some
        | _, _ -> None

    member this.ListDesktops() =
        let rawDesktops = managerInternal.GetDesktops()

        seq {
            for i = 0 to managerInternal.GetCount() - 1 do
                let mutable rrid: VirtualDesktopId =
                    typeof<IVirtualDesktop>.GUID

                let nativeDesktop = rawDesktops.GetAt(i, &rrid)

                let desktop =
                    Desktop(
                        managerInternal,
                        nativeDesktop :?> IVirtualDesktop,
                        applicationViewCollection,
                        i + 1
                    )

                yield desktop
        }

let createVirtualDesktopsManager () =
    let shell =
        Activator.CreateInstance(Type.GetTypeFromCLSID(CLSIDs.ImmersiveShell))
        :?> IServiceProvider10

    let manager =
        Activator.CreateInstance(
            Type.GetTypeFromCLSID(CLSIDs.VirtualDesktopManager)
        )
        :?> IVirtualDesktopManager

    let mutable serviceGuid = CLSIDs.VirtualDesktopManagerInternal

    let mutable riid =
        typeof<IVirtualDesktopManagerInternal>.GUID

    let managerInternal =
        shell.QueryService(&serviceGuid, &riid)
        :?> IVirtualDesktopManagerInternal

    serviceGuid <- typeof<IApplicationViewCollection>.GUID
    riid <- typeof<IApplicationViewCollection>.GUID

    let applicationViewCollection =
        shell.QueryService(&serviceGuid, &riid) :?> IApplicationViewCollection

    serviceGuid <- CLSIDs.VirtualDesktopNotificationService
    riid <- typeof<IVirtualDesktopNotificationService>.GUID

    let notificationService =
        shell.QueryService(&serviceGuid, &riid)
        :?> IVirtualDesktopNotificationService

    Manager(manager, managerInternal, applicationViewCollection)


let virtualDesktopIdsFromRegistry () : VirtualDesktopId [] =
    use key =
        Registry.CurrentUser.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VirtualDesktops"
        )

    let guidsByteArray =
        key.GetValue("VirtualDesktopIDs") :?> byte []

    guidsByteArray
    |> Array.chunkBySize 16
    |> Array.map Guid



let virtualDesktopName (virtualDesktopId: VirtualDesktopId) =
    let idGuid = virtualDesktopId.ToString("B")

    let keyName =
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\"
        + $@"VirtualDesktops\Desktops\%s{idGuid}"

    use key = Registry.CurrentUser.OpenSubKey(keyName)

    if key <> null then
        key.GetValue "Name" |> string |> Some
    else
        None


let virtualDesktopNames () =
    use key =
        Registry.CurrentUser.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\"
            + @"Explorer\VirtualDesktops\Desktops"
        )

    key.GetSubKeyNames()
    |> Array.map
        (fun subkeyName ->
            let desktopId: VirtualDesktopId = Guid.Parse(subkeyName)
            desktopId, (desktopId |> virtualDesktopName))
    |> Array.map
        (fun (id, name) ->
            match name with
            | Some name -> id, name
            | None ->
                invalidOp
                    "BUG: virtual desktop name should always be specified here")
    |> Map.ofArray
