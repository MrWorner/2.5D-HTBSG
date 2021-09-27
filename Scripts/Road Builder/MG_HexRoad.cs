using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
[CreateAssetMenu(fileName = "hexRoad_", menuName = "--->MG/MG_HexRoad", order = 51)]
public class MG_HexRoad : ScriptableObject, IEquatableUID<MG_HexRoad>
{
    [SerializeField] private RoadDirection _id = RoadDirection.None;//айди
    [SerializeField] private Tile _tile;//тайл

    public RoadDirection Id { get => _id; }//айди
    public Tile Tile { get => _tile; }//тайл

    #region МЕТОДЫ ИНТЕРФЕЙСА "IEquatableUID"
    /// <summary>
    /// Сравнить УИДы
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool EqualsUID(MG_HexRoad other)
    {
        return _id == other._id;
    }
    #endregion МЕТОДЫ ИНТЕРФЕЙСА "IEquatableUID"
}
}
