using System;
using Random = UnityEngine.Random;

public static class Array2DExtensions
{
    public static void ForEach<T>(this T[,] array, Action<T> action)
    {
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                action(array[i, j]);
            }
        }
    }

    public static T[,] Transpose<T>(this T[,] array)
    {
        int rows = array.GetLength(0);
        int cols = array.GetLength(1);
        T[,] transposed = new T[cols, rows];

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                transposed[j, i] = array[i, j];

        return transposed;
    }

    public static T[] GetRow<T>(this T[,] array, int row)
    {
        var rowLength = array.GetLength(1);
        var result = new T[rowLength];
        for (int i = 0; i < rowLength; i++)
        {
            result[i] = array[row, i];
        }
        return result;
    }

    public static T[] GetColumn<T>(this T[,] array, int column)
    {
        var columnLength = array.GetLength(0);
        var result = new T[columnLength];
        for (int i = 0; i < columnLength; i++)
        {
            result[i] = array[i, column];
        }
        return result;
    }

    public static T[,] ReverseRows<T>(this T[,] array)
    {
        int rows = array.GetLength(0);
        int cols = array.GetLength(1);
        T[,] result = new T[rows, cols];

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[rows - i - 1, j] = array[i, j];

        return result;
    }

    public static T[,] ReverseColumns<T>(this T[,] array)
    {
        int rows = array.GetLength(0);
        int cols = array.GetLength(1);
        T[,] result = new T[rows, cols];
        
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[i, j] = array[i, cols - j - 1];
        
        return result;
    }

    public static T[,] RotateClockwise<T>(this T[,] array) =>
        array.Transpose().ReverseRows();

    public static T[,] RotateClockwiseRandom<T>(this T[,] array)
    {
        T[,] newArray = array;
        int side = Random.Range(0, 4);

        while (side != 0)
        {
            newArray = newArray.RotateClockwise();
            side--;
        }

        return newArray;
    }

    public static bool IsIndexExist<T>(this T[,] array, int row, int col) =>
        row >= 0 && row < array.GetLength(0) && col >= 0 && col < array.GetLength(1);

    public static T[,] GetSubArray<T>(this T[,] array, int startRow, int startCol, int numRows, int numCols)
    {
        // T[,] subMatrix = new T[numRows, numCols];

        // for (int i = 0; i < numRows; i++)
        //     for (int j = 0; j < numCols; j++)
        //         subMatrix[i, j] = array[startRow + i, startCol + j];

        // return subMatrix;

        int originalRows = array.GetLength(0);
        int originalCols = array.GetLength(1);

        // Проверка на выход индексов за пределы
        int endRow = startRow + numRows;
        int endCol = startCol + numCols;

        // Корректируем размеры, если они выходят за границы
        if (startRow < 0) startRow = 0;
        if (startCol < 0) startCol = 0;
        if (endRow > originalRows) endRow = originalRows;
        if (endCol > originalCols) endCol = originalCols;

        // Вычисление нового размера подматрицы
        int newNumRows = endRow - startRow;
        int newNumCols = endCol - startCol;

        // Создание подматрицы нужного размера
        T[,] subMatrix = new T[newNumRows, newNumCols];

        // Заполнение подматрицы значениями
        for (int i = 0; i < newNumRows; i++)
            for (int j = 0; j < newNumCols; j++)
                subMatrix[i, j] = array[startRow + i, startCol + j];

        return subMatrix;
    }

    public static T[,] SetSubArray<T>(this T[,] array, int startRow, int startCol, T[,] subMatrix)
    {
        T[,] newArray = array;
        int numRows = subMatrix.GetLength(0);
        int numCols = subMatrix.GetLength(1);

        for (int i = 0; i < numRows; i++)
            for (int j = 0; j < numCols; j++)
                newArray[startRow + i, startCol + j] = subMatrix[i, j];

        return newArray;
    }

    public static T[,] FillSubArray<T>(this T[,] array, int startRow, int startCol, int numRows, int numCols, T value)
    {
        T[,] newArray = array;

        for (int i = 0; i < numRows; i++)
            for (int j = 0; j < numCols; j++)
                if (newArray.IsIndexExist(startRow + i, startCol + j))
                    newArray[startRow + i, startCol + j] = value;
                //newArray[startRow + i, startCol + j] = value;

        return newArray;
    }

    public static int RowCount<T>(this T[,] array) => array.GetLength(0);

    public static int ColumnCount<T>(this T[,] array) => array.GetLength(1);

    public static T[,] AppendArrayRows<T>(this T[,] first, T[,] second, T value = default)
    {
        int cols = Math.Max(first.GetLength(1), second.GetLength(1));
        int rows1 = first.GetLength(0), rows2 = second.GetLength(0);
        T[,] result = new T[rows1 + rows2, cols];

        for (int i = 0; i < rows1; i++)
            for (int j = 0; j < cols; j++)
                result[i, j] = j < first.GetLength(1) ? first[i, j] : value;

        for (int i = 0; i < rows2; i++)
            for (int j = 0; j < cols; j++)
                result[rows1 + i, j] = j < second.GetLength(1) ? second[i, j] : value;

        return result;
    }

    public static T[,] AppendArrayColumns<T>(this T[,] first, T[,] second, T value = default)
    {
        int rows = Math.Max(first.GetLength(0), second.GetLength(0));
        int cols1 = first.GetLength(1), cols2 = second.GetLength(1);
        T[,] result = new T[rows, cols1 + cols2];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols1; j++)
                result[i, j] = i < first.GetLength(0) ? first[i, j] : value;
            for (int j = 0; j < cols2; j++)
                result[i, cols1 + j] = i < second.GetLength(0) ? second[i, j] : value;
        }
        return result;
    }

    public static T[,] InsertRow<T>(this T[,] array, int rowIndex, T[] newRow)
    {
        if (newRow.Length != array.GetLength(1))
            throw new ArgumentException("New row must have the same number of columns.");
        
        int rows = array.GetLength(0), cols = array.GetLength(1);
        T[,] result = new T[rows + 1, cols];
        
        for (int i = 0, r = 0; i <= rows; i++)
        {
            if (i == rowIndex)
            {
                for (int j = 0; j < cols; j++)
                    result[i, j] = newRow[j];
            }
            else
            {
                for (int j = 0; j < cols; j++)
                    result[i, j] = array[r, j];
                r++;
            }
        }
        return result;
    }

    public static T[,] InsertColumn<T>(this T[,] array, int colIndex, T[] newCol)
    {
        if (newCol.Length != array.GetLength(0))
            throw new ArgumentException("New column must have the same number of rows.");
        
        int rows = array.GetLength(0), cols = array.GetLength(1);
        T[,] result = new T[rows, cols + 1];
        
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0, c = 0; j <= cols; j++)
            {
                if (j == colIndex)
                    result[i, j] = newCol[i];
                else
                    result[i, j] = array[i, c++];
            }
        }
        return result;
    }

    public static string ToDimentionalString<T>(this T[,] array)
    {
        string result = string.Empty;

        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
                result += array[i,j] + " ";
            result += "\n";
        }

        return result;
    }

    public static T[,] Fill<T>(this T[,] array, T value)
    {
        int rows = array.GetLength(0);
        int cols = array.GetLength(1);
        T[,] result = new T[rows, cols];
        
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[i, j] = value;
        
        return result;
    }

    public static T[] Flatten<T>(this T[,] array)
    {
        var rows = array.GetLength(0);
        var columns = array.GetLength(1);
        var result = new T[rows * columns];
        int index = 0;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                result[index++] = array[i, j];
            }
        }

        return result;
    }

    
}
