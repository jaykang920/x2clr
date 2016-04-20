x2clr
=====

x2clr is the reference port of [x2](https://github.com/jaykang920/x2) written in
C# targeting CLR (Common Language Runtime) environments such as .NET or Mono.

Requirements
------------

* .NET framework 3.5 or newer equivalent environment to run
* Visual Studio 2008 (9.0) or newer equivalent tool to compile C# 3.0

Installation
------------

### Binary

x2clr can be installed via the [NuGet UI](https://docs.nuget.org/consume/package-manager-dialog) (as [x2clr](https://www.nuget.org/packages/x2clr)), or via the NuGet Package Manager console command:

    PM> Install-Package x2clr

The xpiler converts x2 definition files into corresponding C# source code files. So most probably you will want to install the [x2clr.xpiler](https://www.nuget.org/packages/x2clr.xpiler) too.

    PM> Install-Package x2clr.xpiler

### Source

You may clone the latest source code of x2clr from its [GitHub repository](https://github.com/jaykang920/x2clr.git).

Zipped archives containing specific tagged versions of the source code are available in [releases](https://github.com/jaykang920/x2clr/releases).

Getting Started
---------------

[HelloWorld example](https://github.com/jaykang920/x2clr/wiki/HelloWorld-Example) can be a simple start point to learn how x2clr applications are organized.

License
-------

x2clr is distributed under [MIT License](http://opensource.org/licenses/MIT).
See the file LICENSE for details.
