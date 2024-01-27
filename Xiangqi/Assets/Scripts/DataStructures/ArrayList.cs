using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayList<T>
{
    
    private T[] List;
    private int lastIndex;
    
    public ArrayList(int size)
    {
        List = new T[size];
        lastIndex = 0;
    }

    public ArrayList(T[] values)
    {
        List = values;
        lastIndex = values.Length;
    }
   
    public T Get(int index)
    {
        return List[index];
    }

    public void Set(int index, T value)
    {
        List[index] = value;
    }

    public int Size()
    {
        return List.Length;
    }

    public void Add(T value)
    {
        if(lastIndex < List.Length)
            List[lastIndex++] = value;
        else
            throw new System.Exception("ArrayList is full");
    }

    public void Add(ArrayList<T> values)
    {
        for(int i = 0; i < values.Size(); i++)
        {
            Add(values.Get(i));
        }
    }
}
