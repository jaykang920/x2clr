// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace x2
{
    // Extensions.Collections
    public static partial class Extensions
    {
        public static bool EqualsExtended<T>(this IList<T> self, IList<T> other)
        {
            if (Object.ReferenceEquals(self, other))
            {
                return true;
            }
            if ((object)self == null || (object)other == null)
            {
                return false;
            }
            int count = self.Count;
            if (count != other.Count)
            {
                return false;
            }
            for (int i = 0; i < count; ++i)
            {
                T mine = self[i];
                T others = other[i];
                if (typeof(T).IsSubclassOf(typeof(Cell)))
                {
                    if (!Object.ReferenceEquals(mine, others))
                    {
                        if ((object)mine == null || (object)others == null)
                        {
                            return false;
                        }
                        if (!mine.Equals(others))
                        {
                            return false;
                        }
                    }
                }
                else if (!EqualityComparer<T>.Default.Equals(mine, others))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsEquivalent<T>(this IList<T> self, IList<T> other)
        {
            return self.EqualsExtended(other);
        }

        public static void Resize<T>(this IList<T> self, int size)
        {
            while (self.Count < size)
            {
                self.Add(default(T));
            }
        }

        public static string ToStringExtended<T>(this IList<T> self)
        {
            if (Object.ReferenceEquals(self, null))
            {
                return "null";
            }

            var stringBuilder = new StringBuilder();

            stringBuilder.Append("[");
            for (int i = 0, count = self.Count; i < count; ++i)
            {
                if (i != 0)
                {
                    stringBuilder.Append(',');
                }
                stringBuilder.Append(' ');
                T element = self[i];
                stringBuilder.Append((object)element == null ?
                    "null" : self[i].ToString());
            }
            stringBuilder.Append(" ]");

            return stringBuilder.ToString();
        }
    }
}
