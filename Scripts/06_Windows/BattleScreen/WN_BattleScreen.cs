using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public class WN_BattleScreen : MonoBehaviour
{
    [SerializeField] private GameObject window;

    [Required("NOT required", InfoMessageType.Warning)] public MG_Division chosenUnit;
    [Required("NOT required", InfoMessageType.Warning)] public MG_Division chosenEnemy;

    public static WN_BattleScreen Instance { get; set; }

    private void Awake()
    {
        WN_BattleScreen.Instance = this;
    }

    public void ShowWindow(MG_Division unit, MG_Division enemy)
    {
        chosenUnit = unit;
        chosenEnemy = enemy;
        window.gameObject.SetActive(true);
    }


    public void ChosenWIN()
    {
        window.gameObject.SetActive(false);
        chosenEnemy.RemoveFromGame();
    }

    public void ChosenLOSE()
    {
        window.gameObject.SetActive(false);
        chosenUnit.RemoveFromGame();
    }
}
}
