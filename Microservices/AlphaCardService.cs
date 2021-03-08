using Mocks;
using System.Threading.Tasks;

namespace Microservices
{
    public class AlphaCardService
    {
        private readonly ExternalServices externalServices;
        private readonly LocalDB localDB;
        private readonly IExecutor executor;

        public AlphaCardService(IExecutor executor, ExternalServices externalServices, LocalDB localDB)
        {
            this.externalServices = externalServices;
            this.localDB = localDB;
            this.executor = executor;
        }

        public async Task Fill(int requestId, string card, double amount)
        {
            await executor.Execute(
                requestId, "AlphaCardService.Fill", card,
                new ExecutionTask(
                    () => externalServices.FillCard(requestId, card, amount),
                    () => externalServices.PayWithCard(requestId, card, amount)),
                new ExecutionTask(
                    () => localDB.ProcessRequest(requestId, card, amount, false),
                    () => localDB.ProcessRequest(requestId, card, -amount, true))
                );
        }

        public async Task Pay(int requestId, string card, double amount)
        {
            await executor.Execute(
                requestId, "AlphaCardService.Pay", card,
                new ExecutionTask(
                    () => externalServices.PayWithCard(requestId, card, amount),
                    () => externalServices.FillCard(requestId, card, amount)),
                new ExecutionTask(
                    () => localDB.ProcessRequest(requestId, card, -amount, false),
                    () => localDB.ProcessRequest(requestId, card, amount, true))
                );
        }
    }
}
