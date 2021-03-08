using Microservices;
using Mocks;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Client
{
    public class ConsoleExecutor : IExecutor
    {
        private readonly IExecutor actualExecutor;
        private readonly Logger logger;
        public ConsoleExecutor(IExecutor actualExecutor, Logger logger)
        {
            this.actualExecutor = actualExecutor;
            this.logger = logger;
        }
        public async Task Execute(int requestId, string name, string key, params ExecutionTask[] tasks)
        {
            logger.Log(requestId, $"{name} BEGIN");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                await actualExecutor.Execute(requestId, name, key, tasks);
                stopwatch.Stop();
                logger.Log(requestId, $"{name} COMPLETED in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.Log(requestId, $"{name} FAILED in {stopwatch.ElapsedMilliseconds} ms ({ex.GetType().Name})");
            }
        }
    }
}
