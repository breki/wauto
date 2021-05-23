using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TestHotkeys
{
    public class MyKeyboardHandler
    {
        public MyKeyboardHandler(TextBox textBoxLog)
        {
            this.textBoxLog = textBoxLog;
        }

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
            textBoxLog.Text = nCode.ToString();
            
            if (nCode >= 0)
            {
                var keyboardMessage = (NativeKeyboardMessage) wParam;
                var keyboardMessageStr =
                    Enum.GetName(typeof(NativeKeyboardMessage),
                        keyboardMessage);
                
                int vkCode = Marshal.ReadInt32(lParam);
                textBoxLog.Text += " : " + keyboardMessageStr + " : " + vkCode;
            }
            
            // todo: add some custom logic here
            // SystemSounds.Beep.Play();
            
            return NativeApi.CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }

        private IntPtr hookHandle;

        private TextBox textBoxLog;
    }

    public enum NativeKeyboardMessage
    {
        WM_KEYDOWN = NativeApi.WM_KEYDOWN,
        WM_KEYUP = NativeApi.WM_KEYUP,
        WM_SYSKEYDOWN = NativeApi.WM_SYSKEYDOWN,
        WM_SYSKEYUP = NativeApi.WM_SYSKEYUP
    }
}