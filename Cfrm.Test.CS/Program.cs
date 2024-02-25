using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cfrm.Test.CS
{
    using static Cfrm.Test.KuhnPoker;
    using static KuhnPoker.Card;

    class Program
    {
        static void Main(string[] args)
        {
            var deck = new Card[] { Card.Jack, Card.Queen, Card.King };
            var rng = new Random(0);
            var numIterations = 100000;
            var delta = 0.03;

            var (expectedGameValues, strategyProfile) =
                CounterFactualRegret.Minimize(numIterations, 2, i =>
                {
                    var cards = Shuffle(rng, deck)[0..2];
                    return new KuhnPokerState(cards);
                });

            const string path = "Kuhn.strategy";
            strategyProfile.Save(path);
            strategyProfile = StrategyProfile.Load(path);

            // https://en.wikipedia.org/wiki/Kuhn_poker#Optimal_strategy
            var dict = strategyProfile.ToDict();
            Assert.AreEqual(expectedGameValues[0], -1.0 / 18.0, delta);
            var alpha = dict["J"][1];
            Assert.IsTrue(alpha >= 0.0);
            Assert.IsTrue(alpha <= 1.0 / 3.0);
            Assert.AreEqual(dict["Q"][0], 1.0, delta);
            Assert.AreEqual(dict["Qcb"][1], alpha + 1.0 / 3.0, delta);
            Assert.AreEqual(dict["K"][1], 3.0 * alpha, delta);

            // print results
            Console.WriteLine("Expected game values:");
            Console.WriteLine(string.Join(", ", expectedGameValues));
            Console.WriteLine("Strategy profile:");
            foreach (var (key, value) in dict)
            {
                Console.WriteLine($"{key}: {string.Join(", ", value)}");
            }
        }
    }
}