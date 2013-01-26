// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

namespace x2 {
  public interface IBlockingQueue<T> {
    int Length { get; }

    void Close();
    void Close(T finalItem);

    T Dequeue();

    void Enqueue(T item);
    bool TryDequeue(out T value);
  }
}
