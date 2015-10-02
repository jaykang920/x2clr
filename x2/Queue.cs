// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

namespace x2
{
    /// <summary>
    /// Defines the methods and properties of a blocking queue.
    /// </summary>
    public interface IQueue<T>
    {
        int Length { get; }

        void Close();
        void Close(T finalItem);

        T Dequeue();

        int Dequeue(IList<T> list);

        void Enqueue(T item);
        bool TryDequeue(out T value);
        bool TryDequeue(IList<T> list);
    }
}
