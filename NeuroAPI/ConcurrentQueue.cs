using System;
using System.Collections.Generic;
using System.Threading;

namespace TurnaboutAI.NeuroAPI
{
    public sealed class ConcurrentQueue<T> : IDisposable
    {
        private readonly Queue<T> _queue;
        private readonly AutoResetEvent _event;
        private readonly object _lock;

        private bool _disposed;

        public ConcurrentQueue()
        {
            _queue = new Queue<T>();
            _event = new AutoResetEvent(false);
            _lock = new object();
        }

        public int Count
        {
            get
            {
                CheckDisposed();

                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        public IEnumerable<T> BlockingEnumerable()
        {
            CheckDisposed();

            while(!_disposed)
            {
                try
                {
                    _event.WaitOne();
                }
                catch
                {
                    yield break;
                }

                while(true)
                {
                    T obj;

                    lock (_lock)
                    {
                        if (_queue.Count > 0)
                        {
                            obj = _queue.Dequeue();
                        }
                        else
                        {
                            break;
                        }
                    }

                    yield return obj;
                }
            }
        }

        public void Clear()
        {
            CheckDisposed();

            lock (_lock)
            {
                _queue.Clear();
            }
        }

        public void Dispose()
        {
            if(!_disposed)
            {
                _disposed = true;
                ((IDisposable)_event).Dispose();
            }
        }

        public void Enqueue(T item)
        {
            CheckDisposed();

            lock (_lock)
            {
                _queue.Enqueue(item);
            }

            _event.Set();
        }

        public bool TryDequeue(out T item)
        {
            CheckDisposed();

            lock (_lock)
            {
                if (_queue.Count == 0)
                {
                    item = default;
                    return false;
                }

                item = _queue.Dequeue();
                return true;
            }
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ConcurrentQueue<T>));
        }
    }
}
