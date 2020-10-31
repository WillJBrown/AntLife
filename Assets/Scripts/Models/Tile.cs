using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tile
{
    int NumberOfStates;
    Action<Tile> cbTileStateChanged;
    public Vector3Int Position { get; protected set; }
    private int state;
    public int State
    {
        get
        {
            return this.state;
        }
        set
        {
            if (this.NumberOfStates != 0)
            {
                int newstate = value % this.NumberOfStates;
                if (this.state != newstate)
                {
                    this.state = newstate;
                    if (cbTileStateChanged != null)
                    {
                        cbTileStateChanged(this);
                    }
                }

            }
            else
            {
                Debug.LogError("Trying to set the state of a Tile without having set the number of states first.");
            }
        }
    }
    public List<Ant> Ants { get; protected set; }

    public Tile(int numberOfStates, int state, Vector3Int position)
    {
        if (numberOfStates > 0)
        {
            this.NumberOfStates = numberOfStates;
        }
        else { Debug.LogError("Must have at least one possible tile state"); }
        this.State = state;
        this.Position = position;
        this.Ants = new List<Ant>();
    }

    public void AddAnt(Ant ant)
    {
        this.Ants.Add(ant);
    }

    public void RemoveAnt(Ant ant)
    {
        if (this.Ants.Contains(ant))
        {
            this.Ants.Remove(ant);
        }
        else
        {
            Debug.LogError("Tried to remove ant " + ant + " from Tile " + this + " but it is not in the Ant List");
        }
    }

    public void RegisterTileStateChangedCallBack(Action<Tile> callback)
    {
        cbTileStateChanged += callback;
    }

    public void UnregisterTileStateChangedCallBack(Action<Tile> callback)
    {
        cbTileStateChanged -= callback;
    }

    public void CapTileState(int numberOfStates)
    {
        if (this.State >= numberOfStates)
        {
            this.State = 0;
        }
        this.NumberOfStates = numberOfStates;
    }
}
