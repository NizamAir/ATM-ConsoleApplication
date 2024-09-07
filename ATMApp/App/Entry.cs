namespace ATMApp.App
{
    class Entry
    {
        public static void Main(string[] args)
        {

            ATMApp atmApp = new ATMApp();
            atmApp.InitializeData();
            atmApp.Run();

        }
    }
}
