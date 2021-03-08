using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Mocks
{
    public class Logger
    {
        private readonly long offset;
        private readonly object theLock = new object();

        public Logger()
        {
            offset = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        public void Log(int requestId, string message)
        {
            lock (theLock)
            {
                Console.ForegroundColor = (ConsoleColor)((requestId % 6) + 10);
                Console.WriteLine($"R{requestId,-4} T{Thread.CurrentThread.ManagedThreadId,-3} {DateTimeOffset.Now.ToUnixTimeMilliseconds() - offset,5} {message}");
                Console.ResetColor();
            }
        }

        public async Task TimeAndLogTask(int requestId, string name, params Task[] tasks)
        {
            Log(requestId, $"{name} BEGIN");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                await Task.WhenAll(tasks);
                stopwatch.Stop();
                Log(requestId, $"{name} COMPLETED in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Log(requestId, $"{name} FAILED in {stopwatch.ElapsedMilliseconds} ms ({ex.GetType().Name})");
            }
        }
    }
}
