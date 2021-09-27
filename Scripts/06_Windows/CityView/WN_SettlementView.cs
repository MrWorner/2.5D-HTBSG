using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public class WN_SettlementView : MonoBehaviour
{
    [SerializeField] private GameObject window;//[R]
    [SerializeField] private MG_Settlement chosenSettlement;

    public static WN_SettlementView Instance { get; set; }

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Debug.Log("<color=orange>WN_SettlementView Awake(): найдет лишний _instance класса WN_CityView.</color>");
        if (!window)
            Debug.Log("<color=orange>WN_SettlementView Awake():'window' не задан.</color>");

        window.SetActive(false);
    } 

    public void Show(MG_Settlement settlement)
    {
        chosenSettlement = settlement;
        window.SetActive(true);
        chosenSettlement.OnSelect(true);
    }

    public void Close()
    {
        window.SetActive(false);
        chosenSettlement.OnDeselect();
    }
}
}
