/////////////////////////////////////////////////////////////////////////////////
//
//	MG_TestTBSframework.cs
//	Автор: MaximGodyna
//  GitHub: https://github.com/MrWorner
//
//	Предназначение:	Тестовый класс для расстановки необходимых объектов на карте
//  Замечания: ---
//
/////////////////////////////////////////////////////////////////////////////////

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_TestTBSframework : MonoBehaviour
{
    public static MG_TestTBSframework _instance;

    [Required(InfoMessageType.Error), SerializeField] private MG_UnitObjectFactory _unitObjectFactory;

    [Required(InfoMessageType.Error), SerializeField] private MG_Player _sidePC;
    [Required(InfoMessageType.Error), SerializeField] private MG_Player _sideEnemy;

    [Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMap _cellGrid;

    [Required(InfoMessageType.Error), SerializeField] private MG_Game _game;

    [Required(InfoMessageType.Error), SerializeField] private GameObject settlementPrefab;
    [Required(InfoMessageType.Error), SerializeField] private MG_SettlementType settlementType_City;

    [Required(InfoMessageType.Error), SerializeField] private MG_ResourceManager resourceManager;

    [Required(InfoMessageType.Error), SerializeField] private MG_ResourceLibrary resourceLibrary;
    [Required(InfoMessageType.Error), SerializeField] private MG_MiniMap _miniMapTargetCameraHelper;

    //[Required(InfoMessageType.Error), SerializeField] private MG_ResizeSpriteToScreen _resizeSpriteToScreen;


    private void Awake()
    {
  
        _instance = this;

        if (_unitObjectFactory == null) Debug.Log("<color=red>MG_TestTBSframework Awake(): MISSING COMPONENT! _unitObjectFactory</color>");
        if (_sidePC == null) Debug.Log("<color=red>MG_TestTBSframework Awake(): MISSING COMPONENT! _sidePC</color>");
        if (_sideEnemy == null) Debug.Log("<color=red>MG_TestTBSframework Awake(): MISSING COMPONENT! _sideEnemy</color>");
        if (_cellGrid == null) Debug.Log("<color=red>MG_TestTBSframework Awake(): MISSING COMPONENT! _cellGrid</color>");
        if (_game == null) Debug.Log("<color=red>MG_TestTBSframework Awake(): MISSING COMPONENT! _game</color>");
        if (settlementPrefab == null) Debug.Log("<color=red>MG_TestTBSframework Awake(): MISSING COMPONENT! settlementPrefab</color>");
        if (settlementType_City == null) Debug.Log("<color=red>MG_TestTBSframework Awake(): MISSING COMPONENT! settlementType_City</color>");
        if (resourceManager == null) Debug.Log("<color=red>MG_TestTBSframework Awake(): MISSING COMPONENT! resourceManager</color>");   
        if (resourceLibrary == null) Debug.Log("<color=red>MG_TestTBSframework Awake(): MISSING COMPONENT! resourceLibrary</color>");
        if (_miniMapTargetCameraHelper == null) Debug.Log("<color=red>MG_TestTBSframework Awake(): MISSING COMPONENT! _miniMap</color>");
        //if (_resizeSpriteToScreen == null) Debug.Log("<color=red>MG_TestTBSframework Awake(): MISSING COMPONENT! _resizeSpriteToScreen</color>");

    }

    public void StartNow()
    {

        // Debug.Log("MG_TestTBSframework Start()");

        if (_cellGrid.GetCells().Count > 10)
        {
            MG_HexCell randomCell;

            MG_SettlementManager.TEST_Create_remotly();

            //MG_FogOfWar.Instance.Init();

            //randomCell = MG_GlobalHexMap.Instance.GetRandomCell(new List<GroundType>(), false, false, false);
            //MG_SettlementManager.Instance.Show(randomCell, MG_Game.Players[0], test_settlementType_City, settlementPrefab);
            //randomCell = MG_GlobalHexMap.Instance.GetRandomCell(new List<GroundType>(), false, false, false);
            //MG_SettlementManager.Instance.Show(randomCell, MG_Game.Players[1], test_settlementType_City, settlementPrefab);
            //randomCell = MG_GlobalHexMap.Instance.GetRandomCell(new List<GroundType>(), false, false, false);
            //MG_SettlementManager.Instance.Show(randomCell, MG_Game.Players[2], test_settlementType_City, settlementPrefab);

            randomCell = MG_GlobalHexMap.Instance.GetRandomCell(new List<GroundType>() { GroundType.Mountain, GroundType.Water, GroundType.HeavyForest, GroundType.Forest }, true, false, true);
            randomCell = _cellGrid.GetCell(new Vector3Int(1, 3, 0));
            GameObject divisionObject = _unitObjectFactory.Create(randomCell, _sidePC);

            //randomCell = MG_GlobalHexMap.Instance.GetRandomCell(new List<GroundType>() { GroundType.Mountain, GroundType.Water, GroundType.HeavyForest, GroundType.Forest }, true, false, true);
            //randomCell = _cellGrid.GetCell(new Vector3Int(3, 3, 0));
            //GameObject divisionObject2 = _unitObjectFactory.Create(randomCell, _sidePC);

            randomCell = MG_GlobalHexMap.Instance.GetRandomCell(new List<GroundType>() { GroundType.Mountain, GroundType.Water, GroundType.HeavyForest, GroundType.Forest }, true, false, true);
            //randomCell = _cellGrid.GetCell(new Vector3Int(4, 5, 0));
            GameObject divisionObject3 = _unitObjectFactory.Create(randomCell, _sideEnemy);

            

            randomCell = MG_GlobalHexMap.Instance.GetRandomCell(new List<GroundType>() { GroundType.Mountain, GroundType.Water, GroundType.HeavyForest, GroundType.Forest }, false, false, false);
            resourceManager.Create(250, resourceLibrary.Get(0), randomCell);
            randomCell = MG_GlobalHexMap.Instance.GetRandomCell(new List<GroundType>() { GroundType.Mountain, GroundType.Water, GroundType.HeavyForest, GroundType.Forest }, false, false, false);
            resourceManager.Create(410, resourceLibrary.Get(1), randomCell);
            randomCell = MG_GlobalHexMap.Instance.GetRandomCell(new List<GroundType>() { GroundType.Mountain, GroundType.Water, GroundType.HeavyForest, GroundType.Forest }, false, false, false);
            resourceManager.Create(500, resourceLibrary.Get(2), randomCell);
            randomCell = MG_GlobalHexMap.Instance.GetRandomCell(new List<GroundType>() { GroundType.Mountain, GroundType.Water, GroundType.HeavyForest, GroundType.Forest }, false, false, false);
            resourceManager.Create(500, resourceLibrary.Get(3), randomCell);

            //_unit.Init(randomCell, _sidePC);
            //_unit.gameObject.transform.position = randomCell.WorldPos;

            //randomCell = MG_GlobalHexMap.Instance.GetRandomCell(new List<GroundType>() { GroundType.Mountain, GroundType.Water, GroundType.HeavyForest, GroundType.Forest }, true, false, true);
            //_unit2.Side = _sidePC;
            //_unit2.Init(randomCell);
            //_unit2.gameObject.transform.position = randomCell.WorldPos;

            randomCell = MG_GlobalHexMap.Instance.GetRandomCell(new List<GroundType>() { GroundType.Mountain, GroundType.Water, GroundType.HeavyForest, GroundType.Forest }, false, false, true);
            // GameObject divisionObject2 = _unitObjectFactory.Create();
            // divisionObject2.GetComponent<MG_Division>().Init(randomCell, _sideEnemy);

            //game.OnTurnEnd();
            //-----TEST CODE!
            //var allUnits = MG_BasicUnit.GetAllUnits();

            

        }
        //_resizeSpriteToScreen.ResizeTileMapToScreen();
        //MG_HexCell starterCell = _cellGrid.GetCell(new Vector3Int(3, 3, 0));
        //--------------MG_LineOfSight.ShowVisibleArea(_starterCell.HexPos, 6);

        //_miniMap.Init();

    }
}
}
