using System;
using System.Media;
using System.Runtime.InteropServices;

namespace TestHotkeys
{
    public class MyKeyboardHandler
    {
        public void Start()
        {
            this.hookHandle = NativeApi.SetHook(this.KeyboardHook);
        }

        public void Stop()
        {
            NativeApi.UnhookWindowsHookEx(hookHandle);
        }

        private IntPtr KeyboardHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
            }
            
            // todo: add some custom logic here
            SystemSounds.Beep.Play();
            
            return NativeApi.CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }

        private IntPtr hookHandle;
    }
}