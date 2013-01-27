// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

namespace x2.xpiler {
  class CSharpFormatter : IFormatter {
    public bool IsUpToDate(string path) {
      return false;
    }

    public bool Format(Document doc) {
      return false;
    }
  }
}
