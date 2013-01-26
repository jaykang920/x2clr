// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2 {
  /*
  public delegate void ReleaseDelegate(PooledObject o);

  public class PooledObject //: //IDisposable
  {
    private ReleaseDelegate release;

    ~PooledObject() {
      if (release != null) {
        release(this);
        GC.ReRegisterForFinalize(this);
      }
      else {
        //Terminate();
      }
    }

    public void SetPool(ReleaseDelegate release) {
      this.release = release;
    }
  }

  public class Pool<T>
      where T : PooledObject, new() {
    private IList<byte[]> chunks;
  }
  */
}
