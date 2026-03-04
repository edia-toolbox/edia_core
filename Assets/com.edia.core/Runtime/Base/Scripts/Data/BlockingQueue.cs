using System.Threading;
using System.Collections.Generic;
using System.Collections;
using System;

namespace Edia.Data {

    /// <summary>
    /// Thread-safe blocking queue for producer-consumer pattern (used for async file I/O).
    /// </summary>
    public class BlockingQueue<T> : IEnumerable<T> {

        private int _count = 0;
        private Queue<T> _queue = new Queue<T>();

        public T Dequeue() {
            lock (_queue) {
                while (_count <= 0)
                    Monitor.Wait(_queue);
                _count--;
                return _queue.Dequeue();
            }
        }

        public void Enqueue(T data) {
            if (data == null) throw new ArgumentNullException("data");
            lock (_queue) {
                _queue.Enqueue(data);
                _count++;
                Monitor.Pulse(_queue);
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            while (true) yield return Dequeue();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable<T>)this).GetEnumerator();
        }

        public int NumItems() {
            return _count;
        }
    }
}
