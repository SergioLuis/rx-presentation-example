using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ucm.Mii.Pdap.Presentation
{
    public class CryptocurrencyMarketService
    {
        public event EventHandler<MarketUpdateEventArgs> MarketUpdate;

        public CryptocurrencyMarketService()
        {
            mRandom = new Random(Environment.TickCount);
            mActualPrices = new Dictionary<string, double>();

            List<string> cryptoCurrencies = new List<string>()
                { "Bitcoin", "XRP", "Ethereum", "Tether" };

            cryptoCurrencies.ForEach(cryptoName =>
                mActualPrices.Add(cryptoName, 10.0 + mRandom.Next(-2, 3)));
        }

        public void Connect()
        {
            lock (mSyncLock)
            {
                mbIsRunning = true;
                ThreadPool.QueueUserWorkItem((state) => { BackgroundJob(); });
            }
        }

        public void Disconnect()
        {
            lock (mSyncLock)
            {
                mbIsRunning = false;
            }
        }

        public bool IsConnected()
        {
            lock (mSyncLock)
            {
                return mbIsRunning;
            }
        }

        void BackgroundJob()
        {
            while (true)
            {
                // 1.- Choose random cryptocurrency.
                string chosenCrypto = mActualPrices.Keys.ElementAt(
                    mRandom.Next(0, mActualPrices.Count));

                // 2.- Calculate price delta (based on probability!).
                double delta = (mRandom.Next(0, 5) == 0)
                    ? 5.0
                    : mRandom.Next(1, 4);

                delta *= (mRandom.Next(0, 2) == 1) ? -1 : 1;

                // 3.- Update price on the dictionary.
                double oldPrice = mActualPrices[chosenCrypto];
                double newPrice = oldPrice + delta;
                if (newPrice < 0)
                    newPrice = 0;

                mActualPrices[chosenCrypto] = newPrice;

                // 4.- Trigger the event.
                MarketUpdateEventArgs args =
                    new MarketUpdateEventArgs(
                        new CryptoUpdate(
                            chosenCrypto, mActualPrices[chosenCrypto]));

                // The safest way to call an event handler is copying its reference
                // first to a temporal variable.
                // The actual handler could get removed between the nullity check
                // and the invoke if we do it this way:
                //
                // if (StockUpdate != null)
                //      StockUpdate(this, args);
                EventHandler<MarketUpdateEventArgs> temp = MarketUpdate;
                temp?.Invoke(this, args);

                // 5.- Check if the server must keep running.
                if (!IsConnected())
                    return;

                // 6.- Sleep, we don't want to get crazy!
                Thread.Sleep(150);
            }
        }

        readonly Random mRandom;
        readonly Dictionary<string, double> mActualPrices;

        bool mbIsRunning = false;
        object mSyncLock = new object();
    }

    public class CryptoUpdate
    {
        public readonly string Currency;
        public readonly double Price;
        public CryptoUpdate(string currency, double price)
        {
            Currency = currency;
            Price = price;
        }

        public override string ToString() => $"{Currency} -> {Price} USD";
    }

    public class MarketUpdateEventArgs : EventArgs
    {
        public readonly CryptoUpdate Update;
        public MarketUpdateEventArgs(CryptoUpdate update)
        {
            Update = update;
        }
    }
}
