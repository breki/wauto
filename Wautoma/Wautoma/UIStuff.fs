﻿module Wautoma.UIStuff

open System
open System.Drawing
open System.Windows.Forms
open Wautoma.KeyboardHandling
open Wautoma.KeysTypes
open Wautoma.Settings

let logActivityIntoTextBox (loggingTextBox: TextBox) msg : unit =
    let logFunc () =
        loggingTextBox.AppendText(msg + Environment.NewLine)

    loggingTextBox.Invoke(MethodInvoker(logFunc))
    |> ignore


type AppForm(hotkeys: Hotkeys) as this =
    inherit Form()

    let components = new System.ComponentModel.Container()

    let loggingTextBox = new TextBox()

    let logActivity msg =
        logActivityIntoTextBox loggingTextBox msg

    let notifyIcon = new NotifyIcon(components)

    let keyboardHandler =
        new KeyboardHandler(hotkeys, logActivity)

    let settingsFileName = "Wautoma-settings.json"

    do
        let settings = loadSettings settingsFileName

        this.StartPosition <- FormStartPosition.Manual

        this.Location <-
            Point(
                settings |> getSettingInt "form.x" 10,
                settings |> getSettingInt "form.y" 10
            )

        this.Width <- settings |> getSettingInt "form.width" 500
        this.Height <- settings |> getSettingInt "form.height" 500

        let icon =
            new Icon(@"\src\wauto\Wautoma\Wautoma\icon.ico")

        this.Icon <- icon

        loggingTextBox.Anchor <-
            AnchorStyles.Top
            ||| AnchorStyles.Right
            ||| AnchorStyles.Bottom
            ||| AnchorStyles.Left

        loggingTextBox.Multiline <- true
        loggingTextBox.ReadOnly <- true
        loggingTextBox.Dock <- DockStyle.Fill
        loggingTextBox.ScrollBars <- ScrollBars.Vertical

        this.Controls.Add(loggingTextBox)

        // todo igor: use an embedded icon here
        notifyIcon.Icon <- icon
        notifyIcon.Visible <- true

        keyboardHandler.Start()

    member this.LoggingTextBox = loggingTextBox

    override this.OnClosing _ =
        keyboardHandler.Stop()

        loadSettings settingsFileName
        |> setSetting "form.x" this.Location.X
        |> setSetting "form.y" this.Location.Y
        |> setSetting "form.width" this.Width
        |> setSetting "form.height" this.Height
        |> saveSettings settingsFileName

    override this.Dispose disposing =
        (keyboardHandler :> IDisposable).Dispose()

        if disposing then
            if components <> null then
                components.Dispose()

        base.Dispose(disposing)



let createUIElements hotkeys =
    let form = new AppForm(hotkeys)

    let loggingFunc =
        form.LoggingTextBox |> logActivityIntoTextBox

    form, loggingFunc
