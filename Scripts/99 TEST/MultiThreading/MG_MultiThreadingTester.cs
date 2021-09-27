//using DigitalRuby.Threading;
//using Sirenix.OdinInspector;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;


////Необходимо протестировать MultiThreading для Pathfinding!
//public class MG_MultiThreadingTester : MonoBehaviour
//{
//    public static MG_MultiThreadingTester instance;

//    [Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMap _cellGrid;

//    public MG_Division _division;
//    public Vector3Int _cellPos;
//    public int _RESULT;
//    public bool _FINISHED = true;

//    MG_HexCell _cell;

//    private void Awake()
//    {
//        instance = this;
//    }

//    public void Setup(MG_Division division, MG_HexCell cell)
//    {
//        instance._cell = cell;
//        instance._cellPos = cell.Pos;
//        instance._division = division;
//    }

//    //[Button]
//    public void START()
//    {
//        //_division.FindPath(_cell);


//        //_cell = _cellGrid.GetCell(new Vector3Int(Random.Range(0, 199), Random.Range(0, 159), 0));
//        //EZThread.BeginThread(Execute, true);
//        if (_FINISHED)
//        {
//            EZThread.ExecuteInBackground(Execute, Finish);
//        }
//        //var obj = Execute();
//        //Finish(obj);

//    }

//    private object Execute()
//    {
//        _FINISHED = false;
//        _RESULT = -1;

//        //_cell = _cellGrid.GetCell(_cellPos);
//        //_cell = _cellGrid.GetCell(new Vector3Int (Random.Range(0,199), Random.Range(0, 159), 0));
//        //_cellPos = _cell.Pos;

//        var path = _division.FindPath(_cell);


//        //var temp_currentPath = MG_Pathfinding.Astar.FindPath(_division.Cell, _cell, new List<GroundType>() { GroundType.Mountain }, _division.Side);
//        //_RESULT = temp_currentPath.Count;
//        //_RESULT = path.Count;
//        return path;
//    }

//    private void Finish(object path)
//    {
//        var pathCorrect = path as List<MG_HexCell>;
//        _RESULT = pathCorrect.Count;
//        _FINISHED = true;
//    }


//}
