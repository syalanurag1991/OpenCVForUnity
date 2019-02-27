using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleDisplays : MonoBehaviour {

	// Use this for initialization
	void Start () {

        Debug.Log("Number of displays connected: " + Display.displays.Length);

        if (Display.displays.Length > 1)
            Display.displays[1].Activate();	
	}
}
