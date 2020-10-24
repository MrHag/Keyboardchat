using System;
using System.Collections.Generic;
using System.Threading;

namespace Keyboardchat.SaveCollections
{
    public interface ISaveCollection
    {
        abstract private protected Thread CurrentThread { get; set; }
        abstract private protected Mutex Mutex { get; set; }
        abstract private protected ISaveInterface<T5> CreateInterface<T5>() where T5 : ISaveCollection;

        protected internal void Init()
        {
            Mutex = new Mutex();
            CurrentThread = null;
        }

        public T2 Open<T2, T4, T5>(Func<T4, T2> func) where T4 : ISaveInterface<T5> where T5 : ISaveCollection
        {
            T2 result;

            bool threadEqual = CurrentThread == Thread.CurrentThread;

            if (!threadEqual)
            {
                Mutex.WaitOne();
                CurrentThread = Thread.CurrentThread;
            }

            try
            {
                T4 Interface = (T4)CreateInterface<T5>();
                result = func.Invoke(Interface);
            }
            finally
            {   
                if (!threadEqual)
                {
                    CurrentThread = null;
                    Mutex.ReleaseMutex();
                }
            }         

            return result;
        }
        public void Open<T4, T5>(Action<T4> action) where T4 : ISaveInterface<T5> where T5 : ISaveCollection
        {
            Open<bool,T4,T5>((T4 Interface) => { action.Invoke(Interface); return true; });
        }
    }
}
