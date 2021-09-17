module Wautoma.MyHotkeys

open System
open System.IO
open Wautoma.KeysTypes
open Wautoma.Logging
open Wautoma.UIAutomation.Processes
open Wautoma.UIAutomation.Windows
open Wautoma.UIAutomation.Misc
open Wautoma.UIAutomation.Mouse


let openGmail (loggingFunc: LoggingFunc) : unit =
    let chromeMaybe =
        allMainWindows ()
        |> Seq.tryFind (nameEndsWith "Google Chrome")

    match chromeMaybe with
    | Some chrome ->
        chrome |> activate |> focus |> ignore
        pause 250
        "+^A" |> sendKeys loggingFunc
        pause 500
        "gmail" |> sendKeys loggingFunc
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



let hotkeys: Hotkeys =
    [ { Keys = KeyCombo.Parse("Win+Shift+X")
        Action = openGmail
        Description = "Open Gmail" }
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
        Description = "Open fm" } ]
    |> List.map (fun x -> x.Keys, x)
    |> Map.ofSeq
