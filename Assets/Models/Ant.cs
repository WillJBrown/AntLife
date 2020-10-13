using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ant
{
    public int Steps { get; protected set; }
    int lastSteps = 0;
    public float Speed;
    public float StepsPerSecond { get; protected set; }
    float movementPercentage = 0f;
    float rotationPercentage = 0f;
    float timer = 0f;
    Action<Ant> cbAntChanged;
    public Vector2Int Position { get; protected set; }
    Vector2Int nextPosition;
    private Vector3 _ContPosition;
    public Vector3 ContPosition
    {
        get { return this._ContPosition; }
        set
        { 
            this._ContPosition = value;
            if (cbAntChanged != null)
            {
                cbAntChanged(this);
            }
        }
    }
    private float _ContRotation;
    public float ContRotation
    {
        get { return this._ContRotation; }
        set
        {
            this._ContRotation = value;
            if (cbAntChanged != null)
            {
                cbAntChanged(this);
            }
        }
    }

    // Using North=0, East=1, South=2, West=3
    private int _Facing;
    public int Facing
    {
        get { return this._Facing; }
        protected set { this._Facing = (value + 4) % 4; }
    }

    // Using North=0, East=1, South=2, West=3
    private int _nextFacing;
    int nextFacing
    {
        get { return this._nextFacing; }
        set { this._nextFacing = (value + 4) % 4; }
    }

    public Ant(float speed, Vector2Int position, int facing = 1)
    {
        this.Steps = 0;
        this.Speed = speed;
        this.Position = this.nextPosition = position;
        this.Facing = this.nextFacing = facing;
        this.SetContinuousVariables();
    }

    void SetContinuousVariables()
    {
        float x = Mathf.Lerp(this.Position.x, this.nextPosition.x, this.movementPercentage);
        float z = Mathf.Lerp(this.Position.y, this.nextPosition.y, this.movementPercentage);
        this.ContPosition = new Vector3(x, 0f, z);
        this.ContRotation = Clerp(90.0f * this.Facing, 90.0f * this.nextFacing, this.rotationPercentage);
    }

public static float Clerp(float start, float end, float value)
{
    float min = 0.0f;
    float max = 360.0f;
    float half = Mathf.Abs((max - min) / 2.0f);//half the distance between min and max
    float retval = 0.0f;
    float diff = 0.0f;

    if ((end - start) < -half)
    {
        diff = ((max - start) + end) * value;
        retval = start + diff;
    }
    else if ((end - start) > half)
    {
        diff = -((max - end) + start) * value;
        retval = start + diff;
    }
    else retval = start + (end - start) * value;

    // Debug.Log("Start: "  + start + "   End: " + end + "  Value: " + value + "  Half: " + half + "  Diff: " + diff + "  Retval: " + retval);
    return retval;
}

    public void ResetMovement(){
        this.Facing = this.nextFacing;
        this.Position = this.nextPosition;
        this.movementPercentage = 0;
        this.SetContinuousVariables();
    }
    public void AntUpdate(float deltaTime)
    {
        if (this.Speed <= 60)
        {
            if (this.Facing != this.nextFacing)
            {
                if (rotationPercentage <= 1)
                {
                    rotationPercentage += 2 * deltaTime * this.Speed;
                }
                else
                {
                    this.Facing = this.nextFacing;
                }
            }
            else
            {
                if (movementPercentage <= 1)
                {
                    movementPercentage += 2 * deltaTime * this.Speed;
                }
                else
                {
                    this.Position = this.nextPosition;
                }
            }

            if (this.isMoving() == false)
            {
                movementPercentage = rotationPercentage = 0f;
            }
        }
        else
        {
            movementPercentage = rotationPercentage = 0f;
            this.Facing = this.nextFacing;
            this.Position = this.nextPosition;
        }
        updateSteps(deltaTime);
        this.SetContinuousVariables();
    }

    void updateSteps(float deltaTime)
    {
        this.timer += deltaTime;
        if (this.Steps == 0) {
            // Stops it crashing in the first second
            return;
        }
        if (this.timer >= 1)
        {
            this.StepsPerSecond = this.Steps - this.lastSteps;
            this.lastSteps = this.Steps;
            this.timer = 0f;
        }
    }
    
    public bool isMoving()
    {
        return (this.Position != this.nextPosition || this.Facing != this.nextFacing);
    }

    public void MoveForward()
    {
        switch (this.nextFacing)
        {
            case 0:
                this.nextPosition = this.Position + Vector2Int.up;
                break;
            case 1:
                this.nextPosition = this.Position + Vector2Int.right;
                break;
            case 2:
                this.nextPosition = this.Position + Vector2Int.down;
                break;
            case 3:
                this.nextPosition = this.Position + Vector2Int.left;
                break;
            default:
                Debug.LogError("Somehow the direction isn't NESW");
                break;
        }
        this.Steps++;
    }

    public void TurnRight()
    {
        this.nextFacing = this.Facing + 1;
    }

    public void TurnLeft()
    {
        this.nextFacing = this.Facing - 1;
    }

    public void RegisterAntChangedCallBack(Action<Ant> callback)
    {
        cbAntChanged += callback;
    }

    public void UnregisterAntChangedCallBack(Action<Ant> callback)
    {
        cbAntChanged -= callback;
    }

}
