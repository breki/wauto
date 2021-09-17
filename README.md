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

## How to use

As this is still a prototype, you need to [build](#How-to-build) the application
before using it. Once you have successfully built the binaries, follow these
steps to setup the application:

1. Copy all of the files from the build directory to somewhere on your disk.
2. Press `Win+R` to open the `Run` dialog.
3. In the dialog, enter `shell:startup` and press the `Enter` key. This will
   open Windows Explorer in the `Startup` directory where Windows users can
   define which applications they want Windows to automatically start during the
   OS startup.
4. Right-click in the Windows Explorer window and choose `New` / `Shortcut` menu
   item.
5. In the `Create Shortcut` dialog, browse to the path of the `Wautoma.exe` file
   you copied in the first step. Click on the `Next` button.
6. Put `Wautoma` as the name of the shortcut. Click on the `Next` button.
7. You should now have a new `Wautoma` shortcut file shown by the Windows
   Explorer. The next time you restart Windows, Wautoma will be automatically
   started.
8. Double-click on the shortcut to start the application now, so you do not need
   to restart Windows.

TODO: Dropbox hack

TODO: about defining hotkeys

## How to build

In order to build Wautoma, you need .NET 4.8 or higher. And, obviously, you need
to fetch the latest source code from
the [github repository](https://github.com/breki/wauto). Then follow these
steps:

1. Open command line terminal and move to the `Wautoma` subdirectory of the
   repository.
2. Run `build.bat` build script.
3. If everything goes well, the build binaries should be waiting for you in
   the `Wautoma\Wautoma\bin\Release\net48\publish` directory.

## TODO: Technical details