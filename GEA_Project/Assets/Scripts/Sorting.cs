

using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class Sorting : MonoBehaviour
{
    // ��� �ؽ�Ʈ�� ǥ���� UI Text ������Ʈ
    public Text resultText;

    private Stopwatch sw = new Stopwatch();

    // ���� ���� ��ư Ŭ�� �� ȣ��� �Լ�
    public void RunSelectionSort()
    {
        int[] data = GenerateRandomArray(10000);
        sw.Restart();
        SelectionSort(data);
        sw.Stop();
        long selectionTime = sw.ElapsedMilliseconds;

        UpdateResultText("���� ����", selectionTime);
    }

    // ���� ���� ��ư Ŭ�� �� ȣ��� �Լ�
    public void RunBubbleSort()
    {
        int[] data = GenerateRandomArray(10000);
        sw.Restart();
        BubbleSort(data);
        sw.Stop();
        long bubbleTime = sw.ElapsedMilliseconds;

        UpdateResultText("���� ����", bubbleTime);
    }

    // �� ���� ��ư Ŭ�� �� ȣ��� �Լ�
    public void RunQuickSort()
    {
        int[] data = GenerateRandomArray(10000);
        sw.Restart();
        QuickSort(data, 0, data.Length - 1);
        sw.Stop();
        long quickTime = sw.ElapsedMilliseconds;

        UpdateResultText("�� ����", quickTime);
    }

    // ����� UI �ؽ�Ʈ�� ������Ʈ�ϰ� �ֿܼ� ����ϴ� �Լ�
    private void UpdateResultText(string sortName, long time)
    {
        string resultMessage = $"{sortName}: {time} ms";

        // UI �ؽ�Ʈ�� ��� ���
        if (resultText != null)
        {
            resultText.text = resultMessage;
        }

        // ����Ƽ �ֿܼ� ��� ���
        UnityEngine.Debug.Log(resultMessage);
    }

    // --- ���� �˰��� �޼���� (���� �ڵ�� ����) ---
    private int[] GenerateRandomArray(int size)
    {
        int[] data = new int[size];
        for (int i = 0; i < size; i++)
        {
            data[i] = Random.Range(0, size * 2);
        }
        return data;
    }

    private void SelectionSort(int[] arr)
    {
        int n = arr.Length;
        for (int i = 0; i < n - 1; i++)
        {
            int minIdx = i;
            for (int j = i + 1; j < n; j++)
            {
                if (arr[j] < arr[minIdx])
                {
                    minIdx = j;
                }
            }
            int temp = arr[minIdx];
            arr[minIdx] = arr[i];
            arr[i] = temp;
        }
    }

    private void BubbleSort(int[] arr)
    {
        int n = arr.Length;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                if (arr[j] > arr[j + 1])
                {
                    int temp = arr[j];
                    arr[j] = arr[j + 1];
                    arr[j + 1] = temp;
                }
            }
        }
    }

    private void QuickSort(int[] arr, int low, int high)
    {
        if (low < high)
        {
            int pi = Partition(arr, low, high);
            QuickSort(arr, low, pi - 1);
            QuickSort(arr, pi + 1, high);
        }
    }

    private int Partition(int[] arr, int low, int high)
    {
        int pivot = arr[high];
        int i = (low - 1);
        for (int j = low; j < high; j++)
        {
            if (arr[j] < pivot)
            {
                i++;
                int temp = arr[i];
                arr[i] = arr[j];
                arr[j] = temp;
            }
        }
        int temp1 = arr[i + 1];
        arr[i + 1] = arr[high];
        arr[high] = temp1;
        return i + 1;
    }
}