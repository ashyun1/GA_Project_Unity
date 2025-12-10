using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreedyCoinSample : MonoBehaviour
{

    //코인 종류가 내림차순으로 정렬되어 있어야 함.
    int[] coinType = { 500, 100, 50, 10 };

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(CountCoins(1260));
        
    }

    int CountCoins(int amount)
    {
        int count = 0;

        foreach (int c in coinType)
        {
            int use = amount / c;
            count += use;
            amount -= use * c;
        }

        return count;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
