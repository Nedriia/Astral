using UnityEngine;
using System.Collections;

public class ToggleWalls : MonoBehaviour {

	private Shader diffuseShader;
	private Shader transAppShader;
	//public GameObject firstPerson;

	// Use this for initialization
	private void Start () {
		diffuseShader = Shader.Find ("Diffuse");
		transAppShader = Shader.Find ("TransApproach");
		/*
		if (isAstral)) {
			renderer.material.shader = transAppShader;
			firstPerson.camera.renderingPath = RenderingPath.Forward;
		}
		else {
			renderer.material.shader = diffuseShader;
			firstPerson.camera.renderingPath = RenderingPath.DeferredLighting;
		}*/
	}
	
	// Update is called once per frame
	private void Update () {
		//if (isAstral && button) {	
		if (Input.GetKey(KeyCode.G)) {
			renderer.material.shader = transAppShader;
			Camera.main.renderingPath = RenderingPath.Forward;
		}
		else {
			renderer.material.shader = diffuseShader;
			Camera.main.renderingPath = RenderingPath.DeferredLighting;
		}
	}
}
