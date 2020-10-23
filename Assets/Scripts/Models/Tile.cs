using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tile
{
    int NumStates;
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
            if (this.NumStates != 0) 
            {
                this.state = value % this.NumStates;
                if (cbTileStateChanged != null)
                {
                    cbTileStateChanged(this);
                }
            }
            else
            {
                Debug.LogError("Trying to set the state of a Tile without having set the number of states first.");
            }
        }
    }

    public Tile(int numStates, int state, Vector3Int position)
    {
        if (numStates > 0)
        {
            this.NumStates = numStates;
        }
        else { Debug.LogError("Must have at least one possible tile state"); }
        this.State = state;
        this.Position = position;
    }

    public void RegisterTileStateChangedCallBack(Action<Tile> callback)
    {
        cbTileStateChanged += callback;
    }

    public void UnregisterTileStateChangedCallBack(Action<Tile> callback)
    {
        cbTileStateChanged -= callback;
    }

    public void CapTileState(int numStates){
        if (this.State >= numStates){
            this.State = 0;
        }
        this.NumStates = numStates;
    }
}
