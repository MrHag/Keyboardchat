using System;

namespace KeyBoardChat.SaveCollections
{
    public interface ISaveObject
    {
        abstract private protected ISaveCollection SaveCollection { get; set; }

        public T2 Open<T2, T4, T5>(Func<T4, T2> func) where T4 : ISaveInterface<T5> where T5 : ISaveCollection
        {
            return SaveCollection.Open<T2, T4, T5>(func);
        }

        public void Open<T4, T5>(Action<T4> action) where T4 : ISaveInterface<T5> where T5 : ISaveCollection
        {
            SaveCollection.Open<T4, T5>(action);
        }

    }
}
