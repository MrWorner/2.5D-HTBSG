using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
[CreateAssetMenu(fileName = "hexBorder_", menuName = "--->MG/MG_HexBorder", order = 51)]
public class MG_HexBorder : ScriptableObject
{
    [SerializeField] private BorderDirection _id = BorderDirection.None;//айди
    [SerializeField] private Tile _tile;//тайл

    public BorderDirection Id { get => _id; }
    public Tile Tile { get => _tile; }
}
}
