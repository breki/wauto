- MyKeyboardHandler
  - use our own logic for low-level capture (not `GlobalHotKey`)
    - implement a currently pressed keyboard state structure
    - based on that structure and keypress does event, find a shortcut
    - if shortcut exists, do not call CallNextHookEx, instead return -1

    - take the key handling code from KeyboardHookManager
    - how to prevent the registered hotkey event bubbling forwards?
    - why does the second stroke always get ignored?
