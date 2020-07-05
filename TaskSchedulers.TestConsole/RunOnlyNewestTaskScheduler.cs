//--------------------------------------------------------------------------
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  File: LimitedConcurrencyTaskScheduler.cs
//  Source: https://code.msdn.microsoft.com/vstudio/Execution-Time-Based-dd4dbdb6/sourcecode?fileId=67470&pathId=1378361369
//  Modified for .net core
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TaskSchedulers.TestConsole
{
    /// <summary>
    /// Provides a task scheduler that ensures a maximum concurrency level while
    /// running on top of the ThreadPool.
    /// </summary>
    public class RunOnlyNewestTask : IDisposable
    {
        private Task _task;
        private CancellationTokenSource _cancelationSource;
        private CancellationToken _tocken;

        /// <summary>Queues a task to the scheduler.</summary>
        /// <param name="task">The task to be queued.</param>
        public Task RunTask(Action<CancellationToken> action)
        {
            // Add the task to the list of tasks to be processed.  If there aren't enough
            // delegates currently queued or running to process tasks, schedule another.
            lock (_lock)
            {
                if (_task != null && !_task.IsCanceled)
                {
                    _cancelationSource.Cancel();
                }
                _cancelationSource = new CancellationTokenSource();
                var tocken = _cancelationSource.Token;
                _tocken = tocken;
                _task = new TaskFactory().StartNew(() => action(tocken), tocken);
                return _task;
            }
        }

        private readonly object _lock = new object();

        #region IDisposable Support

        private bool disposedValue = false;

        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _cancelationSource.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}