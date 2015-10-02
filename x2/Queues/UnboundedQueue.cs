﻿// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    public class UnboundedQueue<T> : IQueue<T>
    {
        private Queue<T> queue;
        private bool closing;

        public int Length
        {
            get
            {
                lock (queue)
                {
                    return queue.Count;
                }
            }
        }

        public UnboundedQueue()
        {
            queue = new Queue<T>();
            closing = false;
        }

        public void Close()
        {
            Close(default(T));
        }

        public void Close(T finalItem)
        {
            lock (queue)
            {
                queue.Enqueue(finalItem);
                closing = true;
                Monitor.PulseAll(queue);
            }
        }

        public T Dequeue()
        {
            lock (queue)
            {
                while (queue.Count == 0)
                {
                    if (closing)
                    {
                        return default(T);
                    }
                    Monitor.Wait(queue);
                }
                return queue.Dequeue();
            }
        }

        public void Dequeue(List<T> values)
        {
            lock (queue)
            {
                while (queue.Count == 0)
                {
                    if (closing)
                    {
                        return;
                    }
                    Monitor.Wait(queue);
                }
                while (queue.Count != 0)
                {
                    values.Add(queue.Dequeue());
                }
            }
        }

        public void Enqueue(T item)
        {
            lock (queue)
            {
                if (!closing)
                {
                    queue.Enqueue(item);
                    Monitor.Pulse(queue);
                }
            }
        }

        public bool TryDequeue(out T value)
        {
            lock (queue)
            {
                if (queue.Count == 0)
                {
                    value = default(T);
                    return false;
                }
                value = queue.Dequeue();
                return true;
            }
        }

        public bool TryDequeue(List<T> values)
        {
            lock (queue)
            {
                if (queue.Count == 0)
                {
                    return false;
                }
                while (queue.Count != 0)
                {
                    values.Add(queue.Dequeue());
                }
                return true;
            }
        }
    }
}
