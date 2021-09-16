# Wautoma

Prototype of a Windows automation tool. The current version enables defining
keyboard shortcuts to execute various actions in Windows.

## Introduction - why another tool?

I have been using [AutoHotKey](https://www.autohotkey.com/)
for many years. I like to keep my the tools and actions I use frequently
accessible via simple keyboard shortcuts - nothing fancy, just pressing `Win+C`
to open and focus on Windows Terminal, for example. While this is relatively
easily achievable with AutoHotKey,
its [arcane scripting language](https://www.autohotkey.com/docs/Language.htm) (
in [two different versions](https://lexikos.github.io/v2/docs/Language.htm),
actually) was really starting to get on my nerve when I wanted to implement more
advanced automation actions.

So I decided to implement something on my own. I also needed an excuse to do
some work in F# and this looked like a nice challenge.

My initial goal of being able to automate Windows actions via hotkeys is now
mostly done - I managed to reproduce all the hotkeys I used in AutoHotKey and I
have thus stopped using AutoHotKey altogether.

### Future plans

But my goals for Wautoma are wider than just replicating AutoHotKey. Another
tool I've been using for many years is a virtual desktop manager
called [VirtuaWin](https://virtuawin.sourceforge.io/). I use it to reduce the
clutter of too many applications and Windows being visible at one time. The
thing is, I typically rarely restart Windows (mostly just when being forced by
Windows updates), preferring to hibernate the OS when go away from the computer.
This results in a lot of open applications, since I work on multiple different
projects and tasks. VirtuaWin enables me to separate these projects/tasks into
individual virtual desktops and then I simply switch desktops when I switch
tasks.

The problem is VirtuaWin is showing its age. It was very useful on older Windows
versions which did not come with virtual desktops support built in. But now that
Windows 10 has its own virtual desktops manager, I feel it is better integrated
and quicker than VirtuaWin.

One of my ideas for Wautoma is to include support for switching desktops when
executing actions. And perhaps also being able to pre-configure these desktops
when Windows start, so you would not need to manually open applications and move
them to their designated virtual desktops. There are also some other ideas I
have around this, but first I need to investigate what kind of API Microsoft
provides for virtual desktops and how flexible it is.
