using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BlockchainCore
{
    public class Blockchain
    {
        public IList<Transaction> PendingTransactions = new List<Transaction>();
        public IList<Block> Chain { set;  get; }
        private int Difficulty { set; get; } = 2;
        private int _reward = 1; //1 cryptocurrency

        public void InitializeChain()
        {
            Chain = new List<Block>();
            AddGenesisBlock();
        }

        private Block CreateGenesisBlock()
        {
            Block block = new Block(DateTime.Now, null, PendingTransactions);
            block.Mine(Difficulty);
            PendingTransactions = new List<Transaction>();
            return block;
        }

        private void AddGenesisBlock()
        {
            Chain.Add(CreateGenesisBlock());
        }

        private Block GetLatestBlock()
        {
            return Chain[Chain.Count - 1];
        }

        public void CreateTransaction(Transaction transaction)
        {
            PendingTransactions.Add(transaction);
        }
        public void ProcessPendingTransactions(string minerAddress)
        {
            Block block = new Block(DateTime.Now, GetLatestBlock().Hash, PendingTransactions);
            AddBlock(block);

            PendingTransactions = new List<Transaction>();
            CreateTransaction(new Transaction(null, minerAddress, _reward));
        }

        private void AddBlock(Block block)
        {
            Block latestBlock = GetLatestBlock();
            block.Index = latestBlock.Index + 1;
            block.PreviousHash = latestBlock.Hash;
            //block.Hash = block.CalculateHash();
            block.Mine(Difficulty);
            Chain.Add(block);
        }

        public bool IsValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                Block currentBlock = Chain[i];
                Block previousBlock = Chain[i - 1];

                if (currentBlock.Hash != GetNounce(2, currentBlock))
                {
                    Console.WriteLine(currentBlock.TimeStamp);
                    Console.WriteLine(currentBlock.CalculateHash());
                    Console.WriteLine(currentBlock.Hash);
                    return false;
                }

                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    return false;
                }
            }
            return true;
        }
        private int Nonce { get; set; }
        public string Hash { get; private set; }

        public string CalculateHash(Block block)
        {
            Block CheckBlock = block;
            var sha256 = SHA256.Create();

            var inputBytes = Encoding.ASCII.GetBytes($"{CheckBlock.TimeStamp}-{CheckBlock.PreviousHash ?? ""}-{JsonConvert.SerializeObject(CheckBlock.Transactions)}-{Nonce}");
            var outputBytes = sha256.ComputeHash(inputBytes);

            return Convert.ToBase64String(outputBytes);
        }

        public string GetNounce(int difficulty, Block block)
        {
            var leadingZeros = new string('0', difficulty);
            while (Hash == null || Hash.Substring(0, difficulty) != leadingZeros)
            {
                Nonce++;
                Hash = CalculateHash(block);
            }
            return Hash;
        }
        public int GetBalance(string address)
        {
            int balance = 0;

            for (int i = 0; i < Chain.Count; i++)
            {
                for (int j = 0; j < Chain[i].Transactions.Count; j++)
                {
                    var transaction = Chain[i].Transactions[j];

                    if (transaction.FromAddress == address)
                    {
                        balance -= transaction.Amount;
                    }

                    if (transaction.ToAddress == address)
                    {
                        balance += transaction.Amount;
                    }
                }
            }

            return balance;
        }
    }
}
