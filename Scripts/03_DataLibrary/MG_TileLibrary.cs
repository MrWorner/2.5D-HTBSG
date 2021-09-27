using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_TileLibrary : MonoBehaviour
{
    [InfoBox("FILL", InfoMessageType.Warning), SerializeField] private List<Tile> tiles;//[R] лист тайлов
    public List<Tile> Tiles { get => tiles; }//[R] лист тайлов

    void Awake()
    {
        if (Tiles.Count == 0)
            Debug.Log("<color=red>MG_MapGenerator(): лист 'tiles' пустой!</color>");
    }
}
}
