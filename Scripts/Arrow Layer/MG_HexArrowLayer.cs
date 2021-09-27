using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_HexArrowLayer : MonoBehaviour
{
    private static MG_HexArrowLayer _instance;
     

    #region Поля
    private readonly HashSet<MG_HexCell> _cashedCells = new HashSet<MG_HexCell>();//клетки в памяти
    private Coroutine _drawArrowsCoroutine = null;
    [BoxGroup("Цвет стрелки"), SerializeField] Color _arrowColor_InArea = Color.green;//в досягаемости зоны
    [BoxGroup("Цвет стрелки"), SerializeField] Color _arrowColor_OutOfArea = Color.yellow;//вне досягаемости зоны
    [BoxGroup("Цвет стрелки"), SerializeField] Color _arrowColor_Unwalkable = Color.red;//непроходимый
    [BoxGroup("Цвет стрелки"), SerializeField] Color _arrowColor_BlackFog = Color.black;//туман
    #endregion Поля

    #region Поля: необходимые модули
    [Required(InfoMessageType.Error), SerializeField] private Tilemap _arrowTileMap;//[R] Карта
    [Required(InfoMessageType.Error), SerializeField] private MG_HexArrowLibrary _arrowTileLibrary;//[R] библиотека тайлов стрелок
    #endregion Поля: необходимые модули

    #region Методы UNITY
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_HexArrowLayer Awake(): найден лишний MG_ArrowDrawSystemManager!</color>");
        if (_arrowTileMap == null) Debug.Log("<color=red>MG_HexArrowLayer Awake(): '_arrowTileMap' не задан!</color>");
        if (_arrowTileLibrary == null) Debug.Log("<color=red>MG_HexArrowLayer Awake(): '_arrowTileLibrary' не задан!</color>");
    }
    #endregion Методы UNITY

    #region Статический метод GetInstance()
    /// <summary>
    /// Получить экземпляр класса
    /// </summary>
    /// <returns></returns>
    public static MG_HexArrowLayer GetInstance()
    {
        return MG_HexArrowLayer._instance;
    }
    #endregion Статический метод GetInstance()

    #region Публичные методы

    /// <summary>
    /// Показать путь стрелками
    /// </summary>
    /// <param name="path"></param>
    /// <param name="starterCell"></param>
    /// <param name="area"></param>
    public void ShowArrowPath(IReadOnlyList<MG_HexCell> path, MG_HexCell starterCell, IReadOnlyCollection<MG_HexCell> area)
    {      
        if (_drawArrowsCoroutine != null)
        {
            StopCoroutine(_drawArrowsCoroutine);
        }
        ClearArrowTileMap();//очищаем тайл мап от предыдущих стрелок
        _drawArrowsCoroutine = StartCoroutine(DrawArrows(path, starterCell, area));
    }

    /// <summary>
    /// Остановить рисование стрелок
    /// </summary>
    public void CheckAndStopDrawing()
    {
        if (_drawArrowsCoroutine != null)
        {
            StopCoroutine(_drawArrowsCoroutine);
            ClearArrowTileMap();//очищаем тайл мап от предыдущих стрелок
        }
        
    }

    /// <summary>
    /// Очищаем тайл мап от стрелок
    /// </summary>
    public void ClearArrowTileMap()
    {
        if (_cashedCells.Any())
        {
            foreach (var cell in _cashedCells)
                _arrowTileMap.SetTile(cell.Pos, null);
            _cashedCells.Clear();
        }
    }

    public IEnumerator DrawArrows(IReadOnlyList<MG_HexCell> path, MG_HexCell starterCell, IReadOnlyCollection<MG_HexCell> area)
    {

        //yield return new WaitForSeconds(.1f);
        ClearArrowTileMap();//очищаем тайл мап от предыдущих стрелок
        if (path.Any())
        {
            List<MG_HexCell> pathCopy = path.ToList();//необходимая копия пути, чтобы не модифицорвать и не сломать оригинальный лист! Уже такое происходило.
            pathCopy.Add(starterCell);//добавляем стартовую клетку, так как путь не содержит это
            bool first = true;//первый ли элемент
            MG_HexCell pastCell = null;//предыдущая клетка
            MG_HexCell currentCell = null;//текущая клетка

            //----НАЧИНАЕМ ОПРЕДЕЛЯТЬ И РИСОВАТЬ ПУТЬ СТРЕЛКАМИ
            foreach (var nextCell in pathCopy)
            {
                if (first == false)
                {
                    if (pastCell != null)
                    {
                        MG_HexArrow arrow = DefineArrowType(pastCell, currentCell, nextCell);
                        Draw(arrow, currentCell, area);
                    }
                }
                else
                {
                    first = false;
                    //-----------------------Draw(_arrowTileLibrary.TargetArrowType, nextCell, area);
                }
                pastCell = currentCell;
                currentCell = nextCell;
                yield return 0f;
            }
            yield break;
        }


        
    }

    #endregion Публичные методы

    #region Личные методы
    /// <summary>
    /// Нарисовать стрелку
    /// </summary>
    /// <param name="arrow">Тип стрелки</param>
    /// <param name="cell">Клетка</param>
    private void Draw(MG_HexArrow arrow, MG_HexCell cell, IReadOnlyCollection<MG_HexCell> area)
    {
        var pos = cell.Pos;
        _arrowTileMap.SetTile(pos, arrow.Tile);//устанавливаем тайл
        _arrowTileMap.SetTileFlags(pos, TileFlags.None);//сбрасываем флаги тайла
        AddToCash(cell);
        if (MG_VisibilityChecker.IsBlackFog(cell.CurrentVisibility))
            _arrowTileMap.SetColor(pos, _arrowColor_BlackFog);
        else if (area.Contains(cell))
            _arrowTileMap.SetColor(pos, _arrowColor_InArea);
        else
            _arrowTileMap.SetColor(pos, _arrowColor_OutOfArea);
    }

    /// <summary>
    /// Определить тип стрелки
    /// </summary>
    /// <param name="past">Предыдущая клетка</param>
    /// <param name="current">Текущая клетка</param>
    /// <param name="next">Следующая клетка</param>
    /// <returns></returns>
    private MG_HexArrow DefineArrowType(MG_HexCell past, MG_HexCell current, MG_HexCell next)
    {
        //ОБРАТИ ВНИМАНИЕ. ЛИСТ ПУТИ СОДЕРЖИТ ВСЕ ЭЛЕМЕНТЫ НАОБОРОТ от Цели до Юнита.
        //past - клетка от цели
        //current - текущая клетка
        //next - клетка до юнита

        Vector3Int posDirectionFrom = past.Pos - current.Pos;//Определяем координаты направления ОТКУДА
        Vector3Int posDirectionTo = next.Pos - current.Pos;//Определяем координаты направления ДО

        DirectionHexPT dirFrom = MG_Direction.GetDirectionByXY(posDirectionTo.x, posDirectionTo.y, current.IsEvenRow);//определям направления ОТКУДА
        DirectionHexPT dirTo = MG_Direction.GetDirectionByXY(posDirectionFrom.x, posDirectionFrom.y, current.IsEvenRow);//определям направления ДО

        ArrowDirection arrowDirection = DefineArrowDirection(dirFrom, dirTo);//Определяем тип направления стрелки
        MG_HexArrow arrow = _arrowTileLibrary.GetItem(arrowDirection);//Получаем объект стрелки из библиотеки стрелок

        return arrow;
    }

    /// <summary>
    /// Запоминаем клетку
    /// </summary>
    /// <param name="cell"></param>
    private void AddToCash(MG_HexCell cell)
    {
        _cashedCells.Add(cell);
    }


    /// <summary>
    /// Определяем направление по координатам направления | МОЖНО ИСПОЛЬЗОВАТЬ ВМЕСТО " MG_Direction.GetDirectionByXY()"
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    //private DirectionHexPT DefineDirectionByPos(Vector3Int pos)
    //{
    //    //https://www.redblobgames.com/grids/hexagons/
    //    int x = pos.x;
    //    int y = pos.y;
    //    int z = pos.z;

    //    if (x > 0)
    //    {
    //        if (y == 0)
    //        {
    //            if (z < 0)
    //                return DirectionHexPT.NE;//XYZ = 1,0,-1
    //        }
    //        else if (y < 0)
    //        {
    //            if (z == 0)
    //                return DirectionHexPT.E;//XYZ = 1,-1,0
    //        }
    //    }
    //    else if (x == 0)
    //    {
    //        if (y > 0)
    //        {
    //            if (z < 0)
    //                return DirectionHexPT.NW;//XYZ = 0,1,-1
    //        }
    //        else if (y < 0)
    //        {
    //            if (z > 0)
    //                return DirectionHexPT.SE;//XYZ = 0,-1,1
    //        }
    //    }
    //    else if (x < 0)
    //    {
    //        if (y > 0)
    //        {
    //            if (z == 0)
    //                return DirectionHexPT.W;//XYZ = -1,1,0
    //        }
    //        else if (y == 0)
    //        {
    //            if (z > 0)
    //                return DirectionHexPT.SW; //XYZ = -1,0,1
    //        }
    //    }
    //    Debug.Log("ERROR! x=" + x + " y=" + y + " z=" + z);
    //    return DirectionHexPT.NE;
    //


    /// <summary>
    /// Определить направление стрелки
    /// </summary>
    /// <param name="sideFrom">направления ОТКУДА</param>
    /// <param name="sideTo">направление КУДА</param>
    /// <returns></returns>
    private ArrowDirection DefineArrowDirection(DirectionHexPT sideFrom, DirectionHexPT sideTo)
    {
        switch (sideFrom)
        {
            case DirectionHexPT.NE:
                return A(sideTo); //A+(X)
            case DirectionHexPT.E:
                return B(sideTo);//B+(X)
            case DirectionHexPT.SE:
                return C(sideTo);//C+(X)
            case DirectionHexPT.SW:
                return D(sideTo);//D+(X)
            case DirectionHexPT.W:
                return E(sideTo);//E+(X)
            case DirectionHexPT.NW:
                return F(sideTo);//F+(X)
            default:
                Debug.Log("ERROR?");
                return ArrowDirection.None;
        }
    }

    /// <summary>
    ///  Спец метод по выявлению направления стрелки если первый - A
    /// </summary>
    /// <param name="sideTo">Сторона КУДА</param>
    /// <returns></returns>
    private ArrowDirection A(DirectionHexPT sideTo)
    {
        switch (sideTo)
        {
            case DirectionHexPT.NE:
                //A+A
                Debug.Log("ERROR! A+A");
                return ArrowDirection.None;
            case DirectionHexPT.E:
                //A+B
                return ArrowDirection.AB;
            case DirectionHexPT.SE:
                //A+C
                return ArrowDirection.AC;
            case DirectionHexPT.SW:
                //A+D
                return ArrowDirection.AD;
            case DirectionHexPT.W:
                //A+E
                return ArrowDirection.AE;
            case DirectionHexPT.NW:
                //A+F
                return ArrowDirection.AF;
            default:
                Debug.Log("ERROR?");
                return ArrowDirection.None;
        }
    }

    /// <summary>
    /// Спец метод по выявлению направления стрелки если первый - B
    /// </summary>
    /// <param name="sideTo">Сторона КУДА</param>
    /// <returns></returns>
    private ArrowDirection B(DirectionHexPT sideTo)
    {
        switch (sideTo)
        {
            case DirectionHexPT.NE:
                //B+A
                return ArrowDirection.BA;
            case DirectionHexPT.E:
                //B+B
                Debug.Log("ERROR! B+B");
                return ArrowDirection.None;
            case DirectionHexPT.SE:
                //B+C
                return ArrowDirection.BC;
            case DirectionHexPT.SW:
                //B+D
                return ArrowDirection.BD;
            case DirectionHexPT.W:
                //B+E
                return ArrowDirection.BE;
            case DirectionHexPT.NW:
                //B+F
                return ArrowDirection.BF;
            default:
                Debug.Log("ERROR?");
                return ArrowDirection.None;
        }
    }

    /// <summary>
    ///  Спец метод по выявлению направления стрелки если первый - C
    /// </summary>
    /// <param name="sideTo">Сторона КУДА</param>
    /// <returns></returns>
    private ArrowDirection C(DirectionHexPT sideTo)
    {
        switch (sideTo)
        {
            case DirectionHexPT.NE:
                //C+A
                return ArrowDirection.CA;
            case DirectionHexPT.E:
                //C+B
                return ArrowDirection.CB;
            case DirectionHexPT.SE:
                //C+C
                Debug.Log("ERROR! C+C");
                return ArrowDirection.None;
            case DirectionHexPT.SW:
                //C+D
                return ArrowDirection.CD;
            case DirectionHexPT.W:
                //C+E
                return ArrowDirection.CE;
            case DirectionHexPT.NW:
                //C+F
                return ArrowDirection.CF;
            default:
                Debug.Log("ERROR?");
                return ArrowDirection.None;
        }
    }

    /// <summary>
    ///  Спец метод по выявлению направления стрелки если первый - D
    /// </summary>
    /// <param name="sideTo">Сторона КУДА</param>
    /// <returns></returns>
    private ArrowDirection D(DirectionHexPT sideTo)
    {
        switch (sideTo)
        {
            case DirectionHexPT.NE:
                //D+A
                return ArrowDirection.DA;
            case DirectionHexPT.E:
                //D+B
                return ArrowDirection.DB;
            case DirectionHexPT.SE:
                //D+C                
                return ArrowDirection.DC;
            case DirectionHexPT.SW:
                //D+D
                Debug.Log("ERROR! D+D");
                return ArrowDirection.None;
            case DirectionHexPT.W:
                //D+E
                return ArrowDirection.DE;
            case DirectionHexPT.NW:
                //D+F
                return ArrowDirection.DF;
            default:
                Debug.Log("ERROR?");
                return ArrowDirection.None;
        }
    }

    /// <summary>
    ///  Спец метод по выявлению направления стрелки если первый - E
    /// </summary>
    /// <param name="sideTo">Сторона КУДА</param>
    /// <returns></returns>
    private ArrowDirection E(DirectionHexPT sideTo)
    {
        switch (sideTo)
        {
            case DirectionHexPT.NE:
                //E+A
                return ArrowDirection.EA;
            case DirectionHexPT.E:
                //E+B
                return ArrowDirection.EB;
            case DirectionHexPT.SE:
                //E+C                
                return ArrowDirection.EC;
            case DirectionHexPT.SW:
                //E+D            
                return ArrowDirection.ED;
            case DirectionHexPT.W:
                //E+E
                Debug.Log("ERROR! E+E");
                return ArrowDirection.None;
            case DirectionHexPT.NW:
                //E+F
                return ArrowDirection.EF;
            default:
                Debug.Log("ERROR?");
                return ArrowDirection.None;
        }
    }

    /// <summary>
    ///  Спец метод по выявлению направления стрелки если первый - F
    /// </summary>
    /// <param name="sideTo">Сторона КУДА</param>
    /// <returns></returns>
    private ArrowDirection F(DirectionHexPT sideTo)
    {
        switch (sideTo)
        {
            case DirectionHexPT.NE:
                //F+A
                return ArrowDirection.FA;
            case DirectionHexPT.E:
                //F+B
                return ArrowDirection.FB;
            case DirectionHexPT.SE:
                //F+C                
                return ArrowDirection.FC;
            case DirectionHexPT.SW:
                //F+D            
                return ArrowDirection.FD;
            case DirectionHexPT.W:
                //F+E               
                return ArrowDirection.FE;
            case DirectionHexPT.NW:
                //F+F
                Debug.Log("ERROR! F+F");
                return ArrowDirection.None;
            default:
                Debug.Log("ERROR?");
                return ArrowDirection.None;
        }
    }
    #endregion Личные методы
}
}

