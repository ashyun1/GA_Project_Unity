using System;
using System.Linq;
using System.Collections.Generic;

public class EnhancementSystem
{
    public class Stone
    {
        public string Name { get; private set; }
        public int Exp { get; private set; }
        public int Price { get; private set; }
        public float ExpPerGold { get; private set; }

        public Stone(string name, int exp, int price)
        {
            Name = name;
            Exp = exp;
            Price = price;
            ExpPerGold = (float)exp / price;
        }
    }

    public class PurchaseResult
    {
        public Dictionary<string, int> StoneCounts = new Dictionary<string, int>();
        public int TotalExp { get; set; } = 0;
        public int TotalGold { get; set; } = 0;
        public int WastedExp { get; set; } = 0;
    }

    private List<Stone> stones;
    private Stone smallStone;

    public EnhancementSystem()
    {
        stones = new List<Stone>
        {
            new Stone("소", 3, 8),
            new Stone("중", 5, 12),
            new Stone("대", 12, 30),
            new Stone("특대", 20, 45)
        };
        smallStone = stones.First(s => s.Name == "소");
    }

    public List<Stone> GetStones() => stones;

    public PurchaseResult CalculateOptimalBruteForce(int requiredExp)
    {
        int maxExp = requiredExp + stones.Max(s => s.Exp) - 1;
        int[] minGold = new int[maxExp + 1];

        const int INF = 999999;
        for (int i = 1; i <= maxExp; i++) minGold[i] = INF;
        minGold[0] = 0;

        foreach (var stone in stones)
        {
            for (int i = stone.Exp; i <= maxExp; i++)
            {
                minGold[i] = Math.Min(minGold[i], minGold[i - stone.Exp] + stone.Price);
            }
        }

        int minCost = INF;
        int targetTotalExp = requiredExp;

        for (int i = requiredExp; i <= maxExp; i++)
        {
            if (minGold[i] < minCost)
            {
                minCost = minGold[i];
                targetTotalExp = i;
            }
        }

        return new PurchaseResult
        {
            TotalGold = minCost,
            TotalExp = targetTotalExp,
            WastedExp = targetTotalExp - requiredExp
        };
    }

    public PurchaseResult CalculateGreedyMinWaste(int requiredExp)
    {
        PurchaseResult result = new PurchaseResult();
        int remainingExp = requiredExp;

        var sortedByExp = stones.OrderByDescending(s => s.Exp).ToList();

        foreach (var stone in sortedByExp)
        {
            int count = remainingExp / stone.Exp;

            if (count > 0)
            {
                result.StoneCounts[stone.Name] = count;
                result.TotalExp += count * stone.Exp;
                result.TotalGold += count * stone.Price;
                remainingExp -= count * stone.Exp;
            }
        }

        if (remainingExp > 0)
        {
            int count = (remainingExp + smallStone.Exp - 1) / smallStone.Exp;

            if (result.StoneCounts.ContainsKey(smallStone.Name))
            {
                result.StoneCounts[smallStone.Name] += count;
            }
            else
            {
                result.StoneCounts[smallStone.Name] = count;
            }

            int addedExp = count * smallStone.Exp;
            result.TotalExp += addedExp;
            result.TotalGold += count * smallStone.Price;
            remainingExp -= addedExp;
        }

        result.WastedExp = result.TotalExp - requiredExp;
        return result;
    }

    private PurchaseResult RunGreedy(int requiredExp, List<Stone> sortedStones)
    {
        PurchaseResult result = new PurchaseResult();
        int remainingExp = requiredExp;

        foreach (var stone in sortedStones)
        {
            if (remainingExp <= 0) break;

            int count = remainingExp / stone.Exp;

            if (count > 0)
            {
                if (result.StoneCounts.ContainsKey(stone.Name))
                {
                    result.StoneCounts[stone.Name] += count;
                }
                else
                {
                    result.StoneCounts[stone.Name] = count;
                }

                result.TotalExp += count * stone.Exp;
                result.TotalGold += count * stone.Price;
                remainingExp -= count * stone.Exp;
            }
        }

        if (remainingExp > 0)
        {
            int count = (remainingExp + smallStone.Exp - 1) / smallStone.Exp;

            if (result.StoneCounts.ContainsKey(smallStone.Name))
            {
                result.StoneCounts[smallStone.Name] += count;
            }
            else
            {
                result.StoneCounts[smallStone.Name] = count;
            }

            int addedExp = count * smallStone.Exp;
            result.TotalExp += addedExp;
            result.TotalGold += count * smallStone.Price;
        }

        result.WastedExp = result.TotalExp - requiredExp;
        return result;
    }

    public PurchaseResult CalculateGreedyMaxEfficiency(int requiredExp)
    {
        var sortedByEfficiency = stones.OrderByDescending(s => s.ExpPerGold).ToList();
        return RunGreedy(requiredExp, sortedByEfficiency);
    }

    public PurchaseResult CalculateGreedyMaxExp(int requiredExp)
    {
        var sortedByExp = stones.OrderByDescending(s => s.Exp).ToList();
        return RunGreedy(requiredExp, sortedByExp);
    }
}