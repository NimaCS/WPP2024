using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Season Tile Data", menuName = "ScriptableObjects/Season Tile Data")]
public class SeasonTilesSO : ScriptableObject
{
    public string seasonName;
    public List<Tile> tiles;
}
