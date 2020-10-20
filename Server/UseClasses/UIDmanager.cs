using System.Collections.Generic;

namespace Keyboardchat.UseClasses
{
    class UIDmanager
    {
        protected List<int> _uids;

        protected Stack<int> _freeUids;

        public UIDmanager()
        {
            _uids = new List<int>();
            _freeUids = new Stack<int>();     
        }

        public virtual int GetUID()
        {
            int UID;

            if (_freeUids.Count > 0)
                UID = _freeUids.Pop();
            else
            {
                UID = _uids.Count;
            }
            _uids.Add(UID);

            return UID;
        }

        public virtual bool HasUID(int UID)
        {
            return _uids.Contains(UID);
        }

        public virtual void ReleaseUID(int UID)
        {
            if (_uids.Remove(UID))
                _freeUids.Push(UID);
        }

    }
}
