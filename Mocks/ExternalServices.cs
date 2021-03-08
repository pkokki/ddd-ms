using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mocks
{
    public class ExternalServices
    {
        private readonly Logger logger;
        private readonly object accountLock = new object();
        private readonly object cardLock = new object();
        private readonly Dictionary<string, List<double>> accountBalances = new Dictionary<string, List<double>> {
            { "A1", new List<double>() { 1000 } },
            { "A2", new List<double>() { 500 } },
            { "A4", new List<double>() { 0 } },
        };
        private readonly Dictionary<string, List<double>> cardBalances = new Dictionary<string, List<double>> {
            { "C1", new List<double>() { 800 } },
            { "C2", new List<double>() { 400 } },
            { "C4", new List<double>() { 0 } },
        };

        public ExternalServices(Logger logger)
        {
            this.logger = logger;
        }

        public async Task FillCard(int requestId, string card, double amount)
        {
            await Utils.SimulateAsync();
            logger.Log(requestId, $"--ExternalServices.FillCard[{card}, {amount}]");
            if (!cardBalances.ContainsKey(card))
                throw new NotFoundException();
            cardBalances[card].Add(amount);
        }
        public async Task PayWithCard(int requestId, string card, double amount)
        {
            await Utils.SimulateAsync();
            logger.Log(requestId, $"--ExternalServices.PayWithCard[{card}, {amount}]");
            if (!cardBalances.ContainsKey(card))
                throw new NotFoundException();
            lock (cardLock)
            {
                if (cardBalances[card].Sum() < amount)
                    throw new InsufficientBalanceException();
                cardBalances[card].Add(-amount);
            }
        }
        public async Task Deposit(int requestId, string account, double amount)
        {
            await Utils.SimulateAsync();
            logger.Log(requestId, $"--ExternalServices.Deposit[{account}, {amount}]");
            if (!accountBalances.ContainsKey(account))
                throw new NotFoundException();
            accountBalances[account].Add(amount);
        }
        public async Task Withdraw(int requestId, string account, double amount)
        {
            await Utils.SimulateAsync();
            logger.Log(requestId, $"--ExternalServices.Withdraw[{account}, {amount}]");
            if (!accountBalances.ContainsKey(account))
                throw new NotFoundException();
            lock (accountLock)
            {
                if (accountBalances[account].Sum() < amount)
                    throw new InsufficientBalanceException();
                accountBalances[account].Add(-amount);
            }
        }
        public async Task<double> QueryBalance(string account)
        {
            return await Task.FromResult(accountBalances[account].Sum());
        }

        public IEnumerable<KeyValuePair<string, IEnumerable<double>>> GetState()
        {
            var state = new List<KeyValuePair<string, IEnumerable<double>>>();
            state.AddRange(accountBalances
                .Where(o => o.Value.Count > 1)
                .ToList()
                .Select(kvp => new KeyValuePair<string, IEnumerable<double>>(kvp.Key, kvp.Value.Skip(1).ToList())));
            state.AddRange(cardBalances
                .Where(o => o.Value.Count > 1)
                .ToList()
                .Select(kvp => new KeyValuePair<string, IEnumerable<double>>(kvp.Key, kvp.Value.Skip(1).ToList())));
            return state;
        }

        public void Reconciliate()
        {
        }
    }


    [Serializable]
    public class NotFoundException : Exception
    {
        public NotFoundException() { }
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string message, Exception inner) : base(message, inner) { }
        protected NotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class InsufficientBalanceException : Exception
    {
        public InsufficientBalanceException() { }
        public InsufficientBalanceException(string message) : base(message) { }
        public InsufficientBalanceException(string message, Exception inner) : base(message, inner) { }
        protected InsufficientBalanceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
