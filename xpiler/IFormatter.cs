// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

namespace x2.xpiler {
  interface IFormatter {
    bool IsUpToDate(Document doc);
    bool Format(Document doc);
  }
}
