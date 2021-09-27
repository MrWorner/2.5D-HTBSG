using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_SettlementManager : MonoBehaviour
{
    private static MG_SettlementManager instance;

    #region Поля
    [SerializeField] private List<MG_Settlement> _settlements;//поселение
    #endregion Поля

    #region Поля: Необходимые модули
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] GameObject _settlementPrefab;
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] MG_GlobalHexMap _map;//карта
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] MG_HexCellType _HexCellTypeForReplace;//тип клетки для замены после удаления города
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_HexBorderManager _borderManager;//[R] менеджер границ
    #endregion Поля: Необходимые модули

    #region Свойства
    public List<MG_Settlement> Settlements { get => _settlements; }//поселение
    #endregion Свойства

    #region Методы UNITY
    void Awake()
    {
        if (!instance)
            instance = this;
        else
            Debug.Log("<color=orange>MG_SettlementManager Awake(): найдет лишний _instance класса MG_SettlementManager.</color>");

        if (_settlementPrefab == null) Debug.Log("<color=red>MG_SettlementManager Awake(): объект '_settlementPrefab' не прикреплен!</color>");
        if (_map == null) Debug.Log("<color=red>MG_SettlementManager Awake(): объект '_map' не прикреплен!</color>");
        if (_HexCellTypeForReplace == null) Debug.Log("<color=red>MG_SettlementManager Awake(): объект '_HexCellTypeForReplace' не прикреплен!</color>");
        if (_borderManager == null) Debug.Log("<color=red>MG_SettlementManager Awake(): объект '_borderManager' не прикреплен!</color>");
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Установить постройку на клетке
    /// </summary>
    /// <param name="cell">клетка</param>
    /// <param name="side">сторона</param>
    public MG_Settlement Create(MG_HexCell cell, MG_Player side, MG_SettlementType type)//, GameObject buildingPrefab
    {
        if (cell.Settlement == null)
        {
            GameObject gameObj = Instantiate(_settlementPrefab);
            MG_Settlement settlement = gameObj.GetComponent<MG_Settlement>();
            settlement.Init(cell, side, type);
            //Debug.Log("MG_BuildingManager Show(): Construction completed!");
            Settlements.Add(settlement);
            return settlement;
        }

        Debug.Log("<color=red>MG_SettlementManager Create():</color> объект не был создан! Клетка занята.");
        return null;
    }

    public void Remove(MG_HexCell cell)
    {
        MG_Settlement settlement = cell.Settlement;
        if (settlement != null)
        {
            cell.SetSettlement(null);
            _settlements.Remove(settlement);

            IReadOnlyCollection<MG_HexCell> territoryCells = settlement.CellsUnderControl;
            if (territoryCells.Any())
            {
                foreach (var territoryCell in territoryCells.ToList())
                {
                    //var dict = territoryCell.VisionSubscribers;
                    //foreach (var value in dict)
                    //{
                    //    Debug.Log("Player=" + value.Key + " observer=" + value.Value.ToList()[0]);
                    //}

                    _borderManager.Erase(territoryCell);// Очистить территорию от хозяина              
                }
            }

            Destroy(settlement.gameObject);

            Tile tile = _HexCellTypeForReplace.GroundVariants[0];
            Tile tile_obj = _HexCellTypeForReplace.ObjectVariants[0];
            _map.UpdateCell(cell, _HexCellTypeForReplace, tile, tile_obj, TileOrigin.cellType);
            MG_PathAreaFixer.FixAfterCellUpdate(cell);
        }
    }


    /// <summary>
    /// Имеется ли на данной клетке Поселение
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public static bool HasSettlement(MG_HexCell cell)
    {
        return (cell.Settlement != null);
    }

    #endregion Публичные методы


    #region TESTING
    [BoxGroup("Для теста"), Required(InfoMessageType.Error), SerializeField] Vector3Int test_hexCellCoords;
    [BoxGroup("Для теста"), Required(InfoMessageType.Error), SerializeField] bool test_randomCoords;
    [BoxGroup("Для теста"), Required(InfoMessageType.Error), SerializeField] MG_SettlementType test_settlementType_City;
    [BoxGroup("Для теста"), Required(InfoMessageType.Error), SerializeField] MG_Player test_side;
    [Button]
    private void TEST_Create()
    {
        MG_HexCell cell;
        if (test_randomCoords)
            cell = MG_GlobalHexMap.Instance.GetRandomCell(new List<GroundType>() { GroundType.Mountain, GroundType.Water, GroundType.HeavyForest, GroundType.Forest }, false, false, true);
        else
            cell = MG_GlobalHexMap.Instance.GetCell(test_hexCellCoords);

        if (cell != null)
        {
            if (cell.Settlement == null)
            {
                var side = MG_TurnManager.GetCurrentPlayerTurn();
                if (side != null)
                {
                    var settlement = Create(cell, test_side, test_settlementType_City);
                    //Debug.Log("<color=green>MG_SettlementManager TEST_Create()</color>: объект успешно создан!");
                    return;
                }
            }
        }
        Debug.Log("<color=red>MG_SettlementManager TEST_Create():</color> объект не был создан!");
    }
    public static void TEST_Create_remotly()
    {
        //instance.test_hexCellCoords = new Vector3Int(5, 3, 0);
        instance.test_randomCoords = true;
        instance.TEST_Create();
    }

    #endregion TESTING
}
}


