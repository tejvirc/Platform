namespace Aristocrat.Monaco.Common
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A fixed size buffer that automatically drops old messages as it breaches its max limit.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FixedSizedBuffer<T>
    {
        Queue<T> q = new Queue<T>();
        private object lockObject = new object();

        /// <summary>Max no of items this queue can store</summary>
        public int Limit { get; private set; }

        /// <summary>
        /// All items in the buffer
        /// </summary>
        public T[] Items { get { return q.ToArray(); } }

        /// <summary>
        /// Item at index
        /// </summary>
        /// <param name="index">index of item in buffer</param>
        /// <returns></returns>
        public T this[int index]
        {
            get { return q.ToArray()[index]; }
        }

        /// <summary>
        /// Constructor...
        /// </summary>
        /// <param name="limit"></param>
        public FixedSizedBuffer(int limit)
        {
            Limit = limit;
        }

        /// <summary>
        /// Add a new entry to the start of queue and remove the oldest entry if limit is reached.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            lock (lockObject)
            {
                q.Enqueue(item);
                while (q.Count > Limit)
                    q.Dequeue();
            }
        }

        /// <summary>
        /// Add multiple entries to the queue.
        /// </summary>
        /// <param name="items"></param>
        public void Add(IEnumerable<T> items)
        {
            lock (lockObject)
            {
                foreach (var item in items) q.Enqueue(item);
                while (q.Count > Limit)
                    q.Dequeue();
            }
        }

        /// <summary>
        /// Returns the last added item.
        /// </summary>
        /// <returns></returns>
        public T LastItem()
        {
            lock (lockObject)
            {
                if (q.Count == 0)
                    return default(T);

                return q.ToList()[q.Count - 1];
            }
        }
    }
}
