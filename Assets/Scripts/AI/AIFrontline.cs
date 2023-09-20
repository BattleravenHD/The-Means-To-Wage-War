using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIFrontline : MonoBehaviour
{
    public AiFactionCore aiFaction;
    public int CurrentPriority = 0;

    public List<Tile> frontLineTiles;
    public List<Unit> frontLineUnits;

    public UnitScriptableObject[] possibleUnits;

    public bool doneCreating = false;

    int Neededunits;

    public UnitScriptableObject[] CreateRequests()
    {
        List<UnitScriptableObject> needed = new List<UnitScriptableObject>();
        if (frontLineUnits.Count < frontLineTiles.Count)
        {
            Neededunits = frontLineTiles.Count;
        }
        for (int i = 0; i < Neededunits; i++)
        {
            needed.Add(possibleUnits[0]);
        }
        return needed.ToArray();
    }

    public IEnumerator CreateTurnOrders()
    {
        doneCreating = false;

        // Get tiles withou units
        List<Tile> tilesWithoutunits = new List<Tile>();
        foreach (Tile item in frontLineTiles)
        {
            if (!frontLineUnits.Contains(item.CurrentGroundUnit))
            {
                tilesWithoutunits.Add(item);
            }
        }

        HashSet<Unit> UtalisedUnits = new HashSet<Unit>();

        // Get units to the front first
        foreach (Unit unit in frontLineUnits)
        {
            if (!frontLineTiles.Contains(unit.CurrentTile) && tilesWithoutunits.Count > 0)
            {
                unit.Orders = PathFinding.PathToCreateOrder(unit.CurrentTile, tilesWithoutunits[0], unit);
                tilesWithoutunits.RemoveAt(0);
                UtalisedUnits.Add(unit);
            }
        }

        List<Tile> oposingTiles = new List<Tile>();
        // Get all tiles oposing the Front
        foreach (Tile tile in frontLineTiles)
        {
            foreach (Tile adjacentTile in tile.AjacentTiles())
            {
                if (adjacentTile == null) continue;
                if (adjacentTile.FactionOwnerShip != aiFaction.factionData.FactionID)
                {
                    oposingTiles.Add(adjacentTile);
                }
            }
        }

        yield return new WaitForSeconds(0);

        List<AttackDeterminer> possibleAttacks = new List<AttackDeterminer>();

        // Create Possible Moves
        foreach (Tile enemyTile in oposingTiles)
        {
            List<List<Unit>> totalUnits = unitsInRange(enemyTile, 3);

            possibleAttacks.Add(new AttackDeterminer(totalUnits[0], totalUnits[1], enemyTile));
            yield return new WaitForSeconds(0);
        }

        possibleAttacks.Sort(SortAttacks);
        // make them happen if unit is not already used 
        foreach (AttackDeterminer item in possibleAttacks)
        {
            // if no oposition
            if (item.Chance == 100)
            {
                foreach (Unit possibleunit in item.attackingUnits)
                {
                    if (frontLineUnits.Contains(possibleunit) && !UtalisedUnits.Contains(possibleunit))
                    {
                        possibleunit.Orders = PathFinding.PathToCreateOrder(possibleunit.CurrentTile, item.GivingTile, possibleunit);
                        UtalisedUnits.Add(possibleunit);
                    }
                }
            }
            else
            {
                if (UtalisedUnits.Count == frontLineUnits.Count)
                {
                    // No units to give orders too
                    break;
                }
                else if (item.Chance > 0)
                {
                    List<Unit> unitsNotAssignedFromPossibleAttack = new List<Unit>();
                    foreach (Unit possibleAttackers in item.attackingUnits)
                    {
                        if (frontLineUnits.Contains(possibleAttackers) && !UtalisedUnits.Contains(possibleAttackers))
                        {
                            unitsNotAssignedFromPossibleAttack.Add(possibleAttackers);
                        }
                    }
                    item.attackingUnits = unitsNotAssignedFromPossibleAttack;
                    item.CalculateChance();
                    if (item.Chance > 0 && item.attackingUnits.Count > 0)
                    {
                        item.attackingUnits[0].Orders = PathFinding.PathToCreateOrder(item.attackingUnits[0].CurrentTile, item.GivingTile, item.attackingUnits[0], false, false, item.attackingUnits);
                        foreach (Unit attackingUnit in item.attackingUnits)
                        {
                            UtalisedUnits.Add(attackingUnit);
                        }
                    }
                }
            }
        }

        doneCreating = true;
    }

    int SortAttacks(AttackDeterminer front1, AttackDeterminer front2)
    {
        if (front1.Chance < front2.Chance)
        {
            return -1;
        }
        else if (front1.Chance > front2.Chance)
        {
            return 1;
        }
        return 0;
    }

    List<List<Unit>> unitsInRange(Tile tile, int range)
    {
        List<Tile> toVisit = new List<Tile> { tile };
        List<Unit> Friendlyunits = new List<Unit>();
        List<Unit> Enemyunits = new List<Unit>();

        for (int i = 0; i < range; i++)
        {
            int count = toVisit.Count;
            for (int x = 0; x < count; x++)
            {
                foreach (Tile adjcentTile in toVisit[x].AjacentTiles())
                {
                    if (adjcentTile == null) continue;

                    if (!toVisit.Contains(adjcentTile))
                    {
                        toVisit.Add(adjcentTile);
                        if (adjcentTile.CurrentGroundUnit != null)
                        {
                            if (adjcentTile.CurrentGroundUnit.FactionID == aiFaction.factionData.FactionID)
                            {
                                Friendlyunits.Add(adjcentTile.CurrentGroundUnit);
                            }
                            else
                            {
                                Enemyunits.Add(adjcentTile.CurrentGroundUnit);
                            }
                        }
                        if (adjcentTile.CurrentAirUnit != null)
                        {
                            if (adjcentTile.CurrentAirUnit.FactionID == aiFaction.factionData.FactionID)
                            {
                                Friendlyunits.Add(adjcentTile.CurrentAirUnit);
                            }
                            else
                            {
                                Enemyunits.Add(adjcentTile.CurrentAirUnit);
                            }
                        }
                    }
                }
            }
        }
        List<List<Unit>> end = new List<List<Unit>>();
        end.Add(Friendlyunits);
        end.Add(Enemyunits);
        return end;
    }

    public void UpdateFrontLine(List<Tile> possibleFrontLineTiles, HashSet<Tile> FrontBorderTiles)
    {
        Tile newCenter = null;
        foreach (Tile item in frontLineTiles)
        {
            if (possibleFrontLineTiles.Contains(item))
            {
                newCenter = item;
                break;
            }
        }
        frontLineTiles.Clear();
        if (newCenter == null)
        {
            return;
        }

        bool left = false;

        foreach (Tile adjcentTile in newCenter.AjacentTiles())
        {
            if (possibleFrontLineTiles.Contains(adjcentTile) && !FrontBorderTiles.Contains(adjcentTile))
            {
                AddToSide(frontLineTiles, possibleFrontLineTiles, adjcentTile, left);
                left = !left;
            }
        }

    }

    void AddToSide(List<Tile> front, List<Tile> tilesWithoutFronts, Tile currentTile, bool addToLeft)
    {
        if (addToLeft)
        {
            front.Insert(0, currentTile);
        }
        else
        {
            front.Add(currentTile);
        }

        if (front.Count < GameSettings.AIFRONTLINEMAXSIZE)
        {
            foreach (Tile adjcentTile in currentTile.AjacentTiles())
            {
                if (tilesWithoutFronts.Contains(adjcentTile) && !front.Contains(adjcentTile))
                {
                    AddToSide(front, tilesWithoutFronts, adjcentTile, addToLeft);
                    break;
                }
            }
        }

    }
}

public class AttackDeterminer
{
    public Tile GivingTile;
    public List<Unit> attackingUnits;
    public List<Unit> possibleDefenders;

    public int Chance = 0;

    public AttackDeterminer(List<Unit> attackers, List<Unit> defenders, Tile tile)
    {
        attackingUnits = attackers;
        possibleDefenders = defenders;
        GivingTile = tile;
        CalculateChance();
    }

    public void CalculateChance()
    {
        if (possibleDefenders.Count == 0 || GivingTile.CurrentGroundUnit == null)
        {
            Chance = 100;
            return;
        }
        foreach (Unit item in attackingUnits)
        {
            Chance += item.StrengthRating();
        }
        foreach (Unit item in possibleDefenders)
        {
            Chance -= item.StrengthRating();
        }
    }
}
