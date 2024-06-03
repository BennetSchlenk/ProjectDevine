using System;
using System.Collections;
using System.Collections.Generic;

public static class GridConversionUtility
{
    // Convert 2D array to List
    public static List<T> GridToList<T>(T[,] grid)
    {
        List<T> list = new List<T>();

        foreach (T item in grid)
        {
            list.Add(item);
        }

        return list;
    }

    // Convert List back to 2D array
    public static T[,] ListToGrid<T>(List<T> list, int rows, int cols)
    {
        if (list.Count != rows * cols)
        {
            throw new ArgumentException("The size of the list does not match the provided dimensions.");
        }

        T[,] grid = new T[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                grid[i, j] = list[i * cols + j];
            }
        }

        return grid;
    }
}
