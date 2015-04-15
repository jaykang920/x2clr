// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

namespace x2
{
    /// <summary>
    /// Provides the common housekeeping methods for constants.
    /// </summary>
    public sealed class ConstsInfo<T>
    {
        private Dictionary<string, T> map;

        public ConstsInfo()
        {
            map = new Dictionary<string, T>();
        }

        public void Add(string name, T value)
        {
            map.Add(name, value);
        }

        public string GetName(T value)
        {
            foreach (var pair in map)
            {
                if (pair.Value.Equals(value))
                {
                    return pair.Key;
                }
            }
            return null;
        }

        public T Parse(string name)
        {
            return map[name];
        }

        public bool TryParse(string name, out T result)
        {
            return map.TryGetValue(name, out result);
        }
    }
}
