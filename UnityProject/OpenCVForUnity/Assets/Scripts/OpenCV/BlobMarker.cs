using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobMarker : MonoBehaviour {
	public int index;
	public float x;
	public float y;
	public float depth;

	public BlobMarker (int indexValue, float xValue, float yValue, float depthValue) {
		this.index = indexValue;
		this.x = xValue;
		this.y = yValue;
		this.depth = depthValue;
	}

	public BlobMarker () {
		this.index = -1;
		this.x = 0f;
		this.y = 0f;
		this.depth = 0f;
	}


}
