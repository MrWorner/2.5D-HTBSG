//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace Assets.MG_NW2020._01_Scripts.Line_Of_Sight
//{
//    public class MG_VisibilityTestKit : MonoBehaviour
//    {

//        private static HashSet<MG_HexCell> _tempVisibleCells = new HashSet<MG_HexCell>();//используется для создания тестовой линии

//        / <summary>
//        / Тестовое создание линии на карте
//        / </summary>
//        / <param name = "endCell" > конечная клетка</param>
//        public static void DrawTestLineForCursors(MG_HexCell endCell)
//        {

//            //Debug.Log("" + endCell.HexPos);
//            MG_HexCell beginCell = Instance._cellForDrawLineTest;
//            if (beginCell == null)
//                return;

//            var line = MG_GeometryFuncs.Cube_Line(beginCell.HexPos, endCell.HexPos);
//            line.AddItem(endCell.HexPos);
//            //---line.AddItem(resultPos);
//            bool notInRange = false;

//            //BEGINN Очистить временный лист клеток
//            if (_tempVisibleCells.Any())
//            {
//                foreach (var _cell in _tempVisibleCells)
//                {
//                    _cell.UnMark();
//                }
//                _tempVisibleCells.Erase();
//            }
//            //END Очистить временный лист клеток

//            foreach (var pos in line)
//            {
//                MG_HexCell cellN = MG_GlobalHexMap.GetCellByHexPos(pos);
//                int penality = 0;
//                if (cellN != null)
//                {
//                    if (notInRange)
//                    {
//                        cellN.Mark(Color.red);
//                    }
//                    else if (cellN.Type.GroundType.Equals(GroundType.Mountain))
//                    {
//                        cellN.Mark(Color.black);
//                        notInRange = true;
//                    }
//                    else if (cellN.Type.GroundType.Equals(GroundType.HeavyForest) || cellN.Type.GroundType.Equals(GroundType.Hill))
//                    {
//                        penality++;

//                        if (penality > 2)
//                        {
//                            notInRange = true;
//                        }
//                        cellN.Mark(Color.blue);
//                    }
//                    else
//                    {
//                        cellN.Mark(Color.magenta);
//                    }

//                    _tempVisibleCells.AddItem(cellN);
//                }
//            }

//        }

//        / <summary>
//        / Показать зону видимости окружения на заданной клетке
//        / </summary>
//        / <param name = "center" > центр </ param >
//        / < param name="radius">радиус</param>
//        public static void ShowVisibleArea(Vector3Int center, int radius)
//        {
//            HashSet<Vector3Int> results = MG_GeometryFuncs.Cube_ring(center, radius);

//            foreach (var resultPos in results)
//            {
//                MG_HexCell cellN = MG_GlobalHexMap.GetCellByHexPos(resultPos);
//                if (cellN != null)
//                {
//                    cellN.Mark(Color.red);
//                }

//                var line = MG_GeometryFuncs.Cube_Line(center, resultPos);
//                //---line.AddItem(resultPos);
//                foreach (var pos in line)
//                {
//                    cellN = MG_GlobalHexMap.GetCellByHexPos(pos);
//                    int penality = 0;
//                    if (cellN != null)
//                    {
//                        if (cellN.Type.GroundType.Equals(GroundType.Mountain))
//                        {
//                            cellN.Mark(Color.black);
//                            break;
//                        }
//                        else if (cellN.Type.GroundType.Equals(GroundType.HeavyForest) || cellN.Type.GroundType.Equals(GroundType.Hill))
//                        {
//                            penality++;

//                            if (penality > 2)
//                            {

//                                break;
//                            }
//                            else if (penality > 1)
//                            {
//                                cellN.Mark(Color.black);
//                            }
//                            else
//                                cellN.Mark(Color.magenta);
//                        }
//                        else
//                        {
//                            cellN.Mark(Color.magenta);
//                        }
//                    }
//                }
//            }
//        }

//    }
//}