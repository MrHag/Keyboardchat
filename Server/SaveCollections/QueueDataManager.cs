using System.Collections.Generic;

namespace KeyBoardChat.SaveCollections
{
    public class QueueDataManager<T>
    {
        protected Queue<T> _data;

        public QueueDataManager()
        {
            _data = new Queue<T>();
        }

        public int Count
        {
            get
            {
                int output;

                output = _data.Count;

                return output;
            }
        }

        public virtual T GetElem()
        {
            T output;

            output = _data.Dequeue();

            return output;
        }

        public virtual T PeekElem()
        {
            T output;

            output = _data.Peek();

            return output;
        }

        public virtual void PushElem(T elem)
        {
            _data.Enqueue(elem);
        }

    }
}
