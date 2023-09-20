using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnUnit : Order
{
    public UnitScriptableObject ToSpawn;
    public TurnRunner runner;
    public int FactionID;
    public SpawnUnit (UnitScriptableObject unit, Tile targetTile, TurnRunner runner, int FactionID)
    {
        this.StartTile = targetTile;
        this.EndTile = targetTile;
        this.ToSpawn = unit;
        this.OrderSpeed = -1;
        this.runner = runner;
        this.FactionID = FactionID;
    }

    // Returns 0 If spawn with no Issues
    // Returns -1 if tile has become ocupied
    public override int CompleteOrder()
    {
        if (ToSpawn.UnitModel.TryGetComponent<GroundUnit>(out GroundUnit IgnoreMe))
        {
            if (EndTile.CurrentGroundUnit == null)
            {
                unit = runner.SpawnGroundUnit(ToSpawn, EndTile, FactionID);
                if (unit != null)
                    Completed = true;

                return 0;
            }
            return -1;
        }
        else if (ToSpawn.UnitModel.TryGetComponent<AirUnit>(out AirUnit IgnoreMe2))
        {
            return -2;
        }
        else
        {
            return -2;
        }
    }
}
