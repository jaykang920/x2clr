// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

namespace xpiler {
  interface IFormatter {
    string Description { get; }
    bool Format(Document doc);
    bool IsUpToDate(Document doc);
  }
}
