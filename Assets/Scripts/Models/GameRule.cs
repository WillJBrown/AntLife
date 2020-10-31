using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRule
{
    GameBoard Board;

    string[] Parameters;
    //variable, relational operator, variable, logical operator, variable, relational operator, variable, IsIncrement? (As oppose to Assignment), value

    public GameRule(GameBoard board, string[] parameters)
    {
        if (parameters.Length != 9)
        {
            Debug.LogError("GameRule Constructor was not passed a valid length parameter array");
        }
        this.Board = board;
        this.Parameters = parameters;
    }

    public bool Execute(Tile tile, int count)
    {
        int Variable1 = this.GetVariable(this.Parameters[0], tile, count);
        int Variable2 = this.GetVariable(this.Parameters[2], tile, count);
        int Variable3 = this.GetVariable(this.Parameters[4], tile, count);
        int Variable4 = this.GetVariable(this.Parameters[6], tile, count);
        bool First = DoRelationalOperation(this.Parameters[1], Variable1, Variable2);
        bool Second = DoRelationalOperation(this.Parameters[5], Variable3, Variable4);
        bool IsSatisfied = DoLogicalOperation(this.Parameters[3], First, Second);
        int Output = this.GetVariable(this.Parameters[8], tile, count);
        if (IsSatisfied)
        {
            if (IsIncrement(this.Parameters[7]))
            {
                if (tile.State + Output < 0)
                {
                    tile.State = 0;
                }
                else if (tile.State + Output >= this.Board.numStates)
                {
                    tile.State = this.Board.numStates - 1;
                }
                else
                {
                    tile.State = tile.State + Output;
                }
            }
            else
            {
                tile.State = Output;
            }
        }
        return IsSatisfied;
    }

    int GetVariable(string valueString, Tile tile, int count)
    {
        valueString = valueString.ToLower();
        if (int.TryParse(valueString, out int result))
        {
            return result;
        }
        switch (valueString)
        {
            case "state":
                return tile.State;
            case "count":
                return count;
            case "number of states":
                return this.Board.numStates;
            case "maximum state index":
                return this.Board.numStates - 1;
            case "number of directions":
                return this.Board.numDirections;
            case "maximum direction index":
                return this.Board.numDirections - 1;
            case "number of ants":
                return tile.Ants.Count;
            default:
                Debug.LogError(valueString + " is not a valid variable keyword");
                return 0;
        }
    }

    bool DoRelationalOperation(string relationalOperator, int first, int second)
    {
        relationalOperator = relationalOperator.ToLower();
        switch (relationalOperator)
        {
            case "<":
                return first < second;
            case ">":
                return first < second;
            case "<=":
                return first <= second;
            case ">=":
                return first >= second;
            case "is":
                return first == second;
            case "is not":
                return first != second;
            default:
                Debug.LogError(relationalOperator + " is not a valid relational operator");
                return false;
        }
    }

    bool DoLogicalOperation(string logicalOperator, bool first, bool second)
    {
        logicalOperator = logicalOperator.ToUpper();
        switch (logicalOperator)
        {
            case "AND":
                return first && second;
            case "OR":
                return first || second;
            case "NOT EQUALS":
                return first ^ second;
            case "NAND":
                return !(first && second);
            case "NOR":
                return !(first || second);
            case "EQUALS":
                return !(first ^ second);
            default:
                Debug.LogError(logicalOperator + " is not a valid logical operator");
                return false;
        }
    }

    bool IsIncrement(string operationType)
    {
        operationType = operationType.ToLower();
        switch (operationType)
        {
            case "set state":
                return false;
            case "add to state":
                return true;
            default:
                Debug.LogError(operationType + " is not a valid output operator");
                return false;
        }
    }

}