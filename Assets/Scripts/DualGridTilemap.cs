using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Tilemaps;
using static TileType;
using static TileSeason;
using static UnityEditor.PlayerSettings;

public class DualGridTilemap : MonoBehaviour
{
    protected static Vector3Int[] NEIGHBOURS = new Vector3Int[] {
        new Vector3Int(0, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 1, 0)
    };

    protected static Dictionary<Tuple<TileType, TileType, TileType, TileType>, int> neighbourTupleToTileGD;
    protected static Dictionary<Tuple<TileType, TileType, TileType, TileType>, int> neighbourTupleToTileG;

    // Tilemaps
    public Tilemap seasonDataTilemap;
    public Tilemap placeholderTilemap;
    public Tilemap displayTilemap;
    public Tilemap takeoverTilemap;
    public Tilemap takeoverDisplayTilemap;

    // Placeholder tiles
    public Tile fallPlaceholderTile;
    public Tile winterPlaceholderTile;
    public Tile springPlaceholderTile;
    public Tile summerPlaceholderTile;
    public Tile grassPlaceholderTile;
    public Tile dirtPlaceholderTile;
    public Tile waterPlaceholderTile;
    public Tile gravelPlaceholderTile;

    // Tiles
    public Tile[] fallTiles;
    public Tile[] winterTiles;
    public Tile[] springTiles;
    public Tile[] summerTiles;
    public Tile[] dirtlessFallTiles;
    public Tile[] dirtlessWinterTiles;
    public Tile[] dirtlessSpringTiles;
    public Tile[] dirtlessSummerTiles;

    public Tile fallWaterTile;
    public Tile winterWaterTile;
    public Tile springWaterTile;
    public Tile summerWaterTile;

    void Start()
    {
        seasonDataTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
        placeholderTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;

        // This dictionary stores the "rules", each 4-neighbour configuration corresponds to a tile
        // |_1_|_2_|
        // |_3_|_4_|
        neighbourTupleToTileGD = new() {
            {new (Grass, Grass, Grass, Grass), 6},
            {new (Dirt, Dirt, Dirt, Grass), 13}, // OUTER_BOTTOM_RIGHT
            {new (Dirt, Dirt, Grass, Dirt), 0}, // OUTER_BOTTOM_LEFT
            {new (Dirt, Grass, Dirt, Dirt), 8}, // OUTER_TOP_RIGHT
            {new (Grass, Dirt, Dirt, Dirt), 15}, // OUTER_TOP_LEFT
            {new (Dirt, Grass, Dirt, Grass), 1}, // EDGE_RIGHT
            {new (Grass, Dirt, Grass, Dirt), 11}, // EDGE_LEFT
            {new (Dirt, Dirt, Grass, Grass), 3}, // EDGE_BOTTOM
            {new (Grass, Grass, Dirt, Dirt), 9}, // EDGE_TOP
            {new (Dirt, Grass, Grass, Grass), 5}, // INNER_BOTTOM_RIGHT
            {new (Grass, Dirt, Grass, Grass), 2}, // INNER_BOTTOM_LEFT
            {new (Grass, Grass, Dirt, Grass), 10}, // INNER_TOP_RIGHT
            {new (Grass, Grass, Grass, Dirt), 7}, // INNER_TOP_LEFT
            {new (Dirt, Grass, Grass, Dirt), 14}, // DUAL_UP_RIGHT
            {new (Grass, Dirt, Dirt, Grass), 4}, // DUAL_DOWN_RIGHT
            {new (Dirt, Dirt, Dirt, Dirt), 12},
        };

        neighbourTupleToTileG = new() {
            {new (Grass, Grass, Grass, Grass), 6},
            {new (TileType.None, TileType.None, TileType.None, Grass), 13}, // OUTER_BOTTOM_RIGHT
            {new (TileType.None, TileType.None, Grass, TileType.None), 0}, // OUTER_BOTTOM_LEFT
            {new (TileType.None, Grass, TileType.None, TileType.None), 8}, // OUTER_TOP_RIGHT
            {new (Grass, TileType.None, TileType.None, TileType.None), 15}, // OUTER_TOP_LEFT
            {new (TileType.None, Grass, TileType.None, Grass), 1}, // EDGE_RIGHT
            {new (Grass, TileType.None, Grass, TileType.None), 11}, // EDGE_LEFT
            {new (TileType.None, TileType.None, Grass, Grass), 3}, // EDGE_BOTTOM
            {new (Grass, Grass, TileType.None, TileType.None), 9}, // EDGE_TOP
            {new (TileType.None, Grass, Grass, Grass), 5}, // INNER_BOTTOM_RIGHT
            {new (Grass, TileType.None, Grass, Grass), 2}, // INNER_BOTTOM_LEFT
            {new (Grass, Grass, TileType.None, Grass), 10}, // INNER_TOP_RIGHT
            {new (Grass, Grass, Grass, TileType.None), 7}, // INNER_TOP_LEFT
            {new (TileType.None, Grass, Grass, TileType.None), 14}, // DUAL_UP_RIGHT
            {new (Grass, TileType.None, TileType.None, Grass), 4}, // DUAL_DOWN_RIGHT
            {new (TileType.None, TileType.None, TileType.None, TileType.None), 12},
        };

        RefreshDisplayTilemap();
        //RefreshTakeoverDisplayTilemap();
    }

    public void SetCell(Vector3Int coords, Tile tile)
    {
        placeholderTilemap.SetTile(coords, tile);
        setDisplayTile(coords);
    }

    private TileType getPlaceholderTileTypeAt(Vector3Int coords, Tilemap tilemap)
    {
        if (tilemap.GetTile(coords) == grassPlaceholderTile)
            return Grass;
        else if (tilemap.GetTile(coords) == dirtPlaceholderTile)
            return Dirt;
        else if (tilemap.GetTile(coords) == waterPlaceholderTile)
            return Water;
        else if (tilemap.GetTile(coords) == gravelPlaceholderTile)
            return Gravel;
        else
            return TileType.None;
    }

    private TileSeason getPlaceholderTileSeasonAt(Vector3Int coords, Tilemap tilemap)
    {
        if (tilemap.GetTile(coords) == fallPlaceholderTile)
            return Fall;
        else if (tilemap.GetTile(coords) == winterPlaceholderTile)
            return Winter;
        else if (tilemap.GetTile(coords) == springPlaceholderTile)
            return Spring;
        else if (tilemap.GetTile(coords) == summerPlaceholderTile)
            return Summer;
        else
            return Fall;

    }

    protected Tile calculateDisplayTile(Vector3Int coords)
    {
        bool hasNone, hasDirt, isWater;
        hasNone = false;
        hasDirt = false;
        isWater = false;

        // 4 neighbours
        TileType topRight = getPlaceholderTileTypeAt(coords - NEIGHBOURS[0], placeholderTilemap);
        TileType topLeft = getPlaceholderTileTypeAt(coords - NEIGHBOURS[1], placeholderTilemap);
        TileType botRight = getPlaceholderTileTypeAt(coords - NEIGHBOURS[2], placeholderTilemap);
        TileType botLeft = getPlaceholderTileTypeAt(coords - NEIGHBOURS[3], placeholderTilemap);

        TileSeason tileSeason = getPlaceholderTileSeasonAt(coords, seasonDataTilemap);

        if (topRight == Water && topLeft == Water && botRight == Water && botLeft == Water)
            isWater = true;

        if (topRight == Gravel)
            topRight = TileType.None;
        if (topLeft == Gravel)
            topLeft = TileType.None;
        if (botRight == Gravel)
            botRight = TileType.None;
        if (botLeft == Gravel)
            botLeft = TileType.None;

        if (topRight == Water && !isWater)
            topRight = Dirt;
        if (topLeft == Water && !isWater)
            topLeft = Dirt;
        if (botRight == Water && !isWater)
            botRight = Dirt;
        if (botLeft == Water && !isWater)
            botLeft = Dirt;

        Tuple<TileType, TileType, TileType, TileType> neighbourTuple = new(topLeft, topRight, botLeft, botRight);

        if (neighbourTuple.Item1 == TileType.None || neighbourTuple.Item2 == TileType.None || neighbourTuple.Item3 == TileType.None || neighbourTuple.Item4 == TileType.None)
            hasNone = true;
        if (neighbourTuple.Item1 == Dirt || neighbourTuple.Item2 == Dirt || neighbourTuple.Item3 == Dirt || neighbourTuple.Item4 == Dirt)
            hasDirt = true;
        if (neighbourTuple.Item1 == Grass && neighbourTuple.Item2 == Grass && neighbourTuple.Item3 == Grass && neighbourTuple.Item4 == Grass)
            hasDirt = true;

        Tile returnTile = null;

        if (hasDirt && !hasNone && !isWater)
        {
            if (tileSeason == Winter)
                returnTile = winterTiles[neighbourTupleToTileGD[neighbourTuple]];
            else if (tileSeason == Spring)
                returnTile = springTiles[neighbourTupleToTileGD[neighbourTuple]];
            else if (tileSeason == Summer)
                returnTile = summerTiles[neighbourTupleToTileGD[neighbourTuple]];
            else if (tileSeason == Fall)
                returnTile = fallTiles[neighbourTupleToTileGD[neighbourTuple]];
        }
        else if (hasNone && !hasDirt && !isWater)
        {
            if (tileSeason == Winter)
                returnTile = dirtlessWinterTiles[neighbourTupleToTileG[neighbourTuple]];
            else if (tileSeason == Spring)
                returnTile = dirtlessSpringTiles[neighbourTupleToTileG[neighbourTuple]];
            else if (tileSeason == Summer)
                returnTile = dirtlessSummerTiles[neighbourTupleToTileG[neighbourTuple]];
            else if (tileSeason == Fall)
                returnTile = dirtlessFallTiles[neighbourTupleToTileG[neighbourTuple]];
        }
        else if (isWater)
        {
            if (tileSeason == Winter)
                returnTile = winterWaterTile;
            else if (tileSeason == Spring)
                returnTile = springWaterTile;
            else if (tileSeason == Summer)
                returnTile = summerWaterTile;
            else if (tileSeason == Fall)
                returnTile = fallWaterTile;
        }
        else if (hasNone && hasDirt)
            Debug.Log("uhhhh" + coords);

        return returnTile;
    }

    protected Tile calculateTakeoverDisplayTile(Vector3Int coords)
    {
        bool hasNone, hasDirt, isWater;
        hasNone = false;
        hasDirt = false;
        isWater = false;

        // 4 neighbours
        TileType topRight = getPlaceholderTileTypeAt(coords - NEIGHBOURS[0], placeholderTilemap);
        TileType topLeft = getPlaceholderTileTypeAt(coords - NEIGHBOURS[1], placeholderTilemap);
        TileType botRight = getPlaceholderTileTypeAt(coords - NEIGHBOURS[2], placeholderTilemap);
        TileType botLeft = getPlaceholderTileTypeAt(coords - NEIGHBOURS[3], placeholderTilemap);

        TileSeason topRightSN = getPlaceholderTileSeasonAt(coords - NEIGHBOURS[0], takeoverTilemap);
        TileSeason topLeftSN = getPlaceholderTileSeasonAt(coords - NEIGHBOURS[1], takeoverTilemap);
        TileSeason botRightSN = getPlaceholderTileSeasonAt(coords - NEIGHBOURS[2], takeoverTilemap);
        TileSeason botLeftSN = getPlaceholderTileSeasonAt(coords - NEIGHBOURS[3], takeoverTilemap);

        TileSeason tileSeason = TileSeason.None;

        if (topRightSN == Fall && topLeftSN == Fall && botRightSN == Fall && botLeftSN == Fall)
            tileSeason = Fall;
        if (topRightSN == Winter && topLeftSN == Winter && botRightSN == Winter && botLeftSN == Winter)
            tileSeason = Winter;
        if (topRightSN == Spring && topLeftSN == Spring && botRightSN == Spring && botLeftSN == Spring)
            tileSeason = Spring;
        if (topRightSN == Summer && topLeftSN == Summer && botRightSN == Summer && botLeftSN == Summer)
            tileSeason = Summer;

        if (topRight == Water && topLeft == Water && botRight == Water && botLeft == Water)
            isWater = true;

        if (topRight == Gravel)
            topRight = TileType.None;
        if (topLeft == Gravel)
            topLeft = TileType.None;
        if (botRight == Gravel)
            botRight = TileType.None;
        if (botLeft == Gravel)
            botLeft = TileType.None;

        if (topRight == Water && !isWater)
            topRight = Dirt;
        if (topLeft == Water && !isWater)
            topLeft = Dirt;
        if (botRight == Water && !isWater)
            botRight = Dirt;
        if (botLeft == Water && !isWater)
            botLeft = Dirt;

        Tuple<TileType, TileType, TileType, TileType> neighbourTuple = new(topLeft, topRight, botLeft, botRight);

        if (neighbourTuple.Item1 == TileType.None || neighbourTuple.Item2 == TileType.None || neighbourTuple.Item3 == TileType.None || neighbourTuple.Item4 == TileType.None)
            hasNone = true;
        if (neighbourTuple.Item1 == Dirt || neighbourTuple.Item2 == Dirt || neighbourTuple.Item3 == Dirt || neighbourTuple.Item4 == Dirt)
            hasDirt = true;
        if (neighbourTuple.Item1 == Grass && neighbourTuple.Item2 == Grass && neighbourTuple.Item3 == Grass && neighbourTuple.Item4 == Grass)
            hasDirt = true;

        Tile returnTile = null;

        if (hasDirt && !hasNone && !isWater)
        {
            if (tileSeason == Winter)
                returnTile = winterTiles[neighbourTupleToTileGD[neighbourTuple]];
            else if (tileSeason == Spring)
                returnTile = springTiles[neighbourTupleToTileGD[neighbourTuple]];
            else if (tileSeason == Summer)
                returnTile = summerTiles[neighbourTupleToTileGD[neighbourTuple]];
            else if (tileSeason == Fall)
                returnTile = fallTiles[neighbourTupleToTileGD[neighbourTuple]];
        }
        else if (hasNone && !hasDirt && !isWater)
        {
            if (tileSeason == Winter)
                returnTile = dirtlessWinterTiles[neighbourTupleToTileG[neighbourTuple]];
            else if (tileSeason == Spring)
                returnTile = dirtlessSpringTiles[neighbourTupleToTileG[neighbourTuple]];
            else if (tileSeason == Summer)
                returnTile = dirtlessSummerTiles[neighbourTupleToTileG[neighbourTuple]];
            else if (tileSeason == Fall)
                returnTile = dirtlessFallTiles[neighbourTupleToTileG[neighbourTuple]];
        }
        else if (isWater)
        {
            if (tileSeason == Winter)
                returnTile = winterWaterTile;
            else if (tileSeason == Spring)
                returnTile = springWaterTile;
            else if (tileSeason == Summer)
                returnTile = summerWaterTile;
            else if (tileSeason == Fall)
                returnTile = fallWaterTile;
        }
        else if (hasNone && hasDirt)
            Debug.Log("uhhhh" + coords);

        return returnTile;
    }

    protected void setDisplayTile(Vector3Int pos)
    {
        for (int i = 0; i < NEIGHBOURS.Length; i++)
        {
            Vector3Int newPos = pos + NEIGHBOURS[i];
            displayTilemap.SetTile(newPos, calculateDisplayTile(newPos));
        }
    }

    private void setTakeoverDisplayTile(Vector3Int pos)
    {
        for (int i = 0; i < NEIGHBOURS.Length; i++)
        {
            Vector3Int newPos = pos + NEIGHBOURS[i];
            takeoverDisplayTilemap.SetTile(newPos, calculateTakeoverDisplayTile(newPos));
        }
    }

    // The tiles on the display tilemap will recalculate themselves based on the placeholder tilemap
    public void RefreshDisplayTilemap()
    {
        for (int i = -150; i < 150; i++)
        {
            for (int j = -150; j < 150; j++)
            {
                setDisplayTile(new Vector3Int(i, j, 0));
            }
        }
    }

    public void RefreshTakeoverDisplayTilemap()
    {
        for (int i = -150; i < 150; i++)
        {
            for (int j = -150; j < 150; j++)
            {
                setTakeoverDisplayTile(new Vector3Int(i, j, 0));
            }
        }
    }
}

public enum TileType
{
    None,
    Grass,
    Dirt,
    Water,
    Gravel
}

public enum TileSeason
{
    None,
    Fall,
    Winter,
    Spring,
    Summer
}
