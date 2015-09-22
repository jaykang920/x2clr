x2clr
=====

_Under development now_

x2clr is the reference port of [x2](https://github.com/jaykang920/x2) written in
C# targeting CLR (Common Language Runtime) environments such as .NET or Mono.

Requirements
------------

* .NET framework 3.5 or newer equivalent environment to run
* Visual Studio 2008 (9.0) or newer equivalent tool to compile C# 3.0

Installation
------------

x2clr can be installed via the [NuGet UI](https://docs.nuget.org/consume/package-manager-dialog) (as [x2clr](https://www.nuget.org/packages/x2clr)), or via the NuGet Package Manager console command:

    PM> Install-Package x2clr

The xpiler converts x2 definition files into corresponding C# source code files. So most probably you will want to install the [x2clr.xpiler](https://www.nuget.org/packages/x2clr.xpiler) too.

    PM> Install-Package x2clr.xpiler

ZIP archives containing specific tagged versions of the source code are available in [releases](https://github.com/jaykang920/x2clr/releases).

Getting Started
---------------

[HeadFirst examples](https://github.com/jaykang920/x2clr/wiki/HeadFirst-Examples) can be a simple start to learn how to organize x2 applications. 

License
-------

x2clr is distributed under [MIT License](http://opensource.org/licenses/MIT).
See the file LICENSE for details.
