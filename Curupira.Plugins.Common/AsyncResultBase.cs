using System;
using System.Threading;

namespace Curupira.Plugins.Common
{
    public abstract class AsyncResultBase : IAsyncResult
    {
        private readonly ManualResetEvent _waitHandle = new ManualResetEvent(false);
        private bool _isCompleted;

        public object AsyncState { get; }
        public WaitHandle AsyncWaitHandle => _waitHandle;
        public bool CompletedSynchronously => false;
        public bool IsCompleted => _isCompleted;

        protected AsyncResultBase(object state)
        {
            AsyncState = state;
        }

        protected void SetCompleted()
        {
            _isCompleted = true;
            _waitHandle.Set();
        }

        public void Dispose()
        {
            _waitHandle.Close();
        }
    }
}