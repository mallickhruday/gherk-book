﻿using System;
using System.Collections.Generic;
using Bookkeeper.Infrastructure;
using Bookkeeper.Infrastructure.Interfaces;

namespace Bookkeeper.Accounting
{
    public class Business : IDoAccounting
    {
        private readonly IGeneralLedgerRepository _generalLedger;
        private readonly IJournalRepository _journal;
        const int SalesTaxOwing = 3002;
        const int CashRegister = 1000;
        const int SalesTaxPaid = 3001;
        const int OwnerEquity = 7000;

        public static IDoAccounting SetUpAccounting()
        {
            var accountingService = new Business(Ioc.Resolve<IJournalRepository>(), Ioc.Resolve<IGeneralLedgerRepository>());

            accountingService.CreateSalesTaxOwingAccount();
            accountingService.CreateCashRegisterAccount();
            accountingService.CreateOwnerEquityAccount();

            return accountingService;
        }

        public int SalesTaxPaidAcctNo
        {
            get { return SalesTaxPaid; }
        }

        public int CashRegisterAcctNo
        {
            get { return CashRegister; }
        }

        public int OwnersEquityAcctNo
        {
            get { return OwnerEquity; }
        }

        public int SalesTaxOwingAcctNo
        {
            get { return SalesTaxOwing; }
        }

        public IEnumerable<IJournalEntry> GetJournal()
        {
            return _journal.Entries();
        }

        public ITrialBalance GetTrialBalance()
        {
            return TrialBalance.GenerateFrom(_generalLedger.GetAccounts(), _journal);
        }

        public IAccount GetAccount(int accountNo)
        {
            return _generalLedger.GetAccount(accountNo);
        }

        public void CreateNewAccount(int accountNumber, string accountName, AccountType type)
        {
            var account = new Account(accountNumber, accountName, type);
            account.Journal = _journal;
            _generalLedger.AddAccount(accountNumber, account);
        }

        public void RecordTaxFreeSale(int customerAccountNo, decimal amount, DateTime transactionDate, string transactionReference)
        {
            var cashAccount = _generalLedger.GetAccount(CashRegister);
            cashAccount.RecordTransaction(amount,transactionDate, transactionReference);

            var customerAccount = _generalLedger.GetAccount(customerAccountNo);
            customerAccount.RecordTransaction(amount, transactionDate, transactionReference);
        }

        public void RecordTaxableSale(int customerAccountNo, decimal netAmount, decimal salesTaxAmount, DateTime transactionDate, string transactionReference)
        {
            var cashAccount = _generalLedger.GetAccount(CashRegister);
            cashAccount.RecordTransaction(netAmount + salesTaxAmount, transactionDate, transactionReference);

            var customerAccount = _generalLedger.GetAccount(customerAccountNo);
            customerAccount.RecordTransaction(netAmount, transactionDate, transactionReference);

            var salesTaxOwingAccount = _generalLedger.GetAccount(SalesTaxOwing);
            salesTaxOwingAccount.RecordTransaction(salesTaxAmount, transactionDate, transactionReference);
        }

        public void RecordPurchaseFrom(int supplierAccountNo, int assetAccountNo, decimal netAmount, decimal salesTaxAmount, DateTime transactionDate, string transactionReference)
        {
            if(salesTaxAmount > 0)
            {
                DeductFromSalesTaxOwing(salesTaxAmount, transactionDate, transactionReference);
            }
            RecordAmountOwingTo(supplierAccountNo, netAmount + salesTaxAmount, transactionDate, transactionReference);
            RecordAsset(assetAccountNo, netAmount, transactionDate, transactionReference);
        }

        public void RecordPaymentTo(int recipientAccountNo, decimal amount, DateTime transactionDate, string transactionReference)
        {
            var cashAccount = _generalLedger.GetAccount(CashRegister);
            cashAccount.RecordTransaction((amount * -1), transactionDate, transactionReference);

            var recipientAccount = _generalLedger.GetAccount(recipientAccountNo);
            recipientAccount.RecordTransaction((amount * -1), transactionDate, transactionReference);
        }

        public IEnumerable<IAccount> GetChartOfAccounts()
        {
            return _generalLedger.GetAccounts();
        }


        private void RecordAsset(int assetAccountNo, decimal netAmount, DateTime transactionDate, string transactionReference)
        {
            var assetAccount = _generalLedger.GetAccount(assetAccountNo);
            assetAccount.RecordTransaction(netAmount, transactionDate, transactionReference);
        }

        private void RecordAmountOwingTo(int supplierAccountNo, decimal netAmount, DateTime transactionDate, string transactionReference)
        {
            var supplierAccount = _generalLedger.GetAccount(supplierAccountNo);
            supplierAccount.RecordTransaction(netAmount, transactionDate, transactionReference);
        }

        private void DeductFromSalesTaxOwing(decimal salesTaxAmount, DateTime transactionDate, string transactionReference)
        {
            var salesTaxOwingAccount = _generalLedger.GetAccount(SalesTaxOwing);
            salesTaxOwingAccount.RecordTransaction((salesTaxAmount * -1), transactionDate, transactionReference);
        }

        private void CreateCashRegisterAccount()
        {
            CreateNewAccount(CashRegister, "Cash", AccountType.Asset);
        }

        private void CreateSalesTaxOwingAccount()
        {
            CreateNewAccount(SalesTaxOwing, "Sales Tax Owing", AccountType.Liability);
        }

        private void CreateSalesTaxPaidAccount()
        {
            CreateNewAccount(SalesTaxPaid, "Sales Tax Refunds", AccountType.Revenue);
        }

        private void CreateOwnerEquityAccount()
        {
            CreateNewAccount(OwnerEquity, "John Smith (Owner)", AccountType.Equity);
        }

        private Business(IJournalRepository journal, IGeneralLedgerRepository generalLedger)
        {
            _journal = journal;
            _generalLedger = generalLedger;
        }

        public IAccount GetStatementFor(int accountNo)
        {
            return _generalLedger.GetAccount(accountNo);
        }

        public void RecordCashInvestmentBy(int accountNo, decimal amount, DateTime transactionDate, string transactionReference)
        {
            _generalLedger.GetAccount(accountNo).RecordTransaction(amount, transactionDate, transactionReference);
            var cashAccount = _generalLedger.GetAccount(CashRegisterAcctNo);
            cashAccount.RecordTransaction(amount, transactionDate, transactionReference);

        }

        public void RecordCashInjectionByOwner(decimal amount, DateTime transactionDate, string transactionReference)
        {
            var ownerEquityAccount = _generalLedger.GetAccount(OwnersEquityAcctNo);
            ownerEquityAccount.RecordTransaction(amount, transactionDate, transactionReference);

            var cashAccount = _generalLedger.GetAccount(CashRegisterAcctNo);
            cashAccount.RecordTransaction(amount, transactionDate, transactionReference);
        }
    }
}
