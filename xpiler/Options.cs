// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

using x2;

namespace x2.xpiler {
  class Options {
    private string spec;
    private bool recursive;
    private bool force;

    public string Spec {
      get { return spec; }
    }

    public bool Recursive {
      get { return recursive; }
    }

    public bool Force {
      get { return force; }
    }

    public Options() {
      spec = "cs";
      recursive = false;
      force = false;
    }

    static void PrintUsage() {
      Console.WriteLine("usage: xpiler (options) [path...]");
      Console.WriteLine(" options:");
      Console.WriteLine("  -s (--spec) lang : specifies the target language");
      Console.WriteLine("                cs : C# (default)");
      Console.WriteLine("  -r (--recursive) : process subdirectories recursively");
      Console.WriteLine("  -f (--force)     : force all to be recompiled");
      Console.WriteLine("  -h (--help)      : print this message and quit");
    }

    public int Parse(string[] args) {
      Getopt.Option[] longopts = new Getopt.Option[] {
        new Getopt.Option("spec", Getopt.REQUIRED_ARGUMENT, 's'),
        new Getopt.Option("recursive", Getopt.NO_ARGUMENT, 'r'),
        new Getopt.Option("force", Getopt.NO_ARGUMENT, 'f'),
        new Getopt.Option("help", Getopt.NO_ARGUMENT, 'h')
      };

      Getopt getopt = new Getopt(args, "s:rfh", longopts);
      while (getopt.Next() != -1) {
        switch (getopt.Opt) {
          case 's':
            spec = getopt.OptArg.ToLower();
            if (spec != "cs") {
              Console.Error.WriteLine("Unknown target language specified: {0}",
                                      spec);
              System.Environment.Exit(1);
            }
            break;
          case 'r':
            recursive = true;
            break;
          case 'f':
            force = true;
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
