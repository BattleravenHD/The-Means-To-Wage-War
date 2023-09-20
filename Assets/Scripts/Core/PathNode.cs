using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    public int gCost;
    public int hCost;
    public int fCost;

    public Tile tile;
    public PathNode lastNode;

    public PathNode(Tile tile)
    {
        this.tile = tile;
        gCost = int.MaxValue;
        CalculateFcost();
        lastNode = null;
    }

    public void CalculateFcost()
    {
        fCost = gCost + hCost;
    }
}
