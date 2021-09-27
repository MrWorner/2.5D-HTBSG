using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MG_StrategyGame
{
public class MG_MouseS_BorderBuilder : MG_MouseStrategy
{
    #region Поля
    [BoxGroup("Для дебагинга"), Required("NOT required", InfoMessageType.Info), SerializeField] TextMeshProUGUI _debugText1;
    [BoxGroup("Для дебагинга"), Required("NOT required", InfoMessageType.Info), SerializeField] TextMeshProUGUI _debugText2;

    private MG_HexCell _clickedCell;//кликнутая клетка
    private MG_HexCell _cellUnderMouse;//текущая клетка на которой остановился указатель мыши

    [BoxGroup("Для дебагинга"), SerializeField, ReadOnly] Vector3Int _clickedPos;//позиция клика на карте
    [BoxGroup("Настройки"), SerializeField] bool _independendBorderMod = false;//новая территория не будет прикреплена к какому либо городу
    #endregion Поля

    #region Поля: Необходимые модули
    private Camera _mainCamera;//[R]компонен главной камеры (инициализация в Awake())
    private GameObject _mainCameraGameObj;//[R] объект камеры (инициализация в Awake())
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMap _map;//[R] карта
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_HexBorderManager _borderManager;//[R] менеджер границ
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_Player _side;//[R] сторона которая будет владеть созданными границами
   
    #endregion Поля: Необходимые модули

    #region Методы UNITY
    private void Awake()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null) Debug.Log("<color=red>MG_MouseS_UnitControl Awake(): объект '_mainCamera' НЕ НАЙДЕН!</color>");

        _mainCameraGameObj = _mainCamera.gameObject;
        if (_mainCameraGameObj == null) Debug.Log("<color=red>MG_MouseS_UnitControl Awake(): объект '_mainCameraGameObject' не прикреплен!</color>");

        if (_map == null) Debug.Log("<color=red>MG_MouseS_UnitControl Awake(): объект '_map' не прикреплен!</color>");
        if (_borderManager == null) Debug.Log("<color=red>MG_MouseS_UnitControl Awake(): объект '_borderManager' не прикреплен!</color>");
        if (_side == null) Debug.Log("<color=red>MG_MouseS_UnitControl Awake(): объект '_side' не прикреплен!</color>");      
    }
    #endregion Методы UNITY

    #region Родительские виртуальные методы MG_MouseStrategy
    /// <summary>
    /// Любое событие Перед
    /// </summary>
    /// <param name="mousePos">позиция мыши</param>
    public override void AnyBefore(Vector3 mousePos)
    {
        //throw new System.NotImplementedException();
    }

    /// <summary>
    /// Любое событие После
    /// </summary>
    /// <param name="mousePos">позиция мыши</param>
    public override void AnyAfter(Vector3 mousePos)
    {
        UpdateUILabels();//обновляем данные для тестовых лейбелов
        MG_CameraWheelZoomer.ZoomByMouseWeel(_mainCamera);// Зум с помощью колесика мыши
    }

    /// <summary>
    /// Бездействие
    /// </summary>
    /// <param name="mousePos">позиция мыши</param>
    public override void Idling(Vector3 mousePos)
    {
        MG_CameraDragger.KeepDraggingDisabled();// Отключить перетаскивание   

        MG_HexCell cell = FindCellByMousePos(mousePos);
        if (cell != null)
        {
            if (!cell.Equals(_cellUnderMouse))
            {
                _cellUnderMouse = cell;
                OnEnterCell(cell);
            }
        }
    }

    /// <summary>
    /// Левый клик
    /// </summary>
    /// <param name="mousePos">позиция мыши</param>
    public override void LeftClick(Vector3 mousePos)
    {
        MG_CameraDragger.KeepDraggingDisabled();// Отключить перетаскивание

        MG_HexCell cell = FindCellByMousePos(mousePos);
        SaveClickedCell(cell);

        if (EventSystem.current.IsPointerOverGameObject())//ФИКС ДЛЯ ТОГО ЧТОБЫ НЕЛЬЗЯ БЫЛО КЛИКНУТЬ ЧЕРЕЗ UI, багнутый
        {
            //---Reset();//ресет теперь к нужным кнопкам соединяется  
            return;
        }

        if (cell != null)
        {
            OnClickCell(cell, true);
        }
    }

    /// <summary>
    /// Левый клик отпуск
    /// </summary>
    /// <param name="mousePos">позиция мыши</param>
    public override void LeftClickDown(Vector3 mousePos)
    {

    }

    /// <summary>
    /// Средний клик
    /// </summary>
    /// <param name="mousePos">позиция мыши</param>
    public override void MiddleClick(Vector3 mousePos)
    {
        MG_CameraDragger.Dragging(_mainCamera,gameObject);//Перетаскивание с помощью зажатой мыши
    }

    /// <summary>
    /// Правый клик
    /// </summary>
    /// <param name="mousePos">позиция мыши</param>
    public override void RightClick(Vector3 mousePos)
    {
        MG_CameraDragger.KeepDraggingDisabled();// Отключить перетаскивание

        MG_HexCell cell = FindCellByMousePos(mousePos);
        SaveClickedCell(cell);
        if (cell != null)
        {
            OnClickCell(cell, false);
        }
    }
    #endregion Родительские виртуальные методы MG_MouseStrategy

    #region Личные методы

    /// <summary>
    /// Получить клетку с помощью координаты мыши
    /// </summary>
    /// <param name="mousePos"></param>
    /// <returns></returns>
    private MG_HexCell FindCellByMousePos(Vector3 mousePos)
    {
        MG_HexCell cell = _map.GetCell(mousePos);
        return cell;
    }

    /// <summary>
    /// При нажатии на клетку
    /// </summary>
    /// <param name="cell">клетка</param>
    private void OnClickCell(MG_HexCell cell, bool isLeftClick)
    {
        if (isLeftClick)
        {
            if (_independendBorderMod)
            {
                var cellOwner = cell.BorderData.Owner;
                if (cellOwner.EqualsUID(MG_PlayerManager.GetEmptySide()))
                {
                    _borderManager.Place(cell, _side, true);                    
                }
                else
                {
                    //Debug.Log("<color=orange>MG_MouseS_BorderBuilder OnClickCell: у клетки уже есть хозяин! </color>" + cell.Pos + " " + cellOwner.name);
                }
            }
            else
            {
                //Проверить что границы близко к городу и имеют выбранную сторону
                var cellOwner = cell.BorderData.Owner;
                if (cellOwner.EqualsUID(MG_PlayerManager.GetEmptySide()))
                {
                    foreach (var cellN in cell.GetNeighbours())
                    {
                        var cellOwnerN = cellN.BorderData.Owner;
                        if (_side.EqualsUID(cellOwnerN))
                        {
                            var regionOwner = cellN.BorderData.RegionOwner;
                            if (regionOwner is MG_Settlement)
                            {
                                MG_Settlement settlement = regionOwner as MG_Settlement;
                                settlement.GrowTerritory(cell);
                                return;
                            }
                        }
                    }
                    //Debug.Log("<color=orange>MG_MouseS_BorderBuilder OnClickCell: не найден близлежащий регион выбранной стороны! </color>" + cell.Pos + " текущий:" + cellOwner.name + " выбран:" + _side.name);
                }
                else
                {
                    //Debug.Log("<color=orange>MG_MouseS_BorderBuilder OnClickCell: у клетки уже есть хозяин! </color>" + cell.Pos + " " + cellOwner.name);
                }
            }
        }
        else
        {

            MG_HexBorderData borderData = cell.BorderData;
            MG_Player cellOwner = borderData.Owner;
            if (cellOwner.EqualsUID(MG_PlayerManager.GetEmptySide()))
            {
                //Debug.Log("<color=orange>MG_MouseS_BorderBuilder OnClickCell: </color> Данная клетка никому не принадлежит." );
            }
            else
            {           
                _borderManager.Erase(cell);// Очистить территорию от хозяина              
            }
        }
    }

    /// <summary>
    /// При заходе на клетку указателем мыши
    /// </summary>
    /// <param name="cell">клетка</param>
    private void OnEnterCell(MG_HexCell cell)
    {

    }

    /// <summary>
    /// Сохранить кликнутую позицию
    /// </summary>
    /// <param name="cell">клетка</param>
    private void SaveClickedCell(MG_HexCell cell)
    {
        if (cell != null)
        {
            _clickedCell = cell;
            _clickedPos = cell.Pos;
        }
        else
        {
            _clickedCell = null;
            _clickedPos = new Vector3Int(0, 0, -100);
        }
    }

    /// <summary>
    /// Обновить тестовые лейбелы
    /// </summary>
    private void UpdateUILabels()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var cell = _map.GetCell(worldPos);
        if (cell != null)
        {
            if (_debugText1 != null)
            {
                _debugText1.text = "_mouse: " + cell.Pos + " HexPos: " + cell.HexPos;
                _debugText1.text = _debugText1.text + " | Player: " + MG_TurnManager.GetCurrentPlayerTurn().name;
                _debugText1.text = _debugText1.text + " | Turn: " + MG_TurnManager.GetTurn();
            }
        }
        else
        {
            if (_debugText1 != null)
            {
                _debugText1.text = "_mouse: - - -";
                _debugText1.text = _debugText1.text + " | Player: " + MG_TurnManager.GetCurrentPlayerTurn().name;
                _debugText1.text = _debugText1.text + " | Turn: " + MG_TurnManager.GetTurn();
            }

        }

        if (_clickedCell != null)
        {
            if (_debugText2 != null)
                if (cell != null)
                    _debugText2.text = "chosen: " + _clickedCell.Pos + " BorderType=" + cell.BorderData.BorderType.Id;
                else
                    _debugText2.text = "chosen: " + _clickedCell.Pos;

            //_debugText2.text = "chosen: " + _clickedCell.Pos + " Division=" + (_clickedCell.Division == true) + " Settlement=" + (_clickedCell.Settlement == true);
        }
        else
        {
            if (_debugText2 != null)
                _debugText2.text = "chosen: " + _clickedPos;
        }

    }
    #endregion Личные методы
}
}
