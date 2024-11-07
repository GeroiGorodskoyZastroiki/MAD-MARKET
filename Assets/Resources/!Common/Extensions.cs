using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public static class Extensions
{
    public static bool IsIndexExist(this Matrix<double> matrix, int x, int y) =>
        x >= 0 && x < matrix.RowCount && y >= 0 && y < matrix.ColumnCount;

    public static Matrix<double> ReverseRows(this Matrix<double> matrix)
        => matrix.MapIndexed((i, j, _) => matrix[matrix.RowCount - 1 - i, j]);

    public static Matrix<double> ReverseColumns(this Matrix<double> matrix)
        => matrix.MapIndexed((i, j, _) => matrix[i, matrix.ColumnCount - 1 - j]);
}
