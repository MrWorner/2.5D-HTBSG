using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public enum DirectionHexPT { NE, E, SE, SW, W, NW };//Стороны хексагона (pointy topped)// есть еще flat topped 

public static class MG_Direction
{
    private static readonly int[][] coord_EvenRow = new int[6][]//ЧЕТНЫЙ РЯД ПО ГОРИЗОНТАЛИ. массив координат, для нахождения соседей через цикл For
   {
	    //X,Y
	    new int[] { 0, 1},//NE
	    new int[] { 1, 0},//E		
	    new int[] { 0, -1},//SE	
	    new int[] { -1, -1},//SW		
	    new int[] { -1, 0},//W		
	    new int[] { -1, 1},//NW		
   };
    private static readonly int[][] coord_OddRow = new int[6][]//НЕЧЕТНЫЙ РЯД ПО ГОРИЗОНТАЛИ. массив координат, для нахождения соседей через цикл For
    {
	    //X,Y
	    new int[] { 1, 1},//NE
	    new int[] { 1, 0},//E	
	    new int[] { 1, -1},//SE	
	    new int[] { 0, -1},//SW	
	    new int[] { -1, 0},//W	
	    new int[] { 0, 1},//NW	
    };

    public static int[][] Coord_EvenRow => coord_EvenRow;
    public static int[][] Coord_OddRow => coord_OddRow;

    /// <summary>
    /// Получить направление по X и У. Hexagonal Point FLopped
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="isEvenRow"></param>
    /// <returns></returns>
    public static DirectionHexPT GetDirectionByXY(int x, int y, bool isEvenRow)
    {
        int[][] coord;
        if (isEvenRow)
            coord = Coord_EvenRow;
        else
            coord = Coord_OddRow;

        if (coord[0][0] == x && coord[0][1] == y)//СЕВЕР ВОСТОК
            return DirectionHexPT.NE;
        else if (coord[1][0] == x && coord[1][1] == y)//ВОСТОК
            return DirectionHexPT.E;
        else if (coord[2][0] == x && coord[2][1] == y)//ЮГ ВОСТОК
            return DirectionHexPT.SE;
        else if (coord[3][0] == x && coord[3][1] == y)//ЮК ЗАПАД
            return DirectionHexPT.SW;
        else if (coord[4][0] == x && coord[4][1] == y)//ЗАПАД
            return DirectionHexPT.W;
        else if (coord[5][0] == x && coord[5][1] == y)//СЕВЕР ЗАПАД
            return DirectionHexPT.NW;
       
        Debug.Log("<color=red>MG_Direction GetDirectionByXY(): (Hexagonal Point FLopped) по координатам невозможно получить направление</color>");
        Debug.Log("<color=red>MG_Direction GetDirectionByXY(): x|y " + x + "|" + y + " VS " + coord[4][0] + "|" + coord[4][1]  + " isEvenRow=" + isEvenRow + " coord=" + coord +   " </color>");
        Debug.Log("<color=red>MG_Direction GetDirectionByXY(): (coord[4][0] == x && coord[4][1] == y) =" + (coord[4][0] == x && coord[4][1] == y) + "</color>");
        return DirectionHexPT.NE;
    }

    /// <summary>
    /// Отзеркалить направление
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static DirectionHexPT ReverseDir(DirectionHexPT dir)
    {

        switch (dir)
        {
            case DirectionHexPT.E:
                {
                    return DirectionHexPT.W;
                }
            case DirectionHexPT.NE:
                {
                    return DirectionHexPT.SW;
                }
            case DirectionHexPT.NW:
                {
                    return DirectionHexPT.SE;
                }
            case DirectionHexPT.SE:
                {
                    return DirectionHexPT.NW;
                }
            case DirectionHexPT.SW:
                {
                    return DirectionHexPT.NE;
                }
            case DirectionHexPT.W:
                {
                    return DirectionHexPT.E;
                }
            default: return DirectionHexPT.E;
        }
    }
}
}
