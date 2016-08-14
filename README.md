# leaflet
Leaflet is my experimental implementation of the [Infocom Z-Machine spec](http://inform-fiction.org/zmachine/index.html) - aka "a Zork interpreter" - aka "30 year old tech built with modern tools"

The purpose of this project is:
- An exercise in C#. To stretch and experiment to see how modern .NET language techniques can be applied to something low level that was designed over 25 years ago.
- To have something abstract and interesting to experiment with newer tools, libraries and environments such as .NET Core and the cloud. (or "where can I make this run?")

From [Wikipedia](https://en.wikipedia.org/wiki/Z-machine):
> Z-machine is a virtual machine that was developed by Joel Berez and Marc Blank in 1979 and used by Infocom for its text adventure games. Infocom compiled game code to files containing Z-machine instructions (called story files, or Z-code files), and could therefore port all its text adventures to a new platform simply by writing a Z-machine implementation for that platform. With the large number of incompatible home computer systems in use at the time, this was an important advantage over using native code or developing a compiler for each system."

Most people are most familiar with it in the form of the old text adventure game Zork, but it's actually a platform for general interactive fiction which was popular back in the 1980's and still has a community around it today.

There are [numerous implementations](https://github.com/search?utf8=%E2%9C%93&q=zmachine) of the Z-Machine (and several in C#) but those are (1) not written by me :-) and (2) often focused on size and speed. This implementation is initially focused on implementing the spec in an object oriented fashion with current syntax and methodologies with shifts and changes arising as needed. It is purposely not optimized at this time in order to emphasize readability when comparing with the spec for correctness.

The current implementation is a partial implementation of v3 of the Z-Machine.

-------------
The project structure is currently as follows:

### NegativeEddy.Leaflet.dll
- The core Z-Machine interpreter implementation. This is a .NET portable library so that it can run pretty much anywhere needed.

### NegativeEddy.Leaflet.ConsoleHost.exe
- A .NET 4.6.x console application host

### NegativeEddy.Leaflet.CoreConsoleHost.dll
- A .NET Core 1.0 console application host
-This host runs on Windows and Ubuntu Linux

### NegativeEddy.Leaflet.Tests.dll
- Unit tests for NegativeEddy.Leaflet.dll
 
-----------

The tools directory contains a copy of [Ztools](http://inform-fiction.org/zmachine/ztools.html) for Windows and some info dumps of minizork.z3 using those tools. These dumps are used to debug and validate the interpreter.

-----------
Updates and notes about the implementation and the evolution of the project will be posted to https://blog.negativeeddy.com/category/zmachine/