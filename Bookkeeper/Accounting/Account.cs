﻿using System;
using System.Collections.Generic;
using System.Linq;
using Bookkeeper.Infrastructure.Interfaces;

namespace Bookkeeper.Accounting
{
    internal class Account : IAccount
    {
        public int AccountNumber { get; private set; }
        public string Name { get; set; }
        public AccountType Type { get; private set; }

        private readonly List<ITransaction> _transactions;

        public IEnumerable<ITransaction> Transactions {
            get { return _transactions; }
        }

        public decimal Balance
        {
             get
             {
                 var debits = (from e in Transactions
                               select e.Debit).Sum();
                 var credits = (from e in Transactions
                               select e.Credit).Sum();

                 var balance = debits - credits;

                 //Rules from http://en.wikipedia.org/wiki/Debits_and_credits:
                 switch(Type)
                 {
                     case(AccountType.Asset):
                         return DebitsIncreaseThe(balance);
                     case (AccountType.Liability):
                         return CreditsIncreaseThe(balance);
                     case (AccountType.Revenue):
                         return CreditsIncreaseThe(balance);
                     case (AccountType.Expense):
                         //TODO: awb-2 Implement expense account type.
                         throw new NotImplementedException("Expense acct type balance not implemented.");
                     case (AccountType.Equity):
                         return CreditsIncreaseThe(balance);
                 }
                 return 0;
             }
        }

        public void RecordTransaction(decimal amount, DateTime transactionDate, string transactionReference)
        {
            if (amount == 0) throw new AccountException("Cannot record a transaction with a zero amount.");
            ITransaction transaction = null;

            switch (Type)
            {
                case AccountType.Asset:
                    if (amount > 0)
                    {
                        transaction = Debit(amount, transactionDate, transactionReference);
                    }
                    else
                    {
                        transaction = Credit(amount, transactionDate, transactionReference);
                    }
                    break;
                case AccountType.Liability:
                    if (amount > 0)
                    {
                        transaction = Credit(amount, transactionDate, transactionReference);
                    }
                    else
                    {
                        transaction = Debit(amount, transactionDate, transactionReference);
                    }
                    break;
                case AccountType.Revenue:
                    if (amount > 0)
                    {
                        transaction = Credit(amount, transactionDate, transactionReference);
                    }
                    else
                    {
                        transaction = Debit(amount, transactionDate, transactionReference);
                    }
                    break;
                case AccountType.Expense:
                    if (amount > 0)
                    {
                        transaction = Debit(amount, transactionDate, transactionReference);
                    }
                    else
                    {
                        transaction = Credit(amount, transactionDate, transactionReference);
                    }
                    break;
                case AccountType.Equity:
                    if (amount > 0)
                    {
                        transaction = Credit(amount, transactionDate, transactionReference);
                    }
                    else
                    {
                        transaction = Debit(amount, transactionDate, transactionReference);
                    }
                    break;
            }
            _transactions.Add(transaction);
        }

        private ITransaction Debit(decimal amount, DateTime transactionDate, string transactionReference)
        {
            return new Transaction(transactionDate, transactionReference, AccountNumber, Math.Abs(amount), 0.0m);
        }

        private Transaction Credit(decimal amount, DateTime transactionDate, string transactionReference)
        {
            return new Transaction(transactionDate, transactionReference, AccountNumber, 0.0m, Math.Abs(amount));
        }

        private static decimal DebitsIncreaseThe(decimal balance)
        {
            return balance;
        }

        private static decimal CreditsIncreaseThe(decimal balance)
        {
            return balance * (-1);
        }


        public Account(int accountNumber, string name, AccountType accountAccountType)
        {
            AccountNumber = accountNumber;
            Name = name;
            Type = accountAccountType;
            _transactions = new List<ITransaction>();
        }


        internal class AccountException : Exception
        {
            public AccountException(string errorMessage) : base(errorMessage)
            {
            }
        }


    }
}