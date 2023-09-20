using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : MonoBehaviour
{
    public Tile CentreTile;
    public List<Tile> CityTiles = new List<Tile>();
    public int CurrentFactionID = -1;
    public float CurrentFactionOwnershipPercent = 1;
    public int CurrentSupply = 100;
    public int MaxSupply = 1000;
    public List<City> cityLinks = new List<City>();
    public GameObject roadPrefab;
    public GameObject cityPrefab;

    public void GenerateCity(Tile StartTile)
    {
        CentreTile = StartTile;
        StartTile.city = this;
        StartTile.tileTerrian = TileTerrianTypes.City;
        // At some point create sprawling City

        foreach (Tile item in CentreTile.AjacentTiles())
        {
            if (item != null)
            {
                Instantiate(cityPrefab, item.Location, Quaternion.identity, transform);
                CityTiles.Add(item);
                item.tileTerrian = TileTerrianTypes.City;
            }
        }
    }

    public void CaptureCity(int factionID)
    {
        CurrentFactionID = factionID;
        CentreTile.FactionOwnerShip = factionID;
        CentreTile.ResetTileOverLayColor();

        List<Tile> tilesToOwn = new List<Tile>();
        List<Tile> tilesToOwn2 = new List<Tile>();
        foreach (Tile tile in CityTiles)
        {
            if (tile != null && tile.FactionOwnerShip == -1)
            {
                tile.FactionOwnerShip = factionID;
                tile.ResetTileOverLayColor();
                tilesToOwn.AddRange(tile.AjacentTiles());
            }
        }
        foreach (Tile tile in tilesToOwn)
        {
            if (tile != null && tile.FactionOwnerShip == -1)
            {
                tile.FactionOwnerShip = factionID;
                tile.ResetTileOverLayColor();
                tilesToOwn2.AddRange(tile.AjacentTiles());
            }
        }
        foreach (Tile tile in tilesToOwn2)
        {
            if (tile != null && tile.FactionOwnerShip == -1)
            {
                tile.FactionOwnerShip = factionID;
                tile.ResetTileOverLayColor();
            }
        }
    }

    public int GetIncome()
    {
        return Mathf.RoundToInt(CityTiles.Count * CurrentFactionOwnershipPercent * GameSettings.CITYIPCAMOUNT);
    }

    public void CreateRoads(City[] cities)
    {
        foreach (City city in cities)
        {
            if (!cityLinks.Contains(city) && Vector3.Distance(city.transform.position, transform.position) < GameSettings.CITYROADLINKMAXDISTANCE && city != this)
            {
                Tile[] roadPath = PathFinding.CreatePathTo(CentreTile, city.CentreTile).ToArray();
                cityLinks.Add(city);
                city.cityLinks.Add(this);

                LineRenderer road = Instantiate(roadPrefab, Vector3.zero, Quaternion.Euler(90,0,0)).GetComponent<LineRenderer>();
                road.positionCount = roadPath.Length;

                int index = 0;
                foreach (Tile item in roadPath)
                {
                    item.HasRoad = true;
                    road.SetPosition(index, item.Location + Vector3.up / 10);
                    index++;
                }
            }
        }
    }
}
