using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class Move : Order
{

    // Returns 0 If complete move with no issues 
    // Returns -1 if out of Action Points 
    // Returns -2 if the destination is already ocupied
    // Returns -3 if ground unit trys to move onto the ocean
    // returns -4 if unit isnt at starting location

    // Always move ignores tile terrian cost and currentAction Points
    bool AlwaysMove = false;

    public Move(Tile startTile, Tile endTile, Unit unit, bool AlwaysMove=false)
    {
        this.StartTile = startTile;
        this.EndTile = endTile;
        this.unit = unit;
        OrderSpeed = GameSettings.SLOWESTORDERSPEED - unit.MaxActionPoints;
        this.AlwaysMove = AlwaysMove;
    }

    public override int CompleteOrder()
    {
        if (unit.CurrentHealth <= 0)
        {
            return -1;
        }
        if (unit.CurrentActionPoints <= 0)
        {
            return -1;
        }
        if (unit.CurrentTile != StartTile)
        {
            return -4;
        }
        if (unit.TryGetComponent<GroundUnit>(out GroundUnit IgnoreMeImHereOnlyForExistanceCheck))
        {
            if (EndTile.CurrentGroundUnit != null)
            {
                return -2;
            }
            if (EndTile.tileTerrian == TileTerrianTypes.Ocean)
            {
                return -3;
            }
        }else if (unit.TryGetComponent<AirUnit>(out AirUnit IgnoreMeImHereOnlyForExistanceCheck2))
        {
            if (EndTile.CurrentAirUnit != null)
            {
                return -2;
            }
        }
        // Always move ignores tile terrian cost and currentAction Points
        if (!AlwaysMove)
        {
            unit.CurrentActionPoints -= unit.TileTerrianCost(EndTile);
            if (unit.CurrentActionPoints < 0)
            {
                // if action point cost for tile too much return to end order chain
                unit.CurrentActionPoints += unit.TileTerrianCost(EndTile);
                return -1;
            }
        }
        
        WaitForUnitMove();

        return 0;
    }

    async void WaitForUnitMove()
    {
        float percent = 0.0f;
        while (Vector3.Distance(unit.gameObject.transform.position, EndTile.Location) > 0.01f)
        {
            unit.MoveUnit(StartTile, EndTile, percent);
            percent += Time.deltaTime * GameSettings.UNITMOVESPEEDMULTIPLIER;
            await Task.Delay(Mathf.RoundToInt(Time.deltaTime * 1000));
        }
        unit.MoveUnit(EndTile);

        Completed = true;
    }

}
