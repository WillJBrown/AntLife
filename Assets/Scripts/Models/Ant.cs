using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnDir {Right3, Right2, Right, Straight, Left, Left2, Left3};

public class Ant
{
    TileMap tileMap;
    int numDirections;
    public List<TurnDir> Behaviour {get; protected set;}
    public Vector3Int lastPosition { get; protected set; }
    private Vector3Int position;
    public Vector3Int Position{
    get { return this.position; }
    protected set{
        this.lastPosition = this.position;
        this.position = value;
        }
    }

    // Using North=0, East=1, South=2, West=3
    private int lastFacing;
    public int LastFacing
    {
        get { return this.lastFacing; }
        protected set { this.lastFacing = (value + this.numDirections) % this.numDirections; }
    }

    // Using North=0, East=1, South=2, West=3
    private int facing;
    public int Facing
    {
        get { return this.facing; }
        protected set { this.lastFacing = this.facing;
            this.facing = (value + this.numDirections) % this.numDirections; }
    }

    public Ant(TileMap tileMap, List<TurnDir> behaviour, float speed, Vector3Int position, int facing = 1)
    {
        this.tileMap = tileMap;
        this.numDirections = this.tileMap.numDirections;
        int numStates = this.tileMap.numStates;
        this.Behaviour = new List<TurnDir>(behaviour);
        if (numStates < this.Behaviour.Count) {
            this.Behaviour.RemoveRange(numStates, this.Behaviour.Count - numStates);
            Debug.Log("Too many Instructions for the number of Tile States");
        } else if (numStates > this.Behaviour.Count) {
            World_Controller.Instance.CapTileStates(this.Behaviour.Count);
            Debug.Log("Too many Tile States for the number of instructions");
        }
        this.lastPosition = this.Position = position;
        this.LastFacing = this.Facing = facing;
    }

    public void MoveForward()
    {
        switch (this.Facing)
        {
            case 0:
                this.Position = this.Position + Vector3Int.up;
                break;
            case 1:
                this.Position = this.Position + Vector3Int.right;
                break;
            case 2:
                this.Position = this.Position + Vector3Int.down;
                break;
            case 3:
                this.Position = this.Position + Vector3Int.left;
                break;
            default:
                Debug.LogError("Somehow the direction isn't NESW");
                break;
        }
    }

    public void Turn(TurnDir dir)
    {
        this.Facing = this.Facing - ((int)dir -3);
    }

    public void LangtonStep(){
        this.Turn(this.Behaviour[this.tileMap.GetTileStateAt(this.Position)]);
        this.tileMap.IncrementTile(this.Position);
        this.MoveForward();
    }

}
