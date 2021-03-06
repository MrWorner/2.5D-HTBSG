using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_MouseS_LandBrush : MG_MouseStrategy
{
    #region Поля
    [BoxGroup("Для дебагинга"), Required("NOT required", InfoMessageType.Info), SerializeField] TextMeshProUGUI _debugText1;
    [BoxGroup("Для дебагинга"), Required("NOT required", InfoMessageType.Info), SerializeField] TextMeshProUGUI _debugText2;

    private Camera _mainCamera;//компонен главной камеры
    private MG_HexCell _clickedCell;//кликнутая клетка
    private MG_HexCell _mouseCell;//текущая клетка на которой остановился указатель мыши

    private MG_HexCell _previousCell = null;//Предыдущая клетка
    private MG_HexCellType _previousCellType = null;//Предыдущий тип клетки
    private bool _previousIsLeftClicked = false;//Предыдущий раз было ли кликнуто Левая кнопка мыши

    [BoxGroup("Для дебагинга"), SerializeField, ReadOnly] Vector3Int _clickedPos;//позиция клика на карте
    [BoxGroup("Настройки"), SerializeField] bool _doubleBrash;//двойная кисть

    #endregion Поля

    #region Поля: Необходимые модули
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private GameObject _cameraObject;//[R] объект камеры
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMap _map;//[R] карта
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_HexCellType _hexCellType;//[R] тип клетки
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_HexCellType _hexCellTypeAfterRemove;//[R] тип клетки при удалении
    #endregion Поля: Необходимые модули

    #region Методы UNITY
    private void Awake()
    {
        _mainCamera = Camera.main;
        if (_cameraObject == null) Debug.Log("<color=red>MG_MouseS_LandBrush Awake(): объект '_mainCameraGameObject' не прикреплен!</color>");
        if (_map == null) Debug.Log("<color=red>MG_MouseS_LandBrush Awake(): объект '_map' не прикреплен!</color>");
        if (_hexCellType == null) Debug.Log("<color=red>MG_MouseS_LandBrush Awake(): объект '_hexCellType' не прикреплен!</color>");
        if (_hexCellTypeAfterRemove == null) Debug.Log("<color=red>MG_MouseS_LandBrush Awake(): объект '_hexCellTypeAfterRemove' не прикреплен!</color>");
    }
    #endregion Методы UNITY

    #region Родительские перегружаемые методы MG_MouseStrategy
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
            if (!cell.Equals(_mouseCell))
            {
                _mouseCell = cell;
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
        MG_CameraDragger.Dragging(_mainCamera, _cameraObject);//Перетаскивание с помощью зажатой мыши
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
    #endregion Родительские перегружаемые методы MG_MouseStrategy

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
        if (MG_SettlementManager.HasSettlement(cell))
            return;

        if (MG_ResourceManager.HasResource(cell))
            return;

        if (cell.IsTaken)
            return;

        if (isLeftClick)
        {
            if ((!cell.Equals(_previousCell)) || (cell.Equals(_previousCell) && !_previousCellType.Equals(_hexCellType)) || (_previousIsLeftClicked == false))
            {
                _previousCell = cell;
                _previousCellType = _hexCellType;
                _previousIsLeftClicked = true;
                Tile tile = _hexCellType.GroundVariants[0];
                Tile tile_obj = _hexCellType.ObjectVariants[0];
                _map.UpdateCell(cell, _hexCellType, tile, tile_obj, TileOrigin.cellType);
                MG_PathAreaFixer.FixAfterCellUpdate(cell);

                if (_doubleBrash)
                {
                    IReadOnlyList<MG_HexCell> neighbours = cell.GetNeighbours();
                    if (neighbours.Any())
                    {
                        foreach (MG_HexCell cellN in neighbours)
                        {
                            _map.UpdateCell(cellN, _hexCellType, tile, tile_obj, TileOrigin.cellType);
                            MG_PathAreaFixer.FixAfterCellUpdate(cellN);
                        }
                    }
                }

            }
        }
        else
        {
            //УДАЛЕНИЕ ПРАВЫМ КЛИКОМ
            if ((!cell.Equals(_previousCell)) || (cell.Equals(_previousCell) && !_previousCellType.Equals(_hexCellTypeAfterRemove)) || (_previousIsLeftClicked == true))
            {
                _previousCell = cell;
                _previousCellType = _hexCellTypeAfterRemove;
                _previousIsLeftClicked = false;
                Tile tile = _hexCellTypeAfterRemove.GroundVariants[0];
                Tile tile_obj = _hexCellTypeAfterRemove.ObjectVariants[0];
                _map.UpdateCell(cell, _hexCellTypeAfterRemove, tile, tile_obj, TileOrigin.cellType);
                MG_PathAreaFixer.FixAfterCellUpdate(cell);

                if (_doubleBrash)
                {
                    IReadOnlyList<MG_HexCell> neighbours = cell.GetNeighbours();
                    if (neighbours.Any())
                    {
                        foreach (MG_HexCell cellN in neighbours)
                        {
                            _map.UpdateCell(cellN, _hexCellTypeAfterRemove, tile, tile_obj, TileOrigin.cellType);
                            MG_PathAreaFixer.FixAfterCellUpdate(cellN);
                        }
                    }
                }
            }
        }

        cell.RecalculateObserversVisions();

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

