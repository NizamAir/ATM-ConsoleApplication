using ATMApp.Domain.Entities;

namespace ATMApp.UI
{
    public class AppScreen
    {
        internal const string cur = "₽ ";

        internal static void Welcome()
        {
            //clears the console screen
            Console.Clear();
            //sets the title of console window
            Console.Title = "My ATM App";
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("\n\n- - - - - - - - - Добро-пожаловать в ATM App - - - - - - - - -\n\n");

            Console.WriteLine("Пожалуйста, вставьте свою банковскую карту в банкомат.");
            Console.WriteLine("Примечание: Банкомат примет и подтвердит подлинность физической карты, считает номер карты и подтвердит ее подлинность.");

            Utility.PressEnterToContinue();
        }

        internal static UserAccount UserLoginForm()
        {
            UserAccount tempUserAccount = new();

            tempUserAccount.CardNumber = Validator.Convert<long>("номер вашей карты.");
            tempUserAccount.CardPin = Convert.ToInt32(Utility.GetSecretInput("Введите ПИН-код"));

            return tempUserAccount;
        }

        internal static void LoginProcess()
        {
            Console.WriteLine("\nПроверка номера карты и ПИН-кода...");
            Utility.PrintDotAnimation();
        }

        internal static void PrintLockScreen()
        {
            Console.Clear();
            Utility.PrintMessage("Ваша карта заблокирована. Пожалуйста, обратитесь в ближайшее отделение банка, чтобы разблокировать свою карту. Спасибо.");
            Utility.PressEnterToContinue();
            Environment.Exit(1);
        }

        internal static void WelcomeCustomer(string fullName)
        {
            Console.WriteLine($"С возвращением, {fullName}");
            Utility.PressEnterToContinue();
        }

        internal static void DisplayAppMenu()
        {
            Console.Clear();
            Console.WriteLine("- - - - - - Меню ATM App - - - - - -");
            Console.WriteLine(":                                  :");
            Console.WriteLine("1. Баланс карты                    :");
            Console.WriteLine("2. Внести средства                 :");
            Console.WriteLine("3. Вывод средств                   :");
            Console.WriteLine("4. Перевод                         :");
            Console.WriteLine("5. История операций                :");
            Console.WriteLine("6. Выйти                           :");
        }

        internal static void LogoutProcess()
        {
            Console.WriteLine("Спасибо за использоване ATM App.");
            Utility.PrintDotAnimation();
            Console.Clear();
        }

        internal static int SelectAmount()
        {
            Console.WriteLine("");
            Console.WriteLine(":1.{0}500      5.{0}10,000", cur);
            Console.WriteLine(":2.{0}1000     6.{0}15,000", cur);
            Console.WriteLine(":3.{0}2000     7.{0}20,000", cur);
            Console.WriteLine(":4.{0}5000     8.{0}40,000", cur);
            Console.WriteLine(":0.Другое");
            Console.WriteLine("");

            int selectedAmount = Validator.Convert<int>("выбор:");
            switch (selectedAmount)
            {
                case 1:
                    return 500;
                    break;
                case 2:
                    return 1000;
                    break;
                case 3:
                    return 2000;
                    break;
                case 4:
                    return 5000;
                    break;
                case 5:
                    return 10000;
                    break;
                case 6:
                    return 15000;
                    break;
                case 7:
                    return 20000;
                    break;
                case 8:
                    return 40000;
                    break;
                case 0:
                    return 0;
                    break;
                default:
                    Utility.PrintMessage("Неверный ввод. Попробуйте снова.", false);
                    return -1;
                    break;
            }
        }

        internal InternalTransfer InternalTransferForm()
        {
            var internalTransfer = new InternalTransfer();
            internalTransfer.RecipientBankAccountNumber = Validator.Convert<long>("номер счета получателя:");
            internalTransfer.TransferAmount = Validator.Convert<decimal>($"сумму {cur}:");
            internalTransfer.RecipientBankAccountName = Utility.GetUserInput("имя получателя:");
            return internalTransfer;
        }
        
    }
}
