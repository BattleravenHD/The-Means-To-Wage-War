using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirUnit : Unit
{
    public override int CreateAttack(Tile combatTile, Tile StartTile, int roll, float bonus)
    {
        throw new System.NotImplementedException();
    }

    public override int CreateDefence(Tile combatTile, int roll, float bonus)
    {
        throw new System.NotImplementedException();
    }

    public override void MoveUnit(Tile tile)
    {
        throw new System.NotImplementedException();
    }

    public override void MoveUnit(Tile startTile, Tile endTile, float percent)
    {
        throw new System.NotImplementedException();
    }

    public override int TileTerrianCost(Tile tile)
    {
        throw new System.NotImplementedException();
    }
}
