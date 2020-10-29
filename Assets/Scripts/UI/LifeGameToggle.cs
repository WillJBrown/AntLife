using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeGameToggle : MonoBehaviour
{
    World_Controller WC;
    Toggle myToggle;
    Text myText;

    // Start is called before the first frame update
    void Start()
    {
        this.WC = World_Controller.Instance;
        this.WC.RegisterLifeGameCallBack(OnLifeGameChanged);
        this.myToggle = this.GetComponent<Toggle>();
        this.myToggle.isOn = WC.LifeGame;
        this.myText = this.GetComponentInChildren<Text>();
    }

    public void OnLifeGameChanged(World_Controller WC){
        //Debug.Log("OnMouseModeChanged: " + this.thisTogglesMode);
        this.myToggle.isOn = WC.LifeGame;
        if (WC.LifeGame){
            myText.text = "Game of Life: On";
        } else {
            myText.text = "Game of Life: Off";
        }
    }
}
