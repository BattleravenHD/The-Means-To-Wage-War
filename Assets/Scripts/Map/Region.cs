using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Region : MonoBehaviour
{
    [SerializeField]
    public static readonly int regionHeight = 10; // Must be Even
    [SerializeField]
    public static readonly int regionWidth = 11; // Must Be Odd or else regions dont slot together

    [HideInInspector] public GameSettings settings;
    public int regionX;
    public int regionY;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    GameData gameData;
    int vertIndex = 0;
    int subMeshVertIndex = 15 * regionHeight * regionWidth;
    int triIndex = 0;
    int subMeshTriIndex = 0;

    Vector3 TILEDISPLAYHEIGHT = Vector3.up / 100;

    // Array of all verts, Tris, UV for a region (used array for perfomance reasons as list has smaller max size and takes slightly longer to compute)
    // submesh array to allow for a tile overlay to show tile borders and to do faction colours and other tile functions
    Vector3[] verticies = new Vector3[15 * regionHeight * regionWidth + 14 * regionHeight * regionWidth];
    int[] mainTriangels = new int[60 * regionHeight * regionWidth];
    int[] subMeshTriangles = new int[42 * regionHeight * regionWidth];
    Vector2[] uv = new Vector2[15 * regionHeight * regionWidth + 14 * regionHeight * regionWidth];

    public static readonly Vector3[] mainTileVerts = new Vector3[15]
        {
            new Vector3(0f, 0f, 1f),        //top
            new Vector3(-0.86f, 0, 0.5f),   // left top
            new Vector3(-0.86f, 0, 0),      // left mid
            new Vector3(-0.86f, 0, -0.5f),  // left bottom 
            new Vector3(0f, 0f, -1f),       // bottom 
            new Vector3(0.86f, 0, 0.5f),    // right top
            new Vector3(0.86f, 0, 0),       // right mid
            new Vector3(0.86f, 0, -0.5f),   // right bottom

            new Vector3(0f, 0, 0.5f),
            new Vector3(-0.43f, 0, 0.25f),
            new Vector3(-0.43f, 0, -0.25f),
            new Vector3(0f, 0f, -0.5f),
            new Vector3(0.43f, 0, 0.25f),
            new Vector3(0.43f, 0, -0.25f),
            new Vector3(0f, 0f, 0f)
        };
    public static readonly int[,] mainTileTris = new int[20, 3]
    {
        { 1, 0, 8},
        { 9, 1, 8 },
        { 2, 1, 9 },
        { 2, 9, 10 },
        { 3, 2, 10 },
        { 3, 10, 11 },
        { 3, 11, 4 },
        { 7, 4, 11 },
        { 11, 13, 7 },
        { 13, 6, 7 },
        { 13, 12, 6 },
        { 12, 5, 6 },
        { 12, 8, 5 },
        { 5, 8, 0 },
        { 9, 8, 14 },
        { 10, 9, 14 },
        { 11, 10, 14 },
        { 13, 11, 14 },
        { 12, 13, 14 },
        { 8, 12, 14 }
    };

    public static readonly Vector2[] mainTileUVs = new Vector2[15]
    {
        new Vector2(1, 0.5f),
        new Vector2(1, 0.5f),
        new Vector2(1, 0.5f),
        new Vector2(1, 0.5f),
        new Vector2(1, 0.5f),
        new Vector2(1, 0.5f),
        new Vector2(1, 0.5f),
        new Vector2(1, 0.5f),

        new Vector2(1, 0.5f),
        new Vector2(1, 0.5f),
        new Vector2(1, 0.5f),
        new Vector2(1, 0.5f),
        new Vector2(1, 0.5f),
        new Vector2(1, 0.5f),
        new Vector2(1, 0.5f)
    };

    public static readonly Vector3[] subMeshTileVerts = new Vector3[14]
        {
            new Vector3(0f, 0f, 1f),        //top
            new Vector3(-0.86f, 0, 0.5f),   // left top
            new Vector3(-0.86f, 0, 0),      // left mid
            new Vector3(-0.86f, 0, -0.5f),  // left bottom 
            new Vector3(0f, 0f, -1f),       // bottom 
            new Vector3(0.86f, 0, 0.5f),    // right top
            new Vector3(0.86f, 0, 0),       // right mid
            new Vector3(0.86f, 0, -0.5f),   // right bottom

            new Vector3(0f, 0, 0.9f),
            new Vector3(-0.774f, 0, 0.45f),
            new Vector3(-0.774f, 0, -0.45f),
            new Vector3(0f, 0f, -0.9f),
            new Vector3(0.774f, 0, 0.45f),
            new Vector3(0.774f, 0, -0.45f)
        };
    public static readonly int[,] subMeshTileTris = new int[14, 3]
    {
        { 1, 0, 8},
        { 9, 1, 8 },
        { 2, 1, 9 },
        { 2, 9, 10 },
        { 3, 2, 10 },
        { 3, 10, 11 },
        { 3, 11, 4 },
        { 7, 4, 11 },
        { 11, 13, 7 },
        { 13, 6, 7 },
        { 13, 12, 6 },
        { 12, 5, 6 },
        { 12, 8, 5 },
        { 5, 8, 0 }
    };

    public static readonly Vector2[] subMeshTileUVs = new Vector2[14]
    {
        new Vector2(0.5f, 0.5f),
        new Vector2(0.5f, 0.5f),
        new Vector2(0.5f, 0.5f),
        new Vector2(0.5f, 0.5f),
        new Vector2(0.5f, 0.5f),
        new Vector2(0.5f, 0.5f),
        new Vector2(0.5f, 0.5f),
        new Vector2(0.5f, 0.5f),

        new Vector2(0.5f, 1),
        new Vector2(0.5f, 1),
        new Vector2(0.5f, 1),
        new Vector2(0.5f, 1),
        new Vector2(0.5f, 1),
        new Vector2(0.5f, 1)
    };

    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        gameData = GetComponentInParent<GameData>();

        GenerateMap();
    }

    void GenerateMap()
    {
        Mesh tileMesh = new Mesh();

        // This allows for a submesh to allow for a tile overlay to show tile borders and to do faction colours and other tile functions
        tileMesh.subMeshCount = 2;

        for (int y = 0; y < regionHeight; y++)
        {
            for (int x = 0; x < regionWidth; x++)
            {
                if (y % 2 != 0)
                {
                    CreateTileForRegion(new Vector3(x * 1.72f + 0.86f, 0, y * 1.5f), x, y);
                }
                else
                {
                    CreateTileForRegion(new Vector3(x * 1.72f, 0, y * 1.5f), x, y);
                }
            }
        }

        tileMesh.vertices = verticies;
        tileMesh.SetTriangles(mainTriangels, 0);
        tileMesh.SetTriangles(subMeshTriangles, 1);

        tileMesh.uv = uv;

        tileMesh.RecalculateNormals();

        meshFilter.mesh = tileMesh;

        verticies = null;
        uv = null;
        subMeshTriangles = null;
        mainTriangels = null;
    }


    // Creates a tile at a givin position 
    void CreateTileForRegion(Vector3 position, int dataX, int dataY)
    {
        // Tile Data creation and location Assignment
        // current x across entire map + number of tiles in the combined regions underneath + number of tiles underneath in current region
        int TileID = regionX * regionWidth + dataX +
            regionY * regionWidth * MapGenerator.MapWidth * regionHeight +
            dataY * regionWidth * MapGenerator.MapWidth;

        gameData.GameTiles[TileID] = new Tile(transform.position + position, TileID, gameData, meshFilter, subMeshVertIndex + 8, settings);

        //Creates verts for tile based on predifined array of locations + location modifier 
        for (int x = 0; x < mainTileVerts.Length; x++)
        {
            verticies[vertIndex + x] = mainTileVerts[x] + position;
            uv[vertIndex + x] = mainTileUVs[x];
        }

        //Creates tris based on tri array
        for (int x = 0; x < 20; x++)
        {
            for (int i = 0; i < 3; i++)
            {
                mainTriangels[triIndex + x * 3 + i] = mainTileTris[x,i] + vertIndex;
            }
        }

        // Creates the submesh tiles and places them towards the end of the vertex list
        for (int x = 0; x < subMeshTileVerts.Length; x++)
        {
            verticies[subMeshVertIndex + x] = subMeshTileVerts[x] + position + TILEDISPLAYHEIGHT;
            if (x > 7)
            {
                FactionColour UvCoord = (from faction in settings.factionColours where (faction.FactionInt == -1) select faction).First();
                uv[subMeshVertIndex + x] = UvCoord.UVCoord;
            }
            else
            {
                // Assign To Black
                FactionColour UvCoord = (from faction in settings.factionColours where (faction.FactionInt == -10) select faction).First();
                uv[subMeshVertIndex + x] = UvCoord.UVCoord;
            }
        }

        //Creates tris based on tri array for the submesh
        for (int x = 0; x < 14; x++)
        {
            for (int i = 0; i < 3; i++)
            {
                subMeshTriangles[subMeshTriIndex + x * 3 + i] = subMeshTileTris[x, i] + subMeshVertIndex;
            }
        }

        // Allows it to properly cycle through the created array. 
        triIndex += mainTileTris.Length;
        vertIndex += mainTileVerts.Length;
        subMeshVertIndex += subMeshTileVerts.Length;
        subMeshTriIndex += subMeshTileTris.Length;
    }

    
}
