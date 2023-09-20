using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiFactionCore : MonoBehaviour
{
    public GameData gameData;
    public FactionData factionData;
    public UnitScriptableObject[] possibleUnits;
    public TurnRunner runner;
    public GameObject debugObj;

    public GameObject FrontlinePrefab;
    public List<AIFrontline> aIFrontlines = new List<AIFrontline>();
    public List<Tile> borderTiles = new List<Tile>();
    public List<Unit> ReserveUnits = new List<Unit>();

    List<SpawnUnit> reserveWating = new List<SpawnUnit>();

    public bool Ready = false;

    public void CreateBorderTiles()
    {
        borderTiles.Clear();
        HashSet<Tile> VisitedTiles = new HashSet<Tile>();
        List<Tile> tiles = new List<Tile>();

        foreach (City item in factionData.FactionCities)
        {
            tiles.Add(item.CentreTile);
        }

        while (tiles.Count > 0)
        {
            bool isBorderTile = false;

            foreach (Tile adjcentTile in tiles[0].AjacentTiles())
            {
                if (adjcentTile == null) continue;
                
                if (adjcentTile.FactionOwnerShip != factionData.FactionID)
                {
                    isBorderTile = true;
                }else if (!VisitedTiles.Contains(adjcentTile))
                {
                    tiles.Add(adjcentTile);
                } 
            }
            if (isBorderTile && !borderTiles.Contains(tiles[0]))
            {
                borderTiles.Add(tiles[0]);
            }
            VisitedTiles.Add(tiles[0]);
            tiles.RemoveAt(0);
        }
    }
    public IEnumerator CalculateTurn()
    {
        Ready = false;
        // Get any units that were spawned last turn
        foreach (SpawnUnit item in reserveWating)
        {
            ReserveUnits.Add(item.unit);
        }
        reserveWating.Clear();

        
        HashSet<Tile> FrontBorderTiles = new HashSet<Tile>();
        List<AIFrontline> frontlinesRequiringNewTiles = new List<AIFrontline>();
        CreateBorderTiles();

        // First check that all border tiles are covered by a front
        foreach (AIFrontline front in aIFrontlines)
        {
            front.UpdateFrontLine(borderTiles, FrontBorderTiles);
            if (front.frontLineTiles.Count <= 0)
            {
                frontlinesRequiringNewTiles.Add(front);
            }
            else
            {
                foreach (Tile item in front.frontLineTiles)
                {
                    FrontBorderTiles.Add(item);
                }
            }
            yield return new WaitForSeconds(0);
        }

        
        yield return new WaitForSeconds(0);
        List<Tile> tilesWithoutFronts = new List<Tile>();
        // Then compare all border tiles against this to see if any new fronts are needed
        foreach (Tile tile in borderTiles)
        {
            if (!FrontBorderTiles.Contains(tile))
            {
                tilesWithoutFronts.Add(tile);
            }
        }
        yield return new WaitForSeconds(0);

        // Then for each tile start creating new fronts 
        while (tilesWithoutFronts.Count > 0)
        {
            List<Tile> currentFront = new List<Tile>();
            Tile currentTile = tilesWithoutFronts[0];
            currentFront.Add(currentTile);

            List<Tile> tilesToCheckToAddToFront = new List<Tile>();
            bool left = false;

            foreach (Tile adjcentTile in currentTile.AjacentTiles())
            {
                if (tilesWithoutFronts.Contains(adjcentTile) && !currentFront.Contains(adjcentTile))
                {
                    AddToSide(currentFront, tilesWithoutFronts, adjcentTile, left);
                    left = !left;
                }
            }

            if (frontlinesRequiringNewTiles.Count <= 0)
            {
                AIFrontline newFront = Instantiate(FrontlinePrefab, transform).GetComponent<AIFrontline>();
                newFront.frontLineTiles = currentFront;
                newFront.possibleUnits = possibleUnits;
                newFront.aiFaction = this;
                aIFrontlines.Add(newFront);
            }else
            {
                frontlinesRequiringNewTiles[0].frontLineTiles = currentFront;
                frontlinesRequiringNewTiles.RemoveAt(0);
            }
            
            foreach (Tile item in currentFront)
            {
                tilesWithoutFronts.Remove(item);
                //Instantiate(debugObj, item.Location, Quaternion.identity).name = currentFront.IndexOf(item).ToString();
            }
            yield return new WaitForSeconds(0);
        }

        List<UnitScriptableObject> unitCreationRequests = new List<UnitScriptableObject>();

        // Sort frontlines based on the priority for new units and 
        aIFrontlines.Sort(SortByPriority);

        foreach (AIFrontline front in aIFrontlines)
        {
            // get the requests for new units 
            UnitScriptableObject[] unitRequests = front.CreateRequests();
            List<UnitScriptableObject> remainingRequests = new List<UnitScriptableObject>();
            // see if the units are avalibel in reserve
            foreach (UnitScriptableObject unitRequest in unitRequests)
            {
                Unit Foundunit = null;

                foreach (Unit reserveUnit in ReserveUnits)
                {
                    if (reserveUnit == null) continue;
                    if (reserveUnit.unitType == unitRequest)
                    {
                        Foundunit = reserveUnit;
                    }
                }
                if (Foundunit != null)
                {
                    front.frontLineUnits.Add(Foundunit);
                    ReserveUnits.Remove(Foundunit);
                }else
                {
                    remainingRequests.Add(unitRequest);
                }
            }

            // if any units were not in reserve add to a list for creation
            unitCreationRequests.AddRange(remainingRequests);

            front.StartCoroutine(front.CreateTurnOrders());
            while (!front.doneCreating)
            {
                yield return new WaitForSeconds(0);
            }         
        }

        // Create a list of tiles that can be used for unit creation
        List<Tile> possibleContructionTiles = new List<Tile>();

        foreach (City city in factionData.FactionCities)
        {
            possibleContructionTiles.Add(city.CentreTile);
            foreach (Tile item in city.CityTiles)
            {
                if (item.FactionOwnerShip == factionData.FactionID && (item.CurrentGroundUnit == null || item.CurrentGroundUnit.Orders.Count > 0))
                {
                    possibleContructionTiles.Add(item);
                }
            }
        }

        //Create as many units ass possible
        bool continuePlaceing = true;
        while (continuePlaceing)
        {
            if (factionData.FactionIPC <= 0 || possibleContructionTiles.Count <= 0 || unitCreationRequests.Count <= 0 || factionData.FactionIPC - unitCreationRequests[0].Cost < 0)
            {
                continuePlaceing = false;
            }else
            {
                SpawnUnit spawn = new SpawnUnit(unitCreationRequests[0], possibleContructionTiles[0], runner, factionData.FactionID);
                factionData.SpawnOrders.Add(spawn);
                reserveWating.Add(spawn);
                factionData.FactionIPC -= unitCreationRequests[0].Cost;
                unitCreationRequests.RemoveAt(0);
                possibleContructionTiles.RemoveAt(0);
            }
        }

        Ready = true;
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

    public List<Unit> CreateDefence(Tile tile)
    {
        List<Unit> defenders = new List<Unit>();
        foreach (Unit item in unitsInRange(tile, 3)[0])
        {
            if (item.CurrentActionPoints > 0)
            {
                defenders.Add(item);
            }
        }
        return defenders;
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
                            if (adjcentTile.CurrentGroundUnit.FactionID == factionData.FactionID)
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
                            if (adjcentTile.CurrentAirUnit.FactionID == factionData.FactionID)
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

    int SortByPriority(AIFrontline front1, AIFrontline front2)
    {
        if (front1.CurrentPriority < front2.CurrentPriority)
        {
            return -1;
        }else if (front1.CurrentPriority > front2.CurrentPriority)
        {
            return 1;
        }
        return 0;
    }
}
