using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;

namespace Ucm.Mii.Pdap.Presentation
{
    class Program
    {
        static void Main(string[] args)
        {
            CryptocurrencyMarketService market =
                new CryptocurrencyMarketService();

            HashSet<string> watching = new HashSet<string>(args);

            // First, we create an observable that wraps the MarketUpdate event.
            // It'll only emit the CryptoUpdate inside the MarketUpdateEventArgs.
            // The Synchronize calls guarantee that the calls on its observers
            // will be synchronized, useful when the event is triggered from
            // another thread.
            IObservable<CryptoUpdate> observable =
                Observable.FromEventPattern<MarketUpdateEventArgs>(
                    h => market.MarketUpdate += h,
                    h => market.MarketUpdate -= h)
                .Select(u => u.EventArgs.Update)
                .Synchronize();

            // Remember how Observables can be composed?
            // First, we group updates from different cryptocurrencies.
            // Then, we buffer them: each update will be pushed along with the
            // previous one.
            // Then, we select the ones with a big difference between them, and
            // emit them.
            var drasticChanges =
                from update in observable
                group update by update.Currency into crypto
                from updatePair in crypto.Buffer(2, 1)
                let difference = Math.Abs(updatePair[0].Price - updatePair[1].Price)
                where difference >= 5.0
                select updatePair;

            // Now we can subscribe an observer to an observable.
            // For simplicity sake we'll just create an anonymous one that
            // implements the OnNext method. We would'nt know what to do in
            // case of an error, or the sequence finishing.
            IDisposable subscription = drasticChanges.Subscribe(
                change => 
                {
                    var oldPrice = change[0];
                    var newPrice = change[1];
                    var difference = oldPrice.Price - newPrice.Price;
                    Console.WriteLine(
                        $"Alert! Crypto {oldPrice.Currency} changed {difference} USD! " +
                        $"(old: {oldPrice.Price} USD, new: {newPrice.Price} USD)");
                }
            );

            // We only connect the CryptocurrencyMarket service once the event
            // handler is in place, as we don't want to miss events.
            market.Connect();

            // Wait for key press to exit.
            Console.ReadLine();

            // Dispose the subscription and close the connection.
            // No particular order, just for symmetry.
            subscription.Dispose();
            market.Disconnect();
        }
    }
}
