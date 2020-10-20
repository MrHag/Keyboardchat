using Keyboardchat.UseClasses;
using System.Threading;

namespace Keyboardchat.SaveObjects
{
    class SemaphoreUIDmanager : UIDmanager
    {
        protected Semaphore _semaphore;
        public SemaphoreUIDmanager() : base()
        {
            _semaphore = new Semaphore(1, 1);     
        }

        public override int GetUID()
        {
            int output;

            _semaphore.WaitOne();
            output = base.GetUID();
            _semaphore.Release();

            return output;
        }

        public override bool HasUID(int UID)
        {
            bool output;

            _semaphore.WaitOne();
            output = base.HasUID(UID);
            _semaphore.Release();

            return output;
        }

        public override void ReleaseUID(int UID)
        {
            _semaphore.WaitOne();
            base.ReleaseUID(UID);
            _semaphore.Release();
        }

    }
}
