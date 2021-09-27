using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_Division : MG_BasicUnit<MG_HexCell, MG_Division>, IMovable<MG_HexCell>, ICellObserver, IVisibility, IPathContainer<MG_HexCell>
{
    private static uint count = 0;

    #region Поля: необходимые модули
    [Required("НЕ ИНИЦИАЛИЗИРОВАН В Init()!", InfoMessageType.Error), SerializeField] protected MG_LineOfSight _lineOfSight;//[R INIT] модуль зоны видимости 
    #endregion Поля: необходимые модули

    #region Поля
    private Dictionary<MG_HexCell, List<MG_HexCell>> _cachedPaths = null;//сохраненный путь
    protected HashSet<MG_HexCell> _pathsInRange = new HashSet<MG_HexCell>();//путь до которого можно "дотянуться"
    protected HashSet<MG_HexCell> _blockedPathsInRange = new HashSet<MG_HexCell>();//заблокированные пути чем-то кем-то, до которых можно "дотянуться"
    protected List<MG_HexCell> _temp_currentPath = new List<MG_HexCell>();//текущий путь к цели
    [SerializeField] protected List<GroundType> _unwalkableGroundTypes = new List<GroundType>();
    [SerializeField] protected float _temp_pathCost = 0f;//очки строимость передвижения по найденному пути
    [SerializeField] protected bool _isPathTroughtBlackFog = false;//текущий путь через черный туман?
    [SerializeField] private int _vision = 3;//сколько клеток может видеть
    private HashSet<MG_HexCell> _visibleCells = new HashSet<MG_HexCell>();//какие клетки находяться в зоне видимости
    [SerializeField] protected float _movementPoints = 0f;//доступные очки
    //[ShowInInspector, ProgressBar(0, 100, 1f, 1f, 1f)] protected int movementPoints = 0;//доступные очки
    [SerializeField] protected float _movementPointsBase = 3f;//базовые очки передвижения
    [SerializeField] protected bool _isMoving = false;//передвигается на данный момент
    [SerializeField] protected bool _isAttacking = false;//в атаке?
    [SerializeField] protected bool _isScouting = false;//в разведке? НУЖЕН ЧТОБЫ КОГДА ПЕРЕДВИГАЛСЯ ПО ТУМАНУ И НАРВАЛСЯ НА ВРАЖЕСКУЮ АРМИЮ НЕ ШЕЛ ТУПО СРАЗУ В СРАЖЕНИЕ
    [SerializeField] protected MG_Division _opponentInFight = null;//с кем на данный момент ведет бой
    [SerializeField] protected Visibility _currentVisibility = Visibility.Visible;//Текущая видимость
    #endregion Поля

    #region Свойства
    public float MovementPoints { get => _movementPoints; set => _movementPoints = value; }//очки передвижения
    public float MovementPointsBase { get => _movementPointsBase; set => _movementPointsBase = value; }//базовые очки передвижения
    public float Temp_pathCost { get => _temp_pathCost; }//очки строимость передвижения по найденному пути
    public IReadOnlyList<MG_HexCell> Temp_currentPath { get => _temp_currentPath; }//текущий путь к цели
    public IReadOnlyCollection<MG_HexCell> PathsInRange { get => _pathsInRange; }//путь до которого можно "дотянуться"
    public IReadOnlyCollection<MG_HexCell> BlockedPathsInRange { get => _blockedPathsInRange; }//заблокированные пути чем-то кем-то, до которых можно "дотянуться"
    public bool IsPathTroughtBlackFog { get => _isPathTroughtBlackFog; }//текущий путь через черный туман?

    public IReadOnlyDictionary<MG_HexCell, List<MG_HexCell>> CachedPaths { get => _cachedPaths; }//сохраненный путь
    public IReadOnlyList<GroundType> UnwalkableGroundTypes { get => _unwalkableGroundTypes; }
    public int Vision { get => _vision; set => _vision = value; }//сколько клеток может видеть
    public IReadOnlyCollection<MG_HexCell> VisibleCells { get => _visibleCells; }//какие клетки находяться в зоне видимости

    public Visibility CurrentVisibility { get => _currentVisibility; }//Текущая видимость

    public bool IsMoving { get => _isMoving; }//передвигается на данный момент
    public bool IsScouting { get => _isScouting; set => _isScouting = value; }
    public bool IsAttacking { get => _isAttacking; set => _isAttacking = value; }
    #endregion Свойства

    #region Методы UNITY
    private void OnDestroy()
    {
        Destroy(this.gameObject);
        Destroy(_unitHighlighter.gameObject);
    }
    #endregion Методы UNITY

    #region Метод INIT
    public override void Init(MG_HexCell cell, MG_Player side)
    {
        count++;
        this.name = "Division " + count + " (" + side.name + ")";
        Moved += OnMoveFinished;//При завершении хода
        base.Init(cell, side);//Инициализация в родительском Init
        _unitHighlighter.Init(Side);//Инициализация подсветки
        SetPositionOnGlobalMap(cell.WorldPos);//Установить позицию GameObject на глобальной карте
        cell.SetDivision(this);//Установить дивизию, чтобы клетка считалась занятой
        Side.AddDivision(this);//Заявить о себе игровой стороне

        _lineOfSight = MG_LineOfSight.Instance;// модуль зоны видимости
        if (_lineOfSight == null) Debug.Log("<color=red>MG_Division Init(): объект '_lineOfSight' не найден!</color>");

        if (MG_VisibilityChecker.IsVisible(cell.CurrentVisibility) == false)
        {
            //SetVisibility(Visibility.Visible);//reset
            SetVisibility(cell.CurrentVisibility);
        }

        _visibleCells = _lineOfSight.UpdateFOV(Cell, Vision, VisibleCells, this);

        //if (!MG_TurnManager.IsMapVisibleForSide(_side))
        //    SetVisibility(Visibility.BlackFog);

        OnTurnStart();
    }
    #endregion Метод INIT

    #region Публичные перегружаемые методы
    /// <summary>
    /// Удалить из игры
    /// </summary>
    public override void RemoveFromGame()
    {
        Deleted?.Invoke(this);
        Cell.SetDivision(null);
        MG_PathAreaFixer.FindAndFixAfterDestroyedUnit(Side);// Починить зону доступности после уничтожение юнита
        _lineOfSight.RemoveVisionFromObserver(_visibleCells, this);
        Destroy(this.gameObject);
    }
    /// <summary>
    /// При начало хода
    /// </summary>
    public override void OnTurnStart()
    {
        MovementPoints = MovementPointsBase;
        CalculatePathArea();// Вычислить клетки до которым можно "дотянуться"
    }
    /// <summary>
    /// При завершении хода
    /// </summary>
    public override void OnTurnEnd()
    {

    }
    /// <summary>
    /// При выделении
    /// </summary>
    public override void OnSelect()
    {
        _unitHighlighter.OnSelect(true);
        //_pathColorManager.ShowRangeArea(_pathsInRange);
    }

    /// <summary>
    /// При выделении когда под мышкой
    /// </summary>
    public override void OnSelectUnderMouse()
    {
        _unitHighlighter.OnSelect(false);
    }

    /// <summary>
    /// При отмена выделении
    /// </summary>
    public override void OnDeselect()
    {
        //_pathColorManager.HidePatch(_pathsInRange, _temp_currentPath);//прячем путь
        _temp_currentPath = new List<MG_HexCell>();//<-------ЧТОБЫ ПРИ КЛИКЕ НА ТОГО ЖЕ ЮНИТА НЕ ПОКАЗЫВАЛ ПОСЛЕДНИЙ ПУТЬ (он почти всегда равен одной клетки). Если сделать currentPath.Erase(), то получим баг, сотруться данные как добраться на данную клетку!
        _unitHighlighter.OnDeselect();
    }

    /// <summary>
    /// При отмена выделении когда был под мышкой
    /// </summary>
    public override void OnDeselectUnderMouse()
    {
        _unitHighlighter.OnDeselect();
    }

    /// <summary>
    /// При завершении движения
    /// </summary>
    public override void OnMoveFinished()
    {

        //Debug.Log("IsAttacking= " + IsAttacking);
        if (IsAttacking)
        {
            if (MovementPoints > 0)
            {
                WN_BattleScreen.Instance.ShowWindow(this, _opponentInFight);
                MovementPoints -= 1;
                Debug.Log(this.name + "is now in battle with an enemy: " + _opponentInFight.name);
            }
            //else
            //{
            //    Debug.Log(this.name + ": Not enough movement points for attack on an enemy: " + _opponentInFight.name);
            //}
           
        }
        MovementPoints = (float)Math.Floor(MovementPoints);
        CalculatePathArea();// Вычислить клетки до которым можно "дотянуться"

    }
    #endregion Публичные перегружаемые методы

    #region Личные методы
    /// <summary>
    /// Установить позицию GameObject на глобальной карте
    /// </summary>
    /// <param name="position"></param>
    private void SetPositionOnGlobalMap(Vector3 position)
    {
        this.gameObject.transform.position = position;
    }

    private IEnumerator StartMoving(List<MG_HexCell> path)
    {
        var pathTemp = path.ToList();

        _isMoving = true;
        Cell.SetDivision(null);
        Cell = pathTemp.Last();
        Cell.SetDivision(this);

        foreach (var cell in pathTemp)
        {
            Vector3 destination_pos = new Vector3(cell.WorldPos.x, cell.WorldPos.y, transform.localPosition.z);
            while (transform.localPosition != destination_pos)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, destination_pos, Time.deltaTime * 20);
                yield return 0;
            }

            _visibleCells = _lineOfSight.UpdateFOV(cell, Vision, VisibleCells.ToHashSet(), this);
        }

        MG_Settlement settlement = Cell.Settlement;
        if (settlement != null)
        {
            if (!settlement.Side.Equals(_side))
            {
                settlement.SwitchSide(_side);//завоевываем!
            }
        }

        MG_TurnManager.EnableTurnButton();
        _isMoving = false;
        Moved?.Invoke();
    }

    /// <summary>
    /// Вычислить стоимость пути
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private float CalculateMovementCost(List<MG_HexCell> path)
    {
        _temp_pathCost = 0;
        if (path.Any())
            foreach (var cell in path)
                _temp_pathCost = _temp_pathCost + cell._temp_movementCost;

        return _temp_pathCost;
    }

    #endregion Личные методы

    #region МЕТОДЫ ИНТЕРФЕЙСА "IMovable"
    /// <summary>
    /// Начать движение
    /// </summary>
    public void Move()
    {
        if (MovementPoints <= 0)
            return;

        MG_TurnManager.DisableTurnButton();

        bool isTryingToGoTrouhgEnemy = false;
        MG_HexCell previousCell = _cell;

        _temp_currentPath.Reverse();
        List<MG_HexCell> reachablePath = new List<MG_HexCell>();
        foreach (var cell in _temp_currentPath)
        {
            MG_HexCellType cellType = cell.Type;
            GroundType grountType = cellType.GroundType;        

            bool hasRoad = cell.HasAnyRoad();

            bool isUnwalkable = _unwalkableGroundTypes.Contains(grountType);
            if (isUnwalkable && hasRoad == false)
                break;

            bool hasRiver = cell.HasAnyRiver();

            bool hasRoad_currentCell = previousCell.HasAnyRoad();
            bool hasBridge = hasRoad_currentCell && hasRoad;
            bool hasAdvancedRiver = false;
            if (hasBridge == false)
            {
                DirectionHexPT directionOfNeighbourCell = previousCell.GetDirectionOfNeighbour(cell);
                hasAdvancedRiver = previousCell.HasAnyAdvancedRiver(directionOfNeighbourCell);
            }

            previousCell = cell;

            if (hasRoad)
            {
                cell._temp_movementCost = MG_HexRoadManager.RoadMovementCost;//Road
                if (hasBridge == false)//Если нет моста, то есть у текущей и следующей клетки нет дорог которые соединяли бы, то штраф через реку (ADVANCED)
                    if (hasAdvancedRiver)
                        cell._temp_movementCost += MG_HexAdvancedRiverManager.RiverMovementCost;
            }
            else
            {

                cell._temp_movementCost = cell.Type.MovementCost;

                if (hasAdvancedRiver)
                {
                    cell._temp_movementCost += MG_HexAdvancedRiverManager.RiverMovementCost;
                }
                else if (hasRiver)//штраф через реку (ADVANCED)
                {
                    cell._temp_movementCost += MG_HexRiverManager.RiverMovementCost;
                }
            }

            if (MovementPoints >= cell._temp_movementCost)// && (!(_unwalkableGroundTypes.Contains(cell.Type.GroundType) && !hasRoad))
            {

                if (cell.IsVisibleForPlayer(_side) == false)
                {
                    MG_Division notVisibleUnit = cell.Division;
                    if (notVisibleUnit != null)
                    {
                        if (!notVisibleUnit.Side.Equals(_side))
                        {
                            isTryingToGoTrouhgEnemy = true;
                            break;
                        }
                    }
                    else
                    {
                        MG_Settlement settlement = cell.Settlement;
                        if (settlement != null)
                        {
                            if (!settlement.Side.Equals(_side))
                            {
                                isTryingToGoTrouhgEnemy = true;
                                break;
                            }
                        }
                    }
                }

                MovementPoints -= cell._temp_movementCost;
                reachablePath.Add(cell);

            }
            else
                break;
        }

        //--BEGIN проверить что юнит не остановиться на уже занятой клетке (через которую должен был пройти) из-за 1) нет очков 2) к непроходимости
        if (!isTryingToGoTrouhgEnemy)
        {

            int size = reachablePath.Count - 1;

            int skippedCount = 0;
            IsAttacking = false;
            MG_Division targetUnit = null;

            //Debug.Log("проверка отката! НАЧАЛо size=" + size);
            while (size > -1)
            {
                var cell = reachablePath[size];
                //Debug.Log(_cell.Pos + "  IsTaken=" + _cell.IsTaken);
                if (cell.IsTaken)
                {
                    //Debug.Log("ENEMY?");
                    skippedCount++;
                    if (skippedCount == 1)
                    {

                        //Debug.Log("skippedCount =");
                        targetUnit = cell.Division;
                        if (!targetUnit.Side.Equals(Side))
                        {

                            if (!IsScouting)
                            {
                                Debug.Log("ENEMY!");
                                IsAttacking = true;
                                _opponentInFight = targetUnit;
                            }
                        }
                    }
                    else
                    {
                        if (IsAttacking)
                            IsAttacking = false;//чтобы через своего другого юнита не начать воевать с врагом за которым не дотянуться уже    
                    }

                    //bool isEmptyRoad = _cell.RoadData.RiverType.Equals(MG_HexRoadLibrary.GetEmptyRiver());
                    //if (isEmptyRoad)
                    //    cost = _cell.Type.MovementCost;
                    //else
                    //    cost = 0.25f;

                    reachablePath.Remove(cell);
                    MovementPoints += cell._temp_movementCost;
                    //Debug.Log("ОТКАТ!");
                }
                else
                    break;
                size--;
            }
        }
        //this.BlockedPathsInRange.Erase();//Необходимо очистить от клеток, которые могут освободиться. Так как до них не будет уже очков
        //--END проверить что юнит не остановиться на уже занятой клетке (через которую должен был пройти) из-за 1) нет очков 2) к непроходимости

        if (reachablePath.Any())
        {
            //Cell.IsTaken = false;
            //Cell.Division = null;

            //Cell = reachablePath.Last();
            //Cell.IsTaken = true;
            //Cell.Division = this;

            if (MG_GameSettings.Instance().SkipMovementAnimation)
            {
                Debug.LogError("DO NOT USE YET! WIP");
                Cell.SetDivision(null);
                Cell = reachablePath.Last();
                Cell.SetDivision(this);
                transform.position = Cell.WorldPos;//передвинуть на позицию без анимации
                Moved?.Invoke();
                MG_TurnManager.EnableTurnButton();
            }
            else
                StartCoroutine(StartMoving(reachablePath));//показать анимацию, передвинуть на позицию

            //CalculatePathArea();// Вычислить клетки до которым можно "дотянуться"

            MG_PathAreaFixer.FindAndFixAfterMove(this);// Починить зону доступности после передвижения
        }
        else
        {
            OnMoveFinished();
            MG_TurnManager.EnableTurnButton();
        }
    }

    public void CalculatePathArea()
    {
        _pathsInRange = GetAvailableDestinations();
    }

    public List<MG_HexCell> FindPath(MG_HexCell destination)
    {
        _temp_currentPath = MG_Pathfinding.Astar.FindPath(Cell, destination, UnwalkableGroundTypes, _side);

        //bool visibleDestinationCell = !(MG_VisibilityChecker.IsBlackFog(destination.CurrentVisibility));
        //bool visibleDestinationCell = destination.IsVisibleForPlayer(_side);
        bool visibleDestinationCell = destination.IsDiscoveredByPlayer(_side);

        if (visibleDestinationCell)
        {
            bool isNotRoad = destination.RoadData.RoadType.Equals(MG_HexRoadLibrary.GetEmptyRoad());
            if (isNotRoad)
                if (UnwalkableGroundTypes.Contains(destination.Type.GroundType))
                    _temp_currentPath.Remove(destination);
        }

        if (IsPathTroughtBlackFog)
            _isPathTroughtBlackFog = false;

        foreach (var cell in _temp_currentPath.ToList())
        {
            //if (MG_VisibilityChecker.IsBlackFog(_cell.CurrentVisibility))
            if (cell.IsDiscoveredByPlayer(_side) == false)
            {
                _isPathTroughtBlackFog = true;
                break;
            }
        }

        //--BEGIN проверяем чтобы путь не содержал конечную точку, которая уже взята кем-то. Иначе укоротить путь пока не найдется свбободная.
        var fixedPath = new List<MG_HexCell>(_temp_currentPath);//Важная вещь. currentPath <- не нужно изменять в этом листе, иначе сломается путь сам к клетке! Поэтому копируем.
        if (_temp_currentPath.Any())
        {
            bool isFreeCell = false;
            while (!isFreeCell)
            {
                var targetCell = fixedPath.First();
                //if (!MG_VisibilityChecker.IsBlackFog(targetCell.CurrentVisibility))
                if (targetCell.IsDiscoveredByPlayer(_side))
                {

                    if (targetCell.IsTaken)
                    {
                        //bool visible = MG_VisibilityChecker.IsVisible(targetCell.CurrentVisibility);
                        bool visible = targetCell.IsVisibleForPlayer(_side);
                        if (visible)
                        {
                            //Debug.Log("REMOVED: " + targetCell.Pos);
                            fixedPath.Remove(targetCell);

                            if (!fixedPath.Any())
                                isFreeCell = true;
                        }
                        else
                            isFreeCell = true;


                    }
                    else
                        isFreeCell = true;
                }
                else
                {
                    isFreeCell = true;
                }
            }
        }
        //--END проверяем чтобы патч не содержал конечную точку, которая уже взята кем-то. Иначе укоротить путь пока не найдется свбободная в данном пути.

        CalculateMovementCost(fixedPath);

        return fixedPath;
    }

    public HashSet<MG_HexCell> GetAvailableDestinations()
    {
        _blockedPathsInRange.Clear();//очищаем заблокированные пути
        Dictionary<MG_HexCell, List<MG_HexCell>> newCachedPaths = new Dictionary<MG_HexCell, List<MG_HexCell>>();

        var paths = MG_Pathfinding.Dijkstar.FindArea(Cell, UnwalkableGroundTypes, MovementPoints, Side, _blockedPathsInRange);//Обработать ближние клетки

        foreach (var key in paths.Keys)
        {
            var path = paths[key];
            var pathCost = path.Sum(c => c._temp_movementCost);
            if (pathCost <= MovementPoints)
                newCachedPaths.Add(key, path);
        }
        return new HashSet<MG_HexCell>(newCachedPaths.Keys);
    }
    #endregion МЕТОДЫ ИНТЕРФЕЙСА "IMovable"

    #region МЕТОДЫ ИНТЕРФЕЙСА "ICellObserver"
    /// <summary>
    /// Добавить видимую клетку
    /// </summary>
    /// <param name="cell"></param>
    public void AddVisibleCell(MG_HexCell cell)
    {
        _visibleCells.Add(cell);
    }

    /// <summary>
    /// Удалить видимую клетку
    /// </summary>
    /// <param name="cell"></param>
    public void RemoveVisibleCell(MG_HexCell cell)
    {
        _visibleCells.Remove(cell);
    }

    /// <summary>
    /// Получить владельца
    /// </summary>
    /// <returns></returns>
    public MG_Player GetSide()
    {
        return Side;
    }

    /// <summary>
    /// Обновить видимость
    /// </summary>
    public void RecalculateVision()
    {
        _visibleCells = _lineOfSight.UpdateFOV(Cell, Vision, VisibleCells, this);
    }

    /// <summary>
    /// получить все видимые клетки
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<MG_HexCell> GetVisibleCells()
    {
        return _visibleCells;
    }
    #endregion МЕТОДЫ ИНТЕРФЕЙСА "ICellObserver"

    #region МЕТОДЫ ИНТЕРФЕЙСА "IVisibility"
    public void SetVisibility(Visibility visibility)
    {
        if (_isMoving)
            return;//когда передвигается, может быть видимым, то невидимым из-за передвижений! Так что нужен этот фикс

        //Debug.Log("visibility= " + visibility + " _currentVisibility= " + _currentVisibility + " " + this.name);
        switch (visibility)
        {
            case Visibility.BlackFog:
                if (MG_VisibilityChecker.IsGreyFog(_currentVisibility))
                {
                    _spriteRenderer.color = new Color(1, 1, 1, 1);//Отбеливаем после серого. Иначе перейдем потом на видимость, а он будет серым.
                    _spriteRenderer.enabled = false;//отключаем рендер спрайта, чтобы был невидим                   
                    this._currentVisibility = visibility;
                }
                else if (MG_VisibilityChecker.IsVisible(_currentVisibility))
                {
                    //Debug.Log("BLACK!!! visibility= " + visibility);
                    _spriteRenderer.enabled = false;//отключаем рендер спрайта, чтобы был невидим
                    this._currentVisibility = visibility;
                }
                break;
            case Visibility.GreyFog:
                if (MG_VisibilityChecker.IsBlackFog(_currentVisibility))
                {
                    //+++++++++++++++++_spriteRenderer.enabled = true;//включаем рендер спрайта, чтобы был видим
                    //+++++++++++++++++_spriteRenderer.color = MG_LineOfSight.Instance.GreyFogOfWar;
                    this._currentVisibility = visibility;
                }
                else if (MG_VisibilityChecker.IsVisible(_currentVisibility))
                {
                    //++++++++++++++_spriteRenderer.color = MG_LineOfSight.Instance.GreyFogOfWar;
                    _spriteRenderer.enabled = false;//++++++++++++++++отключаем рендер спрайта, чтобы был невидим
                    this._currentVisibility = visibility;
                }

                break;
            case Visibility.Visible:
                if (MG_VisibilityChecker.IsBlackFog(_currentVisibility))
                {
                    _spriteRenderer.color = new Color(1, 1, 1, 1);
                    _spriteRenderer.enabled = true;//включаем рендер спрайта, чтобы был видим
                    this._currentVisibility = visibility;
                }
                else if (MG_VisibilityChecker.IsGreyFog(_currentVisibility))
                {
                    _spriteRenderer.color = new Color(1, 1, 1, 1);
                    _spriteRenderer.enabled = true;//++++++++++++++отключаем рендер спрайта, чтобы был невидим
                    this._currentVisibility = visibility;
                }
                break;
            default:
                Debug.Log("<color=orange>MG_BasicUnit SetVisibility(): switch DEFAULT.</color>");
                break;
        }
        _unitHighlighter.SetVisibility(visibility);
    }
    #endregion МЕТОДЫ ИНТЕРФЕЙСА "IVisibility"

    #region МЕТОДЫ ИНТЕРФЕЙСА "IPathContainer"
    /// <summary>
    /// Добавить заблокированную клетку
    /// </summary>
    /// <param name="cell"></param>
    public void AddBlockedCell(MG_HexCell cell)
    {
        _blockedPathsInRange.Add(cell);
    }

    /// <summary>
    /// Убрать заблокированную клетку
    /// </summary>
    /// <param name="cell"></param>
    public void RemoveBlockedCell(MG_HexCell cell)
    {
        _blockedPathsInRange.Remove(cell);
    }

    /// <summary>
    /// Добавить доступную клетку
    /// </summary>
    /// <param name="cell"></param>
    public void AddAvailableCell(MG_HexCell cell)
    {
        _pathsInRange.Add(cell);
    }

    /// <summary>
    /// Убрать доступную клетку
    /// </summary>
    /// <param name="cell"></param>
    public void RemoveAvailableCell(MG_HexCell cell)
    {
        _pathsInRange.Remove(cell);
    }

    /// <summary>
    /// Является ли текущая клетка проходимой
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public bool IsWalkableCell(MG_HexCell cell)
    {
        bool isEmptyRoad = MG_HexRoadManager.IsEmptyRoad(cell);
        if (isEmptyRoad == false)
            return true;

        GroundType groundType = cell.Type.GroundType;
        if (UnwalkableGroundTypes.Contains(groundType))
            return false;

        return true;
    }
    #endregion
}
}
