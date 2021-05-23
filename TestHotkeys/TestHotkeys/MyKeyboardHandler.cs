using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using NonInvasiveKeyboardHookLibrary;

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
            var forwardToNextHook = true;
            
            textBoxLog.Text = nCode.ToString();
            
            if (nCode >= 0)
            {
                var keyboardMessage = (NativeKeyboardMessage) wParam;
                var keyboardMessageStr =
                    Enum.GetName(typeof(NativeKeyboardMessage),
                        keyboardMessage);

                KBDLLHOOKSTRUCT kbHookStruct =
                    (KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lParam,
                        typeof(KBDLLHOOKSTRUCT));

                var virtualKeyCode = new VirtualKeyCode(kbHookStruct.vkCode);

                var modifierKey = virtualKeyCode.ToModifierKey();

                switch (keyboardMessage)
                {
                    case NativeKeyboardMessage.WM_KEYDOWN:
                    case NativeKeyboardMessage.WM_SYSKEYDOWN:
                        if (modifierKey == 0)
                        {
                            var updatedPressedKeys =
                                currentlyPressedKeys.WithPressedMainKey(
                                    virtualKeyCode);
                            this.currentlyPressedKeys = updatedPressedKeys;
                        }
                        else
                        {
                            var updatedPressedKeys =
                                currentlyPressedKeys.WithPressedModifier(
                                    modifierKey);
                            this.currentlyPressedKeys = updatedPressedKeys;
                        }

                        break;

                    case NativeKeyboardMessage.WM_KEYUP:
                    case NativeKeyboardMessage.WM_SYSKEYUP:
                        if (modifierKey == 0)
                        {
                            var updatedPressedKeys =
                                currentlyPressedKeys.WithUnpressedMainKey();
                            this.currentlyPressedKeys = updatedPressedKeys;
                        }
                        else
                        {
                            var updatedPressedKeys =
                                currentlyPressedKeys.WithUnpressedModifier(
                                    modifierKey);
                            this.currentlyPressedKeys = updatedPressedKeys;
                        }

                        break;
                }

                textBoxLog.Text += " : " + modifierKey + " : " +
                                   keyboardMessageStr + " : " + virtualKeyCode +
                                   Environment.NewLine +
                                   this.currentlyPressedKeys;

                var altSKeyCombo = new KeyCombo(ModifierKeys.WindowsKey,
                    new VirtualKeyCode(83u));
                if(this.currentlyPressedKeys == altSKeyCombo)
                    forwardToNextHook = false;
            }
            
            // todo: add some custom logic here
            // SystemSounds.Beep.Play();

            if (forwardToNextHook)
                return NativeApi.CallNextHookEx(hookHandle, nCode, wParam,
                    lParam);
            else
            {
                // If the hook procedure processed the message, it may return
                // a nonzero value to prevent the system from passing the
                // message to the rest of the hook chain or
                // the target window procedure. 
                return new IntPtr(1);
            }
        }

        private IntPtr hookHandle;
        private readonly TextBox textBoxLog;
        private KeyCombo currentlyPressedKeys = KeyCombo.None;
    }

    public enum NativeKeyboardMessage
    {
        WM_KEYDOWN = NativeApi.WM_KEYDOWN,
        WM_KEYUP = NativeApi.WM_KEYUP,
        WM_SYSKEYDOWN = NativeApi.WM_SYSKEYDOWN,
        WM_SYSKEYUP = NativeApi.WM_SYSKEYUP
    }
}


public record VirtualKeyCode
{
    public VirtualKeyCode(uint code)
    {
        this.Code = code;
    }

    public ModifierKeys ToModifierKey()
    {
        switch (this.Code)
        {
            case 160:
            case 161:
                return ModifierKeys.Shift;
            case 162:
            case 163:
                return ModifierKeys.Control;
            case 164:
            case 165:
                return ModifierKeys.Alt;
            case 91:
                return ModifierKeys.WindowsKey;
            default:
                return 0;
        }
    }

    public override string ToString()
    {
        return string.Format("{0}", Code);
    }

    public readonly uint Code;
}


public record KeyCombo
{
    public KeyCombo(ModifierKeys modifiers, VirtualKeyCode keyCode)
    {
        this.modifiers = modifiers;
        this.keyCode = keyCode;
    }

    public static readonly KeyCombo None = new(0, null);

    public KeyCombo WithPressedModifier(ModifierKeys modifier)
    {
        var newModifiers = this.modifiers | modifier;
        return new KeyCombo(newModifiers, this.keyCode);
    }

    public KeyCombo WithUnpressedModifier(ModifierKeys modifier)
    {
        var newModifiers = this.modifiers & ~modifier;
        return new KeyCombo(newModifiers, this.keyCode);
    }

    public KeyCombo WithPressedMainKey(VirtualKeyCode pressedKeyCode)
    {
        return new(this.modifiers, pressedKeyCode);
    }

    public KeyCombo WithUnpressedMainKey()
    {
        return new(this.modifiers, null);
    }

    public override string ToString()
    {
        var s = new StringBuilder();

        if ((this.modifiers & ModifierKeys.WindowsKey) != 0)
            s.Append("Win+");
         if ((this.modifiers & ModifierKeys.Shift) != 0)
            s.Append("Shift+");
        if ((this.modifiers & ModifierKeys.Control) != 0)
            s.Append("Ctrl+");
        if ((this.modifiers & ModifierKeys.Alt) != 0)
            s.Append("Alt+");

        if (this.keyCode != null)
        {
            switch (this.keyCode.Code)
            {
                case 83u:
                    s.Append("S");
                    break;
            }
        }

        return s.ToString();
    }

    private readonly ModifierKeys modifiers;
    private readonly VirtualKeyCode keyCode;
}
