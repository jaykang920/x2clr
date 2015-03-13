// Copyright (c) 2013-2015 Jae-jun Kang
// See the file COPYING for license details.

using System.Reflection;

[assembly: AssemblyProduct("x2clr")]
[assembly: AssemblyCopyright("Copyright © 2013-2015 Jae-jun Kang")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyVersion("0.5.1.0")]
[assembly: AssemblyFileVersion("0.5.1.0")]
