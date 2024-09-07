using ATMApp.Domain.Entities;
using ATMApp.Domain.Enums;
using ATMApp.Domain.Interfaces;
using ATMApp.UI;
using ConsoleTables;

namespace ATMApp
{
    public class ATMApp : IUserLogin, IUserAccountActions, ITransaction
    {
        private List<UserAccount> _userAccountList;
        private UserAccount _selectedAccount;
        private List<Transaction> _listOfTransactions;
        private const decimal _minimumKeptAmount = 500;
        private readonly AppScreen _screen;

        public ATMApp()
        {
            _screen = new AppScreen();
        }

        public void Run()
        {
            AppScreen.Welcome();
            CheckUserCardNumAndPassword();
            AppScreen.WelcomeCustomer(_selectedAccount.FullName);
            while (true)
            {
                AppScreen.DisplayAppMenu();
                ProcessMenuOption();
            }
        }

        public void InitializeData()
        {
            _userAccountList = new List<UserAccount>
            {
                new UserAccount
                {
                    Id = 1,
                    FullName = "Obinna Ezeh",
                    AccountNumber = 123456,
                    CardNumber = 321321,
                    CardPin = 123123,
                    AccountBalance = 50000.00m,
                    isLocked = false,
                },
                new UserAccount
                {
                    Id = 2,
                    FullName = "Amaka Hope",
                    AccountNumber = 456789,
                    CardNumber = 654654,
                    CardPin = 456456,
                    AccountBalance = 4000.00m,
                    isLocked = false,
                },
                new UserAccount
                {
                    Id = 3,
                    FullName = "Femi Sunday",
                    AccountNumber = 123555,
                    CardNumber = 987987,
                    CardPin = 789789,
                    AccountBalance = 2000.00m,
                    isLocked = true,
                },

            };
            _listOfTransactions = new List<Transaction>();
        }
        public void CheckUserCardNumAndPassword()
        {
            bool isCorrectLogin = false;
            while (!isCorrectLogin)
            {
                UserAccount inputAccount = AppScreen.UserLoginForm();

                AppScreen.LoginProcess();

                foreach (UserAccount account in _userAccountList)
                {
                    _selectedAccount = account;
                    if (inputAccount.CardNumber.Equals(_selectedAccount.CardNumber))
                    {
                        _selectedAccount.TotalLogin++;
                        if (inputAccount.CardPin.Equals(_selectedAccount.CardPin))
                        {
                            _selectedAccount = account;
                            if (_selectedAccount.isLocked || _selectedAccount.TotalLogin > 3)
                            {
                                //Print a lock message
                                AppScreen.PrintLockScreen();
                            }
                            else
                            {
                                _selectedAccount.TotalLogin = 0;
                                isCorrectLogin = true;
                                break;
                            }
                        }
                    }
                    if (!isCorrectLogin)
                    {
                        Utility.PrintMessage("\n Неверный номер карты или ПИН-код", false);
                        _selectedAccount.isLocked = _selectedAccount.TotalLogin == 3;
                        if (_selectedAccount.isLocked)
                        {
                            AppScreen.PrintLockScreen();
                        }
                    }
                    Console.Clear();
                }
            }


        }

        private void ProcessMenuOption()
        {
            switch (Validator.Convert<int>("номер операции:"))
            {
                case (int)AppMenu.CheckBalance:
                    Console.WriteLine("Checking account balance...");
                    CheckBalance();
                    break;
                case (int)AppMenu.PlaceDeposit:
                    PlaceDeposit();
                    break;
                case (int)AppMenu.MakeWithdrawal:
                    MakeWithDrawal();
                    break;
                case (int)AppMenu.InternalTransfer:
                    var internalTransfer = _screen.InternalTransferForm();
                    ProcessInternalTransfer(internalTransfer);
                    break;
                case (int)AppMenu.ViewTransaction:
                    ViewTransaction();
                    break;
                case (int)AppMenu.Logout:
                    AppScreen.LogoutProcess();
                    Utility.PrintMessage("Вы успешно вышли из системы. Пожалуйста, заберите вашу банковскую карту.");
                    Run();
                    break;
                default:
                    Utility.PrintMessage("Недопустимое значение.", false);
                    break;
            }
        }

        public void CheckBalance()
        {
            Utility.PrintMessage($"Баланс карты: {Utility.FormatAmount(_selectedAccount.AccountBalance)}₽");
        }

        public void PlaceDeposit()
        {
            Console.WriteLine("\nДопускаются суммы, кратные 500 и 1000 рублей.\n");
            var transactionAmt = Validator.Convert<int>($"сумму {AppScreen.cur}");

            //simulate counting
            Console.WriteLine("\nПроверка и подсчет денежных купюр.");
            Utility.PrintDotAnimation();
            Console.WriteLine("");

            //some gaurd clause
            if (transactionAmt <= 0)
            {
                Utility.PrintMessage("Сумма должна быть больше нуля. Попробуйте снова.", false); ;
                return;
            }
            if (transactionAmt % 500 != 0)
            {
                Utility.PrintMessage($"Введите сумму, кратную 500 или 1000. Попробуйте снова.", false);
                return;
            }

            if (PreviewBankNotesCount(transactionAmt) == false)
            {
                Utility.PrintMessage($"Вы отменили свое действие.", false);
                return;
            }

            // bind transaction details to transaction object
            InsertTransaction(_selectedAccount.Id, TransactionType.Deposit, transactionAmt, "");

            // update account balance
            _selectedAccount.AccountBalance += transactionAmt;

            // print success message
            Utility.PrintMessage($"Ваш депозит в размере {Utility.FormatAmount(transactionAmt)} был успешно внесен.", true);

        }

        public void MakeWithDrawal()
        {
            var transactionAmt = 0;
            int selectedAmount = AppScreen.SelectAmount();
            if (selectedAmount == -1)
            {
                MakeWithDrawal();
                return;
            }
            else if (selectedAmount != 0)
            {
                transactionAmt = selectedAmount;
            }
            else
            {
                transactionAmt = Validator.Convert<int>($"сумму {AppScreen.cur}");
            }

            //input validation
            if (transactionAmt <= 0)
            {
                Utility.PrintMessage("Сумма должна быть больше нуля. Попробуйте снова.", false);
                return;
            }
            if (transactionAmt % 500 != 0)
            {
                Utility.PrintMessage("Вы можете вывести только сумму, кратную 500 или 1000 рублям. Попробуйте снова.", false);
                return;
            }
            //Business logic validations

            if (transactionAmt > _selectedAccount.AccountBalance)
            {
                Utility.PrintMessage($"Не удалось вывести средства. На вашем балансе недостаточно средств" +
                    $"{Utility.FormatAmount(transactionAmt)}", false);
                return;
            }
            if ((_selectedAccount.AccountBalance - transactionAmt) < _minimumKeptAmount)
            {
                Utility.PrintMessage($"Не удалось вывести средства. На вашем счете должно быть минимум {Utility.FormatAmount(_minimumKeptAmount)}", false);
                return;
            }

            //Bind withdrawal details to transaction object
            InsertTransaction(_selectedAccount.Id, TransactionType.Withdrawal, -transactionAmt, "");

            //update account balance
            _selectedAccount.AccountBalance -= transactionAmt;

            //success message
            Utility.PrintMessage($"Вы успешно завершили вывод средств " +
                $"{Utility.FormatAmount(transactionAmt)}.", true);
        }

        private bool PreviewBankNotesCount(int amount)
        {
            int thousandNotesCount = amount / 1000;
            int fiveHundredNotesCount = (amount % 1000) / 500;
            Console.WriteLine("\n Резюме");
            Console.WriteLine("--------");
            Console.WriteLine($"{AppScreen.cur}1000 X {thousandNotesCount} = {1000 * thousandNotesCount}");
            Console.WriteLine($"{AppScreen.cur}500 X {fiveHundredNotesCount} = {500 * fiveHundredNotesCount}");
            Console.WriteLine($"Итоговая сумма: {Utility.FormatAmount(amount)}\n\n");

            int opt = Validator.Convert<int>("1 для подтверждения.");
            return opt.Equals(1);
        }

        public void InsertTransaction(long _UserBankAccountId, TransactionType _tranType, decimal _tranAmount, string _desc)
        {
            //create a new transaction object
            var transaction = new Transaction()
            {
                TransactionId = Utility.GetTransactionId(),
                UserBankAccountId = _UserBankAccountId,
                TransactionDate = DateTime.Now,
                TransactionType = _tranType,
                TransactionAmount = _tranAmount,
                Description = _desc
            };

            //add transaction object to the list
            _listOfTransactions.Add(transaction);
        }

        public void ViewTransaction()
        {
            var filteredTransactionList = _listOfTransactions.Where(t => t.UserBankAccountId == _selectedAccount.Id).ToList();

            //check if there's a transaction
            if (filteredTransactionList.Count <= 0)
            {
                Utility.PrintMessage("У вас еще не было переводов.", true);
            }
            else
            {
                var table = new ConsoleTable("Id", "Дата перевода", "Тип Перевода", "Описание", "Сумма " + AppScreen.cur);
                foreach (var tran in filteredTransactionList)
                {
                    table.AddRow(tran.TransactionId, tran.TransactionDate, tran.TransactionType, tran.Description, tran.TransactionAmount);
                }
                table.Options.EnableCount = false;
                table.Write();
                Utility.PrintMessage($"У вас {filteredTransactionList.Count} перевод(ов)", true);
            }
        }

        private void ProcessInternalTransfer(InternalTransfer internalTransfer)
        {
            if (internalTransfer.TransferAmount <= 0)
            {
                Utility.PrintMessage("Сумма должна быть больше нуля. Попробуйте снова.", false);
                return;
            }

            //check sender's account balance
            if (internalTransfer.TransferAmount > _selectedAccount.AccountBalance)
            {
                Utility.PrintMessage($"Не удалось выполнить перевод. У вас недостаточно средств для перевода {Utility.FormatAmount(internalTransfer.TransferAmount)}", false);
                return;
            }

            //check the minimum kept amount 
            if ((_selectedAccount.AccountBalance - internalTransfer.TransferAmount) < _minimumKeptAmount)
            {
                Utility.PrintMessage($"Не удалось выполнить перевод. На вашем счете должно остаться минимум {Utility.FormatAmount(_minimumKeptAmount)}", false);
                return;
            }

            //check reciever's account number is valid
            var selectedBankAccountReciever = (from userAcc in _userAccountList
                                               where userAcc.AccountNumber == internalTransfer.RecipientBankAccountNumber
                                               select userAcc).FirstOrDefault();
            if (selectedBankAccountReciever == null)
            {
                Utility.PrintMessage("Перевод не выполнен. Указан неверный номер банковского счета получателя.", false);
                return;
            }

            //check receiver's name
            if (selectedBankAccountReciever.FullName != internalTransfer.RecipientBankAccountName)
            {
                Utility.PrintMessage("Перевод не выполнен. Имя банковского счета получателя не совпадает.", false);
                return;
            }

            //add transaction to transactions record- sender
            InsertTransaction(_selectedAccount.Id, TransactionType.Transfer, -internalTransfer.TransferAmount, "Перевод " +
                $"на {selectedBankAccountReciever.AccountNumber} ({selectedBankAccountReciever.FullName})");

            //update sender's account balance
            _selectedAccount.AccountBalance -= internalTransfer.TransferAmount;

            //add transaction record-reciever
            InsertTransaction(selectedBankAccountReciever.Id, TransactionType.Transfer, internalTransfer.TransferAmount, "Перевод " +
                $"{_selectedAccount.AccountNumber}({_selectedAccount.FullName})");

            //update reciever's account balance
            selectedBankAccountReciever.AccountBalance += internalTransfer.TransferAmount;
            //print success message

            Utility.PrintMessage($"Вы успешно перевели" +
                $" {Utility.FormatAmount(internalTransfer.TransferAmount)} на " +
                $"{internalTransfer.RecipientBankAccountName}", true);

        }
    }
}


