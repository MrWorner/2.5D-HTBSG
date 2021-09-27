using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
[CreateAssetMenu(fileName = "resource_", menuName = "--->MG/MG_ResourceType", order = 51)]
public class MG_ResourceType : ScriptableObject
{
    #region Поля
    [BoxGroup("ID объекта должен быть уникальным"), Required(InfoMessageType.Error), SerializeField] private string id;//уникальный номер
    [BoxGroup("Наименование"), SerializeField] private string resourceName = "noname";//имя
    [BoxGroup("Описание"), SerializeField] private string description;//описание
    [BoxGroup("Тип клетки"), SerializeField] private MG_HexCellType cellType;//тип
    [BoxGroup("СЛОЙ 1: Варианты грунта"), Required(InfoMessageType.Error), SerializeField] private List<Tile> _groundVariants;//варианты земли
    [BoxGroup("СЛОЙ 2: Варианты объектов на грунте"), Required(InfoMessageType.Error), SerializeField] private List<Tile> _objectVariants;//варианты объектов на земле
    #endregion Поля

    #region Свойства
    public string Id { get => id; }//уникальный номер
    public string ResourceName { get => resourceName; set => resourceName = value; }//имя
    public string Description { get => description; set => description = value; }//описание
    public MG_HexCellType CellType { get => cellType; set => cellType = value; }//тип
    public IReadOnlyList<Tile> GroundVariants { get => _groundVariants; }//варианты
    public IReadOnlyList<Tile> ObjectVariants { get => _objectVariants; }//варианты объектов на земле
    #endregion Свойства
}
}
