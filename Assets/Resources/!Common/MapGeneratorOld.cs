// using System.Collections.Generic;
// using System.Linq;
// using Sirenix.OdinInspector;
// using Unity.VisualScripting;
// using UnityEngine;
// using UnityEngine.Tilemaps;
// using MathNet.Numerics;

// public class MapGeneratorOld : MonoBehaviour
// {
//     Matrix
//     Tilemap tilemap;
//     [SerializeField] TileBase[] tiles;

//     [SerializeField] Vector2Int minThirdSize = new Vector2Int(5, 5);
//     [SerializeField] Vector2Int maxThirdSize = new Vector2Int(16, 16);
//     [SerializeField] Vector2Int minPartSize = new Vector2Int(4, 4);
//     [SerializeField] float emptySpaceSpawnChance = 0.3f;
//     [SerializeField] float emptySpaceThirdChance = 1f;

//     Vector2Int mapZoneSize;
//     List<int[,]> mapZones;
//     int[,] mapLayout;
//     System.Numerics.Vector3

//     [SerializeField] int maxEmptySpacesCount = 3;
//     [SerializeField] Vector2Int maxEmptySpaceSize = new Vector2Int(8, 8);
//     [SerializeField] int maxInRowShelvingsCount = 6;
//     [SerializeField] int minShelvingLength = 3;
//     [SerializeField] int maxShelvingLength = 6;
    

//     void Start()
//     {
//         tilemap = FindFirstObjectByType<Tilemap>();
//         //tiles = Resources.LoadAll<TileBase>("Environment/Tilemap");
//         //Debug.Log("Loaded tiles: " + string.Join(", ", tiles.Select(x => x.name).ToArray()));
        
//         GenerateMapLayout();
//         GenerateProps();
//         GenerateMap();
//     }

//     void GenerateMapLayout()
//     {
//         mapZoneSize = new Vector2Int(Random.Range(minThirdSize.x, maxThirdSize.x), Random.Range(minThirdSize.y, maxThirdSize.y));
//         mapLayout = new int[mapZoneSize.x * 3, mapZoneSize.y * 3];
//         mapZones = new List<int[,]>();

//         List<Vector2Int> unavailableSpaces = new List<Vector2Int>(8);
//         for (int i = 0; i < 8; i++)
//         {
//             if (Random.value < emptySpaceSpawnChance)
//                 unavailableSpaces.Add(new Vector2Int(
//                     Random.value < emptySpaceThirdChance ? mapZoneSize.x : Random.Range(minPartSize.x, mapZoneSize.x / 2 + 1),
//                     Random.value < emptySpaceThirdChance ? mapZoneSize.y : Random.Range(minPartSize.y, mapZoneSize.y / 2 + 1)));
//             else
//                 unavailableSpaces.Add(Vector2Int.zero);
//         }      
        
//         if (unavailableSpaces.TrueForAll(x => x != Vector2Int.zero))
//         {
//             int[] indexes = new int[] { 1, 3, 4, 6 };
//             int randomIndex = Random.Range(0, indexes.Length);
//             int index = indexes[randomIndex];
//             unavailableSpaces[index] = Vector2Int.zero;
//         }

//         int[,] GenerateZoneWithOffset(Vector2Int start, Vector2Int end)
//         {
//             int[,] mapZone = new int[mapZoneSize.x, mapZoneSize.y];
//                 for (int j = 0; j < mapZoneSize.x; j++)
//                     for (int k = 0; k < mapZoneSize.y; k++)
//                         mapZone[j, k] = 1;   

//             for (int i = start.x; i < end.x; i++)
//                 for (int j = start.y; j < end.y; j++)
//                     mapZone[i, j] = 0;

//             return mapZone;
//         }

//         mapZones.Add(GenerateZoneWithOffset(new Vector2Int{x = 0, y = 0}, new Vector2Int{x = unavailableSpaces[0].x, y = unavailableSpaces[0].y}));
//         mapZones.Add(GenerateZoneWithOffset(new Vector2Int{x = mapZoneSize.x/2 - unavailableSpaces[1].x/2, y = 0}, new Vector2Int{x = mapZoneSize.x/2 + unavailableSpaces[1].x/2, y = unavailableSpaces[1].y}));
//         mapZones.Add(GenerateZoneWithOffset(new Vector2Int{x = mapZoneSize.x - unavailableSpaces[2].x, y = 0}, new Vector2Int{x = mapZoneSize.x, y = mapZoneSize.y - unavailableSpaces[2].y}));
//         mapZones.Add(GenerateZoneWithOffset(new Vector2Int{x = 0, y = mapZoneSize.y/2 - unavailableSpaces[3].y/2}, new Vector2Int{x = unavailableSpaces[3].x, y = mapZoneSize.y/2 + unavailableSpaces[3].y/2}));
//         mapZones.Add(GenerateZoneWithOffset(new Vector2Int{x = 0, y = 0}, new Vector2Int{x = 0, y = 0}));
//         mapZones.Add(GenerateZoneWithOffset(new Vector2Int{x = mapZoneSize.x - unavailableSpaces[4].x, y = mapZoneSize.y/2 - unavailableSpaces[4].y/2}, new Vector2Int{x = mapZoneSize.x, y = mapZoneSize.y/2 + unavailableSpaces[4].y/2})); //-
//         mapZones.Add(GenerateZoneWithOffset(new Vector2Int{x = 0, y = mapZoneSize.y - unavailableSpaces[5].y}, new Vector2Int{x = unavailableSpaces[5].x, y = mapZoneSize.y}));
//         mapZones.Add(GenerateZoneWithOffset(new Vector2Int{x = mapZoneSize.x/2 - unavailableSpaces[6].x/2, y = mapZoneSize.y - unavailableSpaces[6].y}, new Vector2Int{x = mapZoneSize.x/2 + unavailableSpaces[6].x/2, y = mapZoneSize.y}));
//         mapZones.Add(GenerateZoneWithOffset(new Vector2Int{x = mapZoneSize.x - unavailableSpaces[7].x, y = mapZoneSize.y - unavailableSpaces[7].y}, new Vector2Int{x = mapZoneSize.x, y = mapZoneSize.y}));
        
//         for (int i = 0; i < mapZoneSize.x; i++)
//             for (int j = 0; j < mapZoneSize.y; j++)
//             {
//                 mapLayout[i, j] = mapZones[0][i, j];
//                 mapLayout[i + mapZoneSize.x, j] = mapZones[1][i, j];
//                 mapLayout[i + mapZoneSize.x * 2, j] = mapZones[2][i, j];
//                 mapLayout[i, j + mapZoneSize.y] = mapZones[3][i, j];
//                 mapLayout[i + mapZoneSize.x, j + mapZoneSize.y] = mapZones[4][i, j];
//                 mapLayout[i + mapZoneSize.x * 2, j + mapZoneSize.y] = mapZones[5][i, j];
//                 mapLayout[i, j + mapZoneSize.y * 2] = mapZones[6][i, j];
//                 mapLayout[i + mapZoneSize.x, j + mapZoneSize.y * 2] = mapZones[7][i, j];
//                 mapLayout[i + mapZoneSize.x * 2, j + mapZoneSize.y * 2] = mapZones[8][i, j];
//             }

//         for (int i = 0; i < mapLayout.GetLength(0); i++)
//             for (int j = 0; j < mapLayout.GetLength(1); j++)
//                 if (i == 0 || j == 0 || i == mapLayout.GetLength(0) - 1 || j == mapLayout.GetLength(1) - 1)
//                     mapLayout[i, j] = 0;

//         PrintMapLayout();

//         for (int i = 1; i < mapLayout.GetLength(0) - 1; i++)
//             for (int j = 1; j < mapLayout.GetLength(1) - 1; j++)
//                 if (mapLayout[i, j] == 0 
//                     && (mapLayout[i - 1, j - 1] != 1) 
//                     && (mapLayout[i - 1, j] != 1) 
//                     && (mapLayout[i - 1, j + 1] != 1) 
//                     && (mapLayout[i, j - 1] != 1) 
//                     && (mapLayout[i, j + 1] != 1) 
//                     && (mapLayout[i + 1, j - 1] != 1) 
//                     && (mapLayout[i + 1, j] != 1) 
//                     && (mapLayout[i + 1, j + 1] != 1))
//                     mapLayout[i, j] = -1;


//         PrintMapLayout();
//         //Добавить фикс, чтобы не было зон с полами по краям, если к ним нет прохода и очень тоненькие проходы заделывались
//     }

//     private void PrintMapLayout()
//     {
//         for (int i = 0; i < mapLayout.GetLength(0); i++)
//             for (int j = 0; j < mapLayout.GetLength(1); j++)
//                 Debug.Log(mapLayout[i, j] + (j == mapLayout.GetLength(1) - 1 ? "\n" : ""));
//     }

//     private void GenerateMap()
//     {
//         for (int i = 0; i < mapLayout.GetLength(0); i++)
//             for (int j = 0; j < mapLayout.GetLength(1); j++)
//                 if (mapLayout[i, j] != -1)
//                     tilemap.SetTile(new Vector3Int(i, j, 0), tiles[mapLayout[i, j]]);
//     }

//     private void GenerateProps()
//     {
//         // for (int i = 0; i < mapLayout.GetLength(0); i++)
//         //     for (int j = 0; j < mapLayout.GetLength(1); j++)
//         //         if (mapLayout[i, j] == 1)
//         //             if (((mapLayout[i - 1, j] == 0) || (mapLayout[i + 1, j] == 0)) && ((mapLayout[i, j - 1] == 0) || (mapLayout[i, j + 1] == 0)))
//         //                 GenerateShelvings(new Vector2Int(i, j));

//         //GenerateShelvings(new Vector2Int(0, 0));
//         GenerateEmptySpaces();

//         void GenerateEmptySpaces()
//         {
//             for (int i = 0; i < Random.Range(0, maxEmptySpacesCount + 1); i++)
//             {
//                 Vector2Int emptySpace = new Vector2Int(Random.Range(minPartSize.x, maxEmptySpaceSize.x + 1), Random.Range(minPartSize.y, maxEmptySpaceSize.y + 1));
//                 Vector2Int point = new Vector2Int(Random.Range(0, mapLayout.GetLength(0) - emptySpace.x + 1), Random.Range(0, mapLayout.GetLength(1) - emptySpace.y + 1));

//                 bool fits = true;
//                 for (int j = point.x; j < point.x + emptySpace.x; j++)
//                     for (int k = point.y; k < point.y + emptySpace.y; k++)
//                         if (mapLayout[j, k] != 1)
//                         {
//                             fits = false;
//                             break;
//                         }

//                 if (fits)
//                 {
//                     for (int j = point.x; j < point.x + emptySpace.x; j++)
//                         for (int k = point.y; k < point.y + emptySpace.y; k++)
//                             mapLayout[j, k] = 3;
//                 }
//                 else
//                     i--;
//             }
//         }

//         void GenerateShelvings(Vector2Int startPoint)
//         {
//             bool isVertical = Random.value < 0.5f;
//             int count = Random.Range(1, maxInRowShelvingsCount);
//             int length = Random.Range(minShelvingLength, maxShelvingLength);
//             Vector2Int shelvingsSize;

//             Debug.Log(isVertical);
//             Debug.Log(count);
//             Debug.Log(length);

//             shelvingsSize = new Vector2Int(length, count + count*2);

//             // if (isVertical) shelvingsSize = new Vector2Int (count + count*2, length);
//             // else shelvingsSize = new Vector2Int(length, count + count*2);

//             List<int> shelvingScheme = new List<int>();
//             int [,] shelvings = new int[shelvingsSize.x, shelvingsSize.y];

//             for (int i = 0; i < count; i++)
//                 shelvingScheme.AddRange(new int[] {2, 2, 3});
//             Debug.Log($"Shelving Scheme Contents: {string.Join(", ", shelvingScheme)}");
                
//             for (int i = 0; i < length; i++)
//                 for (int j = 0; j < shelvingScheme.Count; j++)
//                     shelvings[i, j] = shelvingScheme[j];

//             string matrixString = string.Join("\n", Enumerable.Range(0, shelvings.GetLength(0))
//                 .Select(i => string.Join(" ", Enumerable.Range(0, shelvings.GetLength(1))
//                     .Select(j => shelvings[i, j].ToString().PadLeft(4, ' ')))));
//             Debug.Log(matrixString);

//             for (int i = startPoint.x; i < startPoint.x + shelvings.GetLength(0); i++)
//                 for (int j = startPoint.y; j < startPoint.y + shelvings.GetLength(1); j++)
//                     mapLayout[i, j] = shelvings[i - startPoint.x, j - startPoint.y];
            


//             // for (int i = startPoint.x; i < startPoint.x + shelvingsSize.x; i++)
//             //     for (int j = startPoint.y; j < startPoint.y + shelvingsSize.y; j++)
//             //         if (mapLayout[i, j] != 0)
//             //             if (j == 0)
//             //                 if (isVertical) count--;
//             //                 else 
//             //             else
//             //                 if (isVertical)
//             //                 else
//         }

//         void GenerateStands()
//         {

//         }
//     }


//     //сначала должны расставляться метки стелажей, а уже потом на пост процессинге они должны разледяться например на отдельные стелажи
// }
