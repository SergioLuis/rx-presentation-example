using System;
using System.Collections.Generic;

namespace Ucm.Mii.Pdap.Presentation
{
    class Program
    {
        static void Main(string[] args)
        {
            mWatching = new List<string>(args);
            mMarketAlert = new CryptoMarketAlert(5.0);

            CryptocurrencyMarketService market =
                new CryptocurrencyMarketService();

            // Register our event handler
            market.MarketUpdate += MarketService_MarketUpdate;

            // We only connect the CryptocurrencyMarket service once the event
            // handler is in place, as we don't want to miss events.
            market.Connect();

            // Wait for key press to exit.
            Console.ReadLine();

            // Disconnect the service and remove the update handler.
            // No particular order, just for symmetry with the startup code.
            market.Disconnect();
            market.MarketUpdate -= MarketService_MarketUpdate;
        }

        static void MarketService_MarketUpdate(object sender, MarketUpdateEventArgs e)
        {
            // if we don't specified any crypto to watch, we are watching them all.
            if (mWatching.Count == 0)
            {
                mMarketAlert.Update(e.Update.Currency, e.Update.Price);
                return;
            }

            // Otherwise, we check if we are watching the updated crypto, and act
            // accordingly.
            if (!mWatching.Contains(e.Update.Currency))
                return;

            mMarketAlert.Update(e.Update.Currency, e.Update.Price);
        }

        static List<string> mWatching;
        static CryptoMarketAlert mMarketAlert;

        class CryptoMarketAlert
        {
            internal CryptoMarketAlert(double alertThreshold)
            {
                mAlertThreshold = alertThreshold;
                mMarketPrices = new Dictionary<string, double>();
            }

            internal void Update(string crypto, double price)
            {
                // If we don't have any previous data, just save it and return
                if (!mMarketPrices.ContainsKey(crypto))
                {
                    mMarketPrices.Add(crypto, price);
                    return;
                }

                // If the price difference doesn't pass the threshold, just
                // update our data and return.
                double oldPrice = mMarketPrices[crypto];
                if (Math.Abs(oldPrice - price) < mAlertThreshold)
                {
                    mMarketPrices[crypto] = price;
                    return;
                }

                // Otherwise, alert the price change and update our data anyway!
                Console.WriteLine(
                    $"Alert! Crypto {crypto} changed {oldPrice - price} USD! " +
                    $"(old: {oldPrice} USD, new: {price} USD)");

                mMarketPrices[crypto] = price;
            }

            readonly double mAlertThreshold;
            readonly Dictionary<string, double> mMarketPrices;
        }
    }
}
