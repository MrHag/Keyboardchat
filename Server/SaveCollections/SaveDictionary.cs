using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Keyboardchat.SaveCollections
{
    public class SaveDictionary<TKey, TValue> : ISaveCollection
    {
        private Dictionary<TKey, TValue> _dictionary;

        private Mutex _mutex;

        private Thread _currentThread;

        Thread ISaveCollection.CurrentThread { get => _currentThread; set => _currentThread = value; }
        Mutex ISaveCollection.Mutex { get => _mutex; set => _mutex = value; }

        public SaveDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
            ((ISaveCollection)this).Init();
        }

        public SaveDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            _dictionary = new Dictionary<TKey, TValue>(collection);
            ((ISaveCollection)this).Init();
        }

        public SaveDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _dictionary = new Dictionary<TKey, TValue>(dictionary);
            ((ISaveCollection)this).Init();
        }

        public T2 Open<T2>(Func<SaveDictionaryInterface, T2> func)
        {
            return (this as ISaveCollection).Open<T2, SaveDictionaryInterface, SaveDictionary<TKey, TValue>>(func);
        }

        public void Open(Action<SaveDictionaryInterface> action)
        {
            (this as ISaveCollection).Open<SaveDictionaryInterface, SaveDictionary<TKey, TValue>>(action);
        }

        private TValue this[TKey key]
        {

            get
            {
                TValue output;
                try
                {
                    output = _dictionary[key];
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                return output;
            }
            set
            {
               _dictionary[key] = value;
            }

        }

        private ICollection<TKey> Keys 
        {
            get
            {
                ICollection<TKey> output;

                output = new List<TKey>(_dictionary.Keys);

                return output;
            }
        }

        private ICollection<TValue> Values
        {
            get
            {
                ICollection<TValue> output;

                output = new List<TValue>(_dictionary.Values);

                return output;
            }
        }

        private int Count
        {
            get
            {
                int output;

                output = _dictionary.Count;

                return output;
            }
        }

        private bool IsReadOnly => false;

        private void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
        }

        private void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        private void Clear()
        {
            _dictionary.Clear();
        }

        private bool Contains(KeyValuePair<TKey, TValue> item)
        {
            bool output;

            output = ((IDictionary<TKey, TValue>)_dictionary).Contains(item);

            return output;
        }

        private bool ContainsKey(TKey key)
        {
            bool output;

            output = _dictionary.ContainsKey(key);

            return output;
        }

        private void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary<TKey, TValue>)_dictionary).CopyTo(array, arrayIndex);
        }

        private bool Remove(TKey key)
        {
            bool output;
            output = _dictionary.Remove(key);

            return output;
        }

        private bool Remove(KeyValuePair<TKey, TValue> item)
        {
            bool output;
            output = ((IDictionary<TKey, TValue>)_dictionary).Remove(item);

            return output;
        }

        private bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            bool output;
            output = _dictionary.TryGetValue(key, out value);

            return output;
        }

        private IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            IEnumerator<KeyValuePair<TKey, TValue>> output;
            output = _dictionary.GetEnumerator();

            return output;
        }

        ISaveInterface<T4> ISaveCollection.CreateInterface<T4>()
        {
            return  (ISaveInterface<T4>) new SaveDictionaryInterface(this);
        }

        public class SaveDictionaryInterface : ISaveInterface<SaveDictionary<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>
        {
            private SaveDictionary<TKey,TValue> _saveDictionary;

            SaveDictionary<TKey, TValue> ISaveInterface<SaveDictionary<TKey, TValue>>.SaveCollection { get => _saveDictionary; set => _saveDictionary = value; }

            internal SaveDictionaryInterface(SaveDictionary<TKey, TValue> _dictionary) 
            {
                _saveDictionary = _dictionary;
            }

            public TValue this[TKey key]
            {

                get
                {
                    return _saveDictionary[key];
                }
                set
                {
                    _saveDictionary[key] = value;
                }

            }

            public ICollection<TKey> Keys
            {
                get
                {
                    return _saveDictionary.Keys;
                }
            }

            public ICollection<TValue> Values
            {
                get
                {
                    return _saveDictionary.Values;
                }
            }

            public int Count
            {
                get
                {
                    return _saveDictionary.Count;
                }
            }

            public bool IsReadOnly => false;

            public void Add(TKey key, TValue value)
            {
                _saveDictionary.Add(key, value);
            }

            public void Add(KeyValuePair<TKey, TValue> item)
            {
                Add(item.Key, item.Value);
            }

            public void Clear()
            {
                _saveDictionary.Clear();
            }

            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                return _saveDictionary.Contains(item);
            }

            public bool ContainsKey(TKey key)
            {
                return _saveDictionary.ContainsKey(key);
            }

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                _saveDictionary.CopyTo(array, arrayIndex);
            }

            public bool Remove(TKey key)
            {
                return _saveDictionary.Remove(key);
            }

            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                return _saveDictionary.Remove(item);
            }

            public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
            {
                return _saveDictionary.TryGetValue(key, out value);
            }
            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return _saveDictionary.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

        }

    }
}
