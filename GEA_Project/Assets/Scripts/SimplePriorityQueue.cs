using System;
using System.Collections.Generic;

public class SimplePriorityQueue<T>
{
    private class Node
    {
        public T Item;
        public float Priority;

        public Node(T item, float priority)
        {
            Item = item;
            Priority = priority;
        }
    }

    private List<Node> heap = new List<Node>();

    public int Count => heap.Count;

    public void Enqueue(T item, float priority)
    {
        heap.Add(new Node(item, priority));
        HeapifyUp(heap.Count - 1);
    }

    public T Dequeue()
    {
        if (heap.Count == 0)
        {
            throw new InvalidOperationException("Queue is empty");
        }

        T rootItem = heap[0].Item;

        int lastIndex = heap.Count - 1;
        heap[0] = heap[lastIndex];
        heap.RemoveAt(lastIndex);

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
            if (heap[i].Priority >= heap[parent].Priority)
            {
                break;
            }
            (heap[i], heap[parent]) = (heap[parent], heap[i]);
            i = parent;
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

            if (left <= last && heap[left].Priority < heap[smallest].Priority)
            {
                smallest = left;
            }

            if (right <= last && heap[right].Priority < heap[smallest].Priority)
            {
                smallest = right;
            }

            if (smallest == i)
            {
                break;
            }

            (heap[i], heap[smallest]) = (heap[smallest], heap[i]);
            i = smallest;
        }
    }

    public void UpdatePriority(T item, float newPriority)
    {
        for (int i = 0; i < heap.Count; i++)
        {
            if (heap[i].Item.Equals(item))
            {
                float oldPriority = heap[i].Priority;
                heap[i].Priority = newPriority;

                if (newPriority < oldPriority)
                {
                    HeapifyUp(i);
                }
                else
                {
                    HeapifyDown(i);
                }
                return;
            }
        }
        Enqueue(item, newPriority);
    }
}