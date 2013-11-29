// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace x2
{
    public class Mapping<TKey, TValue> : Cell
    {
        private readonly Dictionary<TKey, TValue> store;

        public int Count { get { return store.Count; } }

        public Mapping()
            : base(0)
        {
            store = new Dictionary<TKey, TValue>();
        }

        public void Add(TKey key, TValue value)
        {
            store.Add(key, value);
        }

        public override void Load(Buffer buffer)
        {
            // TODO
        }

        protected override void Dump(Buffer buffer)
        {
            // TODO
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            // TODO
        }
    }

}
