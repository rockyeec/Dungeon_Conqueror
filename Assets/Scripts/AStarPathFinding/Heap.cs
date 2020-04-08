using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heap<T> where T : IHeapable<T>
{
    T[] items;
    int count;

    public int Count
    {
        get
        {
            return count;
        }
    }

    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];
        count = 0;
    }

    public void Add(T item)
    {
        item.HeapIndex = count;
        items[count] = item;
        SortUp(item);
        count++;
    }

    public T RemoveTop()
    {
        T temp = items[0];
        count--;
        items[0] = items[count];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return temp;
    }

    public bool Contains(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }



    void SortUp(T item)
    {
        while (true)
        {
            int parentIndex = ParentIndex(item);

            if (items[parentIndex].CompareTo(item) <= 0)
                return;

            Swap(items[parentIndex], item);
        }
    }

    void SortDown(T item)
    {
        while (true)
        {
            int leftIndex = LeftChildIndex(item);
            if (leftIndex >= count)
                return;
            
            int swapIndex = leftIndex;

            int rightIndex = RightChildIndex(item);
            if (rightIndex < count)
            {
                if (items[leftIndex].CompareTo(items[rightIndex]) > 0)
                    swapIndex = rightIndex;
            }

            if (item.CompareTo(items[swapIndex]) <= 0)
                return;

            Swap(item, items[swapIndex]);
        }
    }


    void Swap(T a, T b)
    {
        int index = a.HeapIndex;
        a.HeapIndex = b.HeapIndex;
        b.HeapIndex = index;
        items[a.HeapIndex] = a;
        items[b.HeapIndex] = b;
    }

    int ParentIndex(T item)
    {
        return (item.HeapIndex - 1) / 2;
    }

    int LeftChildIndex(T item)
    {
        return 2 * item.HeapIndex + 1;
    }

    int RightChildIndex(T item)
    {
        return 2 * item.HeapIndex + 2;
    }
}

public interface IHeapable<T>
{
    int HeapIndex
    {
        get;
        set;
    }

    int CompareTo(T nodeToCompare);

}

