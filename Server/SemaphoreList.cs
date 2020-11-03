using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Semafored
{
    class SemaphoreList<T> : IList<T>
    {

        private List<T> _list;

        private Semaphore _semaphore;

        public int Count
        {
            get
            {
                int output;

                _semaphore.WaitOne();
                output = _list.Count;
                _semaphore.Release();

                return output;
            }
        }

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get
            {
                T output;

                _semaphore.WaitOne();
                try
                {
                    output = _list[index];
                    _semaphore.Release();
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    _semaphore.Release();
                    throw new Exception(ex.Message, ex);
                }

                return output;
            }
            set
            {
                _semaphore.WaitOne();
                try
                {
                    _list[index] = value;
                    _semaphore.Release();
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    _semaphore.Release();
                    throw new Exception(ex.Message, ex);
                }
            }
        }

        public SemaphoreList(IEnumerable<T> Collection)
        {
            _list = new List<T>(Collection);
            Init();
        }

        public SemaphoreList()
        {
            _list = new List<T>();
            Init();
        }

        private void Init()
        {
            _semaphore = new Semaphore(1, 1);
        }

        public void Add(T obj)
        {
            _semaphore.WaitOne();
            _list.Add(obj);
            _semaphore.Release();
        }

        public bool Remove(T obj)
        {
            bool output;
            _semaphore.WaitOne();
            output = _list.Remove(obj);
            _semaphore.Release();
            return output;
        }

        public int IndexOf(T item)
        {
            int output;
            _semaphore.WaitOne();
            output = _list.IndexOf(item);
            _semaphore.Release();
            return output;
        }

        public void Insert(int index, T item)
        {
            _semaphore.WaitOne();
            try
            {
                _list.Insert(index, item);
                _semaphore.Release();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _semaphore.Release();
                throw new Exception(ex.Message, ex);
            }
        }

        public void RemoveAt(int index)
        {
            _semaphore.WaitOne();
            try
            {
                _list.RemoveAt(index);
                _semaphore.Release();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _semaphore.Release();
                throw new Exception(ex.Message, ex);
            }
        }

        public void Clear()
        {
            _semaphore.WaitOne();
            _list.Clear();
            _semaphore.Release();
        }

        public bool Contains(T item)
        {
            bool output;

            _semaphore.WaitOne();
            output = _list.Contains(item);
            _semaphore.Release();

            return output;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _semaphore.WaitOne();
            try
            {
                _list.CopyTo(array, arrayIndex);
                _semaphore.Release();
            }
            catch (Exception ex)
            {
                _semaphore.Release();
                throw ex;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            IEnumerator<T> output;

            _semaphore.WaitOne();
            output = new List<T>(_list).GetEnumerator();
            _semaphore.Release();

            return output;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}
