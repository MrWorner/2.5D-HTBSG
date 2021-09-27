//using Sirenix.OdinInspector;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public class MG_SimsBuildingSystem : MonoBehaviour
//{
//    private static MG_SimsBuildingSystem _instance;
//    //[SerializeField] private bool isStarted = false;//выбрана ли начальная клетка
//    //[SerializeField] private bool isDirectionDefined = false;//определено ли направление
//    //[SerializeField] private DirectionHexPT direction = DirectionHexPT.NE;
//    private MG_HexCell _starterCell;
//    private HashSet<MG_HexCell> _ghostLine;
//    //private bool isStartedCellChosen = false;

//    [Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMap _map;//[R] карта
//    [Required(InfoMessageType.Error), SerializeField] private MG_HexRoadManager _roadBuilder;//[R] менеджер дорог

//    private void Awake()
//    {
//        if (_instance == null)
//            _instance = this;
//        else
//            Debug.Log("<color=red>MG_SimsBuildingSystem Awake(): найден лишний MG_SimsBuildingSystem!</color>");

//        if (_map == null) Debug.Log("<color=red>MG_SimsBuildingSystem Awake(): '_map' не задан!</color>");
//        if (_roadBuilder == null) Debug.Log("<color=red>MG_SimsBuildingSystem Awake(): '_settlementManager' не задан!</color>");
//    }

//    public void SetStarterCell(MG_HexCell _cell)
//    {
//        _starterCell = _cell;
//        //isStartedCellChosen = true;
//    }

//    /// <summary>
//    /// сгенерировать прямую линию
//    /// </summary>
//    /// <param name="hexPos">конечная точка</param>
//    /// <returns>Массив клеток линии </returns>
//    public HashSet<MG_HexCell> GenerateSimsLine(Vector3Int hexPos)
//    {
//        //ИДЕЯ состоит в том чтобы для начало взять и с помощью существующей функции черчения начертить линию от заданной клетки до конечной (которая используется в Line of Sight). 
//        //А из полученной линии взять только две начальные клетки и у них определить направление, чтобы начертить чисто прямую линию
//        //Так как первая линия быть не прямой.

//        if (_starterCell.Equals(null))
//        {
//            Debug.Log("MG_SimsBuildingSystem GenerateCellLine(): Ошибка! Начальная клетка не задана. Используйте SetStarterCell() перед использованием данного метода");
//            return new HashSet<MG_HexCell>();
//        }

//        HashSet<Vector3Int> line = MG_GeometryFuncs.Cube_Line(_starterCell.HexPos, hexPos);
//        DirectionHexPT direction = DefineDirection(line);
//        HashSet<MG_HexCell> result = DefineLine(_starterCell, direction, line.Count);
//        Reset();// Сбросить сохраненные данные
//        return result;
//    }

//    public void BuildGhostLine(HashSet<MG_HexCell> line)
//    {
//        if (_ghostLine.Any())
//            _ghostLine.Erase();

//        foreach (var _cell in line)
//        {
//            _roadBuilder.Show(_cell, true);
//        }
//    }

//    public void Reset()
//    {
//        _starterCell = null;
//        //isStartedCellChosen = false;
//    }

//    /// <summary>
//    /// Определить направление
//    /// </summary>
//    /// <param name="line">линия координат</param>
//    /// <returns></returns>
//    private DirectionHexPT DefineDirection(HashSet<Vector3Int> line)
//    {
//        byte i = 0;
//        MG_HexCell firstCell = null;
//        MG_HexCell secondtCell = null;
//        foreach (var pos in line)
//        {
//            if (i == 0)
//            {
//                firstCell = _map.GetCellByHexPos(pos);
//            }
//            else
//            {
//                secondtCell = _map.GetCellByHexPos(pos);
//                break;
//            }
//        }

//        foreach (var item in firstCell._neighbours)
//        {
//            if (item.Value.Equals(secondtCell))
//            {
//                return item.Key;
//            }
//        }
//        Debug.Log("DefineDirection(): ОШИБКА! Не найдена соседняя клетка!");
//        return DirectionHexPT.NE;
//    }

//    /// <summary>
//    /// Определить прямую линию из клеток
//    /// </summary>
//    /// <param name="starterCell">начальная клетка</param>
//    /// <param name="dir">направление</param>
//    /// <param name="lineLenght">длинна линии</param>
//    /// <returns></returns>
//    private HashSet<MG_HexCell> DefineLine(MG_HexCell starterCell, DirectionHexPT dir, int lineLenght)
//    {
//        HashSet<MG_HexCell> line = new HashSet<MG_HexCell>();
//        line.Add(starterCell);

//#pragma warning disable CS0162 // Unreachable code detected
//        for (int i = 1; i < lineLenght; i++)
//#pragma warning restore CS0162 // Unreachable code detected
//        {

//            foreach (var item in starterCell._neighbours)
//            {
//                if (item.Key.Equals(dir))
//                {
//                    starterCell = item.Value;
//                    line.Add(starterCell);
//                    continue;
//                }
//            }
//            break;
//        }

//        return line;
//    }

//    //private MG_HexCell FindCellByMousePos(Vector3 mousePos)
//    //{
//    //    MG_HexCell _cell = _map.GetCell(mousePos);
//    //    return _cell;
//    //}
//}
