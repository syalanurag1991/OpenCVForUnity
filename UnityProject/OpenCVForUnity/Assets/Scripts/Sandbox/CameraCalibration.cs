using UnityEngine;
using System.Collections;

public class CameraCalibration : MonoBehaviour {


    private Camera mainCamera;

	// Use this for initialization
	void Start () {
        mainCamera = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
	
        if(Input.GetKey(KeyCode.Q))
        {
            mainCamera.orthographicSize++;
        }

        if (Input.GetKey(KeyCode.E))
        {
            mainCamera.orthographicSize--;
        }

        if (Input.GetKey(KeyCode.W))
        {
            mainCamera.transform.position += Vector3.up;
        }

        if (Input.GetKey(KeyCode.S))
        {
            mainCamera.transform.position += Vector3.down;
        }

        if (Input.GetKey(KeyCode.A))
        {
            mainCamera.transform.position += Vector3.left;
        }

        if (Input.GetKey(KeyCode.D))
        {
            mainCamera.transform.position += Vector3.right;
        }
	}
}
