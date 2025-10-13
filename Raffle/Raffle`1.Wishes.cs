using System;
using System.Collections.Generic;

using XRL;
using XRL.Wish;
using XRL.World;

namespace UD_Modding_Toolbox
{
    [HasWishCommand]
    public partial class RaffleWishes
    {
        private static int CounterMax = 49;

        [WishCommand("UD raffle test")]
        public static void RaffleTest_WishHandler()
        {
            int indent = Debug.LastIndent;
            Raffle<string> creatures = new()
            {
                { "Glowcrow", 3 },
                { "Goat", 5 },
                { "Pig", 2 },
            };
            Raffle<string> hat = new()
            {
                { "Jofo Qudwufo", 4 },
                { "UnderDoug", 2 },
                { "Books", 2 },
                { "Sol", 3 },
                { "AFFINE", 2 },
            };
            Raffle<string> bag = new("Test Seed")
            {
                { "Jofo Qudwufo", 4 },
                { "UnderDoug", 2 },
                { "Books", 2 },
                { "Sol", 3 },
                { "AFFINE", 2 },
            };
            Dictionary<string, int> dictionaryTest = new()
            {
                { "Jofo Qudwufo", 2 },
                { "UnderDoug", 3 },
                { "John Qud", 2 },
                { "Books", 5 },
                { "Sol", 1 },
                { "AFFINE", 2 },
            };
            Raffle<string> dictionaryConversionTest = null;
            try
            {
                Debug.Entry(4, nameof(Raffle<string>) + " assignment from dictionary test");
                Debug.Entry(4, nameof(dictionaryTest), Indent: 1);
                foreach ((string key, int value) in dictionaryTest)
                {
                    Debug.LoopItem(4, nameof(key) + ": " + key + ", " + nameof(value) + ": " + value, Indent: 2);
                }
                dictionaryConversionTest = dictionaryTest;
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
                raffleAdditionTest = hat + dictionaryConversionTest;
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
                Debug.Entry(4, nameof(creatures) + "." + nameof(creatures.CanDraw) + " test");
                int counter = 0;
                int startingActiveCount = creatures.ActiveCount;
                Debug.Entry(4, nameof(creatures) + "." + nameof(creatures.CanDraw) + " test", Indent: 1);
                creatures.Vomit(4, nameof(RaffleWishes), "before loop", Indent: 2);
                while (creatures.CanDraw() && counter < startingActiveCount + 5)
                {
                    string blueprint = creatures.Draw();
                    // creatures.Vomit(4, counter.ToString() + ":", blueprint, Indent: 2);
                    if (GameObject.Create(blueprint, AfterObjectCreated: GO => GO.SetStringProperty("Raffled", "true")) is GameObject creatureObject)
                    {
                        Cell spawnCell = The.ActiveZone?.GetRandomCell();
                        spawnCell?.AddObject(creatureObject);
                    }
                    if (++counter > CounterMax)
                    {
                        throw new NotFiniteNumberException("Runaway while loop halted by " + nameof(counter) + " exceeding " + (counter - 1));
                    }
                }
            }
            catch (Exception x)
            {
                MetricsManager.LogException(nameof(Raffle<string>) + "." + nameof(creatures.CanDraw) +
                        " " + nameof(creatures) + ", & while loop test", x, "game_test_exception");
            }
            finally
            {
                creatures.Vomit(4, nameof(RaffleWishes), "after loop", Indent: 2);
            }
            try
            {
                Debug.Entry(4, nameof(hat) + "." + nameof(hat.CanDraw) + " test");
                int counter = 0;
                while (hat.CanDraw() && hat.Draw(false) is string draw)
                {
                    Debug.LoopItem(4, counter.ToString(), draw, Indent: 1);
                    if (draw.IsNullOrEmpty())
                    {
                        throw new NotFiniteNumberException("Runaway while loop halted by null or empty " + nameof(draw));
                    }
                    if (++counter > CounterMax)
                    {
                        throw new NotFiniteNumberException("Runaway while loop halted by " + nameof(counter) + " exceeding " + (counter - 1));
                    }
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
                Debug.Entry(4, nameof(hat) + "." + nameof(hat.DrawN) + "(" + n + ") test");
                foreach (string draw in hat.DrawN(n))
                {
                    Debug.LoopItem(4, counter.ToString(), draw, Indent: 1);
                    if (++counter > CounterMax)
                    {
                        throw new NotFiniteNumberException("Runaway foreach halted by " + nameof(counter) + " exceeding " + (counter - 1));
                    }
                }
            }
            catch (Exception x)
            {
                MetricsManager.LogException(nameof(hat) + "." + nameof(hat.DrawN) +
                    " from " + nameof(hat) + ", & foreach test", x, "game_test_exception");
            }
            finally
            {
                hat.Refill();
            }
            try
            {
                Debug.Entry(4, nameof(bag) + "." + nameof(bag.DrawAll) + " seeded test");
                Debug.Entry(4, "First...", Indent: 1);
                int counter = 0;
                foreach (string draw in bag.DrawAll(true))
                {
                    Debug.LoopItem(4, counter.ToString(), draw, Indent: 2);
                    if (++counter > CounterMax)
                    {
                        throw new NotFiniteNumberException("Runaway foreach halted by " + nameof(counter) + " exceeding " + (counter - 1));
                    }
                }

                try
                {
                    Debug.Entry(4, "Second (true)...", Indent: 1);
                    counter = 0;
                    foreach (string draw in bag.DrawAll(true))
                    {
                        Debug.LoopItem(4, counter.ToString(), draw, Indent: 2);
                        if (++counter > CounterMax)
                        {
                            throw new NotFiniteNumberException("Runaway foreach halted by " + nameof(counter) + " exceeding " + (counter - 1));
                        }
                    }
                }
                catch (Exception x)
                {
                    MetricsManager.LogException("Second (true)", x, "game_test_exception");
                }

                try
                {
                    bag.Refill();
                    Debug.Entry(4, "Third (manual refill)...", Indent: 1);
                    counter = 0;
                    foreach (string draw in bag.DrawAll(true))
                    {
                        Debug.LoopItem(4, counter.ToString(), draw, Indent: 2);
                        if (++counter > CounterMax)
                        {
                            throw new NotFiniteNumberException("Runaway foreach halted by " + nameof(counter) + " exceeding " + (counter - 1));
                        }
                    }
                }
                catch (Exception x)
                {
                    MetricsManager.LogException("Third (manual refill)", x, "game_test_exception");
                }

                try
                {
                    Debug.Entry(4, "Fourth (no refill)...", Indent: 1);
                    counter = 0;
                    foreach (string draw in bag.DrawAll(false))
                    {
                        Debug.LoopItem(4, counter.ToString(), draw, Indent: 2);
                        if (++counter > CounterMax)
                        {
                            throw new NotFiniteNumberException("Runaway foreach halted by " + nameof(counter) + " exceeding " + (counter - 1));
                        }
                    }
                }
                catch (Exception x)
                {
                    if (x.GetType().InheritsFrom(typeof(InvalidOperationException))
                        && x.ToString().Contains("Can't " + nameof(bag.DrawAll) + " from an empty "))
                    {
                        Debug.CheckYeh(4, "Got the expected " + nameof(InvalidOperationException), Indent: 2);
                    }
                    else
                    {
                        MetricsManager.LogException("Fourth (no refill)", x, "game_test_exception");
                    }
                }

            }
            catch (Exception x)
            {
                MetricsManager.LogException(nameof(bag) + "." + nameof(bag.DrawAll) +
                    " from " + nameof(bag) + ", seeded, & foreach test", x, "game_test_exception");
            }
            finally
            {
                bag.Refill();
            }
            try
            {
                Debug.Entry(4, nameof(hat) + " unseeded " + nameof(hat.Sample) + " & " + nameof(hat.Shake) + " test");
                Debug.Entry(4, "First...", Indent: 1);
                int counter = 0;
                foreach (string draw in hat.DrawN(5, true))
                {
                    Debug.LoopItem(4, counter.ToString(), draw, Indent: 2);
                    if (++counter > CounterMax)
                    {
                        throw new NotFiniteNumberException("Runaway foreach halted by " + nameof(counter) + " exceeding " + (counter - 1));
                    }
                }
                Debug.Entry(4, "Sample1", hat.Sample(), Indent: 2);
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
                    if (++counter > CounterMax)
                    {
                        throw new NotFiniteNumberException("Runaway foreach halted by " + nameof(counter) + " exceeding " + (counter - 1));
                    }
                }
                Debug.Entry(4, "Sample1", hat.Sample(), Indent: 2);
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
                    if (++counter > CounterMax)
                    {
                        throw new NotFiniteNumberException("Runaway foreach halted by " + nameof(counter) + " exceeding " + (counter - 1));
                    }
                }

                Debug.Entry(4, nameof(bag.DrawAll) + "...", Indent: 1);
                bag.DrawAll(true);

                Debug.Entry(4, "Second...", Indent: 1);
                Debug.Entry(4, nameof(bag.GroupedActiveTokens), "should be full", Indent: 2);
                counter = 0;
                foreach (string token in bag.GroupedActiveTokens)
                {
                    Debug.LoopItem(4, counter.ToString(), token, Indent: 3);
                    if (++counter > CounterMax)
                    {
                        throw new NotFiniteNumberException("Runaway foreach halted by " + nameof(counter) + " exceeding " + (counter - 1));
                    }
                }
                Debug.Entry(4, nameof(bag.GroupedDrawnTokens), "should be empty", Indent: 2);
                counter = 0;
                foreach (string token in bag.GroupedDrawnTokens)
                {
                    Debug.LoopItem(4, counter.ToString(), token, Indent: 3);
                    if (++counter > CounterMax)
                    {
                        throw new NotFiniteNumberException("Runaway foreach halted by " + nameof(counter) + " exceeding " + (counter - 1));
                    }
                }
                Debug.Entry(4, nameof(bag.Refill) + "...", Indent: 1);
                bag.Refill();

                Debug.Entry(4, "Third...", Indent: 1);
                Debug.Entry(4, nameof(bag.GroupedActiveTokens), "should be full", Indent: 2);
                counter = 0;
                foreach (string token in bag.GroupedActiveTokens)
                {
                    Debug.LoopItem(4, counter.ToString(), token, Indent: 3);
                    if (++counter > CounterMax)
                    {
                        throw new NotFiniteNumberException("Runaway foreach halted by " + nameof(counter) + " exceeding " + (counter - 1));
                    }
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
                Debug.LastIndent = indent;
            }
        }

        [WishCommand("UD raffle cleanup")]
        public static void RaffleCleanup_WishHandler()
        {
            try
            {
                The.ActiveZone.ForeachObjectWithTagOrProperty("Raffled", GO => GO.Obliterate());
            }
            catch (Exception x)
            {
                MetricsManager.LogException(nameof(Raffle<string>) + " borked the cleanup...", x, "game_test_exception");
            }
        }
    }
}
