using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Keyboardchat.SaveCollections
{
    public class QueueDataManager<T>
    {
        protected Queue<T> _data;

        private Semaphore _semaphore;

        public QueueDataManager()
        {
            _data = new Queue<T>();
            _semaphore = new Semaphore(1, 1);
        }

        public int Count { 
            get 
            {
                int output;

                _semaphore.WaitOne();
                output = _data.Count;
                _semaphore.Release();

                return output;
            } 
        }

        public virtual T GetElem()
        {
            T output;

            _semaphore.WaitOne();
            output = _data.Dequeue();
            _semaphore.Release();

            return output;
        }

        public virtual T PeekElem()
        {
            T output;

            _semaphore.WaitOne();
            output = _data.Peek();
            _semaphore.Release();

            return output;
        }

        public virtual void PushElem(T elem)
        {
            _semaphore.WaitOne();
            _data.Enqueue(elem);
            _semaphore.Release();
        }

    }
}
