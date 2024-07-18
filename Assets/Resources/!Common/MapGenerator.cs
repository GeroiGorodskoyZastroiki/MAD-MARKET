using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;
using MathNet.Numerics.LinearAlgebra;

public class MapGenerator : MonoBehaviour
{
    Tilemap tilemap;
    [SerializeField] TileBase[] tiles;

    Vector2Int mapZoneSize;
    List<Matrix<double>> mapZones;
    Matrix<double> mapLayout; //При индексации в Matrix сначала идёт row, потом column

    [SerializeField] Vector2Int minThirdSize = new Vector2Int(5, 5);
    [SerializeField] Vector2Int maxThirdSize = new Vector2Int(16, 16);
    [SerializeField] Vector2Int minPartSize = new Vector2Int(4, 4);
    [SerializeField] float unavailableSpaceSpawnChance = 0.3f;
    [SerializeField] float unavailableSpaceThirdChance = 1f;
    [SerializeField] int minEmptySpacesCount = 0;
    [SerializeField] int maxEmptySpacesCount = 3;
    [SerializeField] Vector2Int maxEmptySpaceSize = new Vector2Int(8, 8);
    [SerializeField] int maxInRowShelvingsCount = 6;
    [SerializeField] int minShelvingLength = 3;
    [SerializeField] int maxShelvingLength = 6;
    
    void Start()
    {
        tilemap = FindFirstObjectByType<Tilemap>();
        //tiles = Resources.LoadAll<TileBase>("Environment/Tilemap");
        //Debug.Log("Loaded tiles: " + string.Join(", ", tiles.Select(x => x.name).ToArray()));
        
        GenerateMapLayout();
        GenerateMap(mapLayout);
    }

    void GenerateMapLayout()
    {
        mapZoneSize = new Vector2Int(Random.Range(minThirdSize.x, maxThirdSize.x), Random.Range(minThirdSize.y, maxThirdSize.y));
        mapLayout = Matrix<double>.Build.Dense(mapZoneSize.x * 3, mapZoneSize.y * 3, 0);
        mapZones = new List<Matrix<double>>();

        GenerateZones();
        InsertZonesInMapLayout();
        GenerateBoundaries();
        ClearUnnecessaryWalls();
        PrintMapLayout();
        GenerateEmptySpaces();

        void GenerateZones()
        {
            List<Vector2Int> unavailableSpaces = new List<Vector2Int>(8);
            GenerateUnavailableSpaces();

            void GenerateUnavailableSpaces()
            {
                for (int i = 0; i < 8; i++)
                {
                    if (Random.value < unavailableSpaceSpawnChance)
                        unavailableSpaces.Add(new Vector2Int(
                            (Random.value < unavailableSpaceThirdChance) || (mapZoneSize.x < minPartSize.x) ? mapZoneSize.x : Random.Range(minPartSize.x, mapZoneSize.x / 2 + 1),
                            (Random.value < unavailableSpaceThirdChance) || (mapZoneSize.y < minPartSize.y) ? mapZoneSize.y : Random.Range(minPartSize.y, mapZoneSize.y / 2 + 1)));
                    else
                        unavailableSpaces.Add(Vector2Int.zero);
                }
                MakeAtLeastOneZoneClear();
                PreventCornerZonesIsolation();
                
                void MakeAtLeastOneZoneClear()
                {
                    if (unavailableSpaces.TrueForAll(x => x != Vector2Int.zero))
                    {
                        int[] indexes = new int[] { 1, 3, 4, 6 };
                        int randomIndex = Random.Range(0, indexes.Length);
                        int index = indexes[randomIndex];
                        unavailableSpaces[index] = Vector2Int.zero;
                    }
                }

                void PreventCornerZonesIsolation()
                {
                    if (unavailableSpaces[1] == mapZoneSize && unavailableSpaces[3] == mapZoneSize)
                        unavailableSpaces[Random.Range(0, 2) == 0 ? 1 : 3] = Vector2Int.zero;

                    if (unavailableSpaces[1] == mapZoneSize && unavailableSpaces[4] == mapZoneSize)
                        unavailableSpaces[Random.Range(0, 2) == 0 ? 1 : 4] = Vector2Int.zero;

                    if (unavailableSpaces[3] == mapZoneSize && unavailableSpaces[6] == mapZoneSize)
                        unavailableSpaces[Random.Range(0, 2) == 0 ? 3 : 6] = Vector2Int.zero;

                    if (unavailableSpaces[4] == mapZoneSize && unavailableSpaces[6] == mapZoneSize)
                        unavailableSpaces[Random.Range(0, 2) == 0 ? 4 : 6] = Vector2Int.zero;
                }
            }

            Matrix<double> GenerateZoneWithOffset(Vector2 start, Vector2 end)
            {
                var mapZone = Matrix<double>.Build.Dense(mapZoneSize.x, mapZoneSize.y, 1);
                for (int i = (int)start.x; i < end.x; i++)
                    for (int j = (int)start.y; j < end.y; j++)
                        mapZone[i, j] = 0;

                return mapZone;
            }
            
            mapZones.Add(GenerateZoneWithOffset(
                new Vector2Int{x = 0, y = 0},
                new Vector2Int{x = unavailableSpaces[0].x, y = unavailableSpaces[0].y}));
            mapZones.Add(GenerateZoneWithOffset(
                new Vector2Int{x = mapZoneSize.x/2 - unavailableSpaces[1].x/2, y = 0},
                new Vector2Int{x = mapZoneSize.x/2 + unavailableSpaces[1].x/2, y = unavailableSpaces[1].y}));
            mapZones.Add(ReverseRows(GenerateZoneWithOffset(
                new Vector2Int{x = 0, y = 0},
                new Vector2Int{x = unavailableSpaces[2].x, y = unavailableSpaces[2].y})));

            mapZones.Add(GenerateZoneWithOffset(
                new Vector2Int{x = 0, y = mapZoneSize.y/2 - unavailableSpaces[3].y/2},
                new Vector2Int{x = unavailableSpaces[3].x, y = mapZoneSize.y/2 + unavailableSpaces[3].y/2}));
            mapZones.Add(GenerateZoneWithOffset(
                new Vector2Int{x = 0, y = 0},
                new Vector2Int{x = 0, y = 0}));
            mapZones.Add(ReverseRows(GenerateZoneWithOffset(
                new Vector2Int{x = 0, y = mapZoneSize.y/2 - unavailableSpaces[4].y/2},
                new Vector2Int{x = unavailableSpaces[4].x, y = mapZoneSize.y/2 + unavailableSpaces[4].y/2})));

            mapZones.Add(ReverseColumns(GenerateZoneWithOffset(
                new Vector2Int{x = 0, y = 0},
                new Vector2Int{x = unavailableSpaces[5].x, y = unavailableSpaces[5].y})));
            mapZones.Add(ReverseColumns(GenerateZoneWithOffset(
                new Vector2Int{x = mapZoneSize.x/2 - unavailableSpaces[6].x/2, y = 0},
                new Vector2Int{x = mapZoneSize.x/2 + unavailableSpaces[6].x/2, y = unavailableSpaces[6].y})));
            mapZones.Add(ReverseColumns(ReverseRows(GenerateZoneWithOffset(
                new Vector2Int{x = 0, y = 0},
                new Vector2Int{x = unavailableSpaces[7].x, y = unavailableSpaces[7].y}))));
        }

        void InsertZonesInMapLayout()
        {
            for (int i = 0; i < 3; i++)
            {
                mapLayout.SetSubMatrix(i * mapZoneSize.x, 0, mapZones[i]);
                mapLayout.SetSubMatrix(i * mapZoneSize.x, mapZoneSize.y, mapZones[i+3]);
                mapLayout.SetSubMatrix(i * mapZoneSize.x, mapZoneSize.y * 2, mapZones[i+6]);
            }
        }

        void GenerateBoundaries()
        {
            mapLayout = mapLayout.InsertColumn(0, Vector<double>.Build.Dense(mapLayout.RowCount, 0))
                                 .InsertColumn(mapLayout.ColumnCount + 1, Vector<double>.Build.Dense(mapLayout.RowCount, 0))
                                 .InsertRow(0, Vector<double>.Build.Dense(mapLayout.ColumnCount + 2, 0))
                                 .InsertRow(mapLayout.RowCount + 1, Vector<double>.Build.Dense(mapLayout.ColumnCount + 2, 0));
        }

        void ClearUnnecessaryWalls()
        {
            bool IsMarketBorder(int i, int j)
            {
                for (int x = -1; x <= 1; x++)
                    for (int y = -1; y <= 1; y++)
                        if (i + x >= 0 && i + x < mapLayout.RowCount && j + y >= 0 && j + y < mapLayout.ColumnCount)
                            if (mapLayout[i + x, j + y] == 1)
                                return true;

                return false;
            }

            for (int i = 0; i < mapLayout.RowCount; i++)
                for (int j = 0; j < mapLayout.ColumnCount; j++)
                    if (mapLayout[i, j] == 0)
                        if (!IsMarketBorder(i, j))
                            mapLayout[i, j] = -1;
        }
    
        void ClearPartsSmallerThanMin()
        {
            
        }

        void GenerateEmptySpaces()
        {
            var emptySpacesCount = Random.Range(minEmptySpacesCount, maxEmptySpacesCount + 1);
            for (int i = 0; i < emptySpacesCount; i++)
            {
                Vector2Int emptySpace = new Vector2Int(Random.Range(minPartSize.x, maxEmptySpaceSize.x + 1), Random.Range(minPartSize.y, maxEmptySpaceSize.y + 1));
                int tries = 0;
                while (tries < 3)
                {
                    Vector2Int point = new Vector2Int(Random.Range(0, mapLayout.RowCount - emptySpace.y), Random.Range(0, mapLayout.ColumnCount - emptySpace.x));
                    if (mapLayout.SubMatrix(point.x, emptySpace.y, point.y, emptySpace.x).Exists(x => x == 0 || x == -1))
                        tries++;
                    else
                    {
                        mapLayout.SetSubMatrix(point.x, point.y, Matrix<double>.Build.Dense(emptySpace.y, emptySpace.x, 3));
                        break;
                    }
                }
            }
        }
    }

    private void PrintMapLayout() => Debug.Log(mapLayout.ToString());

    private void GenerateMap(Matrix<double> mapLayout)
    {
        for (int i = 0; i < mapLayout.RowCount; i++)
            for (int j = 0; j < mapLayout.ColumnCount; j++)
                if (mapLayout[i, j] != -1)
                    tilemap.SetTile(new Vector3Int(i, j, 0), tiles[(int)mapLayout[i, j]]);
    }

    private void GenerateProps()
    {
        // for (int i = 0; i < mapLayout.ColumnCount; i++)
        //     for (int j = 0; j < mapLayout.RowCount; j++)
        //         if (mapLayout[i, j] == 1)
        //             if (((mapLayout[i - 1, j] == 0) || (mapLayout[i + 1, j] == 0)) && ((mapLayout[i, j - 1] == 0) || (mapLayout[i, j + 1] == 0)))
        //                 GenerateShelvings(new Vector2Int(i, j));

        //GenerateShelvings(new Vector2Int(0, 0));
        
        void GenerateShelvings(Vector2Int startPoint)
        {
            bool isVertical = Random.value < 0.5f;
            int count = Random.Range(1, maxInRowShelvingsCount);
            int length = Random.Range(minShelvingLength, maxShelvingLength);
            Vector2Int shelvingsSize;

            Debug.Log(isVertical);
            Debug.Log(count);
            Debug.Log(length);

            shelvingsSize = new Vector2Int(length, count + count*2);

            // if (isVertical) shelvingsSize = new Vector2Int (count + count*2, length);
            // else shelvingsSize = new Vector2Int(length, count + count*2);

            List<int> shelvingScheme = new List<int>();
            int [,] shelvings = new int[shelvingsSize.x, shelvingsSize.y];

            for (int i = 0; i < count; i++)
                shelvingScheme.AddRange(new int[] {2, 2, 3});
            Debug.Log($"Shelving Scheme Contents: {string.Join(", ", shelvingScheme)}");
                
            for (int i = 0; i < length; i++)
                for (int j = 0; j < shelvingScheme.Count; j++)
                    shelvings[i, j] = shelvingScheme[j];

            string matrixString = string.Join("\n", Enumerable.Range(0, shelvings.GetLength(0))
                .Select(i => string.Join(" ", Enumerable.Range(0, shelvings.GetLength(1))
                    .Select(j => shelvings[i, j].ToString().PadLeft(4, ' ')))));
            Debug.Log(matrixString);

            for (int i = startPoint.x; i < startPoint.x + shelvings.GetLength(0); i++)
                for (int j = startPoint.y; j < startPoint.y + shelvings.GetLength(1); j++)
                    mapLayout[i, j] = shelvings[i - startPoint.x, j - startPoint.y];
            


            // for (int i = startPoint.x; i < startPoint.x + shelvingsSize.x; i++)
            //     for (int j = startPoint.y; j < startPoint.y + shelvingsSize.y; j++)
            //         if (mapLayout[i, j] != 0)
            //             if (j == 0)
            //                 if (isVertical) count--;
            //                 else 
            //             else
            //                 if (isVertical)
            //                 else
        }

        void GenerateStands()
        {

        }
    }

    #region Matrix
    private Matrix<double> ReverseRows(Matrix<double> matrix)
        => matrix.MapIndexed((i, j, _) => matrix[matrix.RowCount - 1 - i, j]);

    private Matrix<double> ReverseColumns(Matrix<double> matrix)
        => matrix.MapIndexed((i, j, _) => matrix[i, matrix.ColumnCount - 1 - j]);
    #endregion
}

