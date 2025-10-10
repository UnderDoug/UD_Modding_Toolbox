using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL;
using XRL.Wish;
using XRL.World;

namespace UD_Modding_Toolbox
{
    [HasWishCommand]
    public partial class RaffleWishes
    {
        [WishCommand("UD raffle test")]
        public static void RaffleTest_WishHandler()
        {
            Raffle<string> creatures = new()
            {
                { "Glowcrow", 3 },
                { "Cave Spider", 5 },
                { "Pig", 2 },
            };
            Raffle<string> hat = new()
            {
                { "Jane", 4 },
                { "Doug", 2 },
                { "Books", 2 },
                { "Sol", 3 },
                { "AFFINE", 2 },
            };
            Raffle<string> bag = new("Test Seed")
            {
                { "Jane", 4 },
                { "Doug", 2 },
                { "Books", 2 },
                { "Sol", 3 },
                { "AFFINE", 2 },
            };
            Dictionary<string, int> dictionaryTest = new()
            {
                { "Jane", 2 },
                { "Doug", 3 },
                { "Books", 5 },
                { "Sol", 1 },
                { "AFFINE", 2 },
            };
            Raffle<string> dictionaryConversionTest = null;
            try
            {
                Debug.Entry(4, nameof(Raffle<string>) + " assignment from dictionary test");
                dictionaryConversionTest = dictionaryTest;
                Debug.Entry(4, nameof(dictionaryTest), Indent: 1);
                foreach ((string key, int value) in dictionaryTest)
                {
                    Debug.LoopItem(4, nameof(key) + ": " + key + ", " + nameof(value) + ": " + value, Indent: 2);
                }
                Debug.Entry(4, nameof(dictionaryConversionTest), Indent: 1);
                foreach ((string token, int weight) in dictionaryConversionTest)
                {
                    Debug.LoopItem(4, nameof(token) + ": " + token + ", " + nameof(weight) + ": " + weight, Indent: 2);
                }
            }
            catch (Exception x)
            {
                MetricsManager.LogException(nameof(Raffle<string>) + " assignment from dictionary test", x, "game_test_exception");
            }
            Raffle<string> raffleAdditionTest =  null;
            try
            {
                Debug.Entry(4, nameof(Raffle<string>) + " & " + nameof(Raffle<string>) + " addition test");
                raffleAdditionTest = hat + dictionaryConversionTest;
                Debug.Entry(4, nameof(hat), Indent: 1);
                foreach ((string token, int weight) in hat)
                {
                    Debug.LoopItem(4, nameof(token) + ": " + token + ", " + nameof(weight) + ": " + weight, Indent: 2);
                }
                Debug.Entry(4, nameof(dictionaryConversionTest), Indent: 1);
                foreach ((string token, int weight) in dictionaryConversionTest)
                {
                    Debug.LoopItem(4, nameof(token) + ": " + token + ", " + nameof(weight) + ": " + weight, Indent: 2);
                }
                Debug.Entry(4, nameof(raffleAdditionTest), Indent: 1);
                foreach ((string token, int weight) in raffleAdditionTest)
                {
                    Debug.LoopItem(4, nameof(token) + ": " + token + ", " + nameof(weight) + ": " + weight, Indent: 2);
                }
            }
            catch (Exception x)
            {
                MetricsManager.LogException(nameof(Raffle<string>) + " & " + nameof(Raffle<string>) + " addition test", x, "game_test_exception");
            }
            Raffle<string> raffleDictionaryAdditionTest = null;
            try
            {
                Debug.Entry(4, nameof(Raffle<string>) + " & dictionary addition test");
                raffleDictionaryAdditionTest = raffleAdditionTest + dictionaryTest;
                Debug.Entry(4, nameof(dictionaryTest), Indent: 1);
                foreach ((string token, int weight) in dictionaryTest)
                {
                    Debug.LoopItem(4, nameof(token) + ": " + token + ", " + nameof(weight) + ": " + weight, Indent: 2);
                }
                Debug.Entry(4, nameof(raffleAdditionTest), Indent: 1);
                foreach ((string token, int weight) in raffleAdditionTest)
                {
                    Debug.LoopItem(4, nameof(token) + ": " + token + ", " + nameof(weight) + ": " + weight, Indent: 2);
                }
                Debug.Entry(4, nameof(raffleDictionaryAdditionTest), Indent: 1);
                foreach ((string token, int weight) in raffleDictionaryAdditionTest)
                {
                    Debug.LoopItem(4, nameof(token) + ": " + token + ", " + nameof(weight) + ": " + weight, Indent: 2);
                }
            }
            catch (Exception x)
            {
                MetricsManager.LogException(nameof(Raffle<string>) + " & dictionary addition test", x, "game_test_exception");
            }
            Raffle<string> raffleAssignmentTest = null;
            try
            {
                Debug.Entry(4, nameof(Raffle<string>) + " array assignment test");
                raffleAssignmentTest = new()
                {
                    { "first", 1 },
                    { "second", 2 },
                    { "third", 3 },
                    { "fourth", 4 },
                    { "fifth", 5 },
                };
                foreach ((string token, int weight) in raffleAssignmentTest)
                {
                    Debug.LoopItem(4, nameof(token) + ": " + token + ", " + nameof(weight) + ": " + weight, Indent: 1);
                }
            }
            catch (Exception x)
            {
                MetricsManager.LogException(nameof(Raffle<string>) + " array assignment test", x, "game_test_exception");
            }
            try
            {
                int counter = 0;
                int startingActiveCount = creatures.ActiveCount;
                while (creatures.CanDraw() && counter < startingActiveCount + 5)
                {
                    string blueprint = creatures.Draw();
                    Debug.LoopItem(4, counter.ToString() + ": " + blueprint, Indent: 1);
                    if (GameObject.Create(blueprint) is GameObject creatureObject)
                    {
                        Cell spawnCell = The.ActiveZone?.GetRandomCell();
                        spawnCell?.AddObject(creatureObject);
                    }
                    counter++;
                }
            }
            catch (Exception x)
            {
                MetricsManager.LogException(nameof(Raffle<string>) + "." + nameof(creatures.CanDraw) +
                    " " + nameof(creatures) + ", & while loop test", x, "game_test_exception");
            }
            try
            {
                int counter = 0;
                while (hat.CanDraw())
                {
                    Debug.LoopItem(4, counter.ToString(), hat.Draw(false), Indent: 1);
                    counter++;
                }
            }
            catch (Exception x)
            {
                MetricsManager.LogException(nameof(Raffle<string>) + "." + nameof(hat.CanDraw) +
                    " from " + nameof(hat) + " & while loop test", x, "game_test_exception");
            }
            finally
            {
                hat.Refill();
            }
            try
            {
                int counter = 0;
                int n = 10;
                foreach (string draw in hat.DrawN(n))
                {
                    Debug.LoopItem(4, counter.ToString(), draw, Indent: 1);
                    counter++;
                }
            }
            catch (Exception x)
            {
                MetricsManager.LogException(nameof(Raffle<string>) + "." + nameof(hat.DrawN) +
                    " from " + nameof(hat) + ", & foreach test", x, "game_test_exception");
            }
            finally
            {
                hat.Refill();
            }
            try
            {
                Debug.Entry(4, nameof(Raffle<string>) + " seeded test");
                Debug.Entry(4, "First...", Indent: 1);
                int counter = 0;
                foreach (string draw in bag.DrawAll(true))
                {
                    Debug.LoopItem(4, counter.ToString(), draw, Indent: 2);
                    counter++;
                }

                Debug.Entry(4, "Second (true)...", Indent: 1);
                counter = 0;
                foreach (string draw in bag.DrawAll(true))
                {
                    Debug.LoopItem(4, counter.ToString(), draw, Indent: 2);
                    counter++;
                }

                bag.Refill();
                Debug.Entry(4, "Third (manual refill)...", Indent: 1);
                counter = 0;
                foreach (string draw in bag.DrawAll(true))
                {
                    Debug.LoopItem(4, counter.ToString(), draw, Indent: 2);
                    counter++;
                }

                Debug.Entry(4, "Fourth (no refill)...", Indent: 1);
                counter = 0;
                foreach (string draw in bag.DrawAll(true))
                {
                    Debug.LoopItem(4, counter.ToString(), draw, Indent: 2);
                    counter++;
                }
            }
            catch (Exception x)
            {
                MetricsManager.LogException(nameof(Raffle<string>) + "." + nameof(bag.DrawAll) +
                    " from " + nameof(bag) + ", seeded, & foreach test", x, "game_test_exception");
            }
            finally
            {
                bag.Refill();
            }
            try
            {
                Debug.Entry(4, nameof(Raffle<string>) + " unseeded sample & shake test");
                Debug.Entry(4, "First...", Indent: 1);
                int counter = 0;
                foreach (string draw in hat.DrawN(5))
                {
                    Debug.LoopItem(4, counter.ToString(), draw, Indent: 2);
                    counter++;
                }
                Debug.Entry(4, "Sample", hat.Sample(), Indent: 2);
                Debug.Entry(4, "Sample2", hat.Sample(), Indent: 2);
                Debug.Entry(4, "Shake...", Indent: 2);
                hat.Shake();
                Debug.Entry(4, "Sample3", hat.Sample(), Indent: 2);
                Debug.Entry(4, "Sample4", hat.Sample(), Indent: 2);

                hat.Refill();
                Debug.Entry(4, "Second...", Indent: 1);
                counter = 0;
                foreach (string draw in hat.DrawN(5))
                {
                    Debug.LoopItem(4, counter.ToString(), draw, Indent: 2);
                    counter++;
                }
                Debug.Entry(4, "Sample", hat.Sample(), Indent: 2);
                Debug.Entry(4, "Sample2", hat.Sample(), Indent: 2);
                Debug.Entry(4, "Shake...", Indent: 2);
                hat.Shake();
                Debug.Entry(4, "Sample3", hat.Sample(), Indent: 2);
                Debug.Entry(4, "Sample4", hat.Sample(), Indent: 2);
            }
            catch (Exception x)
            {
                MetricsManager.LogException(nameof(Raffle<string>) + "." + nameof(hat.Sample) +
                    " from " + nameof(hat) + ", unseeded, & shake test", x, "game_test_exception");
            }
            finally
            {
                hat.Refill();
            }
            try
            {
                Debug.Entry(4, nameof(Raffle<string>) + " seeded " + nameof(bag.GroupedActiveTokens) + " test");
                Debug.Entry(4, "First...", Indent: 1);
                int counter = 0;
                foreach (string token in bag.GroupedActiveTokens)
                {
                    Debug.LoopItem(4, counter.ToString(), token, Indent: 2);
                    counter++;
                }

                Debug.Entry(4, nameof(bag.DrawAll) + "...", Indent: 1);
                _ = bag.DrawAll(true);

                Debug.Entry(4, "Second...", Indent: 1);
                Debug.Entry(4, nameof(bag.GroupedActiveTokens), Indent: 2);
                counter = 0;
                foreach (string token in bag.GroupedActiveTokens)
                {
                    Debug.LoopItem(4, counter.ToString(), token, Indent: 3);
                    counter++;
                }
                Debug.Entry(4, nameof(bag.GroupedDrawnTokens), Indent: 2);
                counter = 0;
                foreach (string token in bag.GroupedDrawnTokens)
                {
                    Debug.LoopItem(4, counter.ToString(), token, Indent: 3);
                    counter++;
                }
                Debug.Entry(4, nameof(bag.Refill) + "...", Indent: 1);
                bag.Refill();

                Debug.Entry(4, "Third...", Indent: 1);
                counter = 0;
                foreach (string token in bag.GroupedActiveTokens)
                {
                    Debug.LoopItem(4, counter.ToString(), token, Indent: 2);
                    counter++;
                }
            }
            catch (Exception x)
            {
                MetricsManager.LogException(nameof(Raffle<string>) + "." + nameof(bag.GroupedActiveTokens) +
                    " from " + nameof(bag) + " enumeration test", x, "game_test_exception");
            }
            finally
            {
                bag.Refill();
            }
        }
    }
}
