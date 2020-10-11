using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedText : MonoBehaviour
{
    Ant_Controller AC;
    Text myText;
    // Start is called before the first frame update
    void Start()
    {
        AC = FindObjectOfType<Ant_Controller>();
        myText = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (AC.paused)
        {
            myText.text = "Paused";
        }
        else
        {
            myText.text = "Speed: " + AC.speed.ToString();
        }
    }
}
