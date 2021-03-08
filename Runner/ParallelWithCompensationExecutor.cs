using Microservices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    public class ParallelWithCompensationExecutor : IExecutor
    {
        public async Task Execute(int requestId, string name, string key, params ExecutionTask[] tasks)
        {
            var completed = new List<ExecutionTask>();
            var failed = new List<ExecutionTask>();
            try
            {
                var invocations = tasks.Select(async task => 
                {
                    try
                    {
                        await task.ActionTask();
                        completed.Add(task);
                    }
                    catch (Exception ex)
                    {
                        failed.Add(task);
                        throw ex;
                    }
                });
                await Task.WhenAll(invocations);
            }
            finally
            {
                if (failed.Any())
                {
                    var compensations = completed
                        .Select(task => task.CompensationTask)
                        .Where(t => t != null)
                        .Select(t => t());
                    await Task.WhenAll(compensations);
                }
            }
        }
    }
}
