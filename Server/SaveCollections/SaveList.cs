using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace KeyBoardChat.SaveCollections
{
    public class SaveList<T> : ISaveCollection
    {
        private List<T> _list;

        private Mutex _mutex;

        private Thread _currentThread;

        private int Count
        {
            get
            {
                return _list.Count;
            }
        }

        private bool IsReadOnly => false;

        Thread ISaveCollection.CurrentThread { get => _currentThread; set => _currentThread = value; }
        Mutex ISaveCollection.Mutex { get => _mutex; set => _mutex = value; }

        public T2 Open<T2>(Func<SaveListInterface, T2> func)
        {
            return (this as ISaveCollection).Open<T2,SaveListInterface, SaveList<T>>(func);
        }

        public void Open(Action<SaveListInterface> action)
        {
           (this as ISaveCollection).Open<SaveListInterface, SaveList<T>>(action);
        }

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
            (this as ISaveCollection).Init();
        }

        public SaveList()
        {
            _list = new List<T>();
            (this as ISaveCollection).Init();
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

        ISaveInterface<T4> ISaveCollection.CreateInterface<T4>()
        {
            return (ISaveInterface<T4>) new SaveListInterface(this);
        }

        public class SaveListInterface : ISaveInterface<SaveList<T>>, IEnumerable<T>
        {
            private SaveList<T> _saveList;
            internal SaveListInterface(SaveList<T> saveList)
            {
                _saveList = saveList;
            }

            public int Count
            {
                get
                {
                    return _saveList.Count;
                }
            }

            public bool IsReadOnly => _saveList.IsReadOnly;
            SaveList<T> ISaveInterface<SaveList<T>>.SaveCollection { get => _saveList; set => _saveList = value; }

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
