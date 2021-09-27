using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
[CreateAssetMenu(fileName = "hexRiver_", menuName = "--->MG/MG_HexRiver", order = 51)]
public class MG_HexRiver : ScriptableObject, IEquatableUID<MG_HexRiver>
{
    [SerializeField] private RiverDirection _id = RiverDirection.None;//айди
    [SerializeField] private Tile _tile;//тайл

    public RiverDirection Id { get => _id; }//айди
    public Tile Tile { get => _tile; }//тайл

    #region МЕТОДЫ ИНТЕРФЕЙСА "IEquatableUID"
    /// <summary>
    /// Сравнить УИДы
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool EqualsUID(MG_HexRiver other)
    {
        return _id == other._id;
    }
    #endregion МЕТОДЫ ИНТЕРФЕЙСА "IEquatableUID"
}
}
