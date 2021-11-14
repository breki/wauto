module Wautoma.VirtualDesktops

// copied and adapted from https://github.com/Trundle/Kolonnade
// under Apache License, Version 2.0

open System
open System.Runtime.InteropServices
open Microsoft.Win32

type VirtualDesktopId = Guid

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


module internal CLSIDs =
    let ImmersiveShell =
        Guid("c2f03a33-21f5-47fa-b4bb-156362a2f239")

    let VirtualDesktopManager =
        Guid("aa509086-5ca9-4c25-8f95-589d3c07b48a")

    let VirtualDesktopManagerInternal =
        Guid("c5e0cdca-7b6e-41b2-9fc4-d93975cc467b")

    let VirtualDesktopNotificationService =
        Guid("a501fdec-4a09-464c-ae4e-1b9c21b84918")

    let VirtualDesktopPinnedApps =
        Guid("B5A399E7-1C87-46B8-88E9-FC5747B171BD")

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
    abstract GetAppUserModelId :
        [<MarshalAs(UnmanagedType.LPWStr)>] out: outref<string> -> int

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

let internal getApplicationView
    (hwnd: IntPtr)
    (applicationViewCollection: IApplicationViewCollection)
    =
    try
        let result, applicationView =
            applicationViewCollection.GetViewForHwnd(hwnd)

        if result <> 0 then
            invalidOp "getApplicationView failed"

        Some applicationView
    with
    | :? COMException as ex -> None

let internal getAppId
    (hwnd: IntPtr)
    (applicationViewCollection: IApplicationViewCollection)
    =
    applicationViewCollection
    |> getApplicationView hwnd
    |> function
        | Some appView ->
            let result, appId = appView.GetAppUserModelId()

            if result <> 0 then None else Some appId
        | None -> None

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
  Guid("0F3A72B0-4566-487E-9A33-4ED302F6D6CE")>]
type internal IVirtualDesktopManagerInternal2 =
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


[<ComImport;
  InterfaceType(ComInterfaceType.InterfaceIsIUnknown);
  Guid("4CE81583-1E4C-4632-A621-07A53543148F")>]
type internal IVirtualDesktopPinnedApps =
    abstract IsAppIdPinned : string -> bool
    abstract PinAppID : string -> unit
    abstract UnpinAppID : string -> unit
    abstract IsViewPinned : IApplicationView -> bool
    abstract PinView : IApplicationView -> unit
    abstract UnpinView : IApplicationView -> unit


type VirtualDesktopNotification() =
    interface IVirtualDesktopNotification with
        member this.VirtualDesktopCreated desktop = ()
        member this.VirtualDesktopDestroyBegin desktopA desktopB = ()
        member this.VirtualDesktopDestroyFailed desktopA desktopB = ()
        member this.VirtualDesktopDestroyed desktopA desktopB = ()
        member this.ViewVirtualDesktopChanged applicationView = ()

        member this.CurrentVirtualDesktopChanged desktopA desktopB =
            Logging.log "CurrentVirtualDesktopChanged"


type Desktop
    internal
    (
        manager: IVirtualDesktopManagerInternal,
        manager2: IVirtualDesktopManagerInternal2,
        desktop: IVirtualDesktop,
        applicationViewCollection: IApplicationViewCollection,
        desktopOrdinalNumber
    ) =

    let mutable desktopName: string = ""

    do
        desktopName <-
            match virtualDesktopName (desktop.GetId()) with
            | Some name -> name
            | None -> $"Desktop %i{desktopOrdinalNumber}"

    member this.OrdinalNumber = desktopOrdinalNumber
    member this.Id = desktop.GetId()
    member this.Name = desktopName

    member this.MoveWindowTo(hwnd: IntPtr) =
        match applicationViewCollection.GetViewForHwnd(hwnd) with
        | 0, view -> manager.MoveViewToDesktop(view, desktop)
        | _ -> ()

    member this.SwitchTo() = manager.SwitchDesktop(desktop)

type Manager
    internal
    (
        manager: IVirtualDesktopManager,
        managerInternal: IVirtualDesktopManagerInternal,
        managerInternal2: IVirtualDesktopManagerInternal2,
        applicationViewCollection: IApplicationViewCollection,
        pinnedApps: IVirtualDesktopPinnedApps,
        notificationService: IVirtualDesktopNotificationService
    ) as self =

    let mutable desktops: Map<VirtualDesktopId, Desktop> = Map.empty

    //    let mutable notification: IVirtualDesktopNotification =
//        VirtualDesktopNotification() :> IVirtualDesktopNotification

    do
        desktops <-
            self.ListDesktops()
            |> Seq.map (fun (desktop: Desktop) -> desktop.Id, desktop)
            |> Map.ofSeq

    // this fails for some reason
//        let registrationId =
//            notificationService.Register &notification
//            |> ignore

    member this.GetCurrentDesktop() =
        let virtualDesktop = managerInternal.GetCurrentDesktop()
        // Should always be present (famous last words)
        desktops.[virtualDesktop.GetId()]

    member this.GetDesktop(window: IntPtr) =
        match manager.GetWindowDesktopId(window) with
        | 0, desktopId when desktopId = VirtualDesktopId.Empty -> None
        | 0, desktopId -> desktops.[desktopId] |> Some
        | _, _ -> None

    member this.CurrentDesktopNumber() =
        let currentDesktopId = this.GetCurrentDesktop().Id

        this.ListDesktops()
        |> Seq.toList
        |> List.findIndex (fun desktop -> desktop.Id = currentDesktopId)
        |> (+) 1

    member this.ListDesktops() =
        let rawDesktops = managerInternal.GetDesktops()

        seq {
            for desktopOrdinalNumber = 1 to managerInternal.GetCount() do
                let mutable rrid: VirtualDesktopId =
                    typeof<IVirtualDesktop>.GUID

                let nativeDesktop =
                    rawDesktops.GetAt(desktopOrdinalNumber - 1, &rrid)
                    :?> IVirtualDesktop

                let desktop =
                    Desktop(
                        managerInternal,
                        managerInternal2,
                        nativeDesktop,
                        applicationViewCollection,
                        desktopOrdinalNumber
                    )

                yield desktop
        }

    member this.PinApplication(hwnd: IntPtr) =
        applicationViewCollection
        |> getAppId hwnd
        |> function
            | Some appId -> pinnedApps.PinAppID(appId)
            | None -> ()

    member this.PinWindow(hwnd: IntPtr) : unit =
        match hwnd with
        | hwnd when hwnd = IntPtr.Zero -> invalidArg "hwnd" "hwnd is null"
        | _ ->
            applicationViewCollection
            |> getApplicationView hwnd
            |> function
                | Some appView -> pinnedApps.PinView appView
                | None -> ()


let internal createService<'TService>
    (shell: IServiceProvider10)
    serviceGuid
    : 'TService =
    let mutable servGuid = serviceGuid
    let mutable riid = typeof<'TService>.GUID
    shell.QueryService(&servGuid, &riid) :?> 'TService

let createVirtualDesktopsManager () =
    let shell =
        Activator.CreateInstance(Type.GetTypeFromCLSID(CLSIDs.ImmersiveShell))
        :?> IServiceProvider10

    let manager =
        Activator.CreateInstance(
            Type.GetTypeFromCLSID(CLSIDs.VirtualDesktopManager)
        )
        :?> IVirtualDesktopManager

    let managerInternal =
        createService<IVirtualDesktopManagerInternal>
            shell
            CLSIDs.VirtualDesktopManagerInternal

    let managerInternal2 =
        createService<IVirtualDesktopManagerInternal2>
            shell
            CLSIDs.VirtualDesktopManagerInternal

    let applicationViewCollection =
        createService<IApplicationViewCollection>
            shell
            typeof<IApplicationViewCollection>.GUID

    let pinnedApps =
        createService<IVirtualDesktopPinnedApps>
            shell
            CLSIDs.VirtualDesktopPinnedApps

    let notificationService =
        createService<IVirtualDesktopNotificationService>
            shell
            CLSIDs.VirtualDesktopNotificationService

    Manager(
        manager,
        managerInternal,
        managerInternal2,
        applicationViewCollection,
        pinnedApps,
        notificationService
    )


let mutable _virtualDesktopsManager = createVirtualDesktopsManager ()

let virtualDesktopManagerWrapper () =
    try
        _virtualDesktopsManager.GetCurrentDesktop()
        |> ignore

        _virtualDesktopsManager
    with
    | :? COMException as ex ->
        _virtualDesktopsManager <- createVirtualDesktopsManager ()
        _virtualDesktopsManager
