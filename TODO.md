- run action in a separate thread?

- switching to Chrome is flaky - fix it
  - log actions
  - more waiting?
    - or checking the Chrome is really active?

- prevent from keyboard modifiers spilling over to SendKeys
  - options:
    1. wait for all the keys to be unpressed before running the action
    2. implement "anti-press" SendKeys events, if possible
    3. an in-action wait for keys to be unpressed
- implement a more advanced logging
    - do not clear, just add to the bottom
- implement converting from string names of key combos to KeyCombo 
  and vice versa
- based on the currently pressed key combo, find a shortcut
- investigate Maperitive UI test code for ways to automate things in Windows
- consider moving to F#, if possible
