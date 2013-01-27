// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

using x2;

namespace x2.xpiler {
  class Xpiler {
    private readonly Options options;
    private bool error;

    public Options Options {
      get { return options; }
    }

    public bool Error {
      get { return error; }
    }

    public Xpiler(Options options) {
      this.options = options;
      error = false;
    }

    public void Process(string path) {
      Console.WriteLine(path);
    }
  }
}
