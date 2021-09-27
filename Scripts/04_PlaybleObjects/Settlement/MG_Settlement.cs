using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
    public class MG_Settlement : MonoBehaviour, IVisibility, ICellObserver
    {
        #region Поля
        private MG_HexCell _cell;//местонахождение
        private HashSet<MG_HexCell> _cellsUnderControl = new HashSet<MG_HexCell>();//какие клетки находяться под контролем города
        private Dictionary<int, HashSet<MG_HexCell>> _cellsUnderControlWithDistance;//клетки под контролем c дистанцией
                                                                                    //private Visibility _currentVisibility = Visibility.Visible;//Текущая видимость
                                                                                    //--private HashSet<MG_HexCell> controlledCels = new HashSet<MG_HexCell>();//клетки под контролем
        [SerializeField] private int _currentRegionDistance = 0;//текущая дистанция от поселения до самой далекой клетки
        [SerializeField] private int _notFilledRegionDistance = 0;//текущая дистанция от поселения до самой далекой клетки КОТОРАЯ полностью не заполнена территорией
        #endregion Поля

        #region Поля: требуемые модули
        [Required("Должен быть задан в префабе!", InfoMessageType.Error), SerializeField] private MG_UnitHighlighter _highlighter;//[R INIT] подствеститель
        [Required("НЕ ИНИЦИАЛИЗИРОВАН В Init()!", InfoMessageType.Error), SerializeField] private MG_Player _side;//[R INIT] Игровая сторона
        [Required("НЕ ИНИЦИАЛИЗИРОВАН В Init()!", InfoMessageType.Error), SerializeField] private MG_SettlementType _type;//[R INIT] тип
        [Required("НЕ ИНИЦИАЛИЗИРОВАН В Init()!", InfoMessageType.Error), SerializeField] private Tile _tile;//[R INIT] используемый тайл  
        [Required("НЕ ИНИЦИАЛИЗИРОВАН В Init()!", InfoMessageType.Error), SerializeField] private Tile _tile_obj;//[R INIT] используемый тайл объектов 
        [Required("НЕ ИНИЦИАЛИЗИРОВАН В Init()!", InfoMessageType.Error), SerializeField] MG_LineOfSight _lineOfSight;//[R INIT]
        [Required("НЕ ИНИЦИАЛИЗИРОВАН В Init()!", InfoMessageType.Error), SerializeField] MG_HexBorderManager _borderManager;//[R INIT]
        #endregion Поля: требуемые модули

        #region Свойства
        public MG_Player Side { get => _side; }//Игровая сторона
        public MG_HexCell Cell { get => _cell; }//местонахождение
        public MG_UnitHighlighter Highlighter { get => _highlighter; }//подствеститель
        public MG_SettlementType Type { get => _type; }//тип
        public IReadOnlyCollection<MG_HexCell> CellsUnderControl { get => _cellsUnderControl; }//какие клетки находяться под контролем города
        public Tile ChosenTileObj { get => _tile_obj; }//[R INIT] используемый тайл объектов 

        //public Visibility CurrentVisibility { get => _currentVisibility; set => _currentVisibility = value; }//Текущая видимость
        #endregion Свойства

        #region Метод INIT с параметрами
        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="cell">клетка</param>
        /// <param name="side">сторона</param>
        public void Init(MG_HexCell cell, MG_Player side, MG_SettlementType type)
        {
            FindAllNeededManagers();// Инициализировать все необходимые менеджеры
            this._cell = cell;
            this._side = side;
            this._type = type;
            this.gameObject.transform.position = cell.WorldPos;
            this.gameObject.transform.SetParent(side.SettlementContainer.transform);

            side.AddSettlement(this);
            cell.SetSettlement(this);

            var cellType = type.CellType;
            Tile tile = cellType.GroundVariants[0];
            Tile tile_objs = cellType.ObjectVariants[0];
            _tile = tile;
            _tile_obj = tile_objs;

            MG_GlobalHexMap.Instance.UpdateCell(cell, cellType, tile, tile_objs, TileOrigin.settlement);
            _highlighter.Init(side);
            _cellsUnderControlWithDistance = new Dictionary<int, HashSet<MG_HexCell>>();
            //-------CellsUnderControl = _lineOfSight.UpdateFOV(_cell, Vision, CellsUnderControl, this);
            InitTerritory();
            //GrowTerritory(33);
            GrowTerritory(6);

            SetVisibility(cell.CurrentVisibility);
            //Debug.Log("_cellsUnderControl count=" + _cellsUnderControl.Count + " _currentRegionDistance=" + _currentRegionDistance);
        }

        /// <summary>
        /// Инициализировать все необходимые менеджеры
        /// </summary>
        private void FindAllNeededManagers()
        {
            _lineOfSight = MG_LineOfSight.Instance;
            _borderManager = MG_HexBorderManager.Instance;

            if (_lineOfSight == null) Debug.Log("<color=red>MG_Settlement Init(): _lineOfSight не найден!</color>");
            if (_borderManager == null) Debug.Log("<color=red>MG_Settlement Init(): _borderManager не найден!</color>");
            if (_highlighter == null) Debug.Log("<color=red>MG_Settlement Init(): префаб '_highlighter' не найден!</color>");//[R DEV PREFAB] Подсветка юнита
        }
        #endregion Метод INIT

        #region Методы UNITY
        public void OnDestroy()
        {
            Destroy(this.gameObject);
            Destroy(_highlighter.gameObject);
        }
        #endregion Методы UNITY

        #region Публичные методы
        /// <summary>
        /// при заканчивании уровня
        /// </summary>
        public void OnTurnEnd()
        {
            //GrowTerritory(1);
            //throw new System.NotImplementedException();
        }

        /// <summary>
        /// При отмене выделения
        /// </summary>
        public void OnDeselect()
        {
            _highlighter.OnDeselect();
        }

        /// <summary>
        /// При выделении
        /// </summary>
        /// <param name="isClicked"></param>
        public void OnSelect(bool isClicked)
        {
            //if (isClicked)
            //{
            //    _highlighter.OnSelect(true);
            //}
            //else
            //{
            //    _highlighter.OnSelect(false);
            //}
        }

        /// <summary>
        /// Сменить сторону
        /// </summary>
        /// <param name="newSide">новая сторона</param>
        public void SwitchSide(MG_Player newSide)
        {
            //Debug.Log("newSide= " + newSide.name);
            var tempList = _cellsUnderControl.ToHashSet();//Временный лист необходим из-за того что _cellsUnderControl будет пустым из-за RemoveStaticFOV()!
                                                          //Debug.Log("BEFORE: _cellsUnderControl= " + _cellsUnderControl.Count);
            _lineOfSight.RemoveStaticFOV(_cellsUnderControl, this);
            //Debug.Log("AFTER: _cellsUnderControl= " + _cellsUnderControl.Count);    
            _side.RemoveSettlement(this);
            _side = newSide;
            newSide.AddSettlement(this);
            _cellsUnderControl = tempList;
            _lineOfSight.UpdateStaticFOV(_cellsUnderControl, this);
            _highlighter.ChangeSide(newSide);
            UpdateBordersOwner(newSide);
            //gameObject.transform.SetParent(newSide.SettlementContainer.transform);
            //Debug.Log("FINAL: _cellsUnderControl= " + _cellsUnderControl.Count);
            foreach (var cell in CellsUnderControl)
            {
                cell.BorderData.SetOwner(this);
                //Debug.Log("Changing owner: Cell= " + cell.Pos + "  newSide=" + newSide.name);
            }

        }

        /// <summary>
        /// Увеличить территорию на заданное кол-во клеток
        /// </summary>
        /// <param name="amount"></param>
        public void GrowTerritory(int amount)
        {
            //_currentRegionDistance
            //_notFilledRegionDistance
            if (amount <= 0)
                return;

            int emergencyCount = 0;
            while (amount > 0)
            {
                HashSet<MG_HexCell> BaseCellList;//лист из справочника
                if (!_cellsUnderControlWithDistance.TryGetValue(_notFilledRegionDistance, out BaseCellList))//достаем лист по минимальной дистанции    
                {
                    Debug.Log("<color=red>MG_Settlement GrowTerritory(int amount): Ошибка!</color> Не найден лист с которого необходимо начать поиск для расширения");
                    return;
                }

                HashSet<MG_HexCell> freeCells = GetNewCellsTerritory(BaseCellList, ref amount);// Получить свободные клетки территории

                if (freeCells.Any())
                {
                    //--BEGIN добавить новые клетки в нужный лист по дистанции
                    int nextDistance = _notFilledRegionDistance + 1;
                    if (!_cellsUnderControlWithDistance.TryGetValue(nextDistance, out HashSet<MG_HexCell> existingCells))//достаем лист по текущей дистанции    
                    {
                        //_notFilledRegionDistance++;
                        if (_currentRegionDistance < _notFilledRegionDistance)
                            _currentRegionDistance = _notFilledRegionDistance;
                        CreateNewDistanceTerritoryData(nextDistance, freeCells);//  Добавить территорию к новому списку новой дистанции      
                    }
                    else
                    {
                        AddCellsToExistingDistanceTerritoryData(freeCells, existingCells);// Добавить территорию к существующему списку заданной дистанции                                                                              //Debug.Log("GrowTerritory: " + distance + "| Added to old territory list cell.pos= " + cell.Pos);
                    }
                    //--END добавить новые клетки в нужный лист по дистанции

                    CreateBordersOnCells(freeCells);// Установить видимые границы
                    SubscribeToEraseAction(freeCells);// Подписаться к Erase у клеток borderData
                }
                else
                {
                    //Нет свободных клеток для расширения.
                    if (_cellsUnderControlWithDistance.Count > _notFilledRegionDistance)
                    {
                        //В справочнике больше ключей с дистанциями, чем мы проверили у клеток на расстоянии '_notFilledRegionDistance'
                        _notFilledRegionDistance++;
                    }
                    else
                    {
                        Debug.Log("Нет доступных клеток для расширения территории.");
                        amount = 0;
                    }
                }

                emergencyCount++;
                if (emergencyCount == 10000)
                {
                    Debug.Log("<color=red>MG_Settlement GrowTerritory(int amount): Ошибка!</color> INFINITIVE LOOP WHILE DO");
                    return;
                }
            }
            Debug.Log("Расширение территории завершено!");
        }

        /// <summary>
        /// Увеличить территорию на одну заданную клетку
        /// </summary>
        /// <param name="cell"></param>
        public void GrowTerritory(MG_HexCell cell)
        {
            int distance = MG_GeometryFuncs.Cube_distance(cell.HexPos, _cell.HexPos);

            if (_currentRegionDistance < distance)
                _currentRegionDistance = distance;//текущая дистанция от поселения до самой далекой клетки

            HashSet<MG_HexCell> freeCells = new HashSet<MG_HexCell>();
            freeCells.Add(cell);//новый лист

            HashSet<MG_HexCell> cellList;//лист из справочника
            if (!_cellsUnderControlWithDistance.TryGetValue(distance, out cellList))//достаем лист по текущей дистанции    
            {
                CreateNewDistanceTerritoryData(distance, freeCells);// Добавить клетки в справочник территорий
                                                                    //Debug.Log("GrowTerritory: " + distance + "| Added to new territory list cell.pos= " + cell.Pos);
            }
            else
            {
                AddCellsToExistingDistanceTerritoryData(freeCells, cellList);// Добавить территорию в к существующей
                                                                             //Debug.Log("GrowTerritory: " + distance + "| Added to old territory list cell.pos= " + cell.Pos);
            }
            CreateBordersOnCells(freeCells);// Установить видимые границы
            SubscribeToEraseAction(freeCells);// Подписаться к Erase у клеток borderData
        }


        #endregion Публичные методы

        #region Личные методы
        /// <summary>
        /// Обновить владельца территории
        /// </summary>
        /// <param name="side"></param>
        private void UpdateBordersOwner(MG_Player side)
        {
            foreach (var cell in _cellsUnderControl)
            {
                //_cell.BorderData.UpdateOwnership(_side);
                _borderManager.Place(cell, this, true);
            }
        }

        /// <summary>
        /// Получить новые клетки территории
        /// </summary>
        /// <param name="ownedCells"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        private HashSet<MG_HexCell> GetNewCellsTerritory(HashSet<MG_HexCell> ownedCells, ref int amount)
        {
            MG_Player emptySide = MG_PlayerManager.GetEmptySide();
            bool keepWorking = true;
            HashSet<MG_HexCell> newOwnedCells = new HashSet<MG_HexCell>();
            foreach (var cellM in ownedCells)
            {
                foreach (var cellN in cellM.GetNeighbours())
                {
                    var borderDataN = cellN.BorderData;
                    var ownerN = borderDataN.Owner;

                    if (ownerN.EqualsUID(emptySide))
                    {
                        borderDataN.SetOwner(this);
                        borderDataN.Erase += RemoveVisibleCell;//Подписываемся на удаления
                        newOwnedCells.Add(cellN);

                        amount--;
                        //Debug.Log("inside: amount=" + amount);
                        if (amount <= 0)
                        {
                            keepWorking = false;
                            return newOwnedCells;
                        }

                    }
                }

                if (!keepWorking)
                    return newOwnedCells;
            }
            return newOwnedCells;
        }

        /// <summary>
        /// Установить видимые границы
        /// </summary>
        /// <param name="cells"></param>
        private void CreateBordersOnCells(HashSet<MG_HexCell> cells)
        {
            foreach (var cell in cells)
            {
                _borderManager.Place(cell, this, true);
            }
        }



        /// <summary>
        /// Добавить клетки в справочник территорий
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="cells"></param>
        private void CreateNewDistanceTerritoryData(int distance, HashSet<MG_HexCell> cells)
        {
            _cellsUnderControlWithDistance.Add(distance, cells);
            _lineOfSight.UpdateStaticFOV(cells, this);
            //foreach (var cell in cells)
            //{
            //    AddVisibleCell(cell);
            //}


        }

        /// <summary>
        ///  Добавить территорию в к существующей территории
        /// </summary>
        /// <param name="newCells"></param>
        /// <param name="cells"></param>
        private void AddCellsToExistingDistanceTerritoryData(HashSet<MG_HexCell> newCells, HashSet<MG_HexCell> cells)
        {
            cells.AddRange(newCells);
            _lineOfSight.UpdateStaticFOV(newCells, this);
            //foreach (var cell in newCells)
            //{
            //    AddVisibleCell(cell);
            //}
        }

        /// <summary>
        /// Инициализировать территорию
        /// </summary>
        private void InitTerritory()
        {

            if (_cell.HasOwner())
            {
                _borderManager.Erase(_cell);//Удаляем владельца если имеется, так как начальная клетка должна принадлежать новому поселению.
            }

            //BEGIN создаем первый элемент - где само поселение находиться
            HashSet<MG_HexCell> cells = new HashSet<MG_HexCell>() { _cell };//создаем лист с начальной клеткой
            CreateNewDistanceTerritoryData(0, cells);// Добавить клетки в справочник территорий
            CreateBordersOnCells(cells);// Установить видимые границы
                                        //END создаем первый элемент - где само поселение находиться
        }

        /// <summary>
        /// Удалить клетку из словарика с дистанцией
        /// </summary>
        /// <param name="cell"></param>
        private void RemoveFromDistanceDictionary(MG_HexCell cell)
        {
            if (_cellsUnderControlWithDistance.Any())
            {
                foreach (var item in _cellsUnderControlWithDistance)
                {
                    var list = item.Value;
                    if (list.Any())
                    {
                        foreach (var cellN in list.ToList())
                        {
                            if (cell.Equals(cellN))
                            {
                                list.Remove(cell);
                                return;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Удалить территорию на одну заданную клетку
        /// </summary>
        /// <param name="cell"></param>
        private void RemoveTerritory(MG_HexCell cell)
        {
            RemoveFromDistanceDictionary(cell);
            cell.RemoveObserver(this);
        }

        /// <summary>
        /// Подписаться к Erase у клеток borderData
        /// </summary>
        /// <param name="cells"></param>
        private void SubscribeToEraseAction(HashSet<MG_HexCell> cells)
        {
            foreach (var cell in cells)
            {
                var borderData = cell.BorderData;
                borderData.Erase += RemoveTerritory;
            }
        }

        #endregion Личные методы

        #region МЕТОДЫ ИНТЕРФЕЙСА "ICellObserver"
        /// <summary>
        /// Добавить клетку в список видимых клеток (ICellObserver)
        /// </summary>
        /// <param name="cell">клетка</param>
        public void AddVisibleCell(MG_HexCell cell)
        {
            _cellsUnderControl.Add(cell);
        }

        /// <summary>
        /// Убрать клетку из списка видимых клеток (ICellObserver)
        /// </summary>
        /// <param name="cell">клетка</param>
        public void RemoveVisibleCell(MG_HexCell cell)
        {
            _cellsUnderControl.Remove(cell);
        }

        /// <summary>
        /// Получить владельца (ICellObserver)
        /// </summary>
        /// <returns>владелец</returns>
        public MG_Player GetSide()
        {
            return Side;
        }

        /// <summary>
        /// Обновить видимость
        /// </summary>
        public void RecalculateVision()
        {
            //Не требуется, так как статическое видение
        }


        /// <summary>
        /// получить все видимые клетки
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<MG_HexCell> GetVisibleCells()
        {
            return _cellsUnderControl;
        }
        #endregion МЕТОДЫ ИНТЕРФЕЙСА "ICellObserver"

        #region МЕТОДЫ ИНТЕРФЕЙСА "IVisibility"
        /// <summary>
        /// Установить видимость (IVisibility)
        /// </summary>
        /// <param name="visibility">видимость</param>
        public void SetVisibility(Visibility visibility)
        {
            switch (visibility)
            {
                case Visibility.BlackFog:
                    Cell.Map.TileMap.SetTile(_cell.Pos, MG_FogOfWar.Instance.FogTile);//устанавливаем тайл
                    Cell.Map.TileMap_objects.SetTile(_cell.Pos, MG_FogOfWar.Instance.FogTile);//устанавливаем тайл
                    Cell.Map.TileMap.SetTileFlags(_cell.Pos, TileFlags.None);//сбрасываем флаги тайла
                    Cell.Map.TileMap_objects.SetTileFlags(_cell.Pos, TileFlags.None);//сбрасываем флаги тайла
                    break;
                case Visibility.GreyFog:
                    Cell.Map.TileMap.SetTile(_cell.Pos, _tile);//устанавливаем тайл
                    Cell.Map.TileMap_objects.SetTile(_cell.Pos, _tile_obj);//устанавливаем тайл
                    Cell.Map.TileMap.SetTileFlags(_cell.Pos, TileFlags.None);//сбрасываем флаги тайла
                    Cell.Map.TileMap_objects.SetTileFlags(_cell.Pos, TileFlags.None);//сбрасываем флаги тайла
                    Cell.Mark(_lineOfSight.GreyFogOfWar);
                    break;
                case Visibility.Visible:

                    Cell.Map.TileMap.SetTile(_cell.Pos, _tile);//устанавливаем тайл
                    Cell.Map.TileMap_objects.SetTile(_cell.Pos, _tile_obj);//устанавливаем тайл
                    Cell.Map.TileMap.SetTileFlags(_cell.Pos, TileFlags.None);//сбрасываем флаги тайла
                    Cell.Map.TileMap_objects.SetTileFlags(_cell.Pos, TileFlags.None);//сбрасываем флаги тайла
                    break;
                default:
                    Debug.Log("<color=orange>MG_Settlement SetVisibility(): switch DEFAULT.</color>");
                    break;
            }
            _highlighter.SetVisibility(visibility);
        }
        #endregion МЕТОДЫ ИНТЕРФЕЙСА "IVisibility"

        #region TEST
        public int test_growTerritoryPower = 1;
        [Button]
        private void GrowTerritory()
        {
            //foreach (var item in _cellsUnderControlWithDistance)
            //{
            //    Debug.Log(item.Key + " = " + item.Value.Count);
            //}
            GrowTerritory(test_growTerritoryPower);
        }

        #endregion TEST
    }
}