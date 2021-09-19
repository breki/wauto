module Wautoma.MyHotkeys

open System
open System.IO
open Wautoma.KeysTypes
open Wautoma.Logging
open Wautoma.UIAutomation.Processes
open Wautoma.UIAutomation.Windows
open Wautoma.UIAutomation.Misc
open Wautoma.UIAutomation.Mouse
open Wautoma.VirtualDesktops


let goToChromeTab tabName (loggingFunc: LoggingFunc) : unit =
    let chromeMaybe =
        allMainWindows ()
        |> Seq.tryFind (nameEndsWith "Google Chrome")

    match chromeMaybe with
    | Some chrome ->
        chrome |> activate |> focus |> ignore
        pause 250
        "+^A" |> sendKeys loggingFunc
        pause 500
        tabName |> sendKeys loggingFunc
        pause 250
        "{ENTER}" |> sendKeys loggingFunc
    | None -> ()


let openNotepadPlusPlus (_: LoggingFunc) : unit =
    let notepadMaybe =
        allMainWindows ()
        |> Seq.tryFind (nameEndsWith "Notepad++")

    match notepadMaybe with
    | Some notepad -> notepad |> activate |> focus |> ignore
    | None -> runProgram "notepad++.exe"

let openFm (_: LoggingFunc) : unit =
    let appFormMaybe =
        allMainWindows () |> Seq.tryFind (nameIs "fman")

    match appFormMaybe with
    | Some appForm -> appForm |> activate |> focus |> ignore
    | None ->
        let appDataDir =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)

        Path.Combine(
            [| appDataDir
               @"Microsoft\Windows\Start Menu\Programs\fman.lnk" |]
        )
        |> runProgram

let openFoobar2000 (_: LoggingFunc) : unit =
    let appFormMaybe =
        allMainWindows ()
        |> Seq.tryFind (nameEndsWith "[foobar2000]")

    match appFormMaybe with
    | Some appForm -> appForm |> activate |> focus |> ignore
    | None ->
        let programFilesDir =
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)

        Path.Combine(
            [| programFilesDir
               @"foobar2000\foobar2000.exe" |]
        )
        |> runProgram

let openWindowsTerminal (_: LoggingFunc) : unit =
    let wtProcesses = allProcessesWithName "WindowsTerminal"

    match Array.tryHead wtProcesses with
    | Some wtProcess ->
        windowOfProcess wtProcess.Id
        |> Option.map (fun el -> el |> activate |> focus)
        |> ignore
    | None -> "wt.exe" |> runProgram

let openWindowsExplorer _ = "explorer.exe" |> runProgram


let switchToDesktop desktopNumber (_: LoggingFunc) =
    let manager = createVirtualDesktopsManager ()

    let desktop =
        manager.ListDesktops()
        |> Seq.toList
        |> List.item (desktopNumber - 1)

    desktop.SwitchTo()


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
        Description = "Open foobar2000" }
      { Keys = KeyCombo.Parse("Win+J")
        Action = rightClick
        Description = "Right mouse click" }
      { Keys = KeyCombo.Parse("Win+H")
        Action = hibernate
        Description = "Hibernate" }
      { Keys = KeyCombo.Parse("Win+E")
        Action = openFm
        Description = "Open fm" }
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
        Description = "Switch to desktop 4" } ]
    |> List.map (fun x -> x.Keys, x)
    |> Map.ofSeq
