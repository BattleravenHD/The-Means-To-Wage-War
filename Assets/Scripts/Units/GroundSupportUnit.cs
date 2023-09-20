using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSupportUnit : GroundUnit
{
    public int SupportRange;

    public override void LoadUnitData(UnitScriptableObject data, FactionData factionData)
    {
        base.LoadUnitData(data, factionData);
        SupportRange = data.SupportRange;
    }

    public override int CreateAttack(Tile combatTile, Tile StartTile, int roll, float bonus)
    {
        // Returns attack value 
        // Attack is attack value clamped to min of 1 (independant of rolling) * supply status * roll * bonus 
        return Mathf.RoundToInt(Mathf.Clamp(Attack, 1, float.MaxValue) *
            (Mathf.Clamp(CurrentHealth + MaxHealth * GameSettings.UNITHEALTHCOMBATEFFECTIVENESS, 0, MaxHealth) / MaxHealth) *
            Mathf.Clamp(roll, 1, int.MaxValue)
            * bonus * combatTile.DefenceMultiplier() * StartTile.AttackMultiplier() * SupplyMalice());
    }

    public override int CreateDefence(Tile combatTile, int roll, float bonus)
    {
        // Returns attack value 
        // Attack is attack value clamped to min of 1 (independant of rolling) * supply status * roll * bonus 
        return Mathf.CeilToInt(Mathf.Clamp(Defence, 1, float.MaxValue) *
            (Mathf.Clamp(CurrentHealth + MaxHealth * GameSettings.UNITHEALTHCOMBATEFFECTIVENESS, 0, MaxHealth) / MaxHealth) *
            Mathf.Clamp(roll, 1, int.MaxValue)
            * bonus * combatTile.DefenceMultiplier() * SupplyMalice());
    }
}
