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
    public Vector3Int LastPosition { get; protected set; }
    private Vector3Int position;
    public Vector3Int Position{
    get { return this.position; }
    protected set{
        this.LastPosition = this.position;
        this.lastTile = this.tileMap.GetTileAt(this.position);
        this.tile = this.tileMap.GetTileAt(value);
        this.position = value;
        }
    }
    public Tile tile {get; protected set;}
    public Tile lastTile {get; protected set;}

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

    public Ant(TileMap tileMap, List<TurnDir> behaviour, float speed, Tile t, int facing = 1)
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
        this.tile = this.lastTile = t;
        this.LastPosition = this.Position = t.Position;
        this.LastFacing = this.Facing = facing;
    }

    public void MoveForward()
    {   
        this.Position += this.tileMap.GetNeighbourDirections(this.Position)[this.facing];
    }

    public void Turn(TurnDir dir)
    {
        this.Facing = this.Facing - ((int)dir -3);
    }

    public void LangtonStep(){
        this.Turn(this.Behaviour[this.tile.State]);
        this.tile.State++;
        this.MoveForward();
    }

}
