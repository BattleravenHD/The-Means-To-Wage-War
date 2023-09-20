using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyGroundUnit : GroundUnit
{
    public int SupplyDistributionRate = 20;
    public int SupplyRadius = 3;

    // This is to stop backfilling of supply whilie allowing for multipules supply units to fill each other
    public List<SupplyGroundUnit> Suppliers = new List<SupplyGroundUnit>();

    public override void LoadUnitData(UnitScriptableObject data, FactionData factionData)
    {
        base.LoadUnitData(data, factionData);
        SupplyDistributionRate = data.SupplyDistributionRate;
        SupplyRadius = data.SupplyRadius;
    }

    public override int CreateAttack(Tile combatTile, Tile StartTile, int roll, float bonus)
    {
        return 0;
    }

    public override int CreateDefence(Tile combatTile, int roll, float bonus)
    {
        throw new System.NotImplementedException();
    }
    
}
