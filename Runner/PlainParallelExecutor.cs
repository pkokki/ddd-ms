using Microservices;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Runner
{
    public class PlainParallelExecutor : IExecutor
    {
        public async Task Execute(int requestId, string name, string key, params ExecutionTask[] tasks)
        {
            var invocations = tasks.Select(t => t.ActionTask());
            await Task.WhenAll(invocations);
        }
    }
}
