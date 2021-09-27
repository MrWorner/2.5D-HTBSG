using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

//СОЗДАН ПО ИСТОЧНИКУ: https://habr.com/ru/post/421523/#:~:text=Согласно%20документации%20Unity%2C%20ScriptableObject%20—%20это,не%20зависящих%20от%20экземпляров%20скриптов.&text=Если%20вам%20нужно%20повысить%20свои,изучите%20другие%20туториалы%20по%20Unity.

namespace MG_StrategyGame
{
[CreateAssetMenu(fileName = "hexCellType_", menuName = "--->MG/MG_HexCellType", order = 51)]
public class MG_HexCellType : ScriptableObject
{
    #region Поля
    [BoxGroup("ID объекта должен быть уникальным"), Required(InfoMessageType.Error), SerializeField] private string _id;//айди
    [BoxGroup("Наименование"), SerializeField] private string _cellName = "noname";//наименование клетки
    [BoxGroup("Описание"), SerializeField] private string _description;//описание
    [BoxGroup("СЛОЙ 1: Варианты грунта"), Required(InfoMessageType.Error), SerializeField] private List<Tile> _groundVariants;//варианты земли
    [BoxGroup("СЛОЙ 2: Варианты объектов на грунте"), Required(InfoMessageType.Error), SerializeField] private List<Tile> _objectVariants;//варианты объектов на земле
    [BoxGroup("Очки требуемые на передвижение по этой клетке"), Required(InfoMessageType.Error), SerializeField] private int _movementCost = 1;//штраф при передвижении
    [BoxGroup("Основной тип клетки"), Required(InfoMessageType.Error), SerializeField] GroundType _groundType = GroundType.Normal;//тип грунта
    #endregion Поля

    #region Свойства
    public string Id { get => _id; }//айди
    public string CellName { get => _cellName; }//наименование клетки
    public string Description { get => _description; }//описание
    public int MovementCost { get => _movementCost; }//штраф при передвижении
    public IReadOnlyList<Tile> GroundVariants { get => _groundVariants; }//варианты
    public IReadOnlyList<Tile> ObjectVariants { get => _objectVariants; }//варианты объектов на земле
    public GroundType GroundType { get => _groundType; }//тип грунта
    #endregion Свойства

}
}
