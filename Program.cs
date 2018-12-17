using System;

namespace Ucm.Mii.Pdap.Presentation
{
    class Program
    {
        static void Main(string[] args)
        {
            CryptocurrencyMarketService market = new CryptocurrencyMarketService();

            // You should NEVER register EventHandlers defining them as
            // anonymous functions, as you loose the function's ref and thus you
            // can't unregister it when needed.
            // For this dummy example is OK.
            market.StockUpdate += (sender, e) => 
            {
                Console.WriteLine(
                    $"{e.Update.Currency} -> {e.Update.Price} USD.");
            };

            // We only connect the CryptocurrencyMarket service once the event
            // handler is in place, as we don't want to miss events.
            market.Connect();

            // Wait for key press to exit.
            Console.ReadLine();

            market.Disconnect();
        }
    }
}
