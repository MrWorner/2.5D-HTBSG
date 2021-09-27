using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MG_StrategyGame
{
public class MG_MouseS_Debugger : MG_MouseStrategy
{
    #region Поля  
    [BoxGroup("Дебаг"), SerializeField, ReadOnly] private Camera _mainCamera;//компонен главной камеры
    [BoxGroup("Дебаг"), SerializeField, ReadOnly] Vector3Int _clickedPos;//позиция клика на карте
    private MG_HexCell _clickedCell;//кликнутая клетка
    private MG_HexCell _mouseCell;//текущая клетка на которой остановился указатель мыши
    #endregion Поля

    #region Поля: Необходимые модули + объекты
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] TextMeshProUGUI _debugText1;
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] TextMeshProUGUI _debugText2;
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private GameObject _cameraObject;//[R] объект камеры
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMap _map;//[R] карта
    #endregion Поля: Необходимые модули + объекты

    #region Свойства
    #endregion Свойства

    #region Методы UNITY
    private void Awake()
    {
        _mainCamera = Camera.main;
        if (_cameraObject == null) Debug.Log("<color=red>MG_MouseS_UnitControl Awake(): объект '_mainCameraGameObject' не прикреплен!</color>");
        if (_map == null) Debug.Log("<color=red>MG_MouseS_UnitControl Awake(): объект '_map' не прикреплен!</color>");
    }
    #endregion Методы UNITY

    #region Родительские перегружаемые методы
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

        //MG_HexCell cell = FindCellByMousePos(mousePos);
        //if (cell != null)
        //{

        //}
    }

    /// <summary>
    /// Левый клик
    /// </summary>
    /// <param name="mousePos">позиция мыши</param>
    public override void LeftClick(Vector3 mousePos)
    {
        MG_CameraDragger.KeepDraggingDisabled();// Отключить перетаскивание
    }

    /// <summary>
    /// Левый клик отпуск
    /// </summary>
    /// <param name="mousePos">позиция мыши</param>
    public override void LeftClickDown(Vector3 mousePos)
    {
        MG_HexCell cell = FindCellByMousePos(mousePos);
        SaveClickedCell(cell);

        if (EventSystem.current.IsPointerOverGameObject())//ФИКС ДЛЯ ТОГО ЧТОБЫ НЕЛЬЗЯ БЫЛО КЛИКНУТЬ ЧЕРЕЗ UI, багнутый
        {
            //---Reset();//ресет теперь к нужным кнопкам соединяется  
            return;
        }

        if (cell != null)
        {
            OnClickCell(cell);
        }
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
    }
    #endregion Родительские перегружаемые методы

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
    private void OnClickCell(MG_HexCell cell)
    {

    }

    /// <summary>
    /// При заходе на клетку указателем мыши
    /// </summary>
    /// <param name="cell">клетка</param>
    private void OnEnterCell(MG_HexCell cell)
    {

    }

    /// <summary>
    /// Сбросить все данные
    /// </summary>
    private void Reset()
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
        //StandardText();
        AdvancedRiverText();
    }

    /// <summary>
    /// Стандартный текст
    /// </summary>
    private void StandardText()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var cell = _map.GetCell(worldPos);
        if (cell != null)
        {
            _debugText1.text = "_mouse: " + cell.Pos + " HexPos: " + cell.HexPos;
            _debugText1.text = _debugText1.text + " GroundType: " + cell.Type.GroundType;
            _debugText1.text = _debugText1.text + " | Player: " + MG_TurnManager.GetCurrentPlayerTurn().name;
            _debugText1.text = _debugText1.text + " | Turn: " + MG_TurnManager.GetTurn();

        }
        else
        {
            _debugText1.text = "_mouse: - - -";
            _debugText1.text = _debugText1.text + " | Player: " + MG_TurnManager.GetCurrentPlayerTurn().name;
            _debugText1.text = _debugText1.text + " | Turn: " + MG_TurnManager.GetTurn();
        }

        if (_clickedCell != null)
        {
            _debugText2.text = "chosen: " + _clickedCell.Pos + " | GroundType= " + _clickedCell.Type.GroundType;
            //_debugText2.text = "chosen: " + _clickedCell.Pos + " Division=" + (_clickedCell.Division == true) + " Settlement=" + (_clickedCell.Settlement == true);
        }
        else
        {
            _debugText2.text = "chosen: ---" + _clickedPos;
        }
    }

    /// <summary>
    /// текст модуля AdvancedRiver
    /// </summary>
    private void AdvancedRiverText()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var cell = _map.GetCell(worldPos);
        if (cell != null)
        {
            MG_HexAdvancedRiverData advancedRiverData = cell.AdvancedRiverData;
            _debugText1.text = " HexPos: " + cell.HexPos;
            _debugText1.text = _debugText1.text + " E: " + advancedRiverData.River_E;
            _debugText1.text = _debugText1.text + " NE: " + advancedRiverData.River_NE;
            _debugText1.text = _debugText1.text + " NW: " + advancedRiverData.River_NW;
            _debugText1.text = _debugText1.text + " SE: " + advancedRiverData.River_SE;
            _debugText1.text = _debugText1.text + " SW: " + advancedRiverData.River_SW;
            _debugText1.text = _debugText1.text + " W: " + advancedRiverData.River_W;

        }
        else
        {
            _debugText1.text = "NONE";

        }

        if (_clickedCell != null)
        {
            MG_HexAdvancedRiverData advancedRiverData = _clickedCell.AdvancedRiverData;
            _debugText2.text = "chosen: " + _clickedCell.Pos;
            _debugText2.text = _debugText1.text + " E: " + advancedRiverData.River_E;
            _debugText2.text = _debugText1.text + " NE: " + advancedRiverData.River_NE;
            _debugText2.text = _debugText1.text + " NW: " + advancedRiverData.River_NW;
            _debugText2.text = _debugText1.text + " SE: " + advancedRiverData.River_SE;
            _debugText2.text = _debugText1.text + " SW: " + advancedRiverData.River_SW;
            _debugText2.text = _debugText1.text + " W: " + advancedRiverData.River_W;

        }
        else
        {
            _debugText2.text = "chosen: ---" + _clickedPos;
        }
    }
    #endregion Личные методы
}
}
