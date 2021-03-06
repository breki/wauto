module Wautoma.MyHotkeys

open System
open System.IO
open System.Threading
open System.Windows.Automation
open Wautoma.Async
open Wautoma.Misc
open Wautoma.KeysTypes
open Wautoma.Logging
open Wautoma.UIAutomation.Processes
open Wautoma.UIAutomation.Windows
open Wautoma.UIAutomation.Misc
open Wautoma.UIAutomation.Keyboard
open Wautoma.UIAutomation.Mouse
open Wautoma.VirtualDesktops
open Wautoma.UIStuff

let mutable desktopFocusedElements: Map<int, AutomationElement> = Map.empty


let goToChromeTab tabName (loggingFunc: LoggingFunc) : unit =
    let chromeMaybe =
        allMainWindows ()
        |> Seq.tryFind (nameEndsWith "Google Chrome")

    match chromeMaybe with
    | Some chrome ->
        chrome |> activate |> focus |> ignore
        waitForAllKeysToBeDepressed ()
        //        pause 250
        "+^{a}" |> sendKeys loggingFunc
        pause 500
        tabName |> sendKeys loggingFunc
        pause 250
        "{ENTER}" |> sendKeys loggingFunc
    | None -> ()


let findOrOpenAppByWindow
    (findCriteria: AutomationElement -> bool)
    (programExe: string)
    (makeAppSticky: bool)
    =
    let findApp () =
        allMainWindows ()
        |> Seq.tryFind findCriteria
        |> Option.map (fun app -> app |> activate |> focus)

    findApp ()
    |> function
        | None ->
            log
                $"App '%s{programExe}' not found, running it again and waiting for 2 seconds..."

            runProgram programExe
            wait 2000

            findApp ()
            |> function
                | Some app when makeAppSticky -> app |> makeSticky |> ignore
                | _ -> ()
        | Some _ -> ()


let findOrOpenAppByProcess
    (loggingFunc: LoggingFunc)
    (processName: string)
    (programExe: string)
    (makeAppSticky: bool)
    =
    let findApp () =
        let processes = allProcessesWithName processName

        Array.tryHead processes
        |> function
            | Some prcs ->
                loggingFunc $"Found existing process %s{processName}"
                windowOfProcess prcs.Id
            | None -> None
        |> Option.map (fun app -> app |> activate |> focus)

    findApp ()
    |> function
        | None ->
            loggingFunc (
                $"Not found existing process %s{processName}, "
                + "launching a new instance..."
            )

            runProgram programExe

            loggingFunc "Waiting for two seconds for the app to get running..."
            wait 2000

            findApp ()
            |> function
                | Some app when makeAppSticky -> app |> makeSticky |> ignore
                | _ -> ()
        | Some _ -> ()

let openNotepadPlusPlus (_: LoggingFunc) : unit =
    findOrOpenAppByWindow (nameEndsWith "Notepad++") "notepad++.exe" true

let openFman (_: LoggingFunc) : unit =
    let programExe =
        let appDataDir =
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            )

        Path.Combine([| appDataDir; @"fman\fman.exe" |])

    findOrOpenAppByWindow (nameIs "fman") programExe true

let openFoobar2000 (_: LoggingFunc) : unit =
    let programExe =
        let programFilesDir =
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)

        Path.Combine(
            [| programFilesDir
               @"foobar2000\foobar2000.exe" |]
        )

    findOrOpenAppByWindow (nameContains "foobar2000") programExe true

let openWindowsTerminal (loggingFunc: LoggingFunc) : unit =
    loggingFunc "--> openWindowsTerminal()"

    findOrOpenAppByProcess loggingFunc "WindowsTerminal" "wt.exe" true

    loggingFunc "<-- openWindowsTerminal()"

let openWindowsExplorer _ = "explorer.exe" |> runProgram


let desktopSwitchAllowedSignal = new ManualResetEvent(true)

let switchToDesktop desktopNumber (log: LoggingFunc) =
    let currentDesktopNumber =
        virtualDesktopManagerWrapper()
            .CurrentDesktopNumber()

    if currentDesktopNumber <> desktopNumber then
        log "Waiting for desktopSwitchAllowedSignal..."

        let waitResult =
            desktopSwitchAllowedSignal.WaitOne(10000)

        log $"Result of desktopSwitchAllowedSignal.WaitOne(): %A{waitResult}"

        log "desktopSwitchAllowedSignal.Reset()"
        desktopSwitchAllowedSignal.Reset() |> ignore

        try
            let currentDesktopNumber =
                virtualDesktopManagerWrapper()
                    .CurrentDesktopNumber()

            let focusedEl = AutomationElement.FocusedElement

            desktopFocusedElements <-
                desktopFocusedElements.Change(
                    currentDesktopNumber,
                    (fun _ -> Some focusedEl)
                )

            //            allMainWindows ()
            //            |> Seq.tryFind (classNameIs "Shell_TrayWnd")
            //            |> Option.map
            //                (fun app ->
            //                    app |> activate |> ignore
            //                    "activating Shell_TrayWnd" |> log)
            //            |> ignore

            let formState = showWautomaFormForSwitching ()

            // note: this pause is needed to allow the OS enough time to
            // unfocus the existing window before switching desktops. Otherwise
            // a taskbar flashing occurs.
            wait 250

            log "Manager.ListDesktops()"

            let desktop =
                virtualDesktopManagerWrapper().ListDesktops()
                |> Seq.toList
                |> List.item (desktopNumber - 1)

            log "desktop.SwitchTo()"
            desktop.SwitchTo()

            wait 250

            hideWautomaFormForSwitching formState

            match desktopFocusedElements.TryFind(desktopNumber) with
            | Some el -> el |> activate |> focus |> ignore
            | None -> ()

            wait 250
        finally
            log "desktopSwitchAllowedSignal.Set()"
            desktopSwitchAllowedSignal.Set() |> ignore

let dumpAllWindows (loggingFunc: LoggingFunc) =
    loggingFunc ""
    loggingFunc "Dumping all open windows:"

    allMainWindows ()
    |> Seq.iter
        (fun el ->
            $"%s{el.Current.Name} | {el.Current.ClassName}"
            |> loggingFunc)

    loggingFunc "-------------"

let hotkeys: Hotkeys =
    [ { Keys = KeyCombo.Parse("Win+Shift+G")
        Action = goToChromeTab "gmail"
        Description = "Open Gmail" }
      { Keys = KeyCombo.Parse("Win+Shift+O")
        Action = goToChromeTab "todoist"
        Description = "Open Todoist" }
      { Keys = KeyCombo.Parse("Win+Shift+L")
        Action = goToChromeTab "calendar"
        Description = "Open Google Calendar" }
      { Keys = KeyCombo.Parse("Win+Shift+Q")
        Action = goToChromeTab "clockify"
        Description = "Open Clockify" }
      { Keys = KeyCombo.Parse("Win+N")
        Action = openNotepadPlusPlus
        Description = "Open Notepad++" }
      { Keys = KeyCombo.Parse("Win+W")
        Action = openFoobar2000
        Description = "Open foobar2000" }
      { Keys = KeyCombo.Parse("Win+C")
        Action = openWindowsTerminal
        Description = "Open Windows Terminal" }
      { Keys = KeyCombo.Parse("Win+J")
        Action = rightClick
        Description = "Right mouse click" }
      { Keys = KeyCombo.Parse("Win+H")
        Action = hibernate
        Description = "Hibernate" }
      { Keys = KeyCombo.Parse("Win+E")
        Action = openFman
        Description = "Open fman" }
      { Keys = KeyCombo.Parse("Shift+Win+E")
        Action = openWindowsExplorer
        Description = "Open Windows Explorer" }
      { Keys = KeyCombo.Parse("Win+Num1")
        Action = switchToDesktop 1
        Description = "Switch to desktop 1" }
      { Keys = KeyCombo.Parse("Win+Num2")
        Action = switchToDesktop 2
        Description = "Switch to desktop 2" }
      { Keys = KeyCombo.Parse("Win+Num3")
        Action = switchToDesktop 3
        Description = "Switch to desktop 3" }
      { Keys = KeyCombo.Parse("Win+Num4")
        Action = switchToDesktop 4
        Description = "Switch to desktop 4" }
      { Keys = KeyCombo.Parse("Win+Num9")
        Action = dumpAllWindows
        Description = "Dump all currently open windows to the log" } ]
    |> List.map (fun x -> x.Keys, x)
    |> Map.ofSeq
