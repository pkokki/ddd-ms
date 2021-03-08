using System;
using System.Threading.Tasks;

namespace Microservices
{
    public interface IExecutor
    {
        Task Execute(int requestId, string name, string key, params ExecutionTask[] tasks);
    }

    public class ExecutionTask
    {
        public ExecutionTask(Func<Task> actionTask)
        {
            ActionTask = actionTask;
        }
        public ExecutionTask(Func<Task> actionTask, Func<Task> compensationTask) : this(actionTask)
        {
            CompensationTask = compensationTask;
        }
        public Func<Task> ActionTask { get; }
        public Func<Task> CompensationTask { get; }
    }
}
