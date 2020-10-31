using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeGameToggle : MonoBehaviour
{
    GameBoard GameBoard;
    Toggle myToggle;
    Text myText;

    // Start is called before the first frame update
    void Start()
    {
        this.GameBoard = World_Controller.Instance.GameBoard;
        this.GameBoard.RegisterLifeGameCallBack(OnLifeGameChanged);
        this.myToggle = this.GetComponent<Toggle>();
        this.myToggle.isOn = (this.GameBoard.Rules.Count > 0);
        this.myText = this.GetComponentInChildren<Text>();
    }

    public void OnLifeGameChanged(GameBoard gameBoard){
        //Debug.Log("OnMouseModeChanged: " + this.thisTogglesMode);
        this.myToggle.isOn = (gameBoard.Rules.Count > 0);
        if ((gameBoard.Rules.Count > 0)){
            myText.text = "Game of Life: On";
        } else {
            myText.text = "Game of Life: Off";
        }
    }
}
