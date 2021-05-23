using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using NonInvasiveKeyboardHookLibrary;

namespace TestHotkeys
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class NativeApi
    {
        public const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 256;
        public const int WM_KEYUP = 257;
        public const int WM_SYSKEYDOWN = 260;
        public const int WM_SYSKEYUP = 261;

        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            IntPtr hMod = LoadLibrary("User32");
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, hMod, 0U);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(
            int idHook,
            LowLevelKeyboardProc lpfn,
            IntPtr hMod,
            uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(
            IntPtr hhk,
            int nCode,
            IntPtr wParam,
            IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string lpFileName);

        public delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public KBDLLHOOKSTRUCTFlags flags;
        public uint time;
        public UIntPtr dwExtraInfo;

        public static KBDLLHOOKSTRUCT CreateFromPtr (IntPtr ptr)
        {
            return (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(ptr, typeof(KBDLLHOOKSTRUCT));
        }
    }

    [Flags]
    public enum KBDLLHOOKSTRUCTFlags : uint
    {
        LLKHF_EXTENDED = 0x01,
        LLKHF_INJECTED = 0x10,
        LLKHF_ALTDOWN = 0x20,
        LLKHF_UP = 0x80,
    }}