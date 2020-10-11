using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum TileShape { Tri, Quad, Hex };

public class TileMap
{
    int numStates;
    public Dictionary<Vector2Int, Tile> Tiles { get; protected set; }

    public TileShape Shape {
        get; protected set;
    }

    public TileMap(int numStates, TileShape shape = TileShape.Quad) {
        this.Shape = shape;
        this.numStates = numStates;
        if (this.Shape != TileShape.Quad)
        {
            Debug.LogError("TileShapes other than Quad are not yet implemented.");
        }
        else
        {
            this.Tiles = new Dictionary<Vector2Int, Tile>();
        }
    }
    
    public Tile AddTile(Vector2Int position)
    {
        if (this.Tiles.ContainsKey(position))
        {
            Debug.LogError("Tried to make a duplicate tile");
            return this.Tiles[position];
        }
        else
        {
            Tile t = new Tile(this.numStates, 0, position);
            this.Tiles.Add(position, t);
            return t;
        }

    }

    public int GetTileStateAt(Vector2Int position)
    {
        if (this.Tiles.ContainsKey(position) == false)
        {
            Debug.LogError("Tried to get a non-existent tile");
            return 0;
        }
        else
        {
            return Tiles[position].State;
        }
    }

    public void IncrementTile(Vector2Int position, int increment = 1)
    {
        if (this.Tiles.ContainsKey(position) == false)
        {
            Debug.LogError("Tried increment a non-existent tile");
            return;
        }
        else
        {
            Tiles[position].State = Tiles[position].State + increment;
        }
    }
}
