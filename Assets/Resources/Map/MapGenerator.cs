// using System.Collections.Generic;
// using System.Linq;
// using Sirenix.OdinInspector;
// using UnityEngine;
// using UnityEngine.Tilemaps;
// using MathNet.Numerics.LinearAlgebra;
// using System;

// using Random = UnityEngine.Random;

// // public enum TileType //Положительные индексы - тайлы, отрицательные - служебные ячейки
// // {
// //     NoTile = -10,
// //     PrototypeFloor = -1,
// //     Empty = 0,
// //     Wall = 1,
// //     Floor = 2,
// //     Shelvings = 3,
// //     Checkout = 4,
// //     Stand = 5,
// //     Door = 6
// // }

// public class MapGenerator : MonoBehaviour
// {
//     Tilemap tilemap;
//     [SerializeField] TileBase[] tiles;

//     Vector2Int mapZoneSize;
//     List<Matrix<double>> mapZones;
//     Matrix<double> mapLayout; //При индексации в Matrix сначала идёт row, потом column (ИЛИ НЕТ???)

//     [SerializeField] Vector2Int thirdSizeMin = new Vector2Int(5, 5);
//     [SerializeField] Vector2Int thirdSizeMax = new Vector2Int(16, 16);

//     [SerializeField][InfoBox("Cant be larger than thirdSizeMin")] Vector2Int outerEmptySpaceSizeMin = new Vector2Int(4, 4);
//     [SerializeField] float outerEmptySpacePartSpawnChance = 0.3f;
//     [SerializeField] float outerEmptySpaceThirdSpawnChance = 1f;
//     [SerializeField] int innerEmptySpacesCountMin = 0;
//     [SerializeField] int innerEmptySpacesCountMax = 3;
//     [SerializeField] Vector2Int innerEmptySpaceSizeMax = new Vector2Int(8, 8);

//     [SerializeField] float shelvingsAlongWallsChance = 0.5f;
//     [SerializeField] int shelvingsCountInRowMax = 6;
//     [SerializeField] int shelvingLengthMin = 3;
//     [SerializeField] int shelvingLengthMax = 6;
//     [SerializeField] int countersCountMin = 2;

//     void Start()
//     {
//         tilemap = FindFirstObjectByType<Tilemap>();
//         GenerateMap();
//     }

//     [Button("Generate map")]
//     public void GenerateMap()
//     {
//         tilemap.ClearAllTiles();
//         GenerateMapLayout();
//         GenerateTilemapFromMatrix(mapLayout);
//     }

//     void GenerateMapLayout()
//     {
//         mapZoneSize = new Vector2Int(Random.Range(thirdSizeMin.x, thirdSizeMax.x), Random.Range(thirdSizeMin.y, thirdSizeMax.y));
//         mapLayout = Matrix<double>.Build.Dense(mapZoneSize.x * 3, mapZoneSize.y * 3, 0);
//         mapZones = new List<Matrix<double>>();
//         var exitZone = Matrix<double>.Build.Dense(mapZoneSize.x, mapZoneSize.y, 0);
//         List<Vector2Int> outerEmptySpaces = new List<Vector2Int>(8);

//         GenerateBoundaries(); //+
//         //GenerateInnerEmptySpaces(); //внутри будет GeneratePillars, GenerateInnerEmptySpaces
//         GenerateCheckoutExitZone(); //
//         //GenerateShelvings();
//         //FillFloor(); //+

//         #region GeneralMethods
//         Matrix<double> RotateRight(Matrix<double> matrix) => matrix.Transpose().ReverseColumns();

//         bool CheckTilesTypeInDirection(Vector2Int point, Vector2Int direction, int count, int cellType)
//         {
//             for (int i = 0; i < count; i++)
//             {
//                 var currentPoint = new Vector2Int(i * direction.x + point.x, i * direction.y + point.y);
//                 if (!mapLayout.IsIndexExist(currentPoint.x, currentPoint.y) || mapLayout[currentPoint.x, currentPoint.y] != cellType) 
//                     return false;
//             }
//             return true;
//         }

//         void FillTilesInDirection(Vector2Int point, Vector2Int direction, int tileType, int stopTileType)
//         {
//             Vector2Int currentPoint = point;
//             while (mapLayout.IsIndexExist(currentPoint.x, currentPoint.y) && mapLayout[currentPoint.x, currentPoint.y] != stopTileType)
//             {
//                 mapLayout[currentPoint.x, currentPoint.y] = tileType;
//                 currentPoint += direction;
//             }
//         }

//         bool IsTilesOfTypeFillArea(Vector2Int point, Vector2Int area, int tileType)
//         {
//             for (int i = 0; i < area.x; i++)
//                 for (int j = 0; i < area.y; j++)
//                     if (mapLayout[point.x + i, point.y + j] != tileType) return false;
//             return true;
//         }

//         Matrix<double> GetAllNeighborsOfTileType(Vector2Int point) =>
//             mapLayout.SubMatrix(Math.Clamp(point.x - 1, 0, mapLayout.RowCount), point.x + 1 >= mapLayout.RowCount ? 2 : 3, Math.Clamp(point.y - 1, 0, mapLayout.ColumnCount), point.y + 1 >= mapLayout.ColumnCount ? 2 : 3);
        
//         Matrix<double> GetDirectNeighborsOfTileType(Vector2Int point)
//         {
//             var neighbors = GetAllNeighborsOfTileType(point);
//             for (int i = 0; i < neighbors.RowCount; i++)
//                 for (int j = 0; j < neighbors.ColumnCount; j++)
//                     if ((i == 0 || i == neighbors.RowCount - 1) && (j == 0 || j == neighbors.ColumnCount - 1))
//                         neighbors[i, j] = (int)TileType.NoTile;
//             return neighbors;
//         }

//         bool HasTileTypeInAllNeighbors(Vector2Int point, int tileType) =>
//             GetAllNeighborsOfTileType(point).Find(n => n == tileType) != null;

//         bool HasTileTypeInDirectNeighbors(Vector2Int point, int tileType) =>
//             GetDirectNeighborsOfTileType(point).Find(n => n == tileType) != null;
//         #endregion

//         void GenerateBoundaries()
//         {
//             CalculateZonesSizes();
//             void CalculateZonesSizes()
//             {
//                 for (int i = 0; i < 8; i++)
//                 {
//                     if (Random.value < outerEmptySpacePartSpawnChance)
//                         outerEmptySpaces.Add(new Vector2Int(
//                             Random.value < outerEmptySpaceThirdSpawnChance ? mapZoneSize.x : Random.Range(outerEmptySpaceSizeMin.x, outerEmptySpaceSizeMin.x + mapZoneSize.x / 2),
//                             Random.value < outerEmptySpaceThirdSpawnChance ? mapZoneSize.y : Random.Range(outerEmptySpaceSizeMin.y, outerEmptySpaceSizeMin.y + mapZoneSize.y / 2)));
//                     else
//                         outerEmptySpaces.Add(Vector2Int.zero);
//                 }
                
//                 MakeAtLeastOneZoneClear();                  
//                 void MakeAtLeastOneZoneClear()
//                 {
//                     if (outerEmptySpaces.TrueForAll(x => x != Vector2Int.zero))
//                     {
//                         int[] indexes = new int[] { 1, 3, 4, 6 };
//                         int randomIndex = Random.Range(0, indexes.Length);
//                         int index = indexes[randomIndex];
//                         outerEmptySpaces[index] = Vector2Int.zero;
//                     }
//                 }

//                 PreventCornerZonesIsolation();
//                 void PreventCornerZonesIsolation()
//                 {
//                     if (outerEmptySpaces[1] == mapZoneSize && outerEmptySpaces[3] == mapZoneSize)
//                         outerEmptySpaces[Random.Range(0, 2) == 0 ? 1 : 3] = Vector2Int.zero;

//                     if (outerEmptySpaces[1] == mapZoneSize && outerEmptySpaces[4] == mapZoneSize)
//                         outerEmptySpaces[Random.Range(0, 2) == 0 ? 1 : 4] = Vector2Int.zero;

//                     if (outerEmptySpaces[3] == mapZoneSize && outerEmptySpaces[6] == mapZoneSize)
//                         outerEmptySpaces[Random.Range(0, 2) == 0 ? 3 : 6] = Vector2Int.zero;

//                     if (outerEmptySpaces[4] == mapZoneSize && outerEmptySpaces[6] == mapZoneSize)
//                         outerEmptySpaces[Random.Range(0, 2) == 0 ? 4 : 6] = Vector2Int.zero;
//                 }
//             }

//             GenerateZones();
//             void GenerateZones()
//             {
//                 Matrix<double> GenerateZoneWithOffset(Vector2 start, Vector2 end)
//                 {
//                     var mapZone = Matrix<double>.Build.Dense(mapZoneSize.x, mapZoneSize.y, (int)TileType.PrototypeFloor);
//                     for (int i = (int)start.x; i < end.x; i++)
//                         for (int j = (int)start.y; j < end.y; j++)
//                             mapZone[i, j] = (int)TileType.Wall;

//                     return mapZone;
//                 }
                
//                 mapZones.Add(GenerateZoneWithOffset(
//                     new Vector2Int{x = 0, y = 0},
//                     new Vector2Int{x = outerEmptySpaces[0].x, y = outerEmptySpaces[0].y}));
//                 mapZones.Add(GenerateZoneWithOffset(
//                     new Vector2Int{x = (int)MathF.Floor(mapZoneSize.x/2) - (int)MathF.Ceiling(outerEmptySpaces[1].x/2), y = 0},
//                     new Vector2Int{x = (int)MathF.Floor(mapZoneSize.x/2) - (int)MathF.Ceiling(outerEmptySpaces[1].x/2) + outerEmptySpaces[1].x, y = outerEmptySpaces[1].y}));
//                 mapZones.Add(GenerateZoneWithOffset(
//                     new Vector2Int{x = 0, y = 0},
//                     new Vector2Int{x = outerEmptySpaces[2].x, y = outerEmptySpaces[2].y})
//                     .ReverseRows());

//                 mapZones.Add(GenerateZoneWithOffset(
//                     new Vector2Int{x = 0, y = (int)MathF.Floor(mapZoneSize.y/2) - (int)MathF.Ceiling(outerEmptySpaces[3].y/2)},
//                     new Vector2Int{x = outerEmptySpaces[3].x, y = (int)MathF.Floor(mapZoneSize.y/2) - (int)MathF.Ceiling(outerEmptySpaces[3].y/2) + outerEmptySpaces[3].y}));
//                 mapZones.Add(GenerateZoneWithOffset(
//                     new Vector2Int{x = 0, y = 0},
//                     new Vector2Int{x = 0, y = 0}));
//                 mapZones.Add(GenerateZoneWithOffset(
//                     new Vector2Int{x = 0, y = (int)MathF.Floor(mapZoneSize.y/2) - (int)MathF.Ceiling(outerEmptySpaces[4].y/2)},
//                     new Vector2Int{x = outerEmptySpaces[4].x, y = (int)MathF.Floor(mapZoneSize.y/2) - (int)MathF.Ceiling(outerEmptySpaces[4].y/2)  + outerEmptySpaces[4].y})
//                     .ReverseRows());

//                 mapZones.Add(GenerateZoneWithOffset(
//                     new Vector2Int{x = 0, y = 0},
//                     new Vector2Int{x = outerEmptySpaces[5].x, y = outerEmptySpaces[5].y})
//                     .ReverseColumns());
//                 mapZones.Add(GenerateZoneWithOffset(
//                     new Vector2Int{x = (int)MathF.Floor(mapZoneSize.x/2) - (int)MathF.Ceiling(outerEmptySpaces[6].x/2), y = 0},
//                     new Vector2Int{x = (int)MathF.Floor(mapZoneSize.x/2) - (int)MathF.Ceiling(outerEmptySpaces[6].x/2) + outerEmptySpaces[6].x, y = outerEmptySpaces[6].y})
//                     .ReverseColumns());
//                 mapZones.Add(GenerateZoneWithOffset(
//                     new Vector2Int{x = 0, y = 0},
//                     new Vector2Int{x = outerEmptySpaces[7].x, y = outerEmptySpaces[7].y})
//                     .ReverseRows().ReverseColumns());
//             }

//             InsertZonesInMapLayout();
//             void InsertZonesInMapLayout()
//             {
//                 for (int i = 0; i < 3; i++)
//                 {
//                     mapLayout.SetSubMatrix(i * mapZoneSize.x, 0, mapZones[i]);
//                     mapLayout.SetSubMatrix(i * mapZoneSize.x, mapZoneSize.y, mapZones[i+3]);
//                     mapLayout.SetSubMatrix(i * mapZoneSize.x, mapZoneSize.y * 2, mapZones[i+6]);
//                 }
//             }

//             GenerateBuildingWalls();
//             void GenerateBuildingWalls()
//             {
//                 mapLayout = mapLayout.InsertColumn(0, Vector<double>.Build.Dense(mapLayout.RowCount, (int)TileType.Wall))
//                                     .InsertColumn(mapLayout.ColumnCount + 1, Vector<double>.Build.Dense(mapLayout.RowCount, (int)TileType.Wall))
//                                     .InsertRow(0, Vector<double>.Build.Dense(mapLayout.ColumnCount + 2, (int)TileType.Wall))
//                                     .InsertRow(mapLayout.RowCount + 1, Vector<double>.Build.Dense(mapLayout.ColumnCount + 2, (int)TileType.Wall));
//             }

//             ClearFloorPartsSmallerThanMin();
//             void ClearFloorPartsSmallerThanMin()
//             {
//                 for (int i = 0; i < mapLayout.RowCount; i++)
//                     for (int j = 0; j < mapLayout.ColumnCount; j++)
//                         if (mapLayout[i, j] == (int)TileType.Wall)
//                         {
//                             if (mapLayout.IsIndexExist(i - 1, j) && mapLayout[i - 1, j] == (int)TileType.PrototypeFloor)
//                                 if (!CheckTilesTypeInDirection(new Vector2Int(i - 1, j), new Vector2Int(-1, 0), outerEmptySpaceSizeMin.x, (int)TileType.PrototypeFloor))
//                                     FillTilesInDirection(new Vector2Int(i - 1, j), new Vector2Int(-1, 0), (int)TileType.Wall, (int)TileType.Wall);

//                             if (mapLayout.IsIndexExist(i + 1, j) && mapLayout[i + 1, j] == (int)TileType.PrototypeFloor)
//                                 if (!CheckTilesTypeInDirection(new Vector2Int(i + 1, j), new Vector2Int(1, 0), outerEmptySpaceSizeMin.x, (int)TileType.PrototypeFloor))
//                                     FillTilesInDirection(new Vector2Int(i + 1, j), new Vector2Int(1, 0), (int)TileType.Wall, (int)TileType.Wall);

//                             if (mapLayout.IsIndexExist(i, j - 1) && mapLayout[i, j - 1] == (int)TileType.PrototypeFloor)
//                                 if (!CheckTilesTypeInDirection(new Vector2Int(i, j - 1), new Vector2Int(0, -1), outerEmptySpaceSizeMin.y, (int)TileType.PrototypeFloor))
//                                     FillTilesInDirection(new Vector2Int(i, j - 1), new Vector2Int(0, -1), (int)TileType.Wall, (int)TileType.Wall);

//                             if (mapLayout.IsIndexExist(i, j + 1) && mapLayout[i, j + 1] == (int)TileType.PrototypeFloor)
//                                 if (!CheckTilesTypeInDirection(new Vector2Int(i, j + 1), new Vector2Int(0, 1), outerEmptySpaceSizeMin.y, (int)TileType.PrototypeFloor))
//                                     FillTilesInDirection(new Vector2Int(i, j + 1), new Vector2Int(0, 1), (int)TileType.Wall, (int)TileType.Wall);
//                         }                            
//             }
        
//             ClearUnnecessaryWalls();
//             void ClearUnnecessaryWalls()
//             {
//                 for (int i = 0; i < mapLayout.RowCount; i++)
//                     for (int j = 0; j < mapLayout.ColumnCount; j++)
//                         if (mapLayout[i, j] == (int)TileType.Wall)
//                             if (!HasTileTypeInAllNeighbors(new Vector2Int(i, j), (int)TileType.PrototypeFloor))
//                                 mapLayout[i, j] = (int)TileType.Empty;
//             }
//         }
    
//         void GenerateCheckoutExitZone()
//         {
//             Matrix<double> checkoutExitScheme = null;

//             GenerateCheckoutExitScheme();
//             void GenerateCheckoutExitScheme()
//             {
//                 var counterScheme = Matrix<double>.Build.Dense(4, 2,
//                     new double[]
//                     {
//                         (int)TileType.Floor, (int)TileType.Checkout, (int)TileType.Checkout, (int)TileType.Floor, 
//                         (int)TileType.Floor, (int)TileType.Floor, (int)TileType.Floor, (int)TileType.Floor
//                     });

//                 Matrix<double> checkoutCountersScheme = counterScheme;
//                 for (int i = 0; i < countersCountMin - 1; i++) //стоек должно быть кол-во игроков/2
//                     checkoutCountersScheme = counterScheme.Append(counterScheme);

//                 var possibleExitPosition = new[] { "left", "right" };//, "above" };
//                 var exitPosition = possibleExitPosition[Random.Range(0, possibleExitPosition.Length)];
//                 Debug.Log(exitPosition);
                
//                 checkoutExitScheme = checkoutCountersScheme;
//                 checkoutExitScheme = checkoutExitScheme.InsertColumn(0, Vector<double>.Build.Dense(checkoutExitScheme.RowCount, (int)TileType.Floor));
//                 Debug.Log(checkoutExitScheme);

//                 var exitLength = Random.Range(1, 4) * 2;
//                 var exitWall = Vector<double>.Build.Dense(exitLength, (int)TileType.Wall);
//                 exitWall[exitLength/2-1] = (int)TileType.Door;
//                 exitWall[exitLength/2] = (int)TileType.Door;
//                 var exitScheme = Matrix<double>.Build.Dense(checkoutExitScheme.RowCount, exitLength, (int)TileType.Floor)
//                                 .InsertRow(0, exitWall);
//                 Debug.Log(exitScheme);

//                 if (exitPosition != "above")
//                     checkoutExitScheme = checkoutExitScheme.InsertRow(0, Vector<double>.Build.Dense(checkoutExitScheme.ColumnCount, (int)TileType.Wall))
//                                                             .Append(exitScheme);
//                     if (exitPosition == "left")
//                         checkoutExitScheme = checkoutExitScheme.ReverseColumns();
//                 // else 
//                 // {
//                 //     checkoutExitScheme = checkoutExitScheme.Transpose().ReverseColumns();
//                 //     checkoutExitScheme = checkoutExitScheme.Append(Matrix<double>.Build.Dense(checkoutExitScheme.RowCount, Random.Range(1, 4) * 2, (int)TileType.Floor));
//                 //     checkoutExitScheme = checkoutExitScheme.ReverseColumns().Transpose();
//                 // }
//                 Debug.Log(checkoutExitScheme);
//                 // var aboba = checkoutExitScheme[0,0];
//                 // aboba = 10;
//                 // Debug.Log(checkoutExitScheme);

                
//             }

//             InsertCheckoutExitInMapLayout();
//             void InsertCheckoutExitInMapLayout()
//             {
//                 mapLayout.SetSubMatrix(0, Random.Range(0, mapLayout.ColumnCount - 1 - checkoutExitScheme.ColumnCount), checkoutExitScheme);

//                 var insertSide = Random.Range(0, 4); //"up", "right", "down", "left"
//                 Debug.Log("Choosen insert side: " + insertSide);

//                 for (int i = 0; i < insertSide; i++)
//                     mapLayout = RotateRight(mapLayout);
//                     //checkoutExitScheme = RotateRight(checkoutExitScheme);

//                 //поворачивать сам mapLayout

//                 // unsafe
//                 // {
//                 //     double*[] wallRefs = new double*[mapLayout.Enumerate().Count(x => x == (int)TileType.Wall)];
//                 //     int i = 0;
//                 //     for (int row = 0; row < mapLayout.RowCount; row++)
//                 //         for (int col = 0; col < mapLayout.ColumnCount; col++)
//                 //             if (mapLayout[row, col] == (int)TileType.Wall)
//                 //                 wallRefs[i++] = &mapLayout[row, col];
//                 // }

                
//             }
//         }

//         void GenerateEmptySpaces()
//         {
//             var emptySpacesCount = Random.Range(innerEmptySpacesCountMin, innerEmptySpacesCountMax + 1);
//             for (int i = 0; i < emptySpacesCount; i++)
//             {
//                 Vector2Int emptySpace = new Vector2Int(Random.Range(outerEmptySpaceSizeMin.x, innerEmptySpaceSizeMax.x + 1), Random.Range(outerEmptySpaceSizeMin.y, innerEmptySpaceSizeMax.y + 1));
//                 int tries = 0;
//                 while (tries < 3)
//                 {
//                     Vector2Int point = new Vector2Int(Random.Range(0, mapLayout.RowCount - emptySpace.y), Random.Range(0, mapLayout.ColumnCount - emptySpace.x));
//                     if (mapLayout.SubMatrix(point.x, emptySpace.y, point.y, emptySpace.x).Exists(x => x == 0 || x == -1))
//                         tries++;
//                     else
//                     {
//                         mapLayout.SetSubMatrix(point.x, point.y, Matrix<double>.Build.Dense(emptySpace.y, emptySpace.x, 3));
//                         break;
//                     }
//                 }
//             }
//         }
    
//         void GenerateShelvings()
//         {
//             GenerateShelvingsAlongWalls();
//             void GenerateShelvingsAlongWalls()
//             {
//                 // void FillAlongWall(Vector2Int startPoint, Vector2Int direction, Vector2Int checkOffset)
//                 // {
//                 //     var tile = Random.value < shelvingsAlongWallsChance ? (int)TileType.Floor : (int)TileType.Shelvings;
//                 //     var endPoint = startPoint + direction;
//                 //     while (mapLayout[endPoint.x, endPoint.y] == (int)TileType.PrototypeFloor)
//                 //     {
//                 //         if ((endPoint.x > 0 || endPoint.x <= mapLayout.RowCount) &&
//                 //             (endPoint.y > 0 || endPoint.y <= mapLayout.ColumnCount))
//                 //             if (mapLayout[endPoint.x + checkOffset.x, endPoint.y + checkOffset.y] == (int)TileType.Wall)
//                 //             {
//                 //                 mapLayout[endPoint.x, endPoint.y] = tile;
//                 //                 if (tile == (int)TileType.Shelvings) mapLayout[endPoint.x - checkOffset.x, endPoint.y - checkOffset.y] = (int)TileType.Floor;
//                 //             }

//                 //         endPoint += direction;
//                 //     }
//                 // }

//                 // var tempMap = Matrix<double>.Build.Dense(mapLayout.RowCount, mapLayout.ColumnCount, double.NaN);

//                 // for (int i = 0; i < mapLayout.RowCount; i++)
//                 //     for (int j = 0; j < mapLayout.ColumnCount; j++)
//                 //         if (mapLayout[i, j] == (int)TileType.PrototypeFloor)
//                 //         {
//                 //             if (mapLayout[i - 1, j - 1] == (int)TileType.Wall)
//                 //             {
//                 //                 if (mapLayout[i - 1, j] == (int)TileType.Wall && mapLayout[i, j - 1] == (int)TileType.Wall)
//                 //                     tempMap[i, j] = -2;
//                 //                 if (mapLayout[i - 1, j] == (int)TileType.PrototypeFloor && mapLayout[i, j - 1] == (int)TileType.PrototypeFloor)
//                 //                     tempMap[i, j] = -9;
//                 //             }
//                 //             if (mapLayout[i + 1, j - 1] == (int)TileType.Wall)
//                 //             {
//                 //                 if (mapLayout[i + 1, j] == (int)TileType.Wall && mapLayout[i, j - 1] == (int)TileType.Wall)
//                 //                     tempMap[i, j] = -3;
//                 //                 if (mapLayout[i + 1, j] == (int)TileType.PrototypeFloor && mapLayout[i, j - 1] == (int)TileType.PrototypeFloor)
//                 //                     tempMap[i, j] = -7;
//                 //             }
//                 //             if (mapLayout[i - 1, j + 1] == (int)TileType.Wall)
//                 //             {
//                 //                 if (mapLayout[i - 1, j] == (int)TileType.Wall && mapLayout[i, j + 1] == (int)TileType.Wall)
//                 //                     tempMap[i, j] = -4;
//                 //                 if (mapLayout[i - 1, j] == (int)TileType.PrototypeFloor && mapLayout[i, j + 1] == (int)TileType.PrototypeFloor)
//                 //                     tempMap[i, j] = -8;
//                 //             }
//                 //             if (mapLayout[i + 1, j + 1] == (int)TileType.Wall)
//                 //             {
//                 //                 if (mapLayout[i + 1, j] == (int)TileType.Wall && mapLayout[i, j + 1] == (int)TileType.Wall)
//                 //                     tempMap[i, j] = -5;
//                 //                 if (mapLayout[i + 1, j] == (int)TileType.PrototypeFloor && mapLayout[i, j + 1] == (int)TileType.PrototypeFloor)
//                 //                     tempMap[i, j] = -6;
//                 //             }
//                 //         }

//                 // for (int i = 0; i < mapLayout.RowCount; i++)
//                 //     for (int j = 0; j < mapLayout.ColumnCount; j++)
//                 //         if (!double.IsNaN(tempMap[i, j]))
//                 //             mapLayout[i, j] = tempMap[i, j];

//                 // for (int i = 0; i < mapLayout.RowCount; i++)
//                 //     for (int j = 0; j < mapLayout.ColumnCount; j++)
//                 //         if (mapLayout[i, j] < -1)
//                 //         {
//                 //             switch (mapLayout[i, j])
//                 //             {
//                 //                 case -2:
//                 //                     FillAlongWall(new Vector2Int(i, j), new Vector2Int(1, 0), new Vector2Int(0, -1)); //направо
//                 //                     FillAlongWall(new Vector2Int(i, j), new Vector2Int(0, 1), new Vector2Int(-1, 0)); //вниз
//                 //                     break;
//                 //                 case -3:
//                 //                     FillAlongWall(new Vector2Int(i, j), new Vector2Int(-1, 0), new Vector2Int(0, -1)); //налево
//                 //                     FillAlongWall(new Vector2Int(i, j), new Vector2Int(0, 1), new Vector2Int(1, 0)); //вниз
//                 //                     break;
//                 //                 case -4:
//                 //                     FillAlongWall(new Vector2Int(i, j), new Vector2Int(1, 0), new Vector2Int(0, 1)); //направо
//                 //                     FillAlongWall(new Vector2Int(i, j), new Vector2Int(0, -1), new Vector2Int(-1, 0)); //вверх
//                 //                     break;
//                 //                 case -5:
//                 //                     FillAlongWall(new Vector2Int(i, j), new Vector2Int(-1, 0), new Vector2Int(0, 1)); //налево
//                 //                     FillAlongWall(new Vector2Int(i, j), new Vector2Int(0, -1), new Vector2Int(1, 0)); //вверх
//                 //                     break;
//                 //                 case -6:
//                 //                     FillAlongWall(new Vector2Int(i, j), new Vector2Int(1, 0), new Vector2Int(0, 1)); //направо
//                 //                     FillAlongWall(new Vector2Int(i, j), new Vector2Int(0, 1), new Vector2Int(1, 0)); //вниз
//                 //                     break;
//                 //                 case -7:
//                 //                     FillAlongWall(new Vector2Int(i, j), new Vector2Int(-1, 0), new Vector2Int(0, 1)); //налево
//                 //                     FillAlongWall(new Vector2Int(i, j), new Vector2Int(0, 1), new Vector2Int(1, 0)); //вниз
//                 //                     break;
//                 //                 case -8:
//                 //                     FillAlongWall(new Vector2Int(i, j), new Vector2Int(1, 0), new Vector2Int(0, -1)); //направо
//                 //                     FillAlongWall(new Vector2Int(i, j), new Vector2Int(0, -1), new Vector2Int(1, 0)); //вверх
//                 //                     break;
//                 //                 case -9:
//                 //                     FillAlongWall(new Vector2Int(i, j), new Vector2Int(-1, 0), new Vector2Int(0, -1)); //налево
//                 //                     FillAlongWall(new Vector2Int(i, j), new Vector2Int(0, -1), new Vector2Int(-1, 0)); //вверх
//                 //                     break;
//                 //             }
//                 //         }

//                 // for (int i = 0; i < mapLayout.RowCount; i++)
//                 //     for (int j = 0; j < mapLayout.ColumnCount; j++)
//                 //         switch (mapLayout[i, j])
//                 //         {
//                 //             case -2: case -3: case -4: case -5: //внутренние углы
//                 //                 if ((mapLayout[i - 1, j] == (int)TileType.Floor) || 
//                 //                     (mapLayout[i + 1, j] == (int)TileType.Floor) || 
//                 //                     (mapLayout[i, j - 1] == (int)TileType.Floor) || 
//                 //                     (mapLayout[i, j + 1] == (int)TileType.Floor))
//                 //                     mapLayout[i, j] = (int)TileType.Shelvings;
//                 //                 else mapLayout[i, j] = (int)TileType.Floor;
//                 //                 break;
//                 //         }

//                 // for (int i = 0; i < mapLayout.RowCount; i++)
//                 //     for (int j = 0; j < mapLayout.ColumnCount; j++)
//                 //         switch (mapLayout[i, j])
//                 //         {
//                 //             case -6: case -7: case -8: case -9: //внешние углы
//                 //                 if (((mapLayout[i - 1, j] == (int)TileType.Floor) || (mapLayout[i + 1, j] == (int)TileType.Floor)) && 
//                 //                     ((mapLayout[i, j - 1] == (int)TileType.Floor) || (mapLayout[i, j + 1] == (int)TileType.Floor)))
//                 //                     mapLayout[i, j] = (int)TileType.Floor;
//                 //                 else mapLayout[i, j] = (int)TileType.Shelvings;
//                 //                 break;
//                 //         }

//                 // for (int i = 0; i < mapLayout.RowCount; i++)
//                 //     for (int j = 0; j < mapLayout.ColumnCount; j++)
//                 //         if (mapLayout[i, j] == (int)TileType.Floor)
//                 //         {
//                 //             if (mapLayout[i - 1, j - 1] == (int)TileType.PrototypeFloor) mapLayout[i - 1, j - 1] = (int)TileType.Shelvings;
//                 //             if (mapLayout[i - 1, j] == (int)TileType.PrototypeFloor) mapLayout[i - 1, j] = (int)TileType.Shelvings;
//                 //             if (mapLayout[i - 1, j + 1] == (int)TileType.PrototypeFloor) mapLayout[i - 1, j + 1] = (int)TileType.Shelvings;
//                 //             if (mapLayout[i, j - 1] == (int)TileType.PrototypeFloor) mapLayout[i, j - 1] = (int)TileType.Shelvings;
//                 //             if (mapLayout[i, j + 1] == (int)TileType.PrototypeFloor) mapLayout[i, j + 1] = (int)TileType.Shelvings;
//                 //             if (mapLayout[i + 1, j - 1] == (int)TileType.PrototypeFloor) mapLayout[i + 1, j - 1] = (int)TileType.Shelvings;
//                 //             if (mapLayout[i + 1, j] == (int)TileType.PrototypeFloor) mapLayout[i + 1, j] = (int)TileType.Shelvings;
//                 //             if (mapLayout[i + 1, j + 1] == (int)TileType.PrototypeFloor) mapLayout[i + 1, j + 1] = (int)TileType.Shelvings;
//                 //         }
//             }

//             GenerateShelvingsAreas();
//             void GenerateShelvingsAreas()
//             {
//                 Matrix<double> shelvingsScheme = Matrix<double>.Build.Dense(1, 3, new double[]{2, 2, 3});

//                 bool isVertical = Random.value < 0.5f;
//                 int count = Random.Range(1, shelvingsCountInRowMax);
//                 int length = Random.Range(shelvingLengthMin, shelvingLengthMax);

//                 Matrix<double> shelvings = shelvingsScheme;//Matrix<double>.Build.Dense(shelvingsScheme.RowCount * length, shelvingsScheme.ColumnCount * count);

//                 for (int i = 0; i < count - 1; i++)
//                     shelvings = shelvings.Append(shelvingsScheme);
//                 for (int i = 1; i < length; i++)
//                     shelvings = shelvings.Stack(shelvingsScheme);

//                 bool IsAnySpaceAvailable()
//                 {
//                     //Vector2Int minSpace = new Vector2Int(shelvingsScheme.ColumnCount, minShelvingLength);
                    
//                     for (int i = 0; i < mapLayout.RowCount; i++)
//                         for (int j = 0; j < mapLayout.ColumnCount; j++)
//                             if (mapLayout[i, j] == 1)
//                                 if ((mapLayout[i - 1, j] == 1) &&
//                                     (mapLayout[i + 1, j] == 1) &&
//                                     (mapLayout[i, j - 1] == 1) &&
//                                     (mapLayout[i, j + 1] == 1) &&
//                                     (mapLayout[i - 1, j - 1] == 1) &&
//                                     (mapLayout[i - 1, j + 1] == 1) &&
//                                     (mapLayout[i + 1, j - 1] == 1) &&
//                                     (mapLayout[i + 1, j + 1] == 1))
//                                     return true;

//                     return false;
//                 }
                
//                 // bool CheckAvailableSpace()
//                 // {
//                 //     for (int i = 0; i < count * shelvingsScheme.ColumnCount; i++)
//                 //         for (int j = 0; j < length; j++)
//                 //             if (mapLayout[i, j] != 1)
//                 //             {
//                 //                 if (j >= minShelvingLength)
//                 //                     count--;
//                 //                 else if (i != 1) length--;
//                 //                 else return false;
//                 //             }
//                 // }
//             }

//             GenerateStands();
//             void GenerateStands()
//             {

//             }
//         }

//         void FillFloor() =>
//             mapLayout.MapInplace(tile => tile = tile == (int)TileType.PrototypeFloor ? (int)TileType.Floor : tile); 
//     }

//     private void PrintMapLayout() => Debug.Log(mapLayout.ToString());

//     private void GenerateTilemapFromMatrix(Matrix<double> mapLayout)
//     {
//         for (int i = 0; i < mapLayout.RowCount; i++)
//             for (int j = 0; j < mapLayout.ColumnCount; j++)
//                 if (mapLayout[i, j] > 0)
//                     tilemap.SetTile(new Vector3Int(j, i, 0), tiles[(int)mapLayout[i, j]]);
//     }
// }