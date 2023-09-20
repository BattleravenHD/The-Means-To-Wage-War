using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Tile 
{
    public Vector3 Location; // 3D location of Tile
    public int TileDataID; // Index of tile in the game data array
    public int FactionOwnerShip = -1; // -1 = No Ownership

    public bool HasRoad = false;
    public bool HasRiver = false;
    public TileTerrianTypes tileTerrian = TileTerrianTypes.GrassLand;

    public Unit CurrentGroundUnit = null;
    public Unit CurrentAirUnit = null;
    public City city;

    GameData gameData;

    MeshFilter tileMesh;
    int uvStartIndex;
    GameSettings gameSettings;
    Tile[] adjecentTiles;

    public Tile (Vector3 location, int ID, GameData gameData, MeshFilter mesh, int uVstartIndex, GameSettings gameSettings)
    {
        Location = location;
        TileDataID = ID;
        this.gameData = gameData;
        tileMesh = mesh;
        this.uvStartIndex = uVstartIndex;
        this.gameSettings = gameSettings;
    }

    public void ChangeTileOverlayColor(int faction)
    {
        FactionColour UvCoord = (from fact in gameSettings.factionColours where (fact.FactionInt == faction) select fact).First();
        Vector2[] uv = tileMesh.mesh.uv;
        for (int i = uvStartIndex; i < uvStartIndex + 6; i++)
        {
            uv[i] = UvCoord.UVCoord;
        }
        tileMesh.mesh.uv = uv;
    }

    public void ResetTileOverLayColor()
    {
        FactionColour UvCoord = (from fact in gameSettings.factionColours where (fact.FactionInt == FactionOwnerShip) select fact).First();
        Vector2[] uv = tileMesh.mesh.uv;
        for (int i = uvStartIndex; i < uvStartIndex + 6; i++)
        {
            uv[i] = UvCoord.UVCoord;
        }
        tileMesh.mesh.uv = uv;
    }

    /// <summary>
    /// Multiplies attack value by this mulitplier
    /// </summary>
    /// <returns></returns>

    public float AttackMultiplier()
    {
        switch (tileTerrian)
        {
            case TileTerrianTypes.GrassLand:
                return 0.9f;
            case TileTerrianTypes.Ocean:
                return 1;
            case TileTerrianTypes.City:
                return 1.25f;
            case TileTerrianTypes.Town:
                return 1.15f;
            case TileTerrianTypes.Hamlet:
                return 1.05f;
            default:
                return 1;
        }
    }

    /// <summary>
    /// Multiplies attack value by this mulitplier
    /// </summary>
    /// <returns></returns>
    public float DefenceMultiplier()
    {
        switch (tileTerrian)
        {
            case TileTerrianTypes.GrassLand:
                return 1;
            case TileTerrianTypes.Ocean:
                return 1;
            case TileTerrianTypes.City:
                return 0.5f;
            case TileTerrianTypes.Town:
                return 0.75f;
            case TileTerrianTypes.Hamlet:
                return 0.9f;
            default:
                return 1;
        }
    }

    /// <summary>
    /// Returns The Tiles Adjacent To this Tile in the Order (up left, up right, right, bottom right, bottom left, left)
    /// </summary>
    /// <returns></returns>
    public Tile[] AjacentTiles()
    {
        if (adjecentTiles != null)
        {
            return adjecentTiles;
        }
        Tile[] ajacencyArray = new Tile[6];

        // Order of received = up left, up right, right, bottom right, bottom left, left
        
        // split becasue every second tile's up left is -1 not the same 
        if (Mathf.FloorToInt(TileDataID / (Region.regionWidth * MapGenerator.MapWidth)) % 2 == 1)
        {
            // If less than lenght (eg can do the bottom)
            if (TileDataID - Region.regionWidth * MapGenerator.MapWidth >= 0)
            {
                ajacencyArray[4] = gameData.GameTiles[TileDataID - Region.regionWidth * MapGenerator.MapWidth]; // Bottom Left
                if ((TileDataID - Region.regionWidth * MapGenerator.MapWidth + 1) % (Region.regionWidth * MapGenerator.MapWidth) != 0)
                {
                    ajacencyArray[3] = gameData.GameTiles[TileDataID - Region.regionWidth * MapGenerator.MapWidth + 1]; // Bottom Right
                }
            }
            // if not on right edge
            if ((TileDataID + 1) % (Region.regionWidth * MapGenerator.MapWidth) != 0)
            {
                ajacencyArray[2] = gameData.GameTiles[TileDataID + 1]; // Right
            }
            // not on top 
            if (TileDataID + Region.regionWidth * MapGenerator.MapWidth < gameData.GameTiles.Length)
            {
                ajacencyArray[0] = gameData.GameTiles[TileDataID + Region.regionWidth * MapGenerator.MapWidth]; // Up left
                if ((TileDataID + Region.regionWidth * MapGenerator.MapWidth + 1) % (Region.regionWidth * MapGenerator.MapWidth) != 0)
                {
                    ajacencyArray[1] = gameData.GameTiles[TileDataID + Region.regionWidth * MapGenerator.MapWidth + 1]; // Up Right
                }
            }
            // if can do left
            if ((TileDataID - 1) % (Region.regionWidth * MapGenerator.MapWidth) < TileDataID % (Region.regionWidth * MapGenerator.MapWidth) && TileDataID - 1 >= 0)
            {
                ajacencyArray[5] = gameData.GameTiles[TileDataID - 1]; // Left
            }
        }else
        {
            // If less than lenght (eg can do the bottom)
            if (TileDataID - Region.regionWidth * MapGenerator.MapWidth >= 0)
            {
                ajacencyArray[3] = gameData.GameTiles[TileDataID - Region.regionWidth * MapGenerator.MapWidth]; // Bottom Right
                
                // If the tile to the left % remainder is less than the width of the rows % remainder of the current tile
                if ((TileDataID - Region.regionWidth * MapGenerator.MapWidth - 1) % (Region.regionWidth * MapGenerator.MapWidth) < (TileDataID - Region.regionWidth * MapGenerator.MapWidth) % (Region.regionWidth * MapGenerator.MapWidth) && (TileDataID - Region.regionWidth * MapGenerator.MapWidth - 1) >= 0)
                {
                    ajacencyArray[4] = gameData.GameTiles[TileDataID - Region.regionWidth * MapGenerator.MapWidth - 1]; // Bottom Left
                }
            }
            // if not on right edge
            if ((TileDataID + 1) % (Region.regionWidth * MapGenerator.MapWidth) != 0)
            {
                ajacencyArray[2] = gameData.GameTiles[TileDataID + 1]; // Right
            }
            // not on top 
            if (TileDataID + Region.regionWidth * MapGenerator.MapWidth <= gameData.GameTiles.Length)
            {
                ajacencyArray[1] = gameData.GameTiles[TileDataID + Region.regionWidth * MapGenerator.MapWidth]; // Up Right               
                if ((TileDataID + Region.regionWidth * MapGenerator.MapWidth - 1) % (Region.regionWidth * MapGenerator.MapWidth) < (TileDataID + Region.regionWidth * MapGenerator.MapWidth) % (Region.regionWidth * MapGenerator.MapWidth) && (TileDataID - Region.regionWidth * MapGenerator.MapWidth - 1) >= 0)
                {
                    ajacencyArray[0] = gameData.GameTiles[TileDataID + Region.regionWidth * MapGenerator.MapWidth - 1]; // Up left
                }
            }
            // if can do left
            if ((TileDataID - 1) % (Region.regionWidth * MapGenerator.MapWidth) < TileDataID % (Region.regionWidth * MapGenerator.MapWidth) && TileDataID - 1 >= 0)
            {
                ajacencyArray[5] = gameData.GameTiles[TileDataID - 1]; // Left
            }
        }
        adjecentTiles = ajacencyArray;

        return ajacencyArray;
    }
}

public enum TileTerrianTypes
{
    GrassLand,
    Ocean,


    City,
    Town,
    Hamlet
}
