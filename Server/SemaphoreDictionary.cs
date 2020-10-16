using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;

namespace Semafored
{
    class SemaphoreDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {

        private Dictionary<TKey, TValue> _dictionary;

        private Semaphore _semaphore;


        public SemaphoreDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
            Init();
        }

        public SemaphoreDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            _dictionary = new Dictionary<TKey, TValue>(collection);
            Init();
        }

        public SemaphoreDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _dictionary = new Dictionary<TKey, TValue>(dictionary);
            Init();
        }

        private void Init()
        {
            _semaphore = new Semaphore(1, 1);   
        }

        public TValue this[TKey key]
        {

            get
            {
                TValue output;

                _semaphore.WaitOne();
                try
                {
                    output = _dictionary[key];
                    _semaphore.Release();
                }
                catch (Exception ex)
                {
                    _semaphore.Release();
                    throw ex;
                }

                return output;
            }
            set
            {
                _semaphore.WaitOne();
                try
                {
                    _dictionary[key] = value;
                    _semaphore.Release();
                }
                catch (Exception ex)
                {
                    _semaphore.Release();
                    throw ex;
                }
            }

        }

        public ICollection<TKey> Keys 
        {
            get
            {
                ICollection<TKey> output;

                _semaphore.WaitOne();
                output = new List<TKey>(_dictionary.Keys);
                _semaphore.Release();

                return output;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                ICollection<TValue> output;

                _semaphore.WaitOne();
                output = new List<TValue>(_dictionary.Values);
                _semaphore.Release();

                return output;
            }
        }

        public int Count
        {
            get
            {
                int output;

                _semaphore.WaitOne();
                output = _dictionary.Count;
                _semaphore.Release();

                return output;
            }
        }

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            _semaphore.WaitOne();
            try
            {
                _dictionary.Add(key, value);
                _semaphore.Release();
            }
            catch (Exception ex)
            {
                _semaphore.Release();
                throw ex;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _semaphore.WaitOne();
            _dictionary.Clear();
            _semaphore.Release();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            bool output;

            _semaphore.WaitOne();
            try
            {
                output = ((IDictionary<TKey, TValue>)_dictionary).Contains(item);
                _semaphore.Release();
            }
            catch (Exception ex)
            {
                _semaphore.Release();
                throw ex;
            }

            return output;
        }

        public bool ContainsKey(TKey key)
        {
            bool output;

            _semaphore.WaitOne();
            try
            {
                output = _dictionary.ContainsKey(key);
                _semaphore.Release();
            }
            catch (Exception ex)
            {
                _semaphore.Release();
                throw ex;
            }

            return output;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {

            _semaphore.WaitOne();
            try
            {
                ((IDictionary<TKey, TValue>)_dictionary).CopyTo(array, arrayIndex);
                _semaphore.Release();
            }
            catch (Exception ex)
            {
                _semaphore.Release();
                throw ex;
            }

        }

        public bool Remove(TKey key)
        {
            bool output;

            _semaphore.WaitOne();
            try
            {
                 output = _dictionary.Remove(key);
                _semaphore.Release();
            }
            catch (Exception ex)
            {
                _semaphore.Release();
                throw ex;
            }

            return output;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            bool output;

            _semaphore.WaitOne();
            try
            {
                output = ((IDictionary<TKey, TValue>)_dictionary).Remove(item);
                _semaphore.Release();
            }
            catch (Exception ex)
            {
                _semaphore.Release();
                throw ex;
            }

            return output;
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            bool output;

            _semaphore.WaitOne();
            try
            {
                output = _dictionary.TryGetValue(key, out value);
                _semaphore.Release();
            }
            catch (Exception ex)
            {
                _semaphore.Release();
                throw ex;
            }

            return output;
        }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            IEnumerator<KeyValuePair<TKey, TValue>> output;

            _semaphore.WaitOne();
            output = _dictionary.GetEnumerator();
            _semaphore.Release();

            return output;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
