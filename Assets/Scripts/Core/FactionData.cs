using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionData : MonoBehaviour
{
    public int FactionID;
    public int FactionIPC;
    public List<Order> NormalOrders = new List<Order>();
    public List<Order> SpawnOrders = new List<Order>();
    public List<Unit> FactionUnits = new List<Unit>();
    public List<City> FactionCities = new List<City>();

    public void UpdateOrderLists()
    {
        List<Order> temp = new List<Order>();

        foreach (Order item in NormalOrders)
        {
            if (!item.Completed && item.unit != null)
            {
                temp.Add(item);
            }
        }
        NormalOrders = temp;

        List<Order> temp2 = new List<Order>();

        foreach (Order item in SpawnOrders)
        {
            if (!item.Completed)
            {
                temp2.Add(item);
            }
        }
        SpawnOrders = temp2;

        List<Unit> temp3 = new List<Unit>();

        foreach (Unit item in FactionUnits)
        {
            if (item != null)
            {
                temp3.Add(item);
            }
        }
        FactionUnits = temp3;

        foreach (Unit item in FactionUnits)
        {
            item.NewTurn();
        }
    }

    public void UpdateUnits()
    {
        List<Unit> temp = new List<Unit>();

        foreach (Unit item in FactionUnits)
        {
            if (item != null)
            {
                item.ConsumeSupply();
                if (item.CurrentHealth > 0)
                {
                    temp.Add(item);
                }else
                {
                    Destroy(item.gameObject);
                }
            }
        }
        FactionUnits = temp;
    }

    public void UpdateIPC()
    {
        foreach (City item in FactionCities)
        {
            FactionIPC += item.GetIncome();
        }
    }

    public IEnumerator SupplyStep()
    {
        // First supply flows from capitol out to the cities
        HashSet<Tile> visited = new HashSet<Tile>();
        List<Tile> tilesToSupplyCheck = new List<Tile>();
        tilesToSupplyCheck.Add(FactionCities[0].CentreTile);
        while (tilesToSupplyCheck.Count > 0)
        {
            foreach (City city in FactionCities)
            {
                if (tilesToSupplyCheck[0] == city.CentreTile)
                {
                    city.CurrentSupply = Mathf.Clamp(city.CurrentSupply + GameSettings.CITYSUPPLYINCREASEAMOPUNT, 0, city.MaxSupply);
                }
            }
            foreach (Tile tile in tilesToSupplyCheck[0].AjacentTiles())
            {
                if (tile == null) continue;
                if (tile.HasRoad && tile.FactionOwnerShip == FactionID && !visited.Contains(tile))
                {
                    tilesToSupplyCheck.Add(tile);
                    visited.Add(tile);
                }
            }
            tilesToSupplyCheck.RemoveAt(0);
        }
        yield return new WaitForSeconds(0);

        List<Unit> UnitsToSupply = new List<Unit>();
        List<SupplyGroundUnit> SupplyUnitsForCities = new List<SupplyGroundUnit>();
        List<SupplyGroundUnit> SupplyUnits = new List<SupplyGroundUnit>();

        // Then from each city spreading out
        foreach (City city in FactionCities)
        {
            // starting with all city tiles
            foreach (Tile tile in city.CityTiles)
            {
                if (tile.FactionOwnerShip == FactionID)
                {
                    tilesToSupplyCheck.Add(tile);
                    if (tile.CurrentGroundUnit != null)
                    {
                        if (tile.CurrentGroundUnit.GetType() == typeof(SupplyGroundUnit))
                        {
                            SupplyUnitsForCities.Add(tile.CurrentGroundUnit as SupplyGroundUnit);
                            SupplyUnits.Add(tile.CurrentGroundUnit as SupplyGroundUnit);
                        }
                        else
                        {
                            UnitsToSupply.Add(tile.CurrentGroundUnit);
                        }
                    }
                    if (tile.CurrentAirUnit != null)
                    {
                        UnitsToSupply.Add(tile.CurrentAirUnit);
                    }
                }
            }
            // fan out around the city and add all units and supply units to lists
            for (int i = 0; i < GameSettings.CITYSUPPLYRADIUS; i++)
            {
                int count = tilesToSupplyCheck.Count;
                for (int x = 0; x < count; x++)
                {
                    foreach (Tile adjcentTile in tilesToSupplyCheck[x].AjacentTiles())
                    {
                        if (adjcentTile == null) continue;

                        if (!tilesToSupplyCheck.Contains(adjcentTile) && adjcentTile.FactionOwnerShip == FactionID)
                        {
                            tilesToSupplyCheck.Add(adjcentTile);
                            if (adjcentTile.CurrentGroundUnit != null)
                            {
                                if (adjcentTile.CurrentGroundUnit.GetType() == typeof(SupplyGroundUnit))
                                {
                                    SupplyUnitsForCities.Add(adjcentTile.CurrentGroundUnit as SupplyGroundUnit);
                                    SupplyUnits.Add(adjcentTile.CurrentGroundUnit as SupplyGroundUnit);
                                }else
                                {
                                    UnitsToSupply.Add(adjcentTile.CurrentGroundUnit);
                                }
                            }
                            if (adjcentTile.CurrentAirUnit != null)
                            {
                                UnitsToSupply.Add(adjcentTile.CurrentAirUnit);
                            }
                        }
                    }
                }    
            }
            foreach (Unit item in UnitsToSupply)
            {
                int amount = Mathf.Clamp(city.CurrentSupply / (UnitsToSupply.Count * GameSettings.CITYSUPPLYMAXDISTIBUTIONAMOUNT) * GameSettings.CITYSUPPLYMAXDISTIBUTIONAMOUNT, 0, GameSettings.CITYSUPPLYMAXDISTIBUTIONAMOUNT);
                city.CurrentSupply -= amount;
                city.CurrentSupply += item.AddSupply(amount);
            }
            foreach (SupplyGroundUnit supply in SupplyUnitsForCities)
            {
                int amount = Mathf.Clamp(city.CurrentSupply / (SupplyUnitsForCities.Count * GameSettings.CITYSUPPLYMAXDISTIBUTIONAMOUNT * 5) * GameSettings.CITYSUPPLYMAXDISTIBUTIONAMOUNT * 5, 0, GameSettings.CITYSUPPLYMAXDISTIBUTIONAMOUNT * 5);
                city.CurrentSupply -= amount;
                city.CurrentSupply += supply.AddSupply(amount);
            }

            tilesToSupplyCheck.Clear();
            SupplyUnitsForCities.Clear();
            UnitsToSupply.Clear();
            yield return new WaitForSeconds(0);
        }

        // Then from each supply unit out including supply units checked
        while (SupplyUnits.Count > 0)
        {
            // fan out around the supplyUnit and add all units and supply units to lists
            tilesToSupplyCheck.Add(SupplyUnits[0].CurrentTile);
            for (int i = 0; i < SupplyUnits[0].SupplyRadius; i++)
            {
                int count = tilesToSupplyCheck.Count;
                for (int x = 0; x < count; x++)
                {
                    foreach (Tile adjcentTile in tilesToSupplyCheck[x].AjacentTiles())
                    {
                        if (adjcentTile == null) continue;

                        if (!tilesToSupplyCheck.Contains(adjcentTile) && adjcentTile.FactionOwnerShip == FactionID)
                        {
                            tilesToSupplyCheck.Add(adjcentTile);
                            if (adjcentTile.CurrentGroundUnit != null)
                            {
                                // If the unit has not been supplied by this ground supply unit add it to the list to chain supply along
                                if (adjcentTile.CurrentGroundUnit.GetType() == typeof(SupplyGroundUnit) && !SupplyUnits[0].Suppliers.Contains(adjcentTile.CurrentGroundUnit as SupplyGroundUnit))
                                {
                                    SupplyGroundUnit supplyUnit = adjcentTile.CurrentGroundUnit as SupplyGroundUnit;
                                    SupplyUnitsForCities.Add(supplyUnit);
                                    SupplyUnits.Add(supplyUnit);

                                    supplyUnit.Suppliers.Add(SupplyUnits[0]);
                                }
                                else
                                {
                                    UnitsToSupply.Add(adjcentTile.CurrentGroundUnit);
                                }
                            }
                            if (adjcentTile.CurrentAirUnit != null)
                            {
                                UnitsToSupply.Add(adjcentTile.CurrentAirUnit);
                            }
                        }
                    }
                }
            }
            foreach (Unit item in UnitsToSupply)
            {
                int amount = Mathf.Clamp(SupplyUnits[0].CurrentSupply / (UnitsToSupply.Count * SupplyUnits[0].SupplyDistributionRate) * SupplyUnits[0].SupplyDistributionRate, 0, SupplyUnits[0].SupplyDistributionRate);
                SupplyUnits[0].CurrentSupply -= amount;
                SupplyUnits[0].CurrentSupply += item.AddSupply(amount);
            }
            foreach (SupplyGroundUnit supply in SupplyUnitsForCities)
            {
                int amount = Mathf.Clamp(SupplyUnits[0].CurrentSupply / (SupplyUnitsForCities.Count * SupplyUnits[0].SupplyDistributionRate * 5) * SupplyUnits[0].SupplyDistributionRate * 5, 0, SupplyUnits[0].SupplyDistributionRate * 5);
                SupplyUnits[0].CurrentSupply -= amount;
                SupplyUnits[0].CurrentSupply += supply.AddSupply(amount);
            }

            tilesToSupplyCheck.Clear();
            SupplyUnitsForCities.Clear();

            if (UnitsToSupply.Count > 0)
            {
                UnitsToSupply.RemoveAt(0);
            }
            yield return new WaitForSeconds(0);
        }
    }
}
