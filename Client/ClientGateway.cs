using Microservices;
using Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class ClientGateway
    {
        private int requestId = 0;

        private readonly LocalDB localDB;
        private readonly ExternalServices externalServices;
        private readonly IExecutor executor;

        public ClientGateway(IExecutor executor, LocalDB localDB, ExternalServices externalServices)
        {
            this.localDB = localDB;
            this.externalServices = externalServices;
            this.executor = executor;
        }

        public void RunRandom(int num = 5)
        {
            var asyncRequests = new List<Task>();
            foreach (var _ in Enumerable.Range(1, num))
            {
                asyncRequests.Add(RandomRequest());
            }

            Task.WaitAll(asyncRequests.ToArray());
        }
        public void RunConcurrentWithdrawals()
        {
            var asyncRequests = new Task[]
            {
                Withdraw("A1", 500),
                Withdraw("A1", 400),
                Withdraw("A1", 300),
                Pay("C1", 10),
                Pay("C2", 20),
            };
            Task.WaitAll(asyncRequests);
        }
        public void RunValidWithdrawal()
        {
            var asyncRequests = new Task[]
            {
                Withdraw("A1", 500),
            };
            Task.WaitAll(asyncRequests);
        }
        public void RunInvalidWithdrawal()
        {
            var asyncRequests = new Task[]
            {
                Withdraw("A2", 1500),
            };
            Task.WaitAll(asyncRequests);
        }

        public void Dump()
        {
            Console.WriteLine();
            var list1 = externalServices.GetState();
            var list2 = localDB.GetState();
            var keys = list1.Select(o => o.Key).Concat(list2.Select(o => o.Key)).Distinct();
            foreach (var key in keys)
            {
                var values1 = list1.FirstOrDefault(o => o.Key == key).Value ?? Enumerable.Empty<double>();
                var values2 = list2.FirstOrDefault(o => o.Key == key).Value ?? Enumerable.Empty<double>();
                
                if (values1.Sum() == values2.Sum())
                    Console.BackgroundColor = ConsoleColor.Green;
                else
                    Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write($" {key} ");
                Console.ResetColor();

                var list = values1.Intersect(values2);
                Console.ForegroundColor = ConsoleColor.Green;
                if (list.Any()) Console.Write($" {string.Join(" ", list)}");
                Console.ForegroundColor = ConsoleColor.Red;
                list = values1.Except(values2);
                if (list.Any()) Console.Write($" EXT ONLY: {string.Join(" ", list)}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                list = values2.Except(values1);
                if (list.Any()) Console.Write($" LOC ONLY: {string.Join(" ", list)}");
                Console.ResetColor();
                Console.WriteLine();
            }
        }
        private int GetNextRequestId()
        {
            return Interlocked.Increment(ref requestId);
        }

        public async Task RandomRequest()
        {
            var random = new Random();
            var accounts = localDB.QueryAccounts().ToList();
            var cards = localDB.QueryCards().ToList();

            var number = random.NextDouble();
            if (number < 0.5)
            {
                var service = new CurrentAccountService(executor, externalServices, localDB);
                var account = accounts[random.Next(accounts.Count)];
                if (number < 0.25)
                    await service.Deposit(GetNextRequestId(), account, random.Next(100, 1000));
                else
                    await service.Withdraw(GetNextRequestId(), account, random.Next(100, 1000));
            }
            else
            {
                var service = new AlphaCardService(executor, externalServices, localDB);
                var card = cards[random.Next(cards.Count)];
                if (number < 0.9)
                    await service.Pay(GetNextRequestId(), card, random.Next(100, 1000));
                else
                    await service.Fill(GetNextRequestId(), card, random.Next(100, 1000));
            }
        }

        public async Task Deposit(string account, double amount)
        {
            var service = new CurrentAccountService(executor, externalServices, localDB);
            await service.Deposit(GetNextRequestId(), account, amount);
        }
        public async Task Withdraw(string account, double amount)
        {
            var service = new CurrentAccountService(executor, externalServices, localDB);
            await service.Withdraw(GetNextRequestId(), account, amount);
        }
        public async Task Pay(string card, double amount)
        {
            var service = new AlphaCardService(executor, externalServices, localDB);
            await service.Pay(GetNextRequestId(), card, amount);
        }
        public async Task Fill(string card, double amount)
        {
            var service = new AlphaCardService(executor, externalServices, localDB);
            await service.Fill(GetNextRequestId(), card, amount);
        }
    }
}
