using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mocks
{
    public class LocalDB
    {
        private readonly object theLock = new object();
        private readonly Dictionary<string, List<double>> accounts = new Dictionary<string, List<double>> {
            { "A1", new List<double>() },
            { "A2", new List<double>() },
            { "A3", new List<double>() },
            { "C1", new List<double>() },
            { "C2", new List<double>() },
            { "C3", new List<double>() },
        };
        private readonly Logger logger;
        private readonly Func<int, string, bool, bool> isDown;

        public LocalDB(Logger logger, Func<int, string, bool, bool> isDown = null)
        {
            this.logger = logger;
            this.isDown = isDown;
        }

        public IEnumerable<string> QueryAccounts()
        {
            return accounts.Keys.Where(o => o.StartsWith("A")).ToList().AsReadOnly();
        }
        public IEnumerable<string> QueryCards()
        {
            return accounts.Keys.Where(o => o.StartsWith("C")).ToList().AsReadOnly();
        }
        public async Task ProcessRequest(int requestId, string account, double amount, bool isCompensate)
        {
            await Utils.SimulateAsync();
            logger.Log(requestId, $"--LocalDB.StoreTransaction[{account}, {amount}]");
            if (isDown != null && isDown(requestId, account, isCompensate))
                throw new ServerDownException();
            lock (theLock)
            {
                if (!accounts.ContainsKey(account))
                    throw new NotFoundException();
                accounts[account].Add(amount);
            }
        }

        public IEnumerable<KeyValuePair<string, List<double>>> GetState()
        {
            return accounts.Where(o => o.Value.Count > 0).ToList();
        }

        public void Reconciliate()
        {
        }
    }


    [Serializable]
    public class ServerDownException : Exception
    {
        public ServerDownException() { }
        public ServerDownException(string message) : base(message) { }
        public ServerDownException(string message, Exception inner) : base(message, inner) { }
        protected ServerDownException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
