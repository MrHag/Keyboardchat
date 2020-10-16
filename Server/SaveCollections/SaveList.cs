using Keyboardchat.SaveCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Keyboardchat.SaveCollections
{
    public class SaveList<T>
    {
        private SaveListInterface _currentInterface;

        private List<T> _list;

        private Semaphore _semaphore;

        public SaveListInterface EnterInQueue()
        {
            _semaphore.WaitOne();
            var Interface = new SaveListInterface(this);
            _currentInterface = Interface;
            return Interface;
        }

        public void ExitFromQueue(SaveListInterface Interface)
        {
            if (_currentInterface == Interface)
            {
                _semaphore.Release();
            }
            else
                throw new Exception("Cant exit from queue");
        }

        private int Count
        {
            get
            {
                return _list.Count;
            }
        }

        private bool IsReadOnly => false;

        private T this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                _list[index] = value;
            }
        }

        public SaveList(IEnumerable<T> Collection)
        {
            _list = new List<T>(Collection);
            Init();
        }

        public SaveList()
        {
            _list = new List<T>();
            Init();
        }

        private void Init()
        {
            _semaphore = new Semaphore(1, 1);
        }

        private void Add(T obj)
        {
            _list.Add(obj);
        }

        private bool Remove(T obj)
        {
            return _list.Remove(obj);
        }

        private int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        private void Insert(int index, T item)
        {
            _list.Insert(index, item);
        }

        private void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        private void Clear()
        {
            _list.Clear();
        }

        private bool Contains(T item)
        {
            return _list.Contains(item);
        }

        private void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        private IEnumerator<T> GetEnumerator()
        {
            return new List<T>(_list).GetEnumerator();
        }


        public class SaveListInterface : IEnumerable<T>
        {
            private SaveList<T> _saveList;
            internal SaveListInterface(SaveList<T> saveList)
            {
                _saveList = saveList;
            }

            public void ExitFromQueue()
            {
                _saveList.ExitFromQueue(this);
            }

            public int Count
            {
                get
                {
                    return _saveList.Count;
                }
            }

            public bool IsReadOnly => _saveList.IsReadOnly;

            public T this[int index]
            {
                get
                {
                    return _saveList[index];
                }
                set
                {
                    _saveList[index] = value;
                }
            }
            public void Add(T obj)
            {
                _saveList.Add(obj);
            }

            public bool Remove(T obj)
            {
                return _saveList.Remove(obj);
            }

            public int IndexOf(T item)
            {
                return _saveList.IndexOf(item);
            }

            public void Insert(int index, T item)
            {
                _saveList.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                _saveList.RemoveAt(index);
            }

            public void Clear()
            {
                _saveList.Clear();
            }

            public bool Contains(T item)
            {
                return _saveList.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                _saveList.CopyTo(array, arrayIndex);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _saveList.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

        }

    }

   
}

