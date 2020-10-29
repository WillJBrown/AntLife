using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MyExtensions;


public enum TileShape { Tri, Quad, Hex, DiagQuad };

namespace MyExtensions
{
    public static class Vector3Extensions{
        public static float Sum(this Vector3 pos){
        return pos.x + pos.y + pos.z;
        }
    }
    public static class Vector3IntExtensions{
        public static int Sum(this Vector3Int pos){
            return pos.x + pos.y + pos.z;
        }
    }
}

public class TileMap
{
    static float root2 = Mathf.Sqrt(2f);
    static float root3 = Mathf.Sqrt(3f);
    static float root6;

    public static float[,] RotatePlaneToXYZ;
    public static float[,] RotatePlaneFromXYZ;
    public int numStates { get; protected set; }
    public int numDirections { get; protected set; }
    public Dictionary<Vector3Int, Tile> Tiles { get; protected set; }
    public Vector3Int Centrepoint { get; protected set; } = Vector3Int.zero;
    List<Vector3Int> NeighbourMap;
    public TileShape Shape { get; protected set; }


    public TileMap(int numStates, TileShape shape = TileShape.Quad)
    {
        root6 = root2 * root3;
        RotatePlaneToXYZ = new float[,] {
            {-1/root2,      0f, 1/root2},
            { 1/root3, 1/root3, 1/root3},
            {-1/root6, 2/root6,-1/root6}};
        RotatePlaneFromXYZ = new float[,] {
            {-1/root2, 1/root3,-1/root6},
            {      0f, 1/root3, 2/root6},
            { 1/root2, 1/root3,-1/root6}};
        this.Shape = shape;
        if (this.Shape == TileShape.Tri || this.Shape == TileShape.Hex) {
            this.Centrepoint = new Vector3Int(1, 0, 0);
        }
        switch (this.Shape)
        {
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
        this.Tiles = new Dictionary<Vector3Int, Tile>();
        this.GenerateNeighbourList();
        if (this.numDirections != this.GetNeighbourDirections(this.Centrepoint).Count)
        {
            Debug.LogError("Number of Directions isn't equal to the length of the NeighbourMap");
        }
        //Debug.Log("x=1,y=0,z=1 ->"+this.TileMapCoords(new Vector3(1,0,1)));
    }

    public List<Vector3Int> GetNeighbourDirections(Vector3Int Position)
    {
        if (this.Shape != TileShape.Tri)
        {
            return this.NeighbourMap;
        }
        else if (IsHexCentre(Position))
        {
            return NeighbourMap.GetRange(0, 3);
        }
        else
        {
            return NeighbourMap.GetRange(3, 3);
        }
    }

    public List<Tile> GetNeighbours(Tile t){
        List<Vector3Int> NeighboursDirs = this.GetNeighbourDirections(t.Position);
        List<Tile> Retval = new List<Tile>();
        foreach(Vector3Int dir in NeighboursDirs){
            Retval.Add(this.GetTileAt(t.Position + dir));
        }
        return Retval;
    }

    public Tile GetTileAt(Vector3Int position)
    {
        if (this.Tiles.ContainsKey(position))
        {
            //Debug.LogError("Tried to make a duplicate tile");
            return this.Tiles[position];
        }
        else
        {
            Tile t = new Tile(this.numStates, 0, position);
            this.Tiles.Add(position, t);
            return t;
        }

    }

    public bool IsHexCentre(Vector3 position)
    {
        return (position.x + position.y + position.z) == 1;
    }

    public void CapTileStates(int numStates)
    {
        foreach (Tile tile in this.Tiles.Values)
        {
            tile.CapTileState(numStates);
        }
        this.numStates = numStates;
    }

    public Vector3 CartesianCoords(Vector3 Position)
    {
        if (this.numDirections == 4 || this.numDirections == 8)
        {
            return new Vector3(Position.x, 0, Position.y);
        }
        else if (this.numDirections == 3 || this.numDirections == 6)
        {
            float x = Position.x * TileMap.RotatePlaneToXYZ[0, 0] +
                Position.y * TileMap.RotatePlaneToXYZ[0, 1] +
                Position.z * TileMap.RotatePlaneToXYZ[0, 2];
            float z = Position.x * TileMap.RotatePlaneToXYZ[2, 0] +
                Position.y * TileMap.RotatePlaneToXYZ[2, 1] +
                Position.z * TileMap.RotatePlaneToXYZ[2, 2];
            Vector3 output = new Vector3(x,0,z);
            return root3 * output/root2;
        }
        else
        {
            Debug.LogError("Not a Valid number of Directions");
            return Vector2.zero;
        }
    }

    public Vector3 TileMapCoords(Vector3 Position)
    {
        //Debug.Log("TM.TileMapCoords input is " + Position);
        if (this.numDirections == 4 || this.numDirections == 8)
        {
            return new Vector3(Position.x, Position.z, 0);
        }
        else if (this.numDirections == 3 || this.numDirections == 6)
        {
            Vector3 newPosition = new Vector3(
                Position.x * TileMap.RotatePlaneFromXYZ[0, 0] +
                Position.y * TileMap.RotatePlaneFromXYZ[0, 1] +
                Position.z * TileMap.RotatePlaneFromXYZ[0, 2],
                Position.x * TileMap.RotatePlaneFromXYZ[1, 0] +
                Position.y * TileMap.RotatePlaneFromXYZ[1, 1] +
                Position.z * TileMap.RotatePlaneFromXYZ[1, 2],
                Position.x * TileMap.RotatePlaneFromXYZ[2, 0] +
                Position.y * TileMap.RotatePlaneFromXYZ[2, 1] +
                Position.z * TileMap.RotatePlaneFromXYZ[2, 2]);
            newPosition = newPosition * root2/root3;
            Vector3 output = RaiseFromPlane(newPosition);
            //Debug.Log("TM.TileMapCoords output is " + output);
            return output;
        }
        else
        {
            Debug.LogError("Not a Valid number of Directions");
            return Vector3Int.zero;
        }

    }

    public Tile GetClosestTile(Vector3 position){
        Vector3 TMCoords = this.TileMapCoords(position);
        Vector3 TMCoordsinPlane = this.DroptoPlane(TMCoords);
        if(this.Shape == TileShape.Quad || this.Shape == TileShape.DiagQuad){
            return this.GetTileAt(Vector3Int.RoundToInt(TMCoords));
        } else if (this.Shape == TileShape.Hex){
            return this.GetTileAt(Vector3Int.CeilToInt(TMCoords));
        } else if(Vector3Int.CeilToInt(TMCoordsinPlane).Sum() == 1){
            return this.GetTileAt(Vector3Int.CeilToInt(TMCoordsinPlane)); 
        } else {
            return this.GetTileAt(Vector3Int.FloorToInt(TMCoordsinPlane));
        }
    }

    public Vector3 RaiseFromPlane(Vector3 input){
        if (input.Sum() > Mathf.Pow(10f, -6f)){
            Debug.LogError("Not a Valid input on the plane");
            return Vector3.zero;
        }
        int Direction = 3 -  2 * Vector3Int.CeilToInt(input).Sum();
            float[] Delta = new float[3];
            if (Direction < 0){
                Delta[0] = input.x - Mathf.Floor(input.x);
                Delta[1] = input.y - Mathf.Floor(input.y);
                Delta[2] = input.z - Mathf.Floor(input.z);
            } else {
                Delta[0] = Mathf.Ceil(input.x) - input.x;
                Delta[1] = Mathf.Ceil(input.y) - input.y;
                Delta[2] = Mathf.Ceil(input.z) - input.z;
            }
            float MinDistance = Delta.Min();
            return input + Direction * MinDistance * Vector3.one;

    }

    public Vector3 DroptoPlane(Vector3 input){
        float sum = input.Sum();
        Vector3 output = input - 1/root3 * sum * Vector3.one;
        //Debug.Log("Dropped from " + input + " to " + output);
        return output;
    }

    void GenerateNeighbourList()
    {
        int[] dims = { 3, 4, 6, 8 };
        if (!dims.Contains(this.numDirections))
        {
            Debug.LogError("Not a Valid number of Directions");
        }
        List<Vector3Int> Neighbours;
        switch (this.numDirections)
        {
            case 3:
                Neighbours = new List<Vector3Int> {
                new Vector3Int(-1, 0, -1),
                new Vector3Int(-1, -1, 0),
                new Vector3Int( 0, -1, -1),
                new Vector3Int(0 , 1, 1),
                new Vector3Int(1, 0, 1),
                new Vector3Int(1, 1, 0)};
                this.NeighbourMap = Neighbours;
                break;
            case 4:
                Neighbours = new List<Vector3Int> {
                Vector3Int.up,
                Vector3Int.right,
                Vector3Int.down,
                Vector3Int.left};
                this.NeighbourMap = Neighbours;
                break;
            case 6:
                Neighbours = new List<Vector3Int> {
                new Vector3Int(-1, 1, 0),
                new Vector3Int(-1, 0, 1),
                new Vector3Int(0, -1, 1),
                new Vector3Int(1 , -1, 0),
                new Vector3Int(1, 0, -1),
                new Vector3Int(0, 1, -1)};
                this.NeighbourMap = Neighbours;
                break;
            case 8:
                Neighbours = new List<Vector3Int> {
                Vector3Int.up,
                Vector3Int.up + Vector3Int.right,
                Vector3Int.right,
                Vector3Int.down + Vector3Int.right,
                Vector3Int.down,
                Vector3Int.down + Vector3Int.left,
                Vector3Int.left,
                Vector3Int.up + Vector3Int.left};
                this.NeighbourMap = Neighbours;
                break;
        }
    }
}
