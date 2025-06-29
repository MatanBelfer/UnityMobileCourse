using System;
using UnityEngine;

public class GridPoint :ObjectPoolInterface
{
    public int GridRow { get; set; }
    public int column;
    public bool isBlocked;
}
