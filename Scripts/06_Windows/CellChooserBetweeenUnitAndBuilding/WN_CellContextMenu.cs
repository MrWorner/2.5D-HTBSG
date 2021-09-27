using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MG_StrategyGame
{
public class WN_CellContextMenu : MonoBehaviour
{
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private Image _unitImage;//[R] 
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private Image _buildingImage;//[R] 
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private GameObject _window;//[R] 
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_MouseS_UnitControl _controller;//[R] 
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_PathAreaVisualizator _pathAreaVisualizator;//[R] визуализатор доступной зоны для передвижения

    private static WN_CellContextMenu _instance;

    [BoxGroup("Дебагинг"), SerializeField, ReadOnly] private MG_Division _chosenUnit;
    [BoxGroup("Дебагинг"), SerializeField, ReadOnly] private MG_Settlement _chosenSettlement;


    public static WN_CellContextMenu Instance { get => _instance; }

    private void Awake()
    {
        if (!_unitImage) Debug.Log("<color=red>WN_CellContextMenu Awake(): объект 'unitImage' не прикреплен!</color>");
        if (!_buildingImage) Debug.Log("<color=red>WN_CellContextMenu Awake(): объект 'buildingImage' не прикреплен!</color>");
        if (!_window) Debug.Log("<color=red>WN_CellContextMenu Awake(): объект 'window' не прикреплен!</color>");
        if (!_controller) Debug.Log("<color=red>WN_CellContextMenu Awake(): объект 'controller' не прикреплен!</color>");
        if (!_pathAreaVisualizator) Debug.Log("<color=red>WN_CellContextMenu Awake(): объект '_pathAreaVisualizator' не прикреплен!</color>");
        if (!_instance)
            WN_CellContextMenu._instance = this;
        else
            Debug.Log("<color=red>WN_CellContextMenu Awake(): 'Instance' обнаружен дубликат!</color>");
    }

    public void Show(MG_Division unit, MG_Settlement settlement)
    {
        _chosenUnit = unit;
        _chosenSettlement = settlement;
        _unitImage.sprite = _chosenUnit.SpriteR.sprite;
        _buildingImage.sprite = _chosenSettlement.ChosenTileObj.sprite;
        if (!_window.activeSelf)
            _window.SetActive(true);
    }

    public void Close()
    {
        _chosenUnit = null;
        _chosenSettlement = null;
        if (_window.activeSelf)
            _window.SetActive(false);
    }

    public void UnitButtonPressed()
    {
        if (_chosenUnit.Side.Equals(MG_TurnManager.GetCurrentPlayerTurn()))
        {
            _chosenUnit.OnSelect();
            _controller.ChosenUnit = _chosenUnit;
            _pathAreaVisualizator.Show(_chosenUnit.PathsInRange);
        }      
        Close();
    }

    public void BuildingButtonPressed()
    {
        if (_chosenSettlement.Side.Equals(MG_TurnManager.GetCurrentPlayerVisibleMap()))
        {
            WN_SettlementView.Instance.Show(_chosenSettlement);
        }       
        Close();
    }

}
}
