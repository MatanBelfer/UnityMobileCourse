using System.Collections.Generic;

public static class CollectionUtilities
{
    public static int IncrementWrap(int number, int increment, int maxExclusive)
    {
        //given a number and an increment (positive or negative) 
        //return number+increment wrapped within [0,max]
        number += increment;
        if (number < 0)
        {
            number += maxExclusive * (-number / maxExclusive + 1);
        }
        return number % maxExclusive;
    }
    
    public static LinkedListNode<T> CyclicNext<T>(this LinkedListNode<T> node)
    {
        return node.Next ?? node.List.First;
    }
    
    public static LinkedListNode<T> CyclicPrevious<T>(this LinkedListNode<T> node)
    {
        return node.Previous ?? node.List.Last;
    }

    public static LinkedListNode<T> CyclicAddAfter<T>(this LinkedList<T> list, LinkedListNode<T> node, T valueToAdd)
    {
        if (node.Next == null)
        {
            return list.AddFirst(valueToAdd);
        }
        else
        {
            return list.AddAfter(node, valueToAdd);
        }
    }

    public static LinkedList<T> ToLinkedList<T>(this IEnumerable<T> enumerable)
    {
        var list = new LinkedList<T>();
        foreach (var item in enumerable)
        {
            list.AddLast(item);
        }
        return list;
    }
}
