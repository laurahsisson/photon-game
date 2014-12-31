using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate(Time.deltaTime,0f,0f);
		if (Time.time>3f){
			transform.Translate(0f,0f,Time.deltaTime);	
		}
	}
}
