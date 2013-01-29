// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.IO;

namespace xpiler {
  class Program {
    static int Main(string[] args) {
      Options options = new Options();
      int index = options.Parse(args);
      if (index >= args.Length) {
        Console.WriteLine("{0}: missing arguments", Path.GetFileName(
            System.Reflection.Assembly.GetEntryAssembly().Location));
        return 2;
      }

      Xpiler xpiler = new Xpiler(options);
      while (index < args.Length) {
        xpiler.Process(args[index++]);
      }
      return (xpiler.Error ? 1 : 0);
    }
  }
}
