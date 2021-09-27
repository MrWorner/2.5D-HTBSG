using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_HexRiverLibrary : MG_ScriptableObjectLibrary<MG_HexRiver>
{
    private static MG_HexRiverLibrary _instance;

    #region Поля: необходимые для заполнения
    [Required(InfoMessageType.Error), SerializeField] private MG_HexRiver _emptyRiverType;//Пустой тип реки
    [Required(InfoMessageType.Error), SerializeField] private MG_HexRiver _circleRiverType;//Круговой тип реки
    #endregion Поля: необходимые для заполнения

    #region Методы UNITY
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_HexRiverLibrary Awake(): найден лишний MG_HexRiverLibrary!</color>");

        if (_items.Count == 0)
            Debug.Log("<color=red>MG_HexRiverLibrary Awake(): '_items' пуст! Должен быть заполнен всеми возможными вариантами</color>");
        else if (_items.Count != 65)
            Debug.Log("<color=red>MG_HexRiverLibrary Awake(): '_items' заполнен не до конца! (А также должен включать: Empty River, Cirlce river)</color>");

        if (_emptyRiverType == null) Debug.Log("<color=red>MG_HexRiverLibrary Awake(): '_emptyRiverType' не задан!</color>");

    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Получить объект реки
    /// </summary>
    /// <param name="roadDirection">направление дороги</param>
    /// <returns></returns>
    public MG_HexRiver GetRiver(RiverDirection riverDirection)
    {
        foreach (var riverType in _items)
        {
            RiverDirection arrowDirN = riverType.Id;
            if (arrowDirN.Equals(riverDirection))
                return riverType;
        }
        Debug.Log("<color=red>MG_HexRiverLibrary arrowDirection(): ВНИМАНИЕ ОШИБКА, в библиотеке не найден scriptableObject реки! Разработчик не настроил все scriptableObject для</color> riverDirection=" + riverDirection);
        return _emptyRiverType;
    }
    #endregion Публичные методы

    #region Статические методы
    /// <summary>
    /// Получить пустой объект дороги
    /// </summary>
    /// <returns></returns>
    public static MG_HexRiver GetEmptyRiver()
    {
        return _instance._emptyRiverType;
    }
    #endregion Статические методы

    //public static MG_HexRiver GetCircleRiver()
    //{
    //    return _instance._circleRiverType;
    //}
}
}
