using Mocks;
using System.Threading.Tasks;

namespace Microservices
{
    public class CurrentAccountService
    {
        private readonly ExternalServices externalServices;
        private readonly LocalDB localDB;
        private readonly IExecutor executor;

        public CurrentAccountService(IExecutor executor, ExternalServices externalServices, LocalDB localDB)
        {
            this.externalServices = externalServices;
            this.localDB = localDB;
            this.executor = executor;
        }

        public async Task Deposit(int requestId, string account, double amount)
        {
            await executor.Execute(
                requestId, "CurrentAccountService.Deposit", account,
                new ExecutionTask(
                    () => externalServices.Deposit(requestId, account, amount),
                    () => externalServices.Withdraw(requestId, account, amount)
                    ),
                new ExecutionTask(
                    () => localDB.ProcessRequest(requestId, account, amount, false),
                    () => localDB.ProcessRequest(requestId, account, -amount, true))
                );
        }

        public async Task Withdraw(int requestId, string account, double amount)
        {
            await executor.Execute(
                requestId, "CurrentAccountService.Withdraw", account,
                new ExecutionTask(
                    () => externalServices.Withdraw(requestId, account, amount),
                    () => externalServices.Deposit(requestId, account, amount)),
                new ExecutionTask(
                    () => localDB.ProcessRequest(requestId, account, -amount, false),
                    () => localDB.ProcessRequest(requestId, account, amount, true))
                );
        }
    }
}
