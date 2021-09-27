using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_HexArrowLibrary : MG_ScriptableObjectLibrary<MG_HexArrow>
{
    private static MG_HexArrowLibrary _instance;

    #region Поля: необходимые объекты для заполнения
    [Required(InfoMessageType.Error), SerializeField] private MG_HexArrow emptyArrowType;//Пустой тип стрелки
    [Required(InfoMessageType.Error), SerializeField] private MG_HexArrow targetArrowType;//Цель тип стрелки
    #endregion Поля: необходимые объекты для заполнения

    #region Свойства
    //public MG_HexArrow EmptyArrowType { get => emptyArrowType; }
    public MG_HexArrow TargetArrowType { get => targetArrowType; }
    #endregion Свойства

    #region Методы UNITY
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_HexArrowLibrary Awake(): найден лишний MG_HexArrowLibrary!</color>");

        if (_items.Count == 0)
            Debug.Log("<color=red>MG_HexArrowLibrary Awake(): '_items' пуст! Должен быть заполнен всеми возможными вариантами</color>");
        else if (_items.Count != 32)
            Debug.Log("<color=red>MG_HexArrowLibrary Awake(): '_items' заполнен не до конца! (+ должен включать Empty arrow, Target arrow)</color>");

        if (emptyArrowType == null) Debug.Log("<color=red>MG_HexArrowLibrary Awake(): 'emptyArrowType' не задан!</color>");
        if (targetArrowType == null) Debug.Log("<color=red>MG_HexArrowLibrary Awake(): 'targetArrowType' не задан!</color>");

    }
    #endregion Методы UNITY

    #region Публичный метод GetItem(ArrowDirection arrowDirection)
    /// <summary>
    /// Получить элемент
    /// </summary>
    /// <param name="arrowDirection"></param>
    /// <returns></returns>
    public MG_HexArrow GetItem(ArrowDirection arrowDirection)
    {
        foreach (var arrowType in _items)
        {
            ArrowDirection arrowDirN = arrowType.Id;
            if (arrowDirN.Equals(arrowDirection))
                return arrowType;
        }
        Debug.Log("<color=red>MG_HexArrowLibrary arrowDirection(): ВНИМАНИЕ ОШИБКА, в библиотеке не найдена стрелка! Разработчик не настроил все scriptableObject для</color> arrowDirection=" + arrowDirection);
        return emptyArrowType;
    }
    #endregion Публичный метод GetItem(ArrowDirection arrowDirection)
}
}
