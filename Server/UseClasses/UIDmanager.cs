using System.Collections.Generic;

namespace Keyboardchat.UseClasses
{
    class UIDmanager
    {
        protected SortedSet<int> _uids;

        protected Stack<int> _freeUids;

        public UIDmanager()
        {
            _uids = new SortedSet<int>();
            _freeUids = new Stack<int>();     
        }

        public virtual int GetUID()
        {
            int UID;

            lock (this)
            {
                if (_freeUids.Count > 0)
                    UID = _freeUids.Pop();
                else
                {
                    UID = _uids.Count;
                }
                _uids.Add(UID);
            }
            return UID;
        }

        public virtual bool HasUID(int UID)
        {
            lock (this)
            {
                return _uids.Contains(UID);
            }
        }

        public virtual void ReleaseUID(int UID)
        {
            lock (this)
            {
                if (_uids.Remove(UID))
                    _freeUids.Push(UID);
            }
        }

    }
}
