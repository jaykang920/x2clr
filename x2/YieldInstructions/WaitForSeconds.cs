// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections;

namespace x2.Coroutines
{
    public class WaitForSeconds : YieldInstruction
    {
        private System.DateTime endTime;
        private float duration;

        public WaitForSeconds(float duration)
        {
            this.endTime = DateTime.Now.AddSeconds(duration);
            this.duration = duration;
        }

        public override object Current
        {
            get
            {
                return null;
            }
        }

        public override bool MoveNext()
        {
            if (endTime.CompareTo(DateTime.Now) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
