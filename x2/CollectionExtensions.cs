﻿// Copyright (c) 2013-2015 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Text;

namespace x2
{
    public static class CollectionExtensions
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
                if (!EqualityComparer<T>.Default.Equals(self[i], other[i]))
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

        public static string ToStringExtended<T>(this IList<T> self)
        {
            if (Object.ReferenceEquals(self, null))
            {
                return "[]";
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
                stringBuilder.Append(self[i].ToString());
            }
            stringBuilder.Append(" ]");

            return stringBuilder.ToString();
        }
    }
}