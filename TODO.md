- consider moving to F#, if possible
- prevent from keyboard modifiers spilling over to SendKeys
  - options:
    1. wait for all the keys to be unpressed before running the action
    2. implement "anti-press" SendKeys events, if possible
    3. an in-action wait for keys to be unpressed
- implement converting from string names of key combos to KeyCombo 
  and vice versa
- based on the currently pressed key combo, find a shortcut
- investigate Maperitive UI test code for ways to automate things in Windows
