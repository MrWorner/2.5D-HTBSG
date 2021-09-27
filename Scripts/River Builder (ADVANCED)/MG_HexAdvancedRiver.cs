using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
[CreateAssetMenu(fileName = "hexAdvancedRiver_", menuName = "--->MG/MG_HexAdvancedRiver", order = 51)]
public class MG_HexAdvancedRiver : ScriptableObject, IEquatableUID<MG_HexAdvancedRiver>
{
    [SerializeField] private AdvancedRiverDirection _id = AdvancedRiverDirection.None;//айди
    [SerializeField] private Tile _tile;//тайл
    public AdvancedRiverDirection Id { get => _id; }//айди
    public Tile Tile { get => _tile; }//тайл

    #region МЕТОДЫ ИНТЕРФЕЙСА "IEquatableUID"
    /// <summary>
    /// Сравнить УИДы
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool EqualsUID(MG_HexAdvancedRiver other)
    {
        return _id == other._id;
    }
    #endregion МЕТОДЫ ИНТЕРФЕЙСА "IEquatableUID"
}
}
