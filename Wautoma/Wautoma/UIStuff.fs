module Wautoma.UIStuff

open System
open System.Drawing
open System.Threading
open System.Windows.Forms
open Wautoma.KeyboardHandling
open Wautoma.Logging


let logActivityIntoTextBox msg (loggingTextBox: TextBox) : unit =
    let logFunc () =
        loggingTextBox.Text <- loggingTextBox.Text + msg + Environment.NewLine

    loggingTextBox.Invoke(MethodInvoker(logFunc))
    |> ignore


type AppForm() as this =
    inherit Form()

    let components = new System.ComponentModel.Container()

    let loggingTextBox = new TextBox()

    let logActivity msg =
        loggingTextBox |> logActivityIntoTextBox msg

    let notifyIcon = new NotifyIcon(components)
    let keyboardHandler = new KeyboardHandler(logActivity)

    do
        this.Width <- 500
        this.Height <- 500

        loggingTextBox.Anchor <-
            AnchorStyles.Top
            ||| AnchorStyles.Right
            ||| AnchorStyles.Bottom
            ||| AnchorStyles.Left

        loggingTextBox.Multiline <- true
        loggingTextBox.ReadOnly <- true
        loggingTextBox.Dock <- DockStyle.Fill

        this.Controls.Add(loggingTextBox)

        // todo igor: use an embedded icon here
        notifyIcon.Icon <- new Icon(@"D:\src\wauto\Wautoma\Wautoma\sample.ico")
        notifyIcon.Visible <- true

        keyboardHandler.Start()

    member this.LoggingTextBox = loggingTextBox

    override this.OnClosing(_) = keyboardHandler.Stop()

    override this.Dispose disposing =
        (keyboardHandler :> IDisposable).Dispose()

        if disposing then

            if components <> null then
                components.Dispose()

        base.Dispose(disposing)



let createUIElements () =
    let form = new AppForm()
    (form, form.LoggingTextBox)

let executeInBackground
    (action: LoggingFunc -> unit)
    (logActivity: LoggingFunc)
    : unit =
    let run () = action logActivity
    let thread : Thread = Thread(ThreadStart(run))
    thread.Start()
