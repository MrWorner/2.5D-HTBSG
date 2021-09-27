using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_UnitHighlighterSettings : MonoBehaviour
{
    static private MG_UnitHighlighterSettings _instance;
    public static MG_UnitHighlighterSettings GetInstance()
    {
        return _instance;
    }

    #region Поля
    [SerializeField] Color _color_SelectedFriendly = Color.green;//цвет при выделении
    [SerializeField] Color _defaultColor = new Color(0,1,0,0.5f);//стандартный цвет
    [SerializeField] Color _color_Enemy = Color.red;//цвет при выделении противника
    #endregion Поля

    #region Свойства
    public Color Color_SelectedFriendly { get => _color_SelectedFriendly; set => _color_SelectedFriendly = value; }//цвет при выделении
    public Color ColorDefault { get => _defaultColor; set => _defaultColor = value; }//стандартный цвет
    public Color Color_Enemy { get => _color_Enemy; set => _color_Enemy = value; }//цвет при выделении противника
    #endregion Свойства

    #region Методы UNITY
    private void Awake()
    {
        if (_instance != null)
            Debug.Log("<color=orange>MG_UnitHighlighterSettings Awake(): найдет лишний _instance класса MG_UnitHighlighterSettings.</color>");
        else
            _instance = this;
    }
    #endregion Методы UNITY
 
}
}
