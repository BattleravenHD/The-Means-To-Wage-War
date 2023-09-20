using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : Order
{
    UiManager uiManager;

    public int attackingFactionID;
    public int defendingFactionID;
    public List<Unit> Attackers;

    public Attack(Tile startTile, Tile endTile, Unit unit)
    {
        this.StartTile = startTile;
        this.EndTile = endTile;
        this.unit = unit;
        OrderSpeed = GameSettings.SLOWESTORDERSPEED - unit.MaxActionPoints;
        uiManager = GameObject.FindGameObjectWithTag("UI Manager").GetComponent<UiManager>();
        attackingFactionID = unit.FactionID;
        defendingFactionID = endTile.CurrentGroundUnit.FactionID;
    }

    public Attack(Tile startTile, Tile endTile, Unit unit, List<Unit> extra)
    {
        this.StartTile = startTile;
        this.EndTile = endTile;
        this.unit = unit;
        OrderSpeed = GameSettings.SLOWESTORDERSPEED - unit.MaxActionPoints;
        uiManager = GameObject.FindGameObjectWithTag("UI Manager").GetComponent<UiManager>();
        attackingFactionID = unit.FactionID;
        defendingFactionID = endTile.CurrentGroundUnit.FactionID;
        Attackers = extra;
    }

    // Returns 0 If complete move with no issues 
    // Returns -1 if out of Action Points 
    // Returns -2 if ground unit trys to move onto the ocean
    public override int CompleteOrder()
    {
        if (unit.CurrentHealth <= 0)
        {
            return -1;
        }
        if (EndTile.tileTerrian == TileTerrianTypes.Ocean)
        {
            return -2;
        }
        if (unit.CurrentActionPoints != unit.MaxActionPoints || unit.CurrentActionPoints < GameSettings.ACTIONCOSTREDUCTION)
        {
            return -1;
        }
        if (unit.TryGetComponent<GroundUnit>(out GroundUnit ignore) && EndTile.CurrentGroundUnit == null)
        {
            Completed = true;
            return 0;
        }
        if (unit.FactionID == GameSettings.PLAYERFACTIONID || EndTile.CurrentGroundUnit.FactionID == GameSettings.PLAYERFACTIONID)
        {
            uiManager.combatUIManager.currentAttackOrder = this;
            uiManager.OpenCombat();
            if (Attackers != null)
            {
                uiManager.combatUIManager.StartCombatInvolveingPlayer(EndTile, unit, Attackers.ToArray());
            }
            else
            {
                uiManager.combatUIManager.StartCombatInvolveingPlayer(EndTile, unit);
            }
        }
        else
        {
            // 2 AI duking it out (To be Implemented eventually tm)
            Completed = true;
        }

        return 0;
    }
}
