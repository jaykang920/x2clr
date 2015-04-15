// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2
{
    public interface IQueue<T>
    {
        int Length { get; }

        void Close();
        void Close(T finalItem);

        T Dequeue();

        int Enqueue(T item);
        bool TryDequeue(out T value);
    }
}
