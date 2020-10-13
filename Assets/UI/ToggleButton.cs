using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{   
    Input_Controller input_Controller;
    public MouseMode mouseMode;
    // Start is called before the first frame update
    void Start()
    {
        this.input_Controller = FindObjectOfType<Input_Controller>();
    }

    // Update is called once per frame
    void OnClick()
    {
        this.mouseMode = this.input_Controller.mouseMode;
        if (this.mouseMode == MouseMode.PlaceAnt){
            
        }
    }
}
