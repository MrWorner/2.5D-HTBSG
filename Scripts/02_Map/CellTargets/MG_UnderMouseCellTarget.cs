using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_UnderMouseCellTarget : MonoBehaviour
{
    #region Поля    
    private MG_HexCell _cell;
    [PropertyOrder(0), BoxGroup("ДЕБАГ"), SerializeField, ReadOnly] private MG_Division _chosenUnit;
    [PropertyOrder(0), BoxGroup("ДЕБАГ"), SerializeField, ReadOnly] private Vector3Int _cellPos;
    [PropertyOrder(0), BoxGroup("ДЕБАГ"), SerializeField, ReadOnly] private SpriteRenderer _sprite;
    [PropertyOrder(0), BoxGroup("ДЕБАГ"), SerializeField, ReadOnly] private bool _isVisible = false;
    [BoxGroup("Цвет для подкурсорной клетки"), SerializeField] Color _cellColor_InArea = Color.green;//в досягаемости зоны
    [BoxGroup("Цвет для подкурсорной клетки"), SerializeField] Color _cellColor_OutOfArea = Color.grey;//вне досягаемости зоны
    [BoxGroup("Цвет для подкурсорной клетки"), SerializeField] Color _cellColor_Unwalkable = Color.red;//непроходимый
    [BoxGroup("Цвет для подкурсорной клетки"), SerializeField] Color _cellColor_BlackFog = Color.black;//туман
    #endregion Поля

    #region Методы UNITY    
    private void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        if (_sprite == null) Debug.Log("<color=red>MG_UnderMouseCellTarget Awake(): не найден _sprite (SpriteRenderer)!</color>");
    }

    private void Start()
    {
        SetVisibe(false);
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Установить позицию
    /// </summary>
    /// <param name="cell"></param>
    public void SetPosition(MG_HexCell cell, MG_Division chosenUnit)
    {
        _cell = cell;
        _cellPos = cell.Pos;
        this.transform.position = cell.WorldPos;
        _chosenUnit = chosenUnit;
        if (_isVisible == false)
        {
            SetVisibe(true);
        }
        UpdateColor();
    }

    /// <summary>
    /// Обновить цвет
    /// </summary>
    public void UpdateColor()
    {
        //Курсор на клетки с черным туманом
        if (_chosenUnit.Side.DiscoveredCells.Contains(_cell) == false)
        {
            _sprite.color = _cellColor_BlackFog;
            //Debug.Log("_cellColor_BlackFog");
            return;
        }

        //Курсор на клетки с непроходимым типом местности для выбранного юнита
        IReadOnlyList<GroundType> unwalkableGroundTypes = _chosenUnit.UnwalkableGroundTypes;
        GroundType groundType = _cell.Type.GroundType;
        bool isEmptyRoad = MG_HexRoadManager.IsEmptyRoad(_cell);
        if (unwalkableGroundTypes.Contains(groundType) && isEmptyRoad)
        {
            _sprite.color = _cellColor_Unwalkable;
            //Debug.Log("_cellColor_Unwalkable");
            return;
        }

        //Курсор на клетки в зоне досягаемости либо НЕ
        IReadOnlyCollection<MG_HexCell> area = _chosenUnit.PathsInRange;
        if (area.Contains(_cell))
        {
            _sprite.color = _cellColor_InArea;
            //Debug.Log("_cellColor_InArea");
        }
        else
        {
            _sprite.color = _cellColor_OutOfArea;
            //Debug.Log("_cellColor_OutOfArea");
        }
    }

    /// <summary>
    /// Установить видимость
    /// </summary>
    /// <param name="visible"></param>
    public void SetVisibe(bool visible)
    {

        if (visible)
        {
            if (_isVisible == visible)
                return;

            _isVisible = true;
            if (!_sprite.enabled)
            {
                _sprite.enabled = true;
            }

        }
        else
        {
            if (_isVisible == visible)
                return;

            _isVisible = false;
            if (_sprite.enabled)
            {
                _sprite.enabled = false;
            }
        }
    }
    #endregion Публичные методы

    //#region Личные методы
    //#endregion Личные методы
}
}
