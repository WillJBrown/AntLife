using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Multiplier_Text : MonoBehaviour
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
        myText.text = "x"+this.WC.multiplier.ToString(); 
    }
}
