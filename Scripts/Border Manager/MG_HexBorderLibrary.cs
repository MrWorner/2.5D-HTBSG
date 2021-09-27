using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_HexBorderLibrary : MG_ScriptableObjectLibrary<MG_HexBorder>
{
    private static MG_HexBorderLibrary _instance;

    #region Поля: необходимые для заполнения объектами
    [Required(InfoMessageType.Error), SerializeField] private MG_HexBorder emptyBorderType;//Пустой тип границ
    #endregion Поля: необходимые для заполнения объектами

    #region Методы UNITY
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_HexBorderLibrary Awake(): найден лишний MG_HexBorderLibrary!</color>");

        if (_items.Count == 0)
            Debug.Log("<color=red>MG_HexBorderLibrary Awake(): '_items' пуст! Должен быть заполнен всеми возможными вариантами</color>");
        else if (_items.Count != 64)
            Debug.Log("<color=red>MG_HexBorderLibrary Awake(): '_items' заполнен не до конца! (+ должен включать Empty border)</color>");

        if (emptyBorderType == null)
            Debug.Log("<color=red>MG_HexBorderLibrary Awake(): 'EmptyBorderType' не задан!</color>");
    }
    #endregion Методы UNITY

    #region Публичный метод GetBorder(BorderDirection border)
    /// <summary>
    /// Получить границу по граничному направлению
    /// </summary>
    /// <param name="border"></param>
    /// <returns></returns>
    public MG_HexBorder GetBorder(BorderDirection border)
    {
        foreach (var borderType in _items)
        {
            BorderDirection borderN = borderType.Id;
            if (borderN.Equals(border))
                return borderType;

        }

        Debug.Log("ERROR MG_HexBorderLibrary GetBorder()");
        return emptyBorderType;
    }
    #endregion Публичный метод GetBorder(BorderDirection border)

    #region Статический метод GetEmptyBorder()
    /// <summary>
    /// Получить пустую границу
    /// </summary>
    /// <returns></returns>
    public static MG_HexBorder GetEmptyBorder()
    {
        return _instance.emptyBorderType;
    }
    #endregion Статический метод GetEmptyBorder()
}
}
