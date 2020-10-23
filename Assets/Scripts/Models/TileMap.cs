using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

public enum TileShape { Tri, Quad, Hex, DiagQuad };

public class TileMap
{
    public int numStates {get; protected set;}

    public int numDirections {get; protected set;}
    public Dictionary<Vector3Int, Tile> Tiles { get; protected set; }

    public List<Vector3Int> NeighbourMap {get; protected set;}

    public TileShape Shape {
        get; protected set;
    }

    public TileMap(int numStates, TileShape shape = TileShape.Quad) {
        this.Shape = shape;
        switch(this.Shape){
            case TileShape.Tri:
                this.numDirections = 3;
                break;
            case TileShape.Quad:
                this.numDirections = 4;
                break;
            case TileShape.Hex:
                this.numDirections = 6;
                break;
            case TileShape.DiagQuad:
                this.numDirections = 8;
                break;
        }
        this.numStates = numStates;
        if (this.Shape != TileShape.Quad)
        {
            Debug.LogError("TileShapes other than Quad are not yet implemented.");
        }
        else
        {
            this.Tiles = new Dictionary<Vector3Int, Tile>();
            this.NeighbourMap = new List<Vector3Int>();
        }
    }
    
    public Tile AddTile(Vector3Int position)
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

    public int GetTileStateAt(Vector3Int position)
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

    public void IncrementTile(Vector3Int position, int increment = 1)
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

    public void CapTileStates(int numStates){
        foreach(Tile tile in this.Tiles.Values){
            tile.CapTileState(numStates);
        }
        this.numStates = numStates;
    }

    public Vector2 CartesianCoords(Vector3 Position){
        //Assumes a Triangle Height of 1.5 (Side Length Sqrt(3))
        if (this.numDirections == 4 || this.numDirections == 8) {
            return new Vector2(Position.x, Position.y);
        }
        return Vector2.zero;      

    }

    public Vector3 TileMapCoords( Vector2 Position){
        if (this.numDirections == 4 || this.numDirections == 8) {
            return new Vector3(Position.x, Position.y, 0);
        }
        return Vector3.zero;

    }

    void GenerateNeighbourList(){
        int[] dims = {3,4,6,8};
        if (!dims.Contains(this.numDirections)){
            Debug.LogError("Not a Valid number of Directions");
        }
        Vector3Int[] PossNeighbours = {
            Vector3Int.up,
            Vector3Int.right,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.up + Vector3Int.right,
            Vector3Int.right + Vector3Int.down,
            Vector3Int.down + Vector3Int.left,
            Vector3Int.left + Vector3Int.up};
        
    }
}
