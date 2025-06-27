using System.Collections.Generic;

public static class CollectionUtilities
{
    public static int IncrementWrap(int number, int increment, int maxExclusive)
    {
        //given a number and an increment (positive or negative) 
        //return number+increment wrapped within [0,max]
        return ((number + increment) % maxExclusive + maxExclusive) % maxExclusive;
    }
    
    public static LinkedListNode<T> NextOrFirst<T>(this LinkedListNode<T> node)
    {
        return node.Next ?? node.List.First;
    }
    
    public static LinkedListNode<T> PreviousOrLast<T>(this LinkedListNode<T> node)
    {
        return node.Previous ?? node.List.Last;
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
    
    public static bool IsLast<T>(this LinkedListNode<T> node) => node.Next == null;
}
