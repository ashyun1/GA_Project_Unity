using System.Collections.Generic;

public class SimplePriorityQueue<T>
{
    private List<(T item, float priority)> heap = new List<(T, float)>();

    public int Count => heap.Count;

    public void Enqueue(T item, float priority)
    {
        heap.Add((item, priority));
        HeapifyUp(heap.Count - 1);
    }

    public T Dequeue(out float priority)
    {
        if (heap.Count == 0)
        {
            throw new System.InvalidOperationException("Queue is empty");
        }

        T rootItem = heap[0].item;
        priority = heap[0].priority;

        heap[0] = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);

        if (heap.Count > 0)
        {
            HeapifyDown(0);
        }
        return rootItem;
    }

    private void HeapifyUp(int i)
    {
        while (i > 0)
        {
            int parent = (i - 1) / 2;

            if (heap[i].priority < heap[parent].priority)
            {
                Swap(i, parent);
                i = parent;
            }
            else
            {
                break;
            }
        }
    }

    private void HeapifyDown(int i)
    {
        int last = heap.Count - 1;
        while (true)
        {
            int left = 2 * i + 1;
            int right = 2 * i + 2;
            int smallest = i;

            if (left <= last && heap[left].priority < heap[smallest].priority)
            {
                smallest = left;
            }

            if (right <= last && heap[right].priority < heap[smallest].priority)
            {
                smallest = right;
            }

            if (smallest != i)
            {
                Swap(i, smallest);
                i = smallest;
            }
            else
            {
                break;
            }
        }
    }

    private void Swap(int i, int j)
    {
        var temp = heap[i];
        heap[i] = heap[j];
        heap[j] = temp;
    }
}