using Client;
using Microservices;
using Mocks;
using System;

namespace Runner
{
    class Program
    {
        static void Main()
        {
            //static void requests(ClientGateway g) => g.RunRandom();
            static void requests(ClientGateway g) => g.RunConcurrentWithdrawals();
            //static void requests(ClientGateway g) => g.RunValidWithdrawal();
            //static void requests(ClientGateway g) => g.RunInvalidWithdrawal();

            RunScenario(requests, new PlainParallelExecutor());
            RunScenario(requests, new RedisDistributedLockExecutor());
            RunScenario(requests, new ParallelWithCompensationExecutor());
            RunScenario(requests, new ParallelWithCompensationExecutor(), SimulateLocalDBDown);
            
            Console.WriteLine("END");
        }

        static void RunScenario(Action<ClientGateway> runner, IExecutor executor, 
            Func<int, string, bool, bool> isLocalDBDown = null)
        {
            Console.WriteLine("");
            Console.WriteLine($"----------- {executor.GetType().Name} {isLocalDBDown?.Method.Name} -----------");
            var logger = new Logger();
            var consoleExecutor = new ConsoleExecutor(executor, logger);
            
            var localDB = new LocalDB(logger, isLocalDBDown);
            var externalServices = new ExternalServices(logger);
            var client = new ClientGateway(consoleExecutor, localDB, externalServices);
            runner(client);
            localDB.Reconciliate();
            externalServices.Reconciliate();
            client.Dump();
        }

        static bool SimulateLocalDBDown(int requestId, string account, bool isCompensate)
        {
            return true;
        }
    }
}
