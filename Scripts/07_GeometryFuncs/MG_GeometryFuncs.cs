using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public static class MG_GeometryFuncs
{
    #region Публичные статические методы
    /// <summary>
    /// конвертируем координаты в Хекс координаты https://www.redblobgames.com/grids/hexagons/
    /// </summary>
    /// <param name="pos"></param>
    /// <returns>координаты кубические</returns>
    public static Vector3Int Oddr_to_cube(Vector3Int pos)
    {
        var x = pos.x - ((-pos.y) - ((-pos.y) & 1)) / 2;
        var z = (-pos.y);

        //var x = pos.x - (pos.y - (pos.y & 1)) / 2;   
        //var z = pos.y;

        var y = -x - z;
        return new Vector3Int(x, y, z);
    }

    /// <summary>
    /// Получить дистанцию
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>дистанция</returns>
    public static int Cube_distance(Vector3Int a, Vector3Int b)
    {
        var x = Math.Abs(a.x - b.x);
        var y = Math.Abs(a.y - b.y);
        var z = Math.Abs(a.z - b.z);
        var max = Math.Max(x, y);
        max = Math.Max(max, z);
        return max;
    }

    /// <summary>
    /// Получить координату между линией по всем параметрам координаты хекса
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Vector3 Cube_lerp(Vector3Int a, Vector3Int b, float t)
    {
        return new Vector3(Lerp(a.x, b.x, t), Lerp(a.y, b.y, t), Lerp(a.z, b.z, t));
    }

    /// <summary>
    /// Нарисовать линию
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>лист координат линии</returns>
    public static HashSet<Vector3Int> Cube_Line(Vector3Int a, Vector3Int b)
    {
        int N = Cube_distance(a, b);
        HashSet<Vector3Int> results = new HashSet<Vector3Int>();
        for (int i = 0; i < N; i++)
        {
            results.Add(Cube_round(Cube_lerp(a, b, 1.0f / N * i)));
        }
        return results;
    }

    /// <summary>
    /// Получить координаты кольца //https://www.redblobgames.com/grids/hexagons/
    /// </summary>
    /// <param name="center">центер</param>
    /// <param name="radius">радиус</param>
    public static HashSet<Vector3Int> Cube_ring(Vector3Int center, int radius)
    {
        HashSet<Vector3Int> results = new HashSet<Vector3Int>();
        var cube = MG_HexCell.Cube_add(center, MG_HexCell.Cube_scale(MG_HexCell.Cube_direction(4), radius));

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < radius; j++)
            {
                results.Add(cube);
                cube = MG_HexCell.Cube_neighbor(cube, i);
            }
        }

        return results;


    }

    /// <summary>
    /// Округлить координаты хекса
    /// </summary>
    /// <param name="cube">позиция кубическая</param>
    /// <returns>округленная позиция кубическая</returns>
    public static Vector3Int Cube_round(Vector3 cube)
    {
        var rx = Math.Round(cube.x);//ЯВНО ПРОБЛЕМА (decimal)
        var ry = Math.Round(cube.y);//ЯВНО ПРОБЛЕМА (decimal)
        var rz = Math.Round(cube.z);//ЯВНО ПРОБЛЕМА (decimal)
        var x_diff = Math.Abs(rx - cube.x);
        var y_diff = Math.Abs(ry - cube.y);
        var z_diff = Math.Abs(rz - cube.z);
        if (x_diff > y_diff && x_diff > z_diff)
        {
            rx = -ry - rz;
        }
        else if (y_diff > z_diff)
            ry = -rx - rz;
        else
            rz = -rx - ry;
        return new Vector3Int((int)rx, (int)ry, (int)rz);
    }

    /// <summary>
    /// Получить координату между точек
    /// </summary>
    /// <param name="a">точка А</param>
    /// <param name="b">точка Б</param>
    /// <param name="t">множитель</param>
    /// <returns>средняя точка</returns>
    public static float Lerp(int a, int b, float t)
    {
        return a + (b - a) * t;
    }
    #endregion Публичные статические методы
}
}
