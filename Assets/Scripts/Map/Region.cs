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

    [SerializeField]
    private List<HexObject> PossibleHexs = new List<HexObject>();

    [HideInInspector] public GameSettings settings;
    public int regionX;
    public int regionY;
    public float heightScale = 1f;

    [HideInInspector] public Texture2D HeightMap;

    private MeshFilter meshFilter;

    GameData gameData;
    int vertIndex = 0;
    int subMeshVertIndex = 0;


    Vector3 TILEDISPLAYHEIGHT = Vector3.up / 8;

    // submesh array to allow for a tile overlay to show tile borders and to do faction colours and other tile functions
    List<Vector3> verticies = new List<Vector3>();
    List<Vector3> submeshVerticies = new List<Vector3>();
    List<int> mainTriangels = new List<int>();
    List<int> subMeshTriangles = new List<int>();
    List<Vector2> uv = new List<Vector2>();
    List<Vector2> subMeshUv = new List<Vector2>();

    // Tile Outline Mesh
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

        //Verticies merged so the tris index have to be updated to match the new vert indexs
        for (int i = 0; i < subMeshTriangles.Count; i++)
        {
            subMeshTriangles[i] += verticies.Count();
        }

        verticies.AddRange(submeshVerticies);

        tileMesh.vertices = verticies.ToArray();
        tileMesh.SetTriangles(mainTriangels.ToArray(), 0);
        tileMesh.SetTriangles(subMeshTriangles.ToArray(), 1);

        uv.AddRange(subMeshUv);

        tileMesh.uv = uv.ToArray();

        tileMesh.RecalculateNormals();

        meshFilter.mesh = tileMesh;

        verticies.Clear();
        uv.Clear();
        subMeshTriangles.Clear();
        mainTriangels.Clear();
    }


    Vector3 getVertexHeight(Vector3 postion, Vector3 vertPos)
    {
        postion += transform.position;

        float top = Mathf.Lerp(HeightMap.GetPixel(Mathf.FloorToInt(postion.x + vertPos.x), Mathf.CeilToInt(postion.z + vertPos.z)).r, HeightMap.GetPixel(Mathf.CeilToInt(postion.x + vertPos.x), Mathf.CeilToInt(postion.z + vertPos.z)).r, postion.x + vertPos.x - Mathf.Floor(postion.x + vertPos.x));
        float bottom = Mathf.Lerp(HeightMap.GetPixel(Mathf.FloorToInt(postion.x + vertPos.x), Mathf.FloorToInt(postion.z + vertPos.z)).r, HeightMap.GetPixel(Mathf.CeilToInt(postion.x + vertPos.x), Mathf.FloorToInt(postion.z + vertPos.z)).r, postion.x + vertPos.x - Mathf.Floor(postion.x + vertPos.x));
        
        return new Vector3(0, Mathf.Lerp(bottom, top, postion.y + vertPos.y - Mathf.Floor(postion.y + vertPos.y)), 0);
    }

    Mesh GetTileType()
    {
        // To Return what type base to use
        return PossibleHexs[0].Model;
    }

    // Creates a tile at a givin position 
    void CreateTileForRegion(Vector3 position, int regionBasedX, int regionBasedY)
    {
        // Tile Data creation and location Assignment
        // current x across entire map + number of tiles in the combined regions underneath + number of tiles underneath in current region
        int TileID = regionX * regionWidth + regionBasedX +
            regionY * regionWidth * MapGenerator.MapWidth * regionHeight +
            regionBasedY * regionWidth * MapGenerator.MapWidth;

        Mesh tileType = GetTileType();

        gameData.GameTiles[TileID] = new Tile(transform.position + position + getVertexHeight(position, tileType.vertices[0]), TileID, gameData, meshFilter, -subMeshTileUVs.Length * regionWidth * regionHeight + subMeshVertIndex, settings);

        //Creates verts for tile based on predifined array of locations + location modifier 
        for (int x = 0; x < tileType.vertices.Length; x++)
        {
            verticies.Add(tileType.vertices[x] + position + getVertexHeight(position, tileType.vertices[x]));
            uv.Add(tileType.uv[x]);
        }

        //Creates tris based on tri array
        for (int x = 0; x < tileType.triangles.Length; x++)
        {
            mainTriangels.Add(tileType.triangles[x] + vertIndex);
        }

        
        // Creates the submesh tiles and places them at the end of the vertex list
        for (int x = 0; x < subMeshTileVerts.Length; x++)
        {
            submeshVerticies.Add(subMeshTileVerts[x] + position + TILEDISPLAYHEIGHT + getVertexHeight(position, subMeshTileVerts[x]));
            if (x > 7)
            {
                FactionColour UvCoord = (from faction in settings.factionColours where (faction.FactionInt == -1) select faction).First();
                subMeshUv.Add(UvCoord.UVCoord);
            }
            else
            {
                // Assign To Black
                FactionColour UvCoord = (from faction in settings.factionColours where (faction.FactionInt == -10) select faction).First();
                subMeshUv.Add(UvCoord.UVCoord);
            }
        }

        //Creates tris based on tri array for the submesh
        for (int x = 0; x < 14; x++)
        {
            for (int i = 0; i < 3; i++)
            {
                subMeshTriangles.Add(subMeshTileTris[x, i] + subMeshVertIndex);
            }
        }

        // Allows it to properly cycle through the created array. 
        vertIndex += tileType.vertices.Length;
        subMeshVertIndex += subMeshTileVerts.Length;
    }
}
