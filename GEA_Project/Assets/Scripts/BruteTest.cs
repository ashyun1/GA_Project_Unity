using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BruteTest : MonoBehaviour
{
    void Start()
    {
        int maxCost = 15;
        int maxDamage = 0;

        int quickShotCost = 2, quickShotDamage = 6;
        int heavyShotCost = 3, heavyShotDamage = 8;
        int multiShotCost = 5, multiShotDamage = 16;
        int tripleShotCost = 7, tripleShotDamage = 24;

        for (int quick = 0; quick <= 2; quick++)
        {
            for (int heavy = 0; heavy <= 2; heavy++)
            {
                for (int multi = 0; multi <= 1; multi++)
                {
                    for (int triple = 0; triple <= 1; triple++)
                    {
                        int totalCost = quick * quickShotCost + heavy * heavyShotCost +
                                         multi * multiShotCost + triple * tripleShotCost;

                        if (totalCost <= maxCost)
                        {
                            int totalDamage = quick * quickShotDamage + heavy * heavyShotDamage +
                                              multi * multiShotDamage + triple * tripleShotDamage;

                            if (totalDamage > maxDamage)
                            {
                                maxDamage = totalDamage;

                                Debug.Log($"새로운 최고 조합 발견! " +
                                          $"퀵:{quick}, 헤비:{heavy}, 멀티:{multi}, 트리플:{triple} → " +
                                          $"코스트:{totalCost}, 데미지:{totalDamage}");
                            }
                        }
                    }
                }
            }
        }

        Debug.Log($"최대 데미지: {maxDamage}");
    }
}
