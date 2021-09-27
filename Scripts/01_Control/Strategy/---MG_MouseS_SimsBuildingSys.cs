//using Sirenix.OdinInspector;
//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.EventSystems;

//public class MG_MouseS_SimsBuildingSys : MG_MouseStrategy
//{
//    [Required("NOT required", InfoMessageType.Info), SerializeField] TextMeshProUGUI _debugText1;
//    [Required("NOT required", InfoMessageType.Info), SerializeField] TextMeshProUGUI _debugText2;

//    private Camera _mainCamera;//компонен главной камеры
//    private MG_HexCell _clickedCell;//кликнутая клетка
//    private MG_HexCell _mouseCell;//текущая клетка на которой остановился указатель мыши

//    [SerializeField] private float zoomMin = 3f; //Минимальный зум
//    [SerializeField] private float zoomMax = 35f; //Максимальный зум
//    [Required(InfoMessageType.Error), SerializeField] private GameObject _cameraObject;//[R] объект камеры


//    private Vector3 draggMouseOriginP;//перетаскивание начальная позиция мыши
//    private Vector3 draggMouseOffset;//перетаскивание разница перемещения мыши
//    private bool isDraggingMouse;//активировано ли перетаскивание мыши

//    [Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMap _map;//[R] карта
//    [SerializeField] Vector3Int _clickedPos;//позиция клика на карте

//    private bool isStarterCellSet = false;


//    [Required(InfoMessageType.Error), SerializeField] private MG_SimsBuildingSystem simsBuildingSystem;//менеджер дорог
//    private void Awake()
//    {
//        _mainCamera = Camera.main;
//        if (_cameraObject == null)
//            Debug.Log("<color=red>MG_MouseS_RoadBuilder Awake(): объект '_mainCameraGameObject' не прикреплен!</color>");
//        if (_map == null)
//            Debug.Log("<color=red>MG_MouseS_RoadBuilder Awake(): объект '_map' не прикреплен!</color>");

//        if (simsBuildingSystem == null)
//            Debug.Log("<color=red>MG_MouseS_RoadBuilder Awake(): объект 'simsBuildingSystem' не прикреплен!</color>");
//    }

//    /// <summary>
//    /// Любое событие После
//    /// </summary>
//    /// <param name="mousePos">позиция мыши</param>
//    public override void AnyAfter(Vector3 mousePos)
//    {
//        UpdateUILabels();//обновляем данные для тестовых лейбелов
//        ZoomByMouseWeel();// Зум с помощью колесика мыши
//    }

//    /// <summary>
//    /// Бездействие
//    /// </summary>
//    /// <param name="mousePos">позиция мыши</param>
//    public override void Idling(Vector3 mousePos)
//    {
//        KeepDraggingDisabled();// Отключить перетаскивание   

//        //MG_HexCell _cell = FindCellByMousePos(mousePos);
//        //if (_cell != null)
//        //{
//        //    if (!_cell.Equals(_cellUnderMouse))
//        //    {
//        //        _cellUnderMouse = _cell;
//        //        OnEnterCell(_cell);
//        //    }
//        //}
//    }

//    /// <summary>
//    /// Левый клик
//    /// </summary>
//    /// <param name="mousePos">позиция мыши</param>
//    public override void LeftClick(Vector3 mousePos)
//    {
//        if (EventSystem.current.IsPointerOverGameObject())//ФИКС ДЛЯ ТОГО ЧТОБЫ НЕЛЬЗЯ БЫЛО КЛИКНУТЬ ЧЕРЕЗ UI, багнутый
//        {
//            //---Reset();//ресет теперь к нужным кнопкам соединяется  
//            return;
//        }

//        if (isStarterCellSet == false)
//        {
//            MG_HexCell starterCell = FindCellByMousePos(mousePos);
//            simsBuildingSystem.SetStarterCell(starterCell);
//        }
//        else
//        {
//            simsBuildingSystem.Reset();
//        }
//        isStarterCellSet = !isStarterCellSet;

//        KeepDraggingDisabled();// Отключить перетаскивание


//    }

//    /// <summary>
//    /// Левый клик отпуск
//    /// </summary>
//    /// <param name="mousePos">позиция мыши</param>
//    public override void LeftClickDown(Vector3 mousePos)
//    {

//    }

//    /// <summary>
//    /// Средний клик
//    /// </summary>
//    /// <param name="mousePos">позиция мыши</param>
//    public override void MiddleClick(Vector3 mousePos)
//    {
//        Dragging();//Перетаскивание с помощью зажатой мыши
//    }

//    /// <summary>
//    /// Правый клик
//    /// </summary>
//    /// <param name="mousePos">позиция мыши</param>
//    public override void RightClick(Vector3 mousePos)
//    {
//        KeepDraggingDisabled();// Отключить перетаскивание

//    }

//    /// <summary>
//    /// Перетаскивание с помощью зажатой мыши
//    /// </summary>
//    private void Dragging()
//    {
//        draggMouseOffset = (_mainCamera.ScreenToWorldPoint(Input.mousePosition) - _cameraObject.transform.position);
//        if (!isDraggingMouse)
//        {
//            isDraggingMouse = true;
//            draggMouseOriginP = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
//            //mouseOriginPoint = _mainCameraGameObject.transform.position;
//        }
//        _cameraObject.transform.position = draggMouseOriginP - draggMouseOffset;
//    }

//    /// <summary>
//    /// Получить клетку с помощью координаты мыши
//    /// </summary>
//    /// <param name="mousePos"></param>
//    /// <returns></returns>
//    private MG_HexCell FindCellByMousePos(Vector3 mousePos)
//    {
//        MG_HexCell _cell = _map.GetCell(mousePos);
//        return _cell;
//    }

//    /// <summary>
//    /// Отключить перетаскивание
//    /// </summary>
//    private void KeepDraggingDisabled()
//    {
//        if (isDraggingMouse)
//        {
//            isDraggingMouse = false;
//        };
//    }

//    /// <summary>
//    /// При заходе на клетку указателем мыши
//    /// </summary>
//    /// <param name="pos">позиция</param>
//    private void OnEnterCell(Vector3Int pos)
//    {
//        Vector3Int hexPos = MG_GeometryFuncs.Oddr_to_cube(pos);
//        var line = simsBuildingSystem.GenerateSimsLine(hexPos);
//        //------ simsBuildingSystem.OnDraggingMouse(pos);
//    }

//    /// <summary>
//    /// Сохранить кликнутую позицию
//    /// </summary>
//    /// <param name="_cell">клетка</param>
//    private void SaveClickedCell(MG_HexCell _cell)
//    {
//        if (_cell != null)
//        {
//            _clickedCell = _cell;
//            _clickedPos = _cell.Pos;
//        }
//        else
//        {
//            _clickedCell = null;
//            _clickedPos = new Vector3Int(0, 0, -100);
//        }
//    }

//    /// <summary>
//    /// Обновить тестовые лейбелы
//    /// </summary>
//    private void UpdateUILabels()
//    {
//        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//        var _cell = _map.GetCell(worldPos);
//        if (_cell != null)
//        {
//            if (_debugText1 != null)
//            {
//                _debugText1.text = "_mouse: " + _cell.Pos + " HexPos: " + _cell.HexPos;
//                _debugText1.text = _debugText1.text + " | Player: " + MG_TurnManager.GetCurrentPlayerTurn().name;
//                _debugText1.text = _debugText1.text + " | Turn: " + MG_TurnManager.GetTurn();
//            }
//        }
//        else
//        {
//            if (_debugText1 != null)
//            {
//                _debugText1.text = "_mouse: - - -";
//                _debugText1.text = _debugText1.text + " | Player: " + MG_TurnManager.GetCurrentPlayerTurn().name;
//                _debugText1.text = _debugText1.text + " | Turn: " + MG_TurnManager.GetTurn();
//            }

//        }

//        if (_clickedCell != null)
//        {
//            if (_debugText2 != null)
//                if (_cell != null)
//                    _debugText2.text = "chosen: " + _clickedCell.Pos + " BorderType=" + _cell.BorderData.BorderType.Id;
//                else
//                    _debugText2.text = "chosen: " + _clickedCell.Pos;

//            //_debugText2.text = "chosen: " + _clickedCell.Pos + " Division=" + (_clickedCell.Division == true) + " Settlement=" + (_clickedCell.Settlement == true);
//        }
//        else
//        {
//            if (_debugText2 != null)
//                _debugText2.text = "chosen: " + _clickedPos;
//        }

//    }

//    /// <summary>
//    /// Зум с помощью колесика мыши
//    /// </summary>
//    private void ZoomByMouseWeel()
//    {
//        _mainCamera.orthographicSize = Mathf.Clamp(_mainCamera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * (10f * _mainCamera.orthographicSize * 0.1f), zoomMin, zoomMax);
//    }
//}
