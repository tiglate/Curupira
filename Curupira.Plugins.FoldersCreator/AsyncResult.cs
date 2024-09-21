using System;
using System.Threading;

namespace Curupira.Plugins.FoldersCreator
{
    internal class AsyncResult : IAsyncResult
    {
        private readonly ManualResetEvent _waitHandle = new ManualResetEvent(false);
        private bool _isCompleted;
        private bool _result;

        public AsyncResult(bool result)
        {
            _result = result;
        }

        public object AsyncState => null;
        public WaitHandle AsyncWaitHandle => _waitHandle;
        public bool CompletedSynchronously => false;
        public bool IsCompleted => _isCompleted;

        public bool Result
        {
            get
            {
                _waitHandle.WaitOne(); // Wait if not completed
                return _result;
            }
            set
            {
                _result = value;
                _isCompleted = true;
                _waitHandle.Set(); // Signal completion
            }
        }
    }
}
