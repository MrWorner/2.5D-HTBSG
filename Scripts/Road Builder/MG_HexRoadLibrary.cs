using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_HexRoadLibrary : MG_ScriptableObjectLibrary<MG_HexRoad>
{
    private static MG_HexRoadLibrary _instance;

    #region Поля: необходимые для заполнения
    [Required(InfoMessageType.Error), SerializeField] private MG_HexRoad _emptyRoadType;//Пустой тип дороги
    [Required(InfoMessageType.Error), SerializeField] private MG_HexRoad _circleRoadType;//Круговой тип дороги
    #endregion Поля: необходимые для заполнения

    #region Методы UNITY
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_HexRoadLibrary Awake(): найден лишний MG_HexRoadLibrary!</color>");

        if (_items.Count == 0)
            Debug.Log("<color=red>MG_HexRoadLibrary Awake(): '_items' пуст! Должен быть заполнен всеми возможными вариантами</color>");
        else if (_items.Count != 65)
            Debug.Log("<color=red>MG_HexRoadLibrary Awake(): '_items' заполнен не до конца! (А также должен включать: Empty Road, Cirlce road)</color>");

        if (_emptyRoadType == null) Debug.Log("<color=red>MG_HexRoadLibrary Awake(): '_emptyRoadType' не задан!</color>");

    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Получить объект дороги
    /// </summary>
    /// <param name="roadDirection">направление дороги</param>
    /// <returns></returns>
    public MG_HexRoad GetRoad(RoadDirection roadDirection)
    {
        foreach (var roadType in _items)
        {
            RoadDirection arrowDirN = roadType.Id;
            if (arrowDirN.Equals(roadDirection))
                return roadType;
        }
        Debug.Log("<color=red>MG_HexRoadLibrary arrowDirection(): ВНИМАНИЕ ОШИБКА, в библиотеке не найден scriptableObject дороги! Разработчик не настроил все scriptableObject для</color> RoadDirection=" + roadDirection);
        return _emptyRoadType;
    }
    #endregion Публичные методы

    #region Статические методы
    /// <summary>
    /// Получить пустой объект дороги
    /// </summary>
    /// <returns></returns>
    public static MG_HexRoad GetEmptyRoad()
    {
        return _instance._emptyRoadType;
    }
    #endregion Статические методы

    //public static MG_HexRoad GetCircleRoad()
    //{
    //    return _instance._circleRoadType;
    //}
}
}
