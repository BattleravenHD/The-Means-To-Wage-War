using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding
{
    public static List<Tile> CreatePathTo(Tile startTile, Tile Endtile, bool IgnoreTerrian = false, bool IgnoreUnits = false, Unit searchingUnit = null)
    {
        List<PathNode> openList;
        HashSet<Tile> closedList;
        Dictionary<Tile, PathNode> possibleNodes;
        PathNode startNode = new PathNode(startTile);

        openList = new List<PathNode> { startNode };
        possibleNodes = new Dictionary<Tile, PathNode>();
        possibleNodes.Add(startNode.tile, startNode);

        closedList = new HashSet<Tile>();
        

        startNode.gCost = 0;
        startNode.hCost = CalculateDistance(startTile, Endtile, searchingUnit);
        startNode.CalculateFcost();

        while (openList.Count > 0)
        {
            PathNode currentNode = LowestCostNode(openList);
            if (currentNode.tile == Endtile)
            {
                // the end
                return CalcualtePath(currentNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode.tile);

            foreach (Tile neightborNode in currentNode.tile.AjacentTiles())
            {
                if (neightborNode == null)
                {
                    continue;
                }
                if (closedList.Contains(neightborNode))
                {
                    continue;
                }
                PathNode nodeCheck;
                if (possibleNodes.ContainsKey(neightborNode))
                {
                    nodeCheck = possibleNodes[neightborNode];
                }else
                {
                    nodeCheck = new PathNode(neightborNode);
                    possibleNodes.Add(neightborNode, nodeCheck);
                }

                int PredictiveGcost = currentNode.gCost + CalculateDistance(currentNode.tile, neightborNode, searchingUnit);

                if (PredictiveGcost < nodeCheck.gCost)
                {
                    nodeCheck.lastNode = currentNode;
                    nodeCheck.gCost = PredictiveGcost;
                    nodeCheck.hCost = CalculateDistance(neightborNode, Endtile, searchingUnit);
                    nodeCheck.CalculateFcost();

                    if (!openList.Contains(nodeCheck))
                    {
                        openList.Add(nodeCheck);
                    }
                }
            }
        }

        // cant find path
        return null;
    }

    public static List<Order> PathToCreateOrder(Tile startTile, Tile Endtile, Unit searchingUnit, bool IgnoreTerrian = false, bool IgnoreUnits = false, List<Unit> ExtraAttackers = null)
    {
        Tile[] path = CreatePathTo(startTile, Endtile, IgnoreTerrian, IgnoreUnits, searchingUnit).ToArray();

        List<Order> orders = new List<Order>();
        if (path.Length < 1)
        {
            return null;
        }
        if (path[0].CurrentGroundUnit != null && path[0].CurrentGroundUnit.FactionID != searchingUnit.FactionID)
        {
            if (ExtraAttackers != null)
            {
                orders.Add(new Attack(startTile, path[0], searchingUnit, ExtraAttackers));
            }
            else
            {
                orders.Add(new Attack(startTile, path[0], searchingUnit));
            }
            orders.Add(new Move(startTile, path[0], searchingUnit));
        }
        else
        {
            orders.Add(new Move(startTile, path[0], searchingUnit));
        }
        for (int i = 1; i < path.Length; i++)
        {
            if (searchingUnit.TryGetComponent<GroundUnit>(out GroundUnit IgnoreMeImHereOnlyForExistanceCheck))
            {
                if (path[i].CurrentGroundUnit != null && path[i].CurrentGroundUnit.FactionID != searchingUnit.FactionID)
                {
                    orders.Add(new Attack(path[i - 1], path[i], searchingUnit));
                    orders.Add(new Move(path[i - 1], path[i], searchingUnit));
                }else
                {
                    orders.Add(new Move(path[i - 1], path[i], searchingUnit));
                }
            }
            else if (searchingUnit.TryGetComponent<AirUnit>(out AirUnit IgnoreMeImHereOnlyForExistanceCheck2)) 
            {
                
            }
            else
            {
                // uuuuuuuuhhhhhhhh boat eventualy tm
            }
        }
        return orders;
    }

    static List<Tile> CalcualtePath(PathNode endNode)
    {
        List<Tile> path = new List<Tile>();
        path.Add(endNode.tile);
        PathNode currentNode = endNode.lastNode;
        while(currentNode != null && currentNode.lastNode != null)
        {
            path.Add(currentNode.tile);
            currentNode = currentNode.lastNode;
        }
        path.Reverse();
        return path;
    }

    private static PathNode LowestCostNode(List<PathNode> possibleNodes)
    {
        PathNode lowest = possibleNodes[0];
        foreach (PathNode item in possibleNodes)
        {
            if (item.fCost < lowest.fCost)
            {
                lowest = item;
            }
        }
        return lowest;
    }

    private static int CalculateDistance(Tile startTile, Tile Endtile, Unit searchingUnit = null, bool IgnoreTerrian = false, bool IgnoreUnits = false)
    {
        if (Endtile == null || startTile == null)
        {
            throw new MissingReferenceException();
        }
        if (startTile == Endtile)
        {
            return 0;
        }

        Tile[] adjcent = startTile.AjacentTiles();
        Tile currentNext = adjcent[0];
        int index = 1;
        while (currentNext == null)
        {
            currentNext = adjcent[index];
            index++;
        }

        foreach (Tile tile in adjcent)
        {
            if (tile == null)
            {
                continue;
            }
            if (tile == Endtile)
            {
                if (searchingUnit == null)
                {
                    return (IgnoreTerrian ? 1 : TerrianActionPoints(startTile)) + (IgnoreTerrian ? 1 : TerrianActionPoints(Endtile)) + (IgnoreUnits ? 0 : (currentNext.CurrentGroundUnit == null ? 0 : 1000));
                }
                else
                {
                    return (IgnoreTerrian ? 1 : searchingUnit.TileTerrianCost(startTile)) + (IgnoreTerrian ? 1 : searchingUnit.TileTerrianCost(Endtile)) + (IgnoreUnits ? 0 : (currentNext.CurrentGroundUnit == null ? 0 : 1000));
                }
            }
            else if (Vector3.Distance(tile.Location, Endtile.Location) < Vector3.Distance(currentNext.Location, Endtile.Location))
            {
                currentNext = tile;
            }
        }

        return TerrianActionPoints(startTile) + CalculateDistance(currentNext, Endtile, searchingUnit) + (IgnoreUnits ? 0: (currentNext.CurrentGroundUnit == null ? 0 : 1000));
    }

    // Default Path creation
    static int TerrianActionPoints(Tile tile)
    {
        if (tile == null)
        {
            return int.MaxValue;
        }
        if (tile.HasRoad)
        {
            return 1;   // Faster Movement on roads
        }
        switch (tile.tileTerrian)
        {
            case TileTerrianTypes.GrassLand:
                return 2;
            case TileTerrianTypes.Ocean:
                return 1;
            case TileTerrianTypes.City:
                return 4;
            case TileTerrianTypes.Town:
                return 3;
            case TileTerrianTypes.Hamlet:
                return 2;
            default:
                break;
        }
        return int.MaxValue;
    }

    /*
    // Old Pathfinding Here only as reference
    public List<Order> PathFindTo(Tile TargetTile, Tile StartTile, bool IgnoreTerrian = false, bool IgnoreUnits = false)
    {
        if (TargetTile == null || StartTile == null)
        {
            return null;
        }
        if (TargetTile == StartTile)
        {
            return null;
        }
        List<Order> temp = new List<Order>();
        Tile[] adjcent = StartTile.AjacentTiles();
        Tile currentNext = adjcent[0];
        foreach (Tile tile in adjcent)
        {
            if (tile == null)
            {
                continue;
            }
            if (tile == TargetTile)
            {

                if (tile.CurrentGroundUnit != null && tile.CurrentGroundUnit.FactionID != FactionID)
                {
                    temp.Add(new Attack(StartTile, TargetTile, this));
                    // Creates attack then assumes victory then moves into the givin tile after
                    temp.Add(new Move(StartTile, TargetTile, this, true));
                }
                else
                {
                    temp.Add(new Move(StartTile, TargetTile, this));
                }
                return temp;
            }else if (TargetSuitability(tile, TargetTile, IgnoreTerrian, IgnoreUnits) < TargetSuitability(currentNext, TargetTile, IgnoreTerrian, IgnoreUnits))
            {
                currentNext = tile;
            }
        }
        temp.Add(new Move(StartTile, currentNext, this));
        temp.AddRange(PathFindTo(TargetTile, currentNext, IgnoreTerrian, IgnoreUnits));
        return temp;
    }

    int TargetSuitability(Tile currentTile, Tile TargetTile, bool IgnoreTerrian = false, bool IgnoreUnits = false)
    {
        if (TargetTile == null || currentTile == null)
        {
            return int.MaxValue;
        }
        if (IgnoreTerrian)
        {
            if (IgnoreUnits)
            {
                return Mathf.RoundToInt(Vector3.Distance(currentTile.Location, TargetTile.Location));
            }
            return Mathf.RoundToInt(Vector3.Distance(currentTile.Location, TargetTile.Location) + (currentTile.CurrentGroundUnit != null ? 100 : 0));
        }else
        {
            
            if (IgnoreUnits)
            {
                return Mathf.RoundToInt(Vector3.Distance(currentTile.Location, TargetTile.Location) + TileTerrianCost(currentTile));
            }
            return Mathf.RoundToInt(Vector3.Distance(currentTile.Location, TargetTile.Location) + TileTerrianCost(currentTile) + (currentTile.CurrentGroundUnit != null ? 100 : 0));
        }
    }*/
}
