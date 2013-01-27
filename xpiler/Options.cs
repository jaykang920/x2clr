// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

using x2;

namespace x2.xpiler {
  class Options {
    private bool force;
    private bool recursive;

    public bool Force {
      get { return force; }
    }

    public bool Recursive {
      get { return recursive; }
    }

    public Options() {
      force = false;
      recursive = false;
    }

    static void PrintUsage() {
      Console.WriteLine("usage: xpiler (options) [args]");
      Console.WriteLine(" options:");
      Console.WriteLine("  -f (--force)     : force all to be recompiled");
      Console.WriteLine("  -r (--recursive) : process subdirectories recursively");
      Console.WriteLine("  -h (--help)      : print this message and quit");
    }

    public int Parse(string[] args) {
      Getopt.Option[] longopts = new Getopt.Option[] {
        new Getopt.Option("force", Getopt.NO_ARGUMENT, 'f'),
        new Getopt.Option("recursive", Getopt.NO_ARGUMENT, 'r'),
        new Getopt.Option("help", Getopt.NO_ARGUMENT, 'h')
      };

      Getopt getopt = new Getopt(args, "frh", longopts);
      while (getopt.Next() != -1) {
        switch (getopt.Opt) {
          case 'f':
            force = true;
            break;
          case 'r':
            recursive = true;
            break;
          case 'h':
            PrintUsage();
            System.Environment.Exit(2);
            break;
          default:
            break;
        }
      }
      return getopt.OptInd;
    }
  }
}
