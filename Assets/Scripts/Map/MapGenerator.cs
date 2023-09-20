using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public static readonly int MapWidth  = 5;
    public static readonly int MapHeight = 4;

    [SerializeField] GameSettings settings;
    [SerializeField] GameData gameData;
    [SerializeField] int CityAmount = 10; 

    [SerializeField] GameObject RegionPrefab;
    [SerializeField] GameObject CityPrefab;

    // Start is called before the first frame update
    void Start()
    {
        CreateMap();
        StartCoroutine(CreateCities());
    }

    void CreateMap()
    {
        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                Region region = Instantiate(RegionPrefab, new Vector3(x * 1.72f * Region.regionWidth, 0, y * 1.5f * Region.regionHeight), Quaternion.identity, transform).GetComponent<Region>();
                region.regionX = x;
                region.regionY = y;
                region.settings = settings;
            }
        }
    }

    void DistributeCities()
    {
        foreach (FactionData item in gameData.factionData)
        {
            int atempts = 0;
            City city = gameData.Cities[Random.Range(0, gameData.Cities.Length)];
            while (city.CurrentFactionID != -1 && atempts < 100)
            {
                city = gameData.Cities[Random.Range(0, gameData.Cities.Length)];
                atempts++;
            }
            if (city.CurrentFactionID != -1)
            {
                return;
            }
            item.FactionCities.Add(city);
            city.CaptureCity(item.FactionID);
            if (item.FactionID == GameSettings.PLAYERFACTIONID)
            {
                item.gameObject.transform.position = item.FactionCities[0].CentreTile.Location;
            }
        }
    }

    IEnumerator CreateCities()
    {
        yield return new WaitForSeconds(0.1f);

        List<Tile> cityTiles = new List<Tile>();
        List<City> cities = new List<City>();

        Tile randomTile = gameData.GameTiles[0];

        for (int i = 0; i < CityAmount; i++)
        {
            int searchAmount = 0;

            randomTile = gameData.GameTiles[Random.Range(0, gameData.GameTiles.Length)];
            bool searchLocation = true;
            while (searchLocation && searchAmount < GameSettings.CITYPLACEMENTSEARCHAMOUNT)
            {
                searchLocation = false;
                foreach (Tile city in cityTiles)
                {
                    if (Vector3.Distance(city.Location, randomTile.Location) < GameSettings.MINCITYSPREAD)
                    {
                        searchLocation = true;
                    }
                }
                if (searchLocation)
                {
                    randomTile = gameData.GameTiles[Random.Range(0, gameData.GameTiles.Length)];
                    searchAmount++;
                }
            }

            if (!searchLocation)
            {
                City city = Instantiate(CityPrefab, randomTile.Location, Quaternion.identity).GetComponent<City>();
                city.GenerateCity(randomTile);

                cities.Add(city);

                cityTiles.Add(randomTile);
            }
        }
        foreach (City item in cities)
        {
            item.CreateRoads(cities.ToArray());
        }

        gameData.Cities = cities.ToArray();

        DistributeCities();
    }
}
