using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
[CreateAssetMenu(fileName = "hexArrow_", menuName = "--->MG/MG_HexArrow", order = 51)]
public class MG_HexArrow : ScriptableObject
{
    [SerializeField] private ArrowDirection _id = ArrowDirection.None;//айди
    [SerializeField] private Tile _tile;//тайл

    public ArrowDirection Id { get => _id; }//айди
    public Tile Tile { get => _tile; }//тайл
}
}
