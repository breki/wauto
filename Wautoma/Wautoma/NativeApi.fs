module Wautoma.NativeApi

open System.Runtime.InteropServices


[<Literal>]
let WH_KEYBOARD_LL = 13


type NativeKeyboardMessage =
    | WM_KEYDOWN = 256
    | WM_KEYUP = 257
    | WM_SYSKEYDOWN = 260
    | WM_SYSKEYUP = 261


[<StructLayout(LayoutKind.Sequential)>]
type KBDLLHOOKSTRUCT =
    struct
        val vkCode: uint32
        val scanCode: uint32
        val flags: uint32
        val time: uint32
        val dwExtraInfo: nativeint
    end

type LowLevelKeyboardArgs = int * nativeint * nativeint
type LowLevelKeyboardProc = delegate of int * nativeint * nativeint -> nativeint

type MouseEventFlags =
    | LeftDown = 0x2
    | LeftUp = 0x4
    | MiddleDown = 0x20
    | MiddleUp = 0x40
    | Move = 0x1
    | Absolute = 0x8000
    | RightDown = 0x8
    | RightUp = 0x10

type MousePoint =
    struct
        val x: int
        val y: int
    end

[<DllImport("kernel32.dll")>]
extern uint32 GetCurrentThreadId()

[<DllImport("kernel32.dll", SetLastError = true)>]
extern nativeint GetModuleHandle(string lpModuleName)

[<DllImport("user32.dll", SetLastError = true)>]
extern bool UnhookWindowsHookEx(nativeint hhk)

[<DllImport("user32.dll", SetLastError = true)>]
extern nativeint SetWindowsHookEx(int idhook, LowLevelKeyboardProc proc, nativeint hMod, uint32 threadId)

[<DllImport("kernel32.dll")>]
extern nativeint LoadLibrary(string lpFileName)

[<DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
extern nativeint CallNextHookEx(nativeint hhk, int nCode, nativeint wParam, nativeint lParam)

[<DllImport("user32.dll")>]
extern bool GetCursorPos(MousePoint& lpPoint)

[<DllImport("user32.dll")>]
extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo)
