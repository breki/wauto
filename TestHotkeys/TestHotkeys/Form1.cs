using System;
using System.Windows.Forms;
using NonInvasiveKeyboardHookLibrary;

namespace TestHotkeys
{
    public partial class Form1 : Form, IAppLogging
    {
        // private KeyboardHookManager keyboardHookManager;
        private MyKeyboardHandler keyboardHandler;
        
        public Form1()
        {
            InitializeComponent();
            
            // GlobalHotKey.RegisterHotKey("Alt + Shift + S", DoSomething);
            // GlobalHotKey.RegisterHotKey("Win + A", DoSomethingElse);
            // GlobalHotKey.RegisterHotKey("Win + G", DoSomethingElse);

            // keyboardHookManager = new KeyboardHookManager();
            // keyboardHookManager.Start();
            //
            // keyboardHookManager.RegisterHotkey(0x60, () =>
            // {
            //     DoSomething();
            // });
            //
            // var modifiers = new[]
            // {
            //     NonInvasiveKeyboardHookLibrary.ModifierKeys.WindowsKey
            // };
            //
            // keyboardHookManager.RegisterHotkey(
            //     modifiers, 0x41, () =>
            // {
            //     DoSomething();
            // });

            // keyboardHookManager.RegisterHotkey(
            //     NonInvasiveKeyboardHookLibrary.ModifierKeys.Control, 0x60, () =>
            // {
            //     DoSomething();
            // });

            keyboardHandler = new MyKeyboardHandler(this);
            keyboardHandler.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            keyboardHandler.Stop();
            // keyboardHookManager.UnregisterAll();
            // keyboardHookManager.Stop();
            base.OnClosed(e);
        }

        private void DoSomething()
        {
            MessageBox.Show("Hello World", "Hello World");
        }

        private void DoSomethingElse()
        {
            MessageBox.Show("Notepad, eh?", "Notepad, eh?");
        }

        public void LogMessage(string message)
        {
            textBoxLog.Text += System.Environment.NewLine + message;
        }
    }
}