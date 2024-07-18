using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LocationGenerator : MonoBehaviour
{
    Tilemap tilemap;
    [SerializeField] TileBase[] tiles;

    [SerializeField] Vector2Int minLocationSize = new Vector2Int(15, 15);
    [SerializeField] Vector2Int maxLocationSize = new Vector2Int(45, 45);
    [SerializeField] Vector2Int minPartSize = new Vector2Int(4, 4);
    [SerializeField] float emptySpaceSpawnChance = 0.3f;
    [SerializeField] float emptySpaceThirdChance = 1f;

    Vector2Int mapRect;
    List<int[,]> mapZones;
    int[,] mapLayout;
    

    void Start()
    {
        tilemap = FindFirstObjectByType<Tilemap>();
        //tiles = Resources.LoadAll<TileBase>("Environment/Tilemap");
        //Debug.Log("Loaded tiles: " + string.Join(", ", tiles.Select(x => x.name).ToArray()));
        
        GenerateMapLayout();
        //PrintSpaces();
        GenerateMap();
    }

    void GenerateMapLayout()
    {
        mapRect = new Vector2Int(Random.Range(minLocationSize.x, maxLocationSize.x + 1), Random.Range(minLocationSize.y, maxLocationSize.y + 1));
        Vector2Int mapRectThird = new Vector2Int((int)(mapRect.x / 3f), (int)(mapRect.y / 3f));
        
        List<Vector2Int> emptySpaces = new List<Vector2Int>(8);
        for (int i = 0; i < 8; i++)
        {
            if (Random.value < emptySpaceSpawnChance)
                emptySpaces.Add(new Vector2Int(
                    Random.value < emptySpaceThirdChance ? mapRectThird.x : Random.Range(minPartSize.x, mapRectThird.x / 2 + 1),
                    Random.value < emptySpaceThirdChance ? mapRectThird.y : Random.Range(minPartSize.y, mapRectThird.y / 2 + 1)));
            else
                emptySpaces.Add(Vector2Int.zero);
        } //ПОЛУЧАЕМ ЗОНЫ БЕЗ УЧЁТА ПОВОРОТА

        // for (int i = 0; i < emptySpaces.Count; i++)
        // {
        //     if (i == 3 || i == 4 || i == 6 || i == 7)
        //     {
        //         Vector2Int temp = emptySpaces[i];
        //         emptySpaces[i] = new Vector2Int(temp.y, temp.x);
        //     }
        // }
        
        if (emptySpaces.TrueForAll(x => x != Vector2Int.zero))
            emptySpaces[Random.Range(0, emptySpaces.Count)] = Vector2Int.zero;

        mapLayout = new int[mapRect.x, mapRect.y];

        for (int i = 0; i < mapRect.x; i++)
            for (int j = 0; j < mapRect.y; j++)
                mapLayout[i, j] = 1;

        // for (int i = 0; i < 4; i++)
        // {
        //     for (int j = 0; j < emptySpaces[i].x; j++)
        //         for (int k = 0; k < emptySpaces[i].y; k++)
        //             mapLayout[j,k] = 0;
            
        //     for (int j = 0; j < emptySpaces[i+1].x; j++)
        //         for (int k = 0; k < emptySpaces[i+1].y; k++)
        //             mapLayout[mapLayout.GetLength(0)/2 - emptySpaces[i+1].x/2 + k, j] = 0;

        //     Transponse(ref mapLayout);
        //     Reverse(ref mapLayout);
        // }
        // Transponse(ref mapLayout);
        // Reverse(ref mapLayout);
    }

    // public void PrintSpaces()
    // {
    //     Debug.Log(mapRect);
    //     foreach (var location in emptySpaces)
    //     {
    //         Debug.Log(location);
    //     }
    // }

    private void GenerateMap()
    {
        for (int i = 0; i < mapLayout.GetLength(0); i++)
            for (int j = 0; j < mapLayout.GetLength(1); j++)
                tilemap.SetTile(new Vector3Int(i,j,0), tiles[mapLayout[i, j]]);                
    }

    #region Matrix

    private void PrintMatrix(int[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        string table = "";
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
                table += matrix[i, j] + " ";
            table += "\n";
        }

        Debug.Log(table);
    }


    private void Transponse(ref int[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        int[,] transposedMatrix = new int[cols, rows];

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                transposedMatrix[j, i] = matrix[i, j];

        matrix = transposedMatrix;
    }



    #endregion
}
