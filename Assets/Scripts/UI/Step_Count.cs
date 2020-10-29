using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Step_Count : MonoBehaviour
{
    World_Controller WC;
    Text myText;
    // Start is called before the first frame update
    void Start()
    {
        WC = World_Controller.Instance;
        myText = this.GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        int Steps = this.WC.Steps;
        if (Steps < 1000000) {
            myText.text = Steps.ToString() + " Steps";
        } else {
            myText.text = string.Format("{0:#.##E+0}", Steps) + " Steps";
        }
    }
}
