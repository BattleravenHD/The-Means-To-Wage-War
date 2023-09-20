using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The abstract class used to store the data for an order at the top level before being split between the order types

public abstract class Order
{
    public Unit unit;
    public Tile StartTile;
    public Tile EndTile;
    public int OrderSpeed = 1;
    public bool Completed = false;
    public abstract int CompleteOrder();
}
