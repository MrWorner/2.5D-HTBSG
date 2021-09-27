using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MG_StrategyGame
{
public abstract class IPathfinding<T>
{
    /// <summary>
    /// Найти путь
    /// </summary>
    /// <param name="originNode">начало</param>
    /// <param name="destinationNode">конец</param>
    /// <param name="unwalkableGroundTypes">непроходимый тип клеток</param>
    /// <param name="side">ссторона</param>
    /// <returns></returns>
    public abstract List<T> FindPath(T originNode, T destinationNode, IReadOnlyCollection<GroundType> unwalkableGroundTypes, MG_Player side);//


    /// <summary>
    /// Получить список соседей
    /// </summary>
    /// <param name="walkableCells">клетки ходимые</param>
    /// <param name="node">клетка</param>
    /// <returns></returns>
    protected List<T> GetNeigbours(Dictionary<T, Dictionary<T, float>> walkableCells, T node)
    {
        //Debug.Log("<color=orange>IPathfinding GetNeigbours()</orange>");
        if (!walkableCells.ContainsKey(node))
        {
            return new List<T>();
        }
        return walkableCells[node].Keys.ToList();
    }
}
}
