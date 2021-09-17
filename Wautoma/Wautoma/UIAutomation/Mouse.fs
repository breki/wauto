module Wautoma.UIAutomation.Mouse

open Wautoma.NativeApi

let rightClick _ =
    let mutable cursorPos = MousePoint()
    GetCursorPos(&cursorPos) |> ignore

    mouse_event (int MouseEventFlags.RightUp, cursorPos.x, cursorPos.y, 0, 0)
