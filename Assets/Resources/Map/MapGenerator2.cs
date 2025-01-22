using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;

using Random = UnityEngine.Random;

public enum TileType : sbyte //Положительные индексы - тайлы, отрицательные - служебные ячейки
{
    NoTile = -10,
    Occupied = -1,
    Empty = 0,
    Wall = 1,
    Floor = 2,
    Shelvings = 3,
    Checkout = 4,
    Stand = 5,
    Door = 6
}

public class MapGenerator2 : MonoBehaviour
{
    Tilemap _tilemap;
    [SerializeField] TileBase[] _tiles;

    [InfoBox("Cant be smaller than generated checkout-exit zone")]
    [SerializeField] byte _mapMinDimesions;
    [SerializeField] byte _mapMaxDimesions;
    sbyte[,] _mapLayout;

    [SerializeField] int _occupiedPartMinSize;

    [InfoBox("Empty zones can be generated in same place and overlap each other")]
    [SerializeField] int _emptyZonesCount;
    [SerializeField] int _emptyZonePartMinSize;
    [SerializeField] float _emptyZoneMaxPartPercent;
    [SerializeField] float _emptyZoneInCornerChance;
    
    
    // [SerializeField] float shelvingsAlongWallsChance = 0.5f;
    // [SerializeField] int shelvingsCountInRowMax = 6;
    // [SerializeField] int shelvingLengthMin = 3;
    // [SerializeField] int shelvingLengthMax = 6;
    [SerializeField] int _countersCountMin = 2;

    void Awake()
    {
        _tilemap = FindFirstObjectByType<Tilemap>();
        GenerateMap();
    }

    [Button("Generate map")]
    public void GenerateMap()
    {
        _tilemap.ClearAllTiles();
        GenerateMapLayout();
        GenerateTilemapFromMatrix(_mapLayout);
    }

    void GenerateMapLayout()
    {
        _mapLayout = new sbyte[Random.Range(_mapMinDimesions, _mapMaxDimesions), Random.Range(_mapMinDimesions, _mapMaxDimesions)];
        _mapLayout = _mapLayout.Fill((sbyte)TileType.Occupied);
        sbyte[,] checkoutExitScheme;

        GenerateCheckoutExitZone();
        GenerateEmptyZones();
        //GenerateInnerEmptySpaces(); //внутри будет GeneratePillars, GenerateInnerEmptySpaces
        GenerateWalls();
        GenerateShelvings();

        FillFloor();

        #region GeneralMethods
        bool IsTileTypeContinuesInDirection(int row, int col, Vector2Int direction, int count, sbyte tileType)
        {
            for (int i = 0; i < count; i++)
            {
                var currentPoint = new Vector2Int(i * direction.x + row, i * direction.y + col);
                if (!_mapLayout.IsIndexExist(currentPoint.x, currentPoint.y) || _mapLayout[currentPoint.x, currentPoint.y] != tileType) 
                    return false;
            }
            return true;
        }

        // void FillTilesInDirection(Vector2Int point, Vector2Int direction, sbyte tileType, sbyte stopTileType)
        // {
        //     Vector2Int currentPoint = point;
        //     while (_mapLayout.IsIndexExist(currentPoint.x, currentPoint.y) && _mapLayout[currentPoint.x, currentPoint.y] != stopTileType)
        //     {
        //         _mapLayout[currentPoint.x, currentPoint.y] = tileType;
        //         currentPoint += direction;
        //     }
        // }

        // bool IsTilesOfTypeFillArea(Vector2Int point, Vector2Int area, int tileType)
        // {
        //     for (int i = 0; i < area.x; i++)
        //         for (int j = 0; i < area.y; j++)
        //             if (_mapLayout[point.x + i, point.y + j] != tileType) return false;
        //     return true;
        // }

        // // Matrix<sbyte> GetAllNeighborsOfTileType(Vector2Int point) =>
        // //     _mapLayout.SubMatrix(Math.Clamp(point.x - 1, 0, _mapLayout.RowCount), point.x + 1 >= _mapLayout.RowCount ? 2 : 3, Math.Clamp(point.y - 1, 0, _mapLayout.ColumnCount), point.y + 1 >= _mapLayout.ColumnCount ? 2 : 3);
        
        // // Matrix<sbyte> GetDirectNeighborsOfTileType(Vector2Int point)
        // // {
        // //     var neighbors = GetAllNeighborsOfTileType(point);
        // //     for (int i = 0; i < neighbors.RowCount; i++)
        // //         for (int j = 0; j < neighbors.ColumnCount; j++)
        // //             if ((i == 0 || i == neighbors.RowCount - 1) && (j == 0 || j == neighbors.ColumnCount - 1))
        // //                 neighbors[i, j] = (int)TileType.NoTile;
        // //     return neighbors;
        // // }

        bool HasTileTypeInAllNeighbors(int row, int col, sbyte tileType) =>
            _mapLayout.GetSubArray(row - 1, col - 1, 3, 3).Flatten().Any(n => n == tileType);

        int CountTileTypeInAllNeighbors(int row, int col, sbyte tileType) =>
            _mapLayout.GetSubArray(row - 1, col - 1, 3, 3).Flatten().Count(n => n == tileType);

        int CountTileTypeInDirectNeighbors(int row, int col, sbyte tileType)
        {
            var neighbors =  _mapLayout.GetSubArray(row - 1, col - 1, 3, 3);
            for (int i = 0; i < neighbors.GetLength(0); i++)
                for (int j = 0; j < neighbors.GetLength(1); j++)
                    if ((i == 0 || i == neighbors.GetLength(0) - 1) &&
                        (j == 0 || j == neighbors.GetLength(1) - 1))
                        neighbors[i, j] = (sbyte)(tileType + 1);
            //neighbors [0, 0] = neighbors [2, 0] = neighbors [0, 2] = neighbors [2, 2] = (sbyte)(tileType + 1);
            return neighbors.Flatten().Count(n => n == tileType);
        }
           

        // // bool HasTileTypeInDirectNeighbors(Vector2Int point, int tileType) =>
        // //     GetDirectNeighborsOfTileType(point).Find(n => n == tileType) != null;
        #endregion
    
        void GenerateCheckoutExitZone()
        {
            GenerateCheckoutExitScheme();
            void GenerateCheckoutExitScheme()
            {
                var counterScheme = new sbyte[4,2]
                {
                    {(sbyte)TileType.Floor, (sbyte)TileType.Floor}, 
                    {(sbyte)TileType.Checkout, (sbyte)TileType.Floor},
                    {(sbyte)TileType.Checkout, (sbyte)TileType.Floor}, 
                    {(sbyte)TileType.Floor, (sbyte)TileType.Floor}
                };

                var checkoutCountersScheme = counterScheme;
                for (int i = 0; i < _countersCountMin - 1; i++) //стоек должно быть кол-во игроков/2
                    checkoutCountersScheme = counterScheme.AppendArrayColumns(counterScheme);

                var possibleExitPosition = new[] { "left", "right" };//, "above" };
                var exitPosition = possibleExitPosition[Random.Range(0, possibleExitPosition.Length)];

                checkoutExitScheme = checkoutCountersScheme;
                checkoutExitScheme = checkoutExitScheme.InsertColumn(0, Enumerable.Repeat((sbyte)TileType.Floor, checkoutExitScheme.RowCount()).ToArray());

                var exitLength = Random.Range(2, 4) * 2;
                var exitWall = Enumerable.Repeat((sbyte)TileType.Wall, exitLength).ToArray();
                exitWall[exitLength/2-1] = (int)TileType.Door;
                exitWall[exitLength/2] = (int)TileType.Door;
                var exitScheme = new sbyte[checkoutExitScheme.RowCount(), exitLength];
                exitScheme = exitScheme.Fill((sbyte)TileType.Floor);
                exitScheme = exitScheme.InsertRow(0, exitWall);

                if (exitPosition != "above")
                {
                    checkoutExitScheme = checkoutExitScheme.InsertRow(0, Enumerable.Repeat((sbyte)TileType.Wall, checkoutExitScheme.ColumnCount()).ToArray()).AppendArrayColumns(exitScheme);
                    if (exitPosition == "left")
                        checkoutExitScheme = checkoutExitScheme.ReverseColumns();
                }
                // else 
                // {
                //     checkoutExitScheme = checkoutExitScheme.Transpose().ReverseColumns();
                //     checkoutExitScheme = checkoutExitScheme.Append(Matrix<double>.Build.Dense(checkoutExitScheme.RowCount, Random.Range(1, 4) * 2, (int)TileType.Floor));
                //     checkoutExitScheme = checkoutExitScheme.ReverseColumns().Transpose();
                // }
                //Debug.Log(checkoutExitScheme);
                // var aboba = checkoutExitScheme[0,0];
                // aboba = 10;
                // Debug.Log(checkoutExitScheme);
            }

            InsertCheckoutExitInMapLayout();
            void InsertCheckoutExitInMapLayout()
            {
                _mapLayout = _mapLayout.SetSubArray(0, Random.Range(0, _mapLayout.ColumnCount() - checkoutExitScheme.ColumnCount() - 1), checkoutExitScheme);
                _mapLayout = _mapLayout.RotateClockwiseRandom();

                // var insertSide = Random.Range(0, 4); //"up", "right", "down", "left"
                // Debug.Log("Choosen insert side: " + insertSide);

                // for (int i = 0; i < insertSide; i++)
                //     _mapLayout = _mapLayout.RotateClockwise();
                    //checkoutExitScheme = RotateRight(checkoutExitScheme);

                //поворачивать сам mapLayout
            }
        }

        void GenerateEmptyZones()
        {
            for (int i = 0; i < _emptyZonesCount; i++)
            {
                int maxEmptyZoneSize = (int)(Math.Max(_mapLayout.GetLength(0), _mapLayout.GetLength(1)) * _emptyZoneMaxPartPercent);
                int startColumn;
                int height, width;

                while (true)
                {
                    startColumn = 0;
                    if (_emptyZoneInCornerChance < Random.Range(0f, 1f))
                        startColumn = Random.Range(0, _mapLayout.GetLength(1) - maxEmptyZoneSize);

                    height = Random.Range(_emptyZonePartMinSize, maxEmptyZoneSize);
                    width = Random.Range(_emptyZonePartMinSize, maxEmptyZoneSize);
                    if (_mapLayout.GetSubArray(0, startColumn, height, width).Flatten().All(x => x == (sbyte)TileType.Occupied || x == (sbyte)TileType.NoTile)) break;
                }

                _mapLayout = _mapLayout.FillSubArray(0, startColumn, height, width, (sbyte)TileType.NoTile);
                _mapLayout = _mapLayout.RotateClockwiseRandom();
            }
        }

        void GenerateWalls()
        {
            MarkPerimeter();
            void MarkPerimeter()
            {
                bool CheckIsWall(int row, int col)
                {
                    if (_mapLayout[row, col] != (sbyte)TileType.Floor && _mapLayout[row, col] != (sbyte)TileType.Occupied) return false;
                    if (row == 0 || col == 0 || row == _mapLayout.GetLength(0) - 1 || col == _mapLayout.GetLength(1) - 1) return true;
                    if (HasTileTypeInAllNeighbors(row, col, (sbyte)TileType.NoTile)) return true;
                    return false;
                }

                for (int i = 0; i < _mapLayout.GetLength(0); i++)
                    for (int j = 0; j < _mapLayout.GetLength(1); j++)
                        if (CheckIsWall(i, j)) _mapLayout[i, j] = (sbyte)TileType.Wall;
            }

            ClearOccupiedPartsSmallerThanMin();
            _mapLayout = _mapLayout.RotateClockwise().RotateClockwise();
            ClearOccupiedPartsSmallerThanMin();
            void ClearOccupiedPartsSmallerThanMin()
            {
                for (int i = 0; i < _mapLayout.GetLength(0); i++)
                    for (int j = 0; j < _mapLayout.GetLength(1); j++)
                        if (_mapLayout[i, j] == (sbyte)TileType.Occupied && 
                        CountTileTypeInDirectNeighbors(i, j, (sbyte)TileType.Wall) > 1) //сделать проверку на то, что тайл окружён стенами хотя бы с 2 сторон
                        {
                            if ((!IsTileTypeContinuesInDirection(i, j, new Vector2Int(-1, 0), _occupiedPartMinSize, (sbyte)TileType.Occupied) &&
                                !IsTileTypeContinuesInDirection(i, j, new Vector2Int(1, 0), _occupiedPartMinSize, (sbyte)TileType.Occupied)) ||
                                (!IsTileTypeContinuesInDirection(i, j, new Vector2Int(0, 1), _occupiedPartMinSize, (sbyte)TileType.Occupied) &&
                                !IsTileTypeContinuesInDirection(i, j, new Vector2Int(0, -1), _occupiedPartMinSize, (sbyte)TileType.Occupied)))
                                    _mapLayout[i, j] = (sbyte)TileType.Wall;
                        }
            }

            ClearUnnecessaryWalls();
            void ClearUnnecessaryWalls()
            {
                for (int i = 0; i < _mapLayout.GetLength(0); i++)
                    for (int j = 0; j < _mapLayout.GetLength(1); j++)
                        if (_mapLayout[i, j] == (sbyte)TileType.Wall && 
                        !HasTileTypeInAllNeighbors(i, j, (sbyte)TileType.Occupied) &&
                        !HasTileTypeInAllNeighbors(i, j, (sbyte)TileType.Floor))
                            _mapLayout[i, j] = (sbyte)TileType.NoTile;
            }
        }

        void FillFloor()
        {
            for (int i = 0; i < _mapLayout.GetLength(0); i++)
                for (int j = 0; j < _mapLayout.GetLength(1); j++)
                    if (_mapLayout[i, j] == (sbyte)TileType.Occupied) _mapLayout[i, j] = (sbyte)TileType.Floor;
        }
        //_mapLayout.ForEach(tile => tile = tile == (sbyte)TileType.PrototypeFloor ? (sbyte)TileType.Floor : tile); 

        void GenerateShelvings()
        {
            var counter = 0;
            while (true)
            {
                if (counter > 0) break;
                else counter++;

                var index = GetRandomIndex(_mapLayout, x => x == (sbyte)TileType.Occupied);
                var row = index.row;
                var col = index.col;
                if (row < 0 && col < 0) break;

                GenerateShelvingsZone(row, col);
            }

            void GenerateShelvingsZone(int startRow, int startCol)
            {
                int endRow = startRow;
                int endCol = startCol;
                Debug.Log(startRow + " " + startCol);

                var expressions = new List<Action>
                {
                    () => endCol += ExpandShelvingsArrayInDirection(_mapLayout, startRow, startCol, endRow, endCol, new Vector2Int(1, 0)),
                    () => endRow += ExpandShelvingsArrayInDirection(_mapLayout, startRow, startCol, endRow, endCol, new Vector2Int(0, 1)),
                    () => startCol -= ExpandShelvingsArrayInDirection(_mapLayout, startRow, startCol, endRow, endCol, new Vector2Int(-1, 0)),
                    () => startRow -= ExpandShelvingsArrayInDirection(_mapLayout, startRow, startCol, endRow, endCol, new Vector2Int(0, -1))
                };

                while (expressions.Count > 0)
                {
                    int index = Random.Range(0, expressions.Count); // Выбираем случайный индекс
                    Debug.Log(index);
                    expressions[index](); // Выполняем выражение
                    expressions.RemoveAt(index); // Удаляем выполненное выражение
                }

                // var width = endCol - startCol + 1;
                // var height = endRow - startRow + 1;

                // sbyte[,] shelvingsZone = new sbyte[width, height];

                // var verticalProbability = 1 / (height / width);
                // if (verticalProbability > 0) verticalProbability = -1 + (1 / verticalProbability);
                // else verticalProbability = 1 - verticalProbability;

                // bool isVertical = Random.value < verticalProbability;

                // int shelvingsTargetLength = Random.Range(3, 9);
                // sbyte[,] shelvingsScheme = new sbyte[shelvingsTargetLength, 1];
                // shelvingsScheme.Fill((sbyte)TileType.Shelvings);


                // if (isVertical) 

                // else 
                // {
                //     shelvingsScheme.RotateClockwise().ReverseRows();
                // }




                //_mapLayout = _mapLayout.SetSubArray(startRow, startCol, height, width, (sbyte)TileType.Shelvings);
            }
                
            (int row, int col) GetRandomIndex<T>(T[,] array, Func<T, bool> predicate)
            {
                int rows = array.GetLength(0);
                int cols = array.GetLength(1);

                var validIndices = Enumerable.Range(0, rows)
                    .SelectMany(row => Enumerable.Range(0, cols)
                        .Where(col => predicate(array[row, col]))
                        .Select(col => (row, col)))
                    .ToList();

                if (!validIndices.Any())
                    return (-1, -1);

                return validIndices[Random.Range(0, validIndices.Count)];
            }

            int ExpandShelvingsArrayInDirection(sbyte[,] array, int startRow, int startCol, int endRow, int endCol, Vector2Int direction)
            {
                var numRows = endRow - startRow + 1;
                var numCols = endCol - startCol + 1;
                int i = 0;
                while (true)
                {
                    i++;
                    if (array.GetSubArray(startRow + direction.y * i, startCol + direction.x * i, numRows, numCols).Flatten().Count(x => x != (sbyte)TileType.Occupied) > 0) break;
                }
                return i - 1;
            }
        }
    }

    private void PrintMapLayout() => Debug.Log(_mapLayout.ToString());

    private void GenerateTilemapFromMatrix(sbyte[,] mapLayout)
    {
        for (int i = 0; i < mapLayout.GetLength(0); i++)
            for (int j = 0; j < mapLayout.GetLength(1); j++)
                if (mapLayout[i, j] > 0)
                    _tilemap.SetTile(new Vector3Int(j, i, 0), _tiles[mapLayout[i, j]]);
    }
}