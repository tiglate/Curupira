using System;

namespace Curupira.Plugins.Common
{
    internal class PluginAsyncResult : AsyncResultBase
    {
        private bool _result;
        private Exception _exception;

        public PluginAsyncResult(bool result, object state) : base(state)
        {
            _result = result;
        }

        public bool Result
        {
            get
            {
                AsyncWaitHandle.WaitOne();
                return _result;
            }
            set
            {
                _result = value;
                SetCompleted();
            }
        }

        public Exception Exception
        {
            get
            {
                AsyncWaitHandle.WaitOne(); // Ensure the operation is complete before accessing the exception
                return _exception;
            }
            internal set
            {
                _exception = value;
            }
        }

        public void Complete(bool result)
        {
            Result = result;
        }

        public void Complete(Exception exception)
        {
            _exception = exception;
            Result = false; // Indicate failure if an exception occurred
        }
    }
}
