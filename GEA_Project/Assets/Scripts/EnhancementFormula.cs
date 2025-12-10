using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class EnhancementFormula
{
    public static int CalculateRequiredExp(int targetLevel)
    {
        if (targetLevel <= 1) return 0;

        int N = targetLevel - 1;

        return 8 * N * N;
    }
}