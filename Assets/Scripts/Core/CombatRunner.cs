using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CombatRunner : MonoBehaviour
{
    public List<CombatMatchUp> Combat(List<Unit> Attackers, List<Unit> Defenders)
    {
        List<UnitInfo> AttackerRolls = new List<UnitInfo>();

        float attackerSum = 0;
        float defenderSum = 0; 

        foreach (Unit unit in Attackers)
        {
            int roll = Random.Range(0, 21) + unit.Attack;
            AttackerRolls.Add(new UnitInfo(unit, roll));
            attackerSum += roll;
        }

        List<UnitInfo> DefenderRolls = new List<UnitInfo>();

        foreach (Unit unit in Defenders)
        {
            int roll = Random.Range(0, 21) + unit.Defence;
            DefenderRolls.Add(new UnitInfo(unit, roll));
            defenderSum += roll;
        }

        float attackerBonus = Mathf.Clamp(attackerSum / defenderSum, 0.2f, 2f);
        float DefenderBonus = Mathf.Clamp(defenderSum / attackerSum, 0.2f, 2f);

        List<CombatMatchUp> combatMatches = new List<CombatMatchUp>();
        List<UnitInfo> attackerSupportElements = new List<UnitInfo>();
        List<UnitInfo> defenderSupportElements = new List<UnitInfo>();

        List<UnitInfo> tempList1 = new List<UnitInfo>();

        foreach (UnitInfo attacker in AttackerRolls)
        {
            if (attacker.unit.TryGetComponent<GroundSupportUnit>(out GroundSupportUnit IgnoreMeImHereOnlyForExistanceCheck))
            {
                attackerSupportElements.Add(attacker);
            }else
            {
                tempList1.Add(attacker);
            }
        }
        AttackerRolls = tempList1;

        List<UnitInfo> tempList2 = new List<UnitInfo>();

        foreach (UnitInfo defender in DefenderRolls)
        {
            if (defender.unit.TryGetComponent<GroundSupportUnit>(out GroundSupportUnit IgnoreMeImHereOnlyForExistanceCheck))
            {
                defenderSupportElements.Add(defender);
            }
            else
            {
                tempList2.Add(defender);
            }
        }

        DefenderRolls = tempList2;

        if (AttackerRolls.Count >= DefenderRolls.Count)
        {
            int unitIndex = 0;
            for (float i = 0; i < Defenders.Count; i += (float)DefenderRolls.Count/ (float)AttackerRolls.Count)
            {
                combatMatches.Add(new CombatMatchUp(Attackers[unitIndex], Defenders[Mathf.FloorToInt(i)], AttackerRolls[unitIndex].diceRoll, Mathf.RoundToInt(DefenderRolls[Mathf.FloorToInt(i)].diceRoll * (Defenders.Count / Attackers.Count)), attackerBonus, DefenderBonus));
                unitIndex++;
            }
        }else
        {
            int unitIndex = 0;
            for (float i = 0; i < Attackers.Count; i += (float)AttackerRolls.Count / (float)DefenderRolls.Count)
            {
                combatMatches.Add(new CombatMatchUp(Attackers[Mathf.FloorToInt(i)], Defenders[unitIndex], Mathf.RoundToInt(AttackerRolls[Mathf.FloorToInt(i)].diceRoll * (Attackers.Count / Defenders.Count)), DefenderRolls[unitIndex].diceRoll, attackerBonus, DefenderBonus));
                unitIndex++;
            }
        }

        int attackerSupportFire = 0;
        int defenderSupportFire = 0;

        foreach (UnitInfo item in attackerSupportElements)
        {
            attackerSupportFire += item.unit.CreateAttack(Defenders[0].CurrentTile, Attackers[0].CurrentTile, item.diceRoll, attackerBonus);
        }
        foreach (UnitInfo item in defenderSupportElements)
        {
            defenderSupportFire += item.unit.CreateAttack(Defenders[0].CurrentTile, Attackers[0].CurrentTile, item.diceRoll, attackerBonus);
        }

        attackerSupportFire = Mathf.RoundToInt(attackerSupportFire / DefenderRolls.Count);
        defenderSupportFire = Mathf.RoundToInt(defenderSupportFire / AttackerRolls.Count);
        int defenderOGHealth = Defenders[0].CurrentHealth;
        Tile startTile = Attackers[0].CurrentTile;
        Tile endTile = Defenders[0].CurrentTile;
        foreach (CombatMatchUp item in combatMatches)
        {
            int defenderDamage = item.defender.CreateDefence(endTile, item.defenderRoll, item.defenceBonusPercent);

            if (!item.defender.TakeDamage(item.attacker.CreateAttack(endTile, startTile, item.attackRoll, item.attackBonusPercent)) && !item.defender.TakeDamage(attackerSupportFire))
            {
                Destroy(item.defender.gameObject);
            }else
            {
                item.defender.CurrentActionPoints = Mathf.Clamp(item.defender.CurrentActionPoints - (item.defender.GetType() == typeof(GroundSupportUnit) ? GameSettings.ACTIONCOSTREDUCTION / 2 : GameSettings.ACTIONCOSTREDUCTION), 0, int.MaxValue);
            }

            if (!item.attacker.TakeDamage(defenderDamage) && !item.attacker.TakeDamage(defenderSupportFire))
            {
                Destroy(item.attacker.gameObject);
            }else
            {
                item.attacker.CurrentActionPoints = Mathf.Clamp(item.attacker.CurrentActionPoints - (item.attacker.GetType() == typeof(GroundSupportUnit) ? GameSettings.ACTIONCOSTREDUCTION / 2 : GameSettings.ACTIONCOSTREDUCTION), 0, int.MaxValue);
            }
        }
        if (Defenders[0] != null && Defenders[0].CurrentHealth < defenderOGHealth/2)
        {

            if (Defenders[0].CurrentHealth <= 0)
            {
                return combatMatches;
            }
            Vector3 direction = endTile.Location - startTile.Location;
            direction.Normalize();
            // if pointing up 
            if (direction.z > 0.1f)
            {
                if (direction.x > 0)
                {
                    // left tile 
                    RetreatUnits(endTile, 1, Defenders[0].FactionID);
                }
                else
                {
                    // right tile
                    RetreatUnits(endTile, 0, Defenders[0].FactionID);
                }
            }
            else if (direction.z < -0.1f) // down
            {
                if (direction.x > 0)
                {
                    // left tile 
                    RetreatUnits(endTile, 4, Defenders[0].FactionID);
                }
                else
                {
                    // right tile
                    RetreatUnits(endTile, 3, Defenders[0].FactionID);
                }
            }
            else // Left or right
            {
                if (direction.x > 0)
                {
                    // left tile 
                    RetreatUnits(endTile, 2, Defenders[0].FactionID);
                }else
                {
                    // right tile
                    RetreatUnits(endTile, 5, Defenders[0].FactionID);
                }
            }
        }
        return combatMatches;
    }

    void RetreatUnits(Tile tile, int direction, int factionID)
    {
        Tile desitnationTile = tile.AjacentTiles()[direction];
        
        if (desitnationTile == null)
        {
            return;
        }
        if (desitnationTile.CurrentGroundUnit == null)
        {
            tile.CurrentGroundUnit.CurrentActionPoints = 0;
            tile.CurrentGroundUnit.MoveUnit(desitnationTile);
        }else
        {
            if (desitnationTile.CurrentGroundUnit.FactionID == factionID)
            {
                RetreatUnits(desitnationTile, direction, factionID);
                if (desitnationTile.CurrentGroundUnit == null)
                {
                    tile.CurrentGroundUnit.CurrentActionPoints = 0;
                    tile.CurrentGroundUnit.MoveUnit(desitnationTile);
                }
            }
        }
    }
}

public class CombatMatchUp
{
    public Unit attacker;
    public Unit defender;

    public int attackRoll;
    public int defenderRoll;

    public float defenceBonusPercent;
    public float attackBonusPercent;

    public CombatMatchUp(Unit attacker, Unit defender, int attackRoll, int defenderRoll, float attackbonusPercent, float defenceBonusPercent)
    {
        this.attacker = attacker;
        this.defender = defender;
        this.attackRoll = attackRoll;
        this.defenderRoll = defenderRoll;
        this.attackBonusPercent = attackbonusPercent;
        this.defenceBonusPercent = defenceBonusPercent;
    }
}

public class UnitInfo
{
    public Unit unit;
    public int diceRoll;

    public UnitInfo (Unit unit, int diceRoll)
    {
        this.unit = unit;
        this.diceRoll = diceRoll;
    }
}
