using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_Player : MonoBehaviour, ICellObserver, IEquatableUID<MG_Player>
{
    [SerializeField, ReadOnly] private uint uid;//УНИКАЛЬНЫЙ ID ОБЪЕКТА (для IEquatableUID)
    private static uint uidCounter = 0;//Счетчик уидов

    #region Поля: требуемые модули
    [Required(InfoMessageType.Error), SerializeField] private GameObject _unitContainer;//[R]
    [Required(InfoMessageType.Error), SerializeField] private GameObject _settlementContainer;//[R]
    #endregion Поля: требуемые модули

    #region Поля
    //[SerializeField] private sbyte _sideNum = -1;//номер стороны
    [SerializeField] private Color _color = Color.white;//цвет игрока
    [SerializeField] private PlayerType _type = PlayerType.AI;//тип игрока  
    [SerializeField] private List<MG_Division> _divisions;//все дивизии
    [SerializeField] private List<MG_Settlement> _settlements;//все поселения
    [SerializeField] private bool _isFogDeactivated = false;//отключен ли туман войны для данного игрока
    private HashSet<MG_HexCell> _discoveredCells = new HashSet<MG_HexCell>();//Список тех клеток, которые были обнаружены юнитами и не содержат черного тумана. Используется в FOG OF WAR.
    private HashSet<MG_HexCell> _visibleCells = new HashSet<MG_HexCell>();//какие клетки находяться в зоне видимости
    //private HashSet<MG_HexCell> _cellsUnderControl = new HashSet<MG_HexCell>();//
    // private HashSet<MG_HexCell> _greyCells = new HashSet<MG_HexCell>();  
    #endregion Поля

    #region Свойства
    public GameObject UnitContainer { get => _unitContainer; }//контейнер юнитов
    public GameObject SettlementContainer { get => _settlementContainer; }//контейнер поселений
    public Color Color { get => _color; set => _color = value; }//цвет игрока
    public PlayerType Type { get => _type; set => _type = value; }//тип игрока
    public bool IsFogDeactivated { get => _isFogDeactivated; set => _isFogDeactivated = value; }//отключен ли туман войны для данного игрока
    public IReadOnlyCollection<MG_HexCell> DiscoveredCells { get => _discoveredCells; }//Список тех клеток, которые были обнаружены юнитами и уже не содержат черного тумана. Используется в FOG OF WAR.
    public IReadOnlyCollection<MG_HexCell> VisibleCells { get => _visibleCells; }//какие клетки находяться в зоне видимости
    #endregion Свойства

    #region Методы UNITY
    private void Awake()
    {
        GenerateUID();
        if (_unitContainer == null) Debug.Log("<color=red>MG_Player Awake(): объект 'unitContainer' не прикреплен!</color>");
        if (_settlementContainer == null) Debug.Log("<color=red>MG_Player Awake(): объект 'settlementContainer' не прикреплен!</color>");
    }
    #endregion Методы UNITY

    #region Личные методы
    /// <summary>
    /// Сгенерировать UID
    /// </summary>
    private void GenerateUID()
    {
        uidCounter++;
        uid = MG_Player.uidCounter;
    }
    #endregion

    #region Публичные методы
    /// <summary>
    /// Добавить дивизию
    /// </summary>
    /// <param name="division">юнит</param>
    public void AddDivision(MG_Division division)
    {
        if (!_divisions.Contains(division))
        {
            _divisions.Add(division);
            division.Deleted += RemoveUnit;
            division.transform.SetParent(UnitContainer.transform);
        }
        else
        {
            Debug.Log("<color=red>MG_Player AddDivision(): попытка добавить элемент, который уже в списке.</color>");
        }
    }

    /// <summary>
    /// Удалить дивизию
    /// </summary>
    /// <param name="unit">юнит</param>
    public void RemoveUnit(MG_Division division)
    {
        if (_divisions.Contains(division))
        {
            division.Deleted -= RemoveUnit;
            Debug.Log("<color=red>УДАЛЕН division:</color> " + division.name + " (" + this.name + ")");
            _divisions.Remove(division);
        }
        else
        {
            Debug.Log("<color=red>MG_Player RemoveUnit(): попытка удалить элемент, который отсутствует в списке.</color>");
            Debug.Log("<color=red>this</color> " + this.name);
            Debug.Log("<color=red>division</color> " + division.name);
        }
    }

    /// <summary>
    /// Добавить поселение
    /// </summary>
    /// <param name="settlement"></param>
    public void AddSettlement(MG_Settlement settlement)
    {
        if (!_settlements.Contains(settlement))
        {
            _settlements.Add(settlement);
            settlement.transform.SetParent(SettlementContainer.transform);
        }
        else
        {
            Debug.Log("<color=red>MG_Player AddSettlement(): попытка добавить элемент, который уже в списке.</color>");
        }
    }

    /// <summary>
    /// Удалить дивизию
    /// </summary>
    /// <param name="unit">юнит</param>
    public void RemoveSettlement(MG_Settlement settlement)
    {
        if (_settlements.Contains(settlement))
        {
            _settlements.Remove(settlement);
        }
        else
        {
            Debug.Log("<color=red>MG_Player RemoveSettlement(): попытка удалить элемент, который отсутствует в списке.</color>");
        }
    }

    /// <summary>
    /// Получить лист всех дивизий
    /// </summary>
    /// <returns></returns>
    public IEnumerable<MG_Division> GetDivisions() //IEnumerable? или IReadOnlyList?
    {
        return _divisions;
        // return _divisions.AsReadOnly();
    }

    /// <summary>
    /// Получить лист всех поселений
    /// </summary>
    /// <returns></returns>
    public IEnumerable<MG_Settlement> GetSettlements() //IEnumerable? или IReadOnlyList?
    {
        return _settlements;
    }

    /// <summary>
    /// Очистить от всех Поселений
    /// </summary>
    public void ClearAllSettlements()
    {
        foreach (var settlement in _settlements.ToList())
        {
            _settlements.Remove(settlement);
            Destroy(settlement);
        }
    }

    /// <summary>
    /// Очистить от всех дивизий
    /// </summary>
    public void ClearAllDivisions()
    {
        foreach (var divsion in _divisions.ToList())
        {
            _divisions.Remove(divsion);
            Destroy(divsion);
        }
    }

    ///// <summary>
    ///// Очистить лист посещенных клеток
    ///// </summary>
    //public void ResetDiscoveredCells()//List<MG_HexCell> exceptionCells
    //{
    //    //if (exceptionCells.Any())
    //    //{
    //    //    _discoveredCells = new HashSet<MG_HexCell>(exceptionCells);//Список тех клеток, которые были обнаружены юнитами и не содержат черного тумана. Используется в FOG OF WAR.
    //    //}
    //    //else
    //    //{
    //        _discoveredCells = new HashSet<MG_HexCell>();//Список тех клеток, которые были обнаружены юнитами и не содержат черного тумана. Используется в FOG OF WAR.
    //    //}

    //}

    #endregion Публичные методы

    #region МЕТОДЫ ДЛЯ ВЗАИМОДЕЙСТВИЯ С МОДУЛЕМ "ТУМАН ВОЙНЫ"
    /// <summary>
    /// Сохранить посещенную клетку
    /// </summary>
    /// <param name="cell">клетка</param>
    public void SaveDiscoveredCell(MG_HexCell cell)
    {
        if (!_discoveredCells.Contains(cell))
        {
            _discoveredCells.Add(cell);
        }
        else
        {
            Debug.Log("<color=red>MG_Player SaveDiscoveredCell(): уже в списке.</color>");
        }

    }

    /// <summary>
    /// Забыть о всех посещенных клетках
    /// </summary>
    public void ForgetAllDiscoveredCells()
    {
        if (_discoveredCells.Any())
        {
            HashSet<MG_HexCell> allVisibleCells = GetAllVisibleCells();
            if (allVisibleCells.Any())
            {
                foreach (var cell in _discoveredCells)
                {
                    if (allVisibleCells.Contains(cell) == false)//не трогать те клетки, которые видимы сейчас юнитами, поселениями и тд.
                        cell.ForgetDiscover(this);// Удалить информацию о том что сторона знала что то 
                }
            }

            _discoveredCells.Clear();

            if (allVisibleCells.Any())
            {
                _discoveredCells.UnionWith(allVisibleCells);//объединить с видимыми
            }
        }
    }

    /// <summary>
    /// Была ли клетка разведана
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public bool IsCellDiscovered(MG_HexCell cell)
    {
        return DiscoveredCells.Contains(cell);
    }

    /// <summary>
    /// Получить все видимые клетки
    /// </summary>
    /// <returns></returns>
    private HashSet<MG_HexCell> GetAllVisibleCells()
    {
        HashSet<MG_HexCell> allVisibleCells = new HashSet<MG_HexCell>();
        allVisibleCells.UnionWith(_visibleCells);

        if (_settlements.Any())
            foreach (var settlement in _settlements)
            {
                allVisibleCells.UnionWith(settlement.CellsUnderControl);
            }

        if (_divisions.Any())
            foreach (var division in _divisions)
            {
                allVisibleCells.UnionWith(division.VisibleCells);
            }

        return allVisibleCells;
    }
    #endregion МЕТОДЫ ДЛЯ ВЗАИМОДЕЙСТВИЯ С МОДУЛЕМ "ТУМАН ВОЙНЫ"

    #region МЕТОДЫ ИНТЕРФЕЙСА "ICellObserver" <- необходим для раскрытия всей карты с помощью чита
    /// <summary>
    /// Добавить видимую клетку
    /// </summary>
    /// <param name="cell"></param>
    public void AddVisibleCell(MG_HexCell cell)
    {
        _visibleCells.Add(cell);
    }

    /// <summary>
    /// Удалить видимую клетку
    /// </summary>
    /// <param name="cell"></param>
    public void RemoveVisibleCell(MG_HexCell cell)
    {
        _visibleCells.Remove(cell);
    }

    /// <summary>
    /// Получить владельца
    /// </summary>
    /// <returns></returns>
    public MG_Player GetSide()
    {
        return this;
    }

    /// <summary>
    /// Обновить видимость
    /// </summary>
    public void RecalculateVision()
    {
        //Не требуется, так как статическое видение
    }

    /// <summary>
    /// получить все видимые клетки
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<MG_HexCell> GetVisibleCells()
    {
        return _visibleCells;
    }
    #endregion МЕТОДЫ ИНТЕРФЕЙСА "ICellObserver" <- необходим для раскрытия всей карты с помощью чита

    #region МЕТОДЫ ИНТЕРФЕЙСА "IEquatableUID"
    /// <summary>
    /// Сравнить УИДы
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool EqualsUID(MG_Player other)
    {
        return uid == other.uid;
    }
    #endregion МЕТОДЫ ИНТЕРФЕЙСА "IEquatableUID"

}
}
