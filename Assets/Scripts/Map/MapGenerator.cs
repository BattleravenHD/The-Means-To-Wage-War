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

    public Texture2D HeightMap;


    public float noiseScale = 0.25F;
    public float heightScale = 1f;


    // Start is called before the first frame update
    void Start()
    {
        CreateHeightMap();
        CreateMap();
        StartCoroutine(CreateCities());
    }

    void CreateHeightMap()
    {
        HeightMap = new Texture2D(MapWidth * Region.regionWidth * 2, MapHeight * Region.regionHeight * 2);

        for (int x = 0; x < HeightMap.width; x++)
        {
            for (int y = 0; y < HeightMap.height; y++)
            {
                // The origin of the sampled area in the plane.
                float xOrg = 0;
                float yOrg = 0;

                // The number of cycles of the basic noise pattern that are repeated
                // over the width and height of the texture.

                float xCoord = (xOrg + x) * noiseScale;
                float yCoord = (yOrg + y) * noiseScale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord) * heightScale;

                HeightMap.SetPixel(x, y, new Color(sample, sample, sample));
            }
        }
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
                region.HeightMap = HeightMap;
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
