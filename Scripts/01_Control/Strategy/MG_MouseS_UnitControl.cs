using DigitalRuby.Threading;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MG_StrategyGame
{
    public class MG_MouseS_UnitControl : MG_MouseStrategy
    {
        #region Поля
        [BoxGroup("Текст для дебага (необязательный)"), Required("NOT required", InfoMessageType.Info), SerializeField] TextMeshProUGUI _debugText1;
        [BoxGroup("Текст для дебага (необязательный)"), Required("NOT required", InfoMessageType.Info), SerializeField] TextMeshProUGUI _debugText2;

        [BoxGroup("Дебаг"), SerializeField, ReadOnly] private Camera _mainCamera;//компонен главной камеры
        private MG_HexCell _clickedCell;//кликнутая клетка
        private MG_HexCell _mouseCell;//текущая клетка на которой остановился указатель мыши
        [BoxGroup("Дебаг"), SerializeField, ReadOnly] private MG_Division _chosenUnit;//выбранный юнит
        [BoxGroup("Дебаг"), SerializeField, ReadOnly] private MG_Division _underMouseUnit;//выбранный юнит противника для атаки
                                                                                          //private MG_Settlement _chosenSettlement;//выбранное здание

        [BoxGroup("Дебаг"), SerializeField, ReadOnly] Vector3Int _clickedPos;//позиция клика на карте

        private MG_HexCell _UnderMouseCell = null;//выбранная клетка для показа пути к ней

        private bool _needToStartPathfinding = false;


        //private EZThread.EZThreadRunner _thread = null;
        #endregion Поля

        #region Поля: Необходимые модули + объекты
        [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private TextMeshPro _MovementCostLabel;//[R] объект текста стоимости пути
        [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private GameObject _cameraObject;//[R] объект камеры
        [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMap _map;//[R] карта
        [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_HexArrowLayer _arrowLayerManager;//[R] Менеджер стрелочных путей
        [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_TurnManager _turnManager;//[R] Менеджер хода
        [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_PathAreaVisualizator _pathAreaVisualizator;//[R] визуализатор доступной зоны для передвижения
        [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_MultiThreadPathExecuter _multiThreadPathExecuter;
        [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_UnderMouseCellTarget _underMouseCellTarget;

        #endregion Поля: Необходимые модули + объекты

        #region Свойства
        public MG_Division ChosenUnit { get => _chosenUnit; set => _chosenUnit = value; }
        //public MG_Settlement ChosenSettlement { get => _chosenSettlement; set => _chosenSettlement = value; }
        #endregion Свойства

        #region Методы UNITY
        private void Awake()
        {
            _mainCamera = Camera.main;
            if (_MovementCostLabel == null) Debug.Log("<color=red>MG_MouseS_UnitControl Awake(): объект '_MovementCostLabel' не прикреплен!</color>");
            if (_cameraObject == null) Debug.Log("<color=red>MG_MouseS_UnitControl Awake(): объект '_mainCameraGameObject' не прикреплен!</color>");
            if (_map == null) Debug.Log("<color=red>MG_MouseS_UnitControl Awake(): объект '_map' не прикреплен!</color>");
            if (_arrowLayerManager == null) Debug.Log("<color=red>MG_MouseS_UnitControl Awake(): объект '_arrowLayerManager' не прикреплен!</color>");
            if (_turnManager == null) Debug.Log("<color=red>MG_MouseS_UnitControl Awake(): объект '_turnManager' не прикреплен!</color>");
            if (_pathAreaVisualizator == null) Debug.Log("<color=red>MG_MouseS_UnitControl Awake(): объект '_pathAreaVisualizator' не прикреплен!</color>");
            if (_multiThreadPathExecuter == null) Debug.Log("<color=red>MG_MouseS_UnitControl Awake(): объект '_multiThreadPathExecuter' не прикреплен!</color>");
            if (_underMouseCellTarget == null) Debug.Log("<color=red>MG_MouseS_UnitControl Awake(): объект '_underMouseCellTarget' не прикреплен!</color>");
        }

        private void Start()
        {
            _MovementCostLabel.text = "";
            _turnManager.EndTurn += Reset;
        }

        private void OnDisable()
        {
            _turnManager.EndTurn -= Reset;
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

            UpdatePathfindingInfo();
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
            if (MG_TurnManager.IsTurnButtonEnabled() == false)
                return;

            var unit = cell.Division;
            var building = cell.Settlement;

            //bool isUnitOnCell = unit != null;
            //bool isChosenUnitExist = _chosenUnit != null;

            if (!_MovementCostLabel.text.Equals(""))
                _MovementCostLabel.text = "";

            if (unit != null && unit.IsMoving)
            {
                return;// Пока движется юнит, нельзя кликнуть на него
            }

            if (unit != null && building != null && _chosenUnit == null)
            {
                WN_CellContextMenu.Instance.Show(unit, building);//Показать окно выбора: юнит или город
                return;//
            }

            if (building != null && _chosenUnit == null)
            {
                if (building.Side.EqualsUID(MG_TurnManager.GetCurrentPlayerVisibleMap()))
                {
                    WN_SettlementView.Instance.Show(building);//показать окно города
                }
                return;
            }

            if (unit != null)//есть юнит на клетке
            {

                if (!unit.Equals(_chosenUnit))//если есть ЧУЖОЙ Юнит на выбранной клетке 
                {
                    if (unit.Side.Equals(MG_TurnManager.GetCurrentPlayerTurn()))
                    {
                        if (_chosenUnit != null)//если кликнут новый юнит, то нужно предыдущий ресетнуть также если есть
                        {
                            _chosenUnit.OnDeselect();
                            _pathAreaVisualizator.Clear();//прячем доступную зону для передвижения
                        }

                        if (_underMouseUnit != null)
                        {
                            _underMouseUnit.OnDeselectUnderMouse();
                            _underMouseUnit = null;
                        }

                        //Debug.Log("unit Clicked!");
                        _chosenUnit = unit;
                        unit.OnSelect();
                        _pathAreaVisualizator.Show(_chosenUnit.PathsInRange);
                    }
                    else
                    {
                        if (_chosenUnit != null)
                        {
                            if (cell.IsVisibleForPlayer(_chosenUnit.Side))
                            {
                                Debug.Log(_chosenUnit.name + " is moving to attack " + unit.name);
                                _chosenUnit.IsScouting = false;
                            }
                            else
                            {
                                _chosenUnit.IsScouting = true;
                                Debug.Log(_chosenUnit.name + " is scouting and will face with the enemy: " + unit.name);
                            }

                            if (_chosenUnit.IsWalkableCell(cell) == false && cell.IsVisibleForPlayer(_chosenUnit.Side))
                                return;

                            bool isRunning = _multiThreadPathExecuter.IsRunning;
                            if (isRunning)
                                return;

                            _chosenUnit.Move();
                            Reset();
                        }
                    }
                }
                else
                {//если кликнут тот же юнит
                    Reset();
                }
                return;
            }

            //если выбрана пустая свободная клетка
            if (_chosenUnit != null)
            {//если до этого был выбрат юнит, то значит передвинуться на клетку
             //Debug.Log("MOVING!");

                if (_chosenUnit.IsWalkableCell(cell) == false && cell.IsVisibleForPlayer(_chosenUnit.Side))
                    return;

                bool isRunning = _multiThreadPathExecuter.IsRunning;
                if (isRunning)
                    return;

                _chosenUnit.Move();
                _underMouseCellTarget.SetVisibe(false);
                Reset();
            }

        }

        /// <summary>
        /// При заходе на клетку указателем мыши
        /// </summary>
        /// <param name="cell">клетка</param>
        private void OnEnterCell(MG_HexCell cell)
        {

            _UnderMouseCell = cell;
            if (_underMouseUnit != null)
            {
                _underMouseUnit.OnDeselectUnderMouse();
                _underMouseUnit = null;
            }

            bool isMouseUnderOtherUnit = false;
            if (_chosenUnit != null)//Выбран юнит до этого
            {
                _underMouseCellTarget.SetPosition(cell, _chosenUnit);
                if (!_chosenUnit.Cell.EqualsUID(cell))//Если другая клетка
                {

                    _needToStartPathfinding = true;

                    var unit = cell.Division;
                    if (unit != null)
                    {
                        isMouseUnderOtherUnit = true;
                        _underMouseUnit = unit;
                        if (_underMouseUnit.Side.EqualsUID(_chosenUnit.Side))
                        {
                            _arrowLayerManager.ClearArrowTileMap();// Очищаем от стрелок
                            if (!_MovementCostLabel.text.Equals(""))
                                _MovementCostLabel.text = "";
                            _underMouseUnit.OnSelectUnderMouse();
                        }
                        else
                        {
                            if (!_MovementCostLabel.text.Equals(""))
                                _MovementCostLabel.text = "";
                            _underMouseUnit.OnSelectUnderMouse();
                        }
                    }
                }
                else
                {

                    _underMouseCellTarget.SetVisibe(false);
                    _arrowLayerManager.ClearArrowTileMap();// Очищаем от стрелок
                    if (!_MovementCostLabel.text.Equals(""))
                        _MovementCostLabel.text = "";
                }
            }
            else
            {
                if (!_MovementCostLabel.text.Equals(""))
                    _MovementCostLabel.text = "";
            }

            if (!isMouseUnderOtherUnit)//если под мышью НЕТ ЧУЖОГО ЮНИТА
            {

                if (_underMouseUnit != null)//если ДО ЭТОГО был ЧУЖОЙ ЮНИТ под мышью, то СБРАСЫВАЕМ ВЫДЕЛЕНИЕ
                {
                    _underMouseUnit.OnDeselectUnderMouse();
                    _underMouseUnit = null;
                }
            }

        }

        /// <summary>
        /// Сбросить все данные
        /// </summary>
        private void Reset()
        {
            _arrowLayerManager.ClearArrowTileMap();

            if (_chosenUnit != null)
            {
                _chosenUnit.OnDeselect();
                _pathAreaVisualizator.Clear();//прячем доступную зону для передвижения
                _chosenUnit = null;
            }

            if (_underMouseUnit != null)
            {
                _underMouseUnit.OnDeselectUnderMouse();
                _underMouseUnit = null;
            }

            if (!_MovementCostLabel.text.Equals(""))
                _MovementCostLabel.text = "";
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
                    _debugText1.text = _debugText1.text + " GroundType: " + cell.Type.GroundType;
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

        /// <summary>
        /// Обновить pathfinding
        /// </summary>
        private void UpdatePathfindingInfo()
        {
            //BEGIN Отмена нахождения пути, если клетка непроходимая для текущего юнита
            if (_chosenUnit != null)
            {
                if (_chosenUnit.Side.IsCellDiscovered(_UnderMouseCell))
                {

                    //Debug.Log(hasAnyRoad);
                    if (_chosenUnit.IsWalkableCell(_UnderMouseCell) == false)
                    {
                        _needToStartPathfinding = false;
                        _arrowLayerManager.ClearArrowTileMap();
                        if (!_MovementCostLabel.text.Equals(""))
                            _MovementCostLabel.text = "";

                        return;
                    }
                }
            }
            //END Отмена нахождения пути, если клетка непроходимая для текущего юнита

            if (_needToStartPathfinding)
            {
                bool isRunning = _multiThreadPathExecuter.IsRunning;//необходимо ограничить кол-во запущенных потоков до 1
                if (isRunning == false)
                {
                    _arrowLayerManager.ClearArrowTileMap();// Очищаем от стрелок
                    _needToStartPathfinding = false;
                    _multiThreadPathExecuter.Execute(_chosenUnit, _UnderMouseCell);
                }
                else
                {
                    if (!_MovementCostLabel.text.Equals(""))
                        _MovementCostLabel.text = "";
                }
            }

            bool isReady = _multiThreadPathExecuter.IsReady;
            if (isReady)
            {
                if (_chosenUnit != null)//19.09.2021
                {
                    List<MG_HexCell> path = _multiThreadPathExecuter.GetResult();
                    ShowPath(path);
                }

            }
            else
            {
                if (_chosenUnit == null)
                    _arrowLayerManager.CheckAndStopDrawing();
            }
        }

        /// <summary>
        /// Показать путь
        /// </summary>
        /// <param name="path"></param>
        private void ShowPath(List<MG_HexCell> path)
        {
            if (path.Any())
            {

                _arrowLayerManager.ShowArrowPath(_chosenUnit.Temp_currentPath, _chosenUnit.Cell, _chosenUnit.PathsInRange);//// Показать путь стрелками
                                                                                                                           // _MovementCostLabel.gameObject.transform.position = _UnderMouseCell.WorldPos;
                _MovementCostLabel.gameObject.transform.position = path.First().WorldPos;
                _MovementCostLabel.gameObject.transform.position += new Vector3(0.25f, 0, -1f);
                if (_chosenUnit.IsPathTroughtBlackFog)
                    _MovementCostLabel.text = "?";
                else
                    //   _MovementCostLabel.text = " " + (_chosenUnit.MovementPoints - _chosenUnit.Temp_pathCost);
                    _MovementCostLabel.text = " " + (float)Math.Floor((_chosenUnit.MovementPoints - _chosenUnit.Temp_pathCost));
            }
            else
            {
                if (!_MovementCostLabel.text.Equals(""))
                    _MovementCostLabel.text = "";
            }

        }

        #endregion Личные методы

    }
}
