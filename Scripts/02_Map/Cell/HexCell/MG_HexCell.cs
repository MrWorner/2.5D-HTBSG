using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
    public class MG_HexCell : MG_BasicCell<MG_GlobalHexMap, MG_HexCellType>, ICellNeighbours<MG_HexCell, DirectionHexPT>, IMarkable, IObserverHandler, IVisibility, IEquatableUID<MG_HexCell>//IEquatable<MG_BasicCell>
    {
        private uint uid;//УНИКАЛЬНЫЙ ID ОБЪЕКТА (для IEquatableUID)
        private static uint uidCounter = 0;//Счетчик уидов
        private static readonly Vector3Int[] _cube_directions = //https://www.redblobgames.com/grids/hexagons/
         {
        new Vector3Int(+1, -1, 0),
        new Vector3Int(+1, 0, -1),
        new Vector3Int(0, +1, -1),
        new Vector3Int(-1, +1, 0),
        new Vector3Int(-1, 0, +1),
        new Vector3Int(0, -1, +1)
    };

        #region Особые Поля и Свойства HexCell
        private readonly bool _isEvenRow;//нечетный ли ряд (нужен для HEXAGON PointTopped)// НА ЗАМЕТКУ: а у Float Topped не ряд, а столбец
        private Vector3Int _hexPos;//хексовая координата
        private Tile _tile_Object;//текущий используемый вариант тайла из всех доступных вариантов тайлов cellType (LEVEL 2: OBJECTS)
        private TileOrigin _tileOrigin;//откуда взят спрайт тайла


        public bool IsEvenRow { get => _isEvenRow; }//нечетный ли ряд (нужен для HEXAGON PointTopped)// НА ЗАМЕТКУ: а у Float Topped не ряд, а столбец
        public Vector3Int HexPos { get => _hexPos; }//хексовая координата (особый формат!)
        public Tile Tile_Object { get => _tile_Object; }//используемый тайл из всех доступных вариантов тайлов cellType (LEVEL 2: OBJECTS)
        public TileOrigin TileOrigin { get => _tileOrigin; }//откуда взят спрайт тайла
        #endregion Особые Поля и Свойства HexCell

        #region Поля и свойства для хранение объектов (юнитов, построек)
        private MG_Division _division;//текущий юнит на клетке
        private MG_Settlement _settlement;//текущая постройка на клетке
        private MG_Resource _resource;//ресурс на клетке
        public MG_Division Division { get => _division; }
        public MG_Settlement Settlement { get => _settlement; }
        public MG_Resource Resource { get => _resource; }
        #endregion Поля и свойства для хранение объектов (юнитов, построек)

        #region Поля и Свойства для взаимодействия с разными модулями
        //--ДЛЯ МОДУЛЯ: Видимости
        private MG_LineOfSight _lineOfSight;//[R CONSTRUCTOR] модуль зоны видимости
        private Dictionary<MG_Player, HashSet<ICellObserver>> _visionSubscribers = new Dictionary<MG_Player, HashSet<ICellObserver>>();//Словарик, который содержит ключ - Игрок/сторона и значение - Лист, который содержит юниты/постройки данной стороны, которые видят эту клетку. Нужен для того, чтобы определять накладывать ли туман для определенного игрока.
                                                                                                                                       //public Dictionary<MG_Player, HashSet<ICellObserver>> VisionSubscribers { get => _visionSubscribers; }//НЕОБХОДИМО УБРАТЬ!

        private HashSet<MG_Player> _discoveredForSide = new HashSet<MG_Player>();// стороны, которые посещали данную клетку
        private HashSet<MG_Player> _visibleForSide = new HashSet<MG_Player>();// стороны, которые имеют прямую видимость к данной клетке

        private Visibility _currentVisibility = Visibility.Visible;//Текущая видимость локальная!
        public Visibility CurrentVisibility { get => _currentVisibility; }//Текущая видимость

        //---ДЛЯ МОДУЛЯ: Территорий
        private MG_HexBorderData _borderData;//[R CONSTRUCTOR] границы
        public MG_HexBorderData BorderData { get => _borderData; }//[R INIT] границы

        //---ДЛЯ МОДУЛЯ: Дороги
        private MG_HexRoadData _roadData;//[R CONSTRUCTOR] дорога
        public MG_HexRoadData RoadData { get => _roadData; }

        //---ДЛЯ МОДУЛЯ: Реки
        private MG_HexRiverData _riverData;//[R CONSTRUCTOR] река
        public MG_HexRiverData RiverData { get => _riverData; }

        //---ДЛЯ МОДУЛЯ: Реки (Продвинутая версия)
        private MG_HexAdvancedRiverData _advancedRiverData;//[R CONSTRUCTOR] река
        public MG_HexAdvancedRiverData AdvancedRiverData { get => _advancedRiverData; }

        //---ДЛЯ МОДУЛЯ: Pathfinding
        private bool _isTaken = false;//Занята ли клетка кем то
        private Dictionary<DirectionHexPT, MG_HexCell> _neighbours = new Dictionary<DirectionHexPT, MG_HexCell>();//соседи по определенным сторонам
        public float _temp_movementCost { get; set; } = 0;//временное хранилище для хранения стоимости передвижения, ЧТОБЫ НЕ ПЕРЕСЧИТЫВАТЬ ДВАЖДЫ-ТРИЖДЫ  

        public bool IsTaken { get => _isTaken; }//занята ли клетка   
        #endregion Поля и Свойства для взаимодействия с разными модулями

        #region Конструктор
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="isOddRow">Четный ли ряд</param>
        /// <param name="cellType">Тип клетки</param>
        /// <param name="tile">Используемый вариант тайла</param>
        /// <param name="tile_Object">Используемый вариант тайла для слоя с объектом</param>
        /// <param name="pos">Позиция</param>
        /// <param name="worldPos">Глобальная позиция</param>
        /// <param name="map">Принадлежность к карте</param>
        public MG_HexCell(MG_HexCellType cellType, Tile tile, Tile tile_Object, Vector3Int pos, Vector3 worldPos, MG_GlobalHexMap map, bool isOddRow) : base(cellType, tile, pos, worldPos, map)
        {
            GenerateUID();//Сгенерировать УИД
            this._tile_Object = tile_Object;
            this._isEvenRow = isOddRow;//принадлежность к карте       
            this._hexPos = MG_GeometryFuncs.Oddr_to_cube(pos);//конвертируем координаты в Хекс координаты
            this._borderData = new MG_HexBorderData(this);
            this._roadData = new MG_HexRoadData(this);
            this._riverData = new MG_HexRiverData(this);
            this._advancedRiverData = new MG_HexAdvancedRiverData(this);
            //Debug.Log("hexPos= " + pos + " converted again: " + Cube_to_oddr(pos) + " normal: " + pos);

            _lineOfSight = MG_LineOfSight.Instance;//Получаем модуль зоны видимости
        }
        #endregion

        #region Статические методы MG_HexCell
        /// <summary>
        /// конвертируем Хекс координаты в локальные координаты карты https://www.redblobgames.com/grids/hexagons/
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>координаты классические</returns>
        public static Vector3Int Cube_to_oddr(Vector3Int pos)
        {
            var col = pos.x + (pos.z - (pos.z & 1)) / 2;
            var row = pos.z;

            return new Vector3Int(col, -row, 0);
            //return new Vector3Int(col, row, 0);
        }

        /// <summary>
        /// Увеличить/уменьшить размер координата хекса //https://www.redblobgames.com/grids/hexagons/
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="scale"></param>
        /// <returns>координаты измененные в x раз</returns>
        public static Vector3Int Cube_scale(Vector3Int pos, int scale)
        {
            return new Vector3Int(pos.x * scale, pos.y * scale, pos.z * scale);
        }

        /// <summary>
        /// Прибавить две координаты хекса //https://www.redblobgames.com/grids/hexagons/
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <returns>сумма двух координат</returns>
        public static Vector3Int Cube_add(Vector3Int pos1, Vector3Int pos2)
        {
            return (pos1 + pos2);
        }

        /// <summary>
        /// Получить соседа //https://www.redblobgames.com/grids/hexagons/
        /// </summary>
        /// <param name="cube"></param>
        /// <param name="direction"></param>
        /// <returns>координаты соседа</returns>
        public static Vector3Int Cube_neighbor(Vector3Int cube, int direction)
        {
            return Cube_add(cube, Cube_direction(direction));
        }

        /// <summary>
        /// Получить направление //https://www.redblobgames.com/grids/hexagons/
        /// </summary>
        /// <param name="direction"></param>
        /// <returns>направление кубическое</returns>
        public static Vector3Int Cube_direction(int direction)
        {
            return _cube_directions[direction];
        }
        #endregion  Статические методы MG_HexCell

        #region Публичные методы
        /// <summary>
        /// Установить на клетке дивизию
        /// </summary>
        /// <param name="division"></param>
        public void SetDivision(MG_Division division)
        {
            if (division != null)
            {
                _isTaken = true;
                _division = division;
            }
            else
            {
                _isTaken = false;
                _division = null;
            }
        }

        /// <summary>
        /// Установить на клетке поселение
        /// </summary>
        /// <param name="settlement"></param>
        public void SetSettlement(MG_Settlement settlement) => _settlement = settlement;

        /// <summary>
        /// Установить на клетке ресурс
        /// </summary>
        /// <param name="resource"></param>
        public void SetResource(MG_Resource resource) => _resource = resource;

        /// <summary>
        /// Была ли клетка разведана игроком
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public bool IsDiscoveredByPlayer(MG_Player side)
        {
            return _discoveredForSide.Contains(side);
        }

        /// <summary>
        /// Удалить информацию о том что сторона знала что то 
        /// </summary>
        /// <param name="side"></param>
        public void ForgetDiscover(MG_Player side)
        {
            if (IsDiscoveredByPlayer(side))
            {
                _discoveredForSide.Remove(side);
            }
        }



        /// <summary>
        /// Была ли клетка разведана игроком
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public bool IsVisibleForPlayer(MG_Player side)
        {
            return _visibleForSide.Contains(side);
        }


        /// <summary>
        /// Больше нет видимости у игрока над клеткой
        /// </summary>
        /// <param name="side"></param>
        public void NotVisibleAnymoreFor(MG_Player side)
        {
            if (_visibleForSide.Contains(side))
                _visibleForSide.Remove(side);
            else
                Debug.Log("MG_HexCell NotVisibleAnymoreFor(): нет в списке! <color=red>(возможно ОШИБКА!)</color> _pos= " + _pos);
        }

        /// <summary>
        /// Обновить тип и тайл
        /// </summary>
        /// <param name="cellType"></param>
        /// <param name="tile">используемый тайл из всех доступных вариантов тайлов cellType (LEVEL 1: LAND)</param>
        /// <param name="tile_Object">используемый тайл из всех доступных вариантов тайлов cellType (LEVEL 2: OBJECTS)</param>
        /// <param name="_tileOrigin"></param>
        public void SetTypeAndTiles(MG_HexCellType cellType, Tile tile, Tile tile_Object, TileOrigin tileOrigin)
        {
            this._type = cellType;
            this._tile = tile;
            this._tile_Object = tile_Object;
            this._tileOrigin = tileOrigin;
        }
        #endregion Публичные методы

        #region Публичные методы (взаимодействия с разными модулями)
        /// <summary>
        /// Имеется ли дорога
        /// </summary>
        /// <returns></returns>
        public bool HasAnyRoad()
        {
            return (_roadData.RoadType.EqualsUID(MG_HexRoadLibrary.GetEmptyRoad()) == false);
        }

        /// <summary>
        /// Имеется ли река
        /// </summary>
        /// <returns></returns>
        public bool HasAnyRiver()
        {
            return (_riverData.RiverType.EqualsUID(MG_HexRiverLibrary.GetEmptyRiver()) == false);
        }

        /// <summary>
        /// Имеется ли река (Advanced)
        /// </summary>
        /// <returns></returns>
        public bool HasAnyAdvancedRiver()
        {
            if (_advancedRiverData.River_E) return true;
            if (_advancedRiverData.River_NE) return true;
            if (_advancedRiverData.River_NW) return true;
            if (_advancedRiverData.River_SE) return true;
            if (_advancedRiverData.River_SW) return true;
            if (_advancedRiverData.River_W) return true;

            return false;
        }

        /// <summary>
        /// Имеется ли река по направлению (Advanced)
        /// </summary>
        /// <returns></returns>
        public bool HasAnyAdvancedRiver(DirectionHexPT dir)
        {
            return _advancedRiverData.HasRiver(dir);
        }

        /// <summary>
        /// Получить направление где имеются реки (Advanced)
        /// </summary>
        /// <returns></returns>
        public List<DirectionHexPT> GetDirectionsOfAdvancedRiver()
        {
            List<DirectionHexPT> result = new List<DirectionHexPT>();

            if (_advancedRiverData.River_E) result.Add(DirectionHexPT.E);
            if (_advancedRiverData.River_NE) result.Add(DirectionHexPT.NE);
            if (_advancedRiverData.River_NW) result.Add(DirectionHexPT.NW);
            if (_advancedRiverData.River_SE) result.Add(DirectionHexPT.SE);
            if (_advancedRiverData.River_SW) result.Add(DirectionHexPT.SW);
            if (_advancedRiverData.River_W) result.Add(DirectionHexPT.W);

            return result;
        }

        /// <summary>
        /// Имеется ли ресурс
        /// </summary>
        /// <returns></returns>
        public bool HasResource()
        {
            return (_resource != null);

        }

        /// <summary>
        /// Имеет ли владельца
        /// </summary>
        /// <returns></returns>
        public bool HasOwner()
        {
            return BorderData.HasOwner();
        }
        #endregion Публичные методы (взаимодействия с разными модулями)

        #region Личные методы
        /// <summary>
        /// Сгенерировать UID
        /// </summary>
        private void GenerateUID()
        {
            uidCounter++;
            uid = MG_HexCell.uidCounter;
        }

        /// <summary>
        /// Разведано игроком
        /// </summary>
        /// <param name="side"></param>
        private void DiscoveredBy(MG_Player side)
        {
            if (_discoveredForSide.Contains(side) == false)
                _discoveredForSide.Add(side);
            else
                Debug.Log("<color=red>MG_HexCell DiscoveredBy(): уже в списке! (ERROR!)</color> _pos= " + _pos);
        }

        /// <summary>
        /// Видимость есть у игрока
        /// </summary>
        /// <param name="side"></param>
        private void VisibleBy(MG_Player side)
        {
            if (_visibleForSide.Contains(side) == false)
                _visibleForSide.Add(side);
            else
                Debug.Log("<color=red>MG_HexCell VisibleBy(): уже в списке! (ERROR!)</color> _pos= " + _pos);
        }
        #endregion

        #region МЕТОДЫ ИНТЕРФЕЙСА "ICellNeighbours"
        /// <summary>
        /// Есть ли сосед (ICellNeighbours)
        /// </summary>
        /// <param name="cell">Клетка</param>
        /// <returns></returns>
        public bool HasNeighbour(MG_HexCell cell)
        {
            return _neighbours.ContainsValue(cell);
        }

        /// <summary>
        /// Добавить соседа (ICellNeighbours)
        /// </summary>
        /// <param name="cell">клетка</param>
        /// <param name="dir">Направление соседа</param>
        public void AddNeighbour(MG_HexCell cell, DirectionHexPT dir)
        {
            if (!_neighbours.ContainsValue(cell))
                _neighbours.Add(dir, cell);
            else
                Debug.Log("<color=red>MG_HexCell SetNeighbour(): 'cellN' уже присутствует в справочнике cells!</color>");
        }

        /// <summary>
        /// Удалить соседа из списка (ICellNeighbours)
        /// </summary>
        /// <param name="cell">клетка</param>
        public void RemoveNeighbour(MG_HexCell cell)
        {
            if (_neighbours.ContainsValue(cell))
            {//https://stackoverflow.com/questions/1636885/remove-item-in-dictionary-based-on-value
                foreach (var item in _neighbours.Where(key => key.Value == cell).ToList())
                {
                    _neighbours.Remove(item.Key);
                }
            }
            else
                Debug.Log("<color=red>MG_HexCell RemoveNeighbour(): 'cellN' НЕ присутствует в справочнике cells!</color>");
        }

        /// <summary>
        /// Удалить соседа (ICellNeighbours)
        /// </summary>
        /// <param name="dir">Направление</param>
        public void RemoveNeighbour(DirectionHexPT dir)
        {
            if (_neighbours.ContainsKey(dir))
                _neighbours.Remove(dir);
            else
                Debug.Log("<color=red>MG_HexCell RemoveNeighbour(): dir=" + dir + " НЕ присутствует в справочнике cells!</color>");

            //var test = _neighbours.Values;
            //Debug.Log("MG_HexCell RemoveNeighbour(): curernt _cell= " + this.Pos + " searching dir=" + dir + " test= " + test);
        }

        /// <summary>
        /// Очистить все данные об этой клетки у соседях и у самой клетки (двухсторонняя связь)
        /// </summary>
        /// <param name="cell"></param>
        public void ClearAllNeighbours()
        {
            foreach (var neighbour in _neighbours.ToList())
            {
                MG_HexCell cellN = neighbour.Value;
                DirectionHexPT dir = neighbour.Key;//получаем направление соседа
                DirectionHexPT dirN = MG_Direction.ReverseDir(dir);//зеркалим направление

                RemoveNeighbour(dir);//убираем соседа
                cellN.RemoveNeighbour(dirN);//теперь сосед также убирает выбранную клетку
            }
        }

        /// <summary>
        /// Получить список всех соседов (ICellNeighbours)
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<MG_HexCell> GetNeighbours()
        {
            return _neighbours.Values.ToList();
        }

        /// <summary>
        /// получить словарик, который содержит соседей по направлениям (ICellNeighbours)
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<DirectionHexPT, MG_HexCell> GetNeighboursWithDirection()
        {
            return _neighbours;
        }

        /// <summary>
        /// Получить соседа по направлению (ICellNeighbours)
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public MG_HexCell GetNeighbourByDirection(DirectionHexPT dir)
        {
            MG_HexCell cell;
            _neighbours.TryGetValue(dir, out cell);
            return cell;
        }

        /// <summary>
        /// Получить направление по соседу (ICellNeighbours)
        /// </summary>
        /// <param name="cellN"></param>
        /// <returns></returns>
        public DirectionHexPT GetDirectionOfNeighbour(MG_HexCell cellN)
        {
            DirectionHexPT dir = _neighbours.FirstOrDefault(x => x.Value == cellN).Key; ;
            return dir;
        }
        #endregion

        #region МЕТОДЫ ИНТЕРФЕЙСА "IMarkable"
        /// <summary>
        /// пометить клетку цветом (IMarkable)
        /// </summary>
        /// <param name="color">Цвет</param>
        public void Mark(Color color)
        {
            //Map.TileMap.UpdateColor(Pos, color);
            Map.ChangeCellColor(_pos, color);
        }

        /// <summary>
        /// Сбросить выделение (IMarkable)
        /// </summary>
        public void UnMark()
        {
            //Map.TileMap.UpdateColor(Pos, new Color(1, 1, 1, 1));
            Map.ChangeCellColor(_pos, new Color(1, 1, 1, 1));
        }
        #endregion

        #region МЕТОДЫ ИНТЕРФЕЙСА "IObserverManager"
        /// <summary>
        /// Добавить Наблюдателя (IObserver)
        /// </summary>
        /// <param name="side"></param>
        /// <param name="observer"></param>
        public void AddObserver(ICellObserver observer)
        {
            MG_Player side = observer.GetSide();

            if (HasObserver(observer))
                return;

            if (!HasAnyObserverBySide(side))//Если не видима никем из своих
            {
                if (IsDiscoveredByPlayer(side) == false)//если клетка никогда не была до этого разведана
                {
                    DiscoveredBy(side);
                    side.SaveDiscoveredCell(this);
                }
                if (IsVisibleForPlayer(side) == false)
                    VisibleBy(side);//запоминаем что данную клетку видет заданная сторона
            }

            AddToObserversDictionary(observer);
            observer.AddVisibleCell(this);

            if (MG_TurnManager.GetCurrentPlayerVisibleMap().Equals(side))//Если сторона юнита - текущий игрок, то необходимо сделать клетку видимой
                SetVisibility(Visibility.Visible);
        }

        /// <summary>
        /// Имеет ли заданного наблюдателя (IObserverManager)
        /// </summary>
        /// <param name="side"></param>
        /// <param name="observer"></param>
        /// <returns></returns>
        public bool HasObserver(ICellObserver observer)
        {
            var side = observer.GetSide();
            if (_visionSubscribers.TryGetValue(side, out HashSet<ICellObserver> units))
            {
                return units.Contains(observer);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Удалить наблюдателя (IObserverManager)
        /// </summary>
        /// <param name="side"></param>
        /// <param name="observer"></param>
        public void RemoveObserver(ICellObserver observer)
        {
            var side = observer.GetSide();
            bool found = _visionSubscribers.TryGetValue(side, out HashSet<ICellObserver> units);

            if (found)
            {
                units.Remove(observer);
                observer.RemoveVisibleCell(this);

                if (units.Any() == false)
                {
                    //если не осталось никаких юнитов, которые видят данную клетку               
                    NotVisibleAnymoreFor(side);
                    if (MG_TurnManager.GetCurrentPlayerVisibleMap().Equals(side))//Если сторона юнита - текущий игрок
                        SetVisibility(Visibility.GreyFog);//если нет других юнитов, которые бы видели данную клетку, то перекрашиваем в туман войны
                }
            }
            else
                Debug.Log("<color=red>MG_HexCell RemoveObserver(): не найден! (WARNING!)</color> _pos= " + _pos);
        }

        /// <summary>
        /// Имеет ли какого нибудь наблюдателя (IObserverManager)
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public bool HasAnyObserverBySide(MG_Player side)
        {
            if (_visionSubscribers.TryGetValue(side, out HashSet<ICellObserver> observers))
            {
                return observers.Any();
            }
            else
            {
                return false;//если такая сторона не числиться вообще в справочнике (никогда не посещала данную клетку)
            }
        }

        /// <summary>
        /// Переформировать видимость для наблюдателей (IObserverManager).
        /// </summary>
        public void RecalculateObserversVisions()
        {
            if (_visionSubscribers.Any())
            {
                foreach (var pair in _visionSubscribers)
                {
                    HashSet<ICellObserver> observers = pair.Value;
                    if (observers.Any())
                    {
                        foreach (var observer in observers)
                        {
                            observer.RecalculateVision();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Добавить в справочник Наблюдателей
        /// </summary>
        /// <param name="observer"></param>
        private void AddToObserversDictionary(ICellObserver observer)
        {
            MG_Player side = observer.GetSide();
            if (_visionSubscribers.TryGetValue(side, out HashSet<ICellObserver> observers))
            {
                observers.Add(observer);
            }
            else
            {
                //если не найдена сторона (ключ) в справочнике, то добавляем + лист с клеткой
                observers = new HashSet<ICellObserver>();
                observers.Add(observer);
                _visionSubscribers.Add(side, observers);
            }
        }
        #endregion МЕТОДЫ ИНТЕРФЕЙСА "IObserverManager"

        #region МЕТОДЫ ИНТЕРФЕЙСА "IVisibility"
        /// <summary>
        /// Установить видимость (IVisibility)
        /// </summary>
        /// <param name="visibility">видимость</param>
        public void SetVisibility(Visibility visibility)
        {
            if (Division != null) Division.SetVisibility(visibility);
            if (Settlement != null) Settlement.SetVisibility(visibility);
            _borderData.SetVisibility(visibility);
            _roadData.SetVisibility(visibility);
            _riverData.SetVisibility(visibility);
            _advancedRiverData.SetVisibility(visibility);

            switch (visibility)
            {
                case Visibility.BlackFog:
                    Map.TileMap.SetTile(Pos, MG_FogOfWar.Instance.FogTile);//устанавливаем тайл
                    Map.TileMap_objects.SetTile(Pos, null);//устанавливаем тайл
                    Map.TileMap.SetTileFlags(Pos, TileFlags.None);//сбрасываем флаги тайла
                    Map.TileMap_objects.SetTileFlags(Pos, TileFlags.None);//сбрасываем флаги тайла
                    this._currentVisibility = visibility;
                    break;
                case Visibility.GreyFog:
                    Map.TileMap.SetTile(Pos, Tile);//устанавливаем тайл
                    Map.TileMap_objects.SetTile(Pos, Tile_Object);//устанавливаем тайл
                    Map.TileMap.SetTileFlags(Pos, TileFlags.None);//сбрасываем флаги тайла
                    Map.TileMap_objects.SetTileFlags(Pos, TileFlags.None);//сбрасываем флаги тайла
                    this.Mark(_lineOfSight.GreyFogOfWar);
                    this._currentVisibility = visibility;
                    break;
                case Visibility.Visible:
                    Map.TileMap.SetTile(Pos, Tile);//устанавливаем тайл
                    Map.TileMap_objects.SetTile(Pos, Tile_Object);//устанавливаем тайл
                    Map.TileMap.SetTileFlags(Pos, TileFlags.None);//сбрасываем флаги тайла
                    Map.TileMap_objects.SetTileFlags(Pos, TileFlags.None);//сбрасываем флаги тайла
                    UnMark();
                    this._currentVisibility = visibility;
                    break;
                default:
                    Debug.Log("<color=orange>MG_HexCell SetVisibility(): switch DEFAULT.</color>");
                    break;
            }
        }
        #endregion

        #region МЕТОДЫ ИНТЕРФЕЙСА "IEquatableUID"
        /// <summary>
        /// Сравнить УИДы
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EqualsUID(MG_HexCell other)
        {
            return uid == other.uid;
        }
        #endregion МЕТОДЫ ИНТЕРФЕЙСА "IEquatableUID"

    }


    [Serializable]
    public class JSON_HexCell
    {
        public Vector2Int pos;
        //public Vector3Int _hexPos;//хексовая координата

        public string type;// id Тип клетки 
        public int tile;// id тайла
        public int tile_Object;// id тайла объекта
        public TileOrigin tileOrigin;
    }
}
