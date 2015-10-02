// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

namespace x2
{
    public interface IQueue<T>
    {
        int Length { get; }

        void Close();
        void Close(T finalItem);

        T Dequeue();

        void Dequeue(List<T> list);

        void Enqueue(T item);
        bool TryDequeue(out T value);
        bool TryDequeue(List<T> list);
    }
}
