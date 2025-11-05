using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class BruteForceSample : MonoBehaviour
{
    public Button startButton;

    string secretPin;

    Coroutine runningRoutine;

    // Start is called before the first frame update
    void Start()
    {
        secretPin = Random.Range(0, 10000).ToString("D4");
        Debug.Log($"[Auth] 생성된 PIN = {secretPin}");

    }

    public void OnStartButtonClicked()
    {
        if (runningRoutine != null)
        {
            Debug.Log("[Brute] 이미 실행중입니다.");
            return;
        }
        runningRoutine = StartCoroutine(BruteForceRoutine());

    }

    IEnumerator BruteForceRoutine()
    {
        Debug.Log("[Brute] 시뮬레이션 시작 ");

        Stopwatch sw = new Stopwatch();
        sw.Start();

        int tryCount = 0;
        int max = 10000;

        for (int i = 0; i < max; i++)
        {
            string tryString = i.ToString("D4");
            tryCount++;

            if (tryString == secretPin)
            {
                sw.Stop();
                double seconds = sw.Elapsed.TotalSeconds;
                Debug.Log($"[Brute] 성공! PIN={tryString} 시도수={tryCount} 소요={seconds:F3}초");
                runningRoutine = null;
                yield break;
            }
            if (i % 100 == 0)
            {
                yield return null;
            }
        }

        sw.Stop();
        Debug.Log($"[Brute] 모든 조합 시도 완료(발견 실패). 소요={sw.Elapsed.TotalSeconds:F3}초");
        runningRoutine = null;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
