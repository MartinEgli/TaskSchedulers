using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace TaskSchedulers.TestConsole
{
    internal class Program
    {
        private static async Task Main()
        {
            // ClassicMain();
            // TaskMain();
            // LimitedConcurrencyMain();
            // CancellationTokenMain();
            // InnerCancelationTokenMain();
            //await InnerCancelationTokenAsyncMain();

            // RunOnlyNewestTaskMain();
            // RxMain();
        }

        private static void ClassicMain()
        {
            // Use our factory to run a set of tasks.
            var lockObj = new Object();

            for (int taskCounter = 0; taskCounter <= 4; taskCounter++)
            {
                int iteration = taskCounter;
                NewMethod(lockObj, iteration);
            }

            // Wait for the tasks to complete before displaying a completion message.
            Console.WriteLine("\n\nSuccessful completion.");
        }

        private static void TaskMain()
        {
            // Create a scheduler that uses two threads.
            List<Task> tasks = new List<Task>();

            // Create a TaskFactory and pass it our custom scheduler.
            TaskFactory factory = new TaskFactory();

            // Use our factory to run a set of tasks.
            var lockObj = new Object();

            for (int taskCounter = 0; taskCounter <= 4; taskCounter++)
            {
                int iteration = taskCounter;
                Task t = factory.StartNew(() => NewMethod(lockObj, iteration));
                tasks.Add(t);
                Thread.Sleep(1000);
            }

            // Wait for the tasks to complete before displaying a completion message.
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("\n\nSuccessful completion.");
        }

        private static void CancellationTokenMain()
        {
            // Create a scheduler that uses two threads.
            List<Task> tasks = new List<Task>();

            // Create a TaskFactory and pass it our custom scheduler.
            TaskFactory factory = new TaskFactory();

            // Use our factory to run a set of tasks.
            var lockObj = new Object();
            CancellationTokenSource cancellationTokenSource = null;

            for (int taskCounter = 0; taskCounter <= 4; taskCounter++)
            {
                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();
                }
                cancellationTokenSource = new CancellationTokenSource();
                var token = cancellationTokenSource.Token;
                int iteration = taskCounter;
                Task t = factory.StartNew(() => NewMethod(lockObj, iteration), token);
                tasks.Add(t);
                Thread.Sleep(1000);
            }

            // Wait for the tasks to complete before displaying a completion message.
            Task.WaitAll(tasks.ToArray());
            cancellationTokenSource.Dispose();
            Console.WriteLine("\n\nSuccessful completion.");
        }

        private static void InnerCancelationTokenMain()
        {
            // Create a scheduler that uses two threads.
            List<Task> tasks = new List<Task>();

            // Create a TaskFactory and pass it our custom scheduler.
            TaskFactory factory = new TaskFactory();

            // Use our factory to run a set of tasks.
            var lockObj = new Object();
            CancellationTokenSource cancellationTokenSource = null;

            for (int taskCounter = 0; taskCounter <= 4; taskCounter++)
            {
                int iteration = taskCounter;
                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();
                }
                cancellationTokenSource = new CancellationTokenSource();
                var token = cancellationTokenSource.Token;
                Task t = factory.StartNew(() => NewMethod(token, lockObj, iteration), token);
                tasks.Add(t);
                Thread.Sleep(1000);
            }

            // Wait for the tasks to complete before displaying a completion message.
            Task.WaitAll(tasks.Where((t) => !t.IsCompleted).ToArray());
            cancellationTokenSource.Dispose();
            Console.WriteLine("\n\nSuccessful completion.");
        }

        //public static void InnerCancelationTokenAsyncMain()
        //{
        //    InnerCancelationTokenAsync().GetAwaiter().GetResult();
        //}

        private static async Task InnerCancelationTokenAsyncMain()
        {
            // Create a scheduler that uses two threads.
            List<Task> tasks = new List<Task>();

            // Create a TaskFactory and pass it our custom scheduler.
            TaskFactory factory = new TaskFactory();

            // Use our factory to run a set of tasks.
            var lockObj = new Object();
            CancellationTokenSource cancellationTokenSource = null;

            for (int taskCounter = 0; taskCounter <= 4; taskCounter++)
            {
                int iteration = taskCounter;
                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();
                }
                cancellationTokenSource = new CancellationTokenSource();
                var token = cancellationTokenSource.Token;
                var t = await factory.StartNew(async () => await NewMethodAsync(token, lockObj, iteration), token);
                tasks.Add(t);
                await Task.Delay(1000);
            }
            await Task.WhenAll(tasks);
            // Wait for the tasks to complete before displaying a completion message.
            cancellationTokenSource.Dispose();
            Console.WriteLine("\n\nSuccessful completion.");
        }

        private static void LimitedConcurrencyMain()
        {
            // Create a scheduler that uses two threads.
            LimitedConcurrencyLevelTaskScheduler lcancellationTokenSource = new LimitedConcurrencyLevelTaskScheduler(2);
            List<Task> tasks = new List<Task>();

            // Create a TaskFactory and pass it our custom scheduler.
            TaskFactory factory = new TaskFactory(lcancellationTokenSource);

            // Use our factory to run a set of tasks.
            var lockObj = new Object();

            for (int taskCounter = 0; taskCounter <= 4; taskCounter++)
            {
                int iteration = taskCounter;
                Task t = factory.StartNew(() => NewMethod(lockObj, iteration));
                tasks.Add(t);
                Thread.Sleep(1000);
            }

            // Wait for the tasks to complete before displaying a completion message.
            Task.WaitAll(tasks.Where((t) => !t.IsCompleted).ToArray());
            Console.WriteLine("\n\nSuccessful completion.");
        }

        private static void RunOnlyNewestTaskMain()
        {
            // Create a scheduler that uses two threads.
            List<Task> tasks = new List<Task>();

            // Create a TaskFactory and pass it our custom scheduler.
            RunOnlyNewestTask taskRunner = new RunOnlyNewestTask();

            // Use our factory to run a set of tasks.
            var lockObj = new Object();

            for (int taskCounter = 0; taskCounter <= 4; taskCounter++)
            {
                int iteration = taskCounter;
                Task task = taskRunner.RunTask((token) => NewMethod(token, lockObj, iteration));
                tasks.Add(task);
                Thread.Sleep(1000);
            }

            // Wait for the tasks to complete before displaying a completion message.
            Task.WaitAll(tasks.Where((t) => !t.IsCompleted).ToArray());
            taskRunner.Dispose();
            Console.WriteLine("\n\nSuccessful completion.");
        }

        private static void RxMain()
        {
            // Create a scheduler that uses two threads.
            LimitedConcurrencyLevelTaskScheduler lcancellationTokenSource = new LimitedConcurrencyLevelTaskScheduler(2);
            List<Task> tasks = new List<Task>();

            // Create a TaskFactory and pass it our custom scheduler.
            TaskFactory factory = new TaskFactory(lcancellationTokenSource);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            // Use our factory to run a set of tasks.
            var lockObj = new Object();

            Subject<int> subject = new Subject<int>();

            subject.Select(iteration => Observable.FromAsync(async token =>
            {
                var task = factory.StartNew(() => NewMethod(token, lockObj, iteration), token);
                tasks.Add(task);
                return task;
            }))
             .Switch()
             .Subscribe();

            for (int taskCounter = 0; taskCounter <= 4; taskCounter++)
            {
                subject.OnNext(taskCounter);
                Thread.Sleep(1000);
            }

            Task.WaitAll(tasks.Where((t) => !t.IsCompleted).ToArray());
            cancellationTokenSource.Dispose();
            Console.WriteLine("\n\nSuccessful completion.");
        }

        private static async Task RxAsyncMain()
        {
            // Create a scheduler that uses two threads.
            LimitedConcurrencyLevelTaskScheduler lcancellationTokenSource = new LimitedConcurrencyLevelTaskScheduler(2);
            List<Task> tasks = new List<Task>();

            // Create a TaskFactory and pass it our custom scheduler.
            TaskFactory factory = new TaskFactory(lcancellationTokenSource);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            // Use our factory to run a set of tasks.
            var lockObj = new Object();

            var subject = SequentialSimpleAsyncSubject;

            subject.Select(iteration => Observable.FromAsync(async token =>
            {
                var task = factory.StartNew(() => NewMethod(token, lockObj, iteration), token);
                tasks.Add(task);
                return task;
            }))
             .Switch()
             .Subscribe();

            for (int taskCounter = 0; taskCounter <= 4; taskCounter++)
            {
                subject.OnNext(taskCounter);
                Thread.Sleep(1000);
            }

            Task.WaitAll(tasks.Where((t) => !t.IsCompleted).ToArray());
            cancellationTokenSource.Dispose();
            Console.WriteLine("\n\nSuccessful completion.");
        }

        private static async Task NewMethodAsync(CancellationToken token, object lockObj, int iteration)
        {
            try
            {
                lock (lockObj)
                {
                    Console.WriteLine(" -- Start on task {0}, thread {1} --",
                              Task.CurrentId,
                              Thread.CurrentThread.ManagedThreadId);
                }

                if (token.IsCancellationRequested)
                {
                    lock (lockObj)
                    {
                        Console.WriteLine(" -- Canceld befor loop on task {0}, thread {1} --",
                                  Task.CurrentId,
                                  Thread.CurrentThread.ManagedThreadId);
                    }
                    throw new OperationCanceledException(token);
                }

                for (int i = 0; i <= 100; i++)
                {
                    await Task.Delay(100, token);
                    lock (lockObj)
                    {
                        Console.WriteLine("{0} in task t-{1} on task {2}, thread {3}",
                                      i, iteration,
                                      Task.CurrentId,
                                      Thread.CurrentThread.ManagedThreadId);
                    }
                    if (token.IsCancellationRequested)
                    {
                        lock (lockObj)
                        {
                            Console.WriteLine(" -- Canceld {0} in task t-{1} on task {2}, thread {3} --",
                                      i, iteration,
                                      Task.CurrentId,
                                      Thread.CurrentThread.ManagedThreadId);
                        }
                        throw new OperationCanceledException(token);
                    }
                }
            }
            finally
            {
                lock (lockObj)
                {
                    Console.WriteLine(" -- End on task {0}, thread {1} --",
                              Task.CurrentId,
                              Thread.CurrentThread.ManagedThreadId);
                }
            }
        }

        private static void NewMethod(CancellationToken token, object lockObj, int iteration)
        {
            try
            {
                lock (lockObj)
                {
                    Console.WriteLine(" -- Start on task {0}, thread {1} --",
                              Task.CurrentId,
                              Thread.CurrentThread.ManagedThreadId);
                }

                if (token.IsCancellationRequested)
                {
                    lock (lockObj)
                    {
                        Console.WriteLine(" -- Canceld befor loop on task {0}, thread {1} --",
                                  Task.CurrentId,
                                  Thread.CurrentThread.ManagedThreadId);
                    }
                    throw new OperationCanceledException(token);
                }

                for (int i = 0; i <= 100; i++)
                {
                    Thread.Sleep(100);
                    lock (lockObj)
                    {
                        Console.WriteLine("{0} in task t-{1} on task {2}, thread {3}",
                                      i, iteration,
                                      Task.CurrentId,
                                      Thread.CurrentThread.ManagedThreadId);
                    }
                    if (token.IsCancellationRequested)
                    {
                        lock (lockObj)
                        {
                            Console.WriteLine(" -- Canceld {0} in task t-{1} on task {2}, thread {3} --",
                                      i, iteration,
                                      Task.CurrentId,
                                      Thread.CurrentThread.ManagedThreadId);
                        }
                        throw new OperationCanceledException(token);
                    }
                }
            }
            finally
            {
                lock (lockObj)
                {
                    Console.WriteLine(" -- End on task {0}, thread {1} --",
                              Task.CurrentId,
                              Thread.CurrentThread.ManagedThreadId);
                }
            }
        }

        private static void NewMethod(object lockObj, int iteration)
        {
            try
            {
                lock (lockObj)
                {
                    Console.WriteLine(" -- Start on task {0}, thread {1} --",
                              Task.CurrentId,
                              Thread.CurrentThread.ManagedThreadId);
                }

                for (int i = 0; i <= 100; i++)
                {
                    Thread.Sleep(100);
                    lock (lockObj)
                    {
                        Console.WriteLine("{0} in task t-{1} on task {2}, thread {3}",
                                      i, iteration,
                                      Task.CurrentId,
                                      Thread.CurrentThread.ManagedThreadId);
                    }
                }
            }
            finally
            {
                lock (lockObj)
                {
                    Console.WriteLine(" -- End on task {0}, thread {1} --",
                              Task.CurrentId,
                              Thread.CurrentThread.ManagedThreadId);
                }
            }
        }
    }
}