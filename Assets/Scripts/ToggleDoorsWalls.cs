using UnityEngine;
using System.Collections;

public class ToggleDoorsWalls : MonoBehaviour {

	public float transVar;

	private Shader diffuseShader;
	private Shader transAppShader;
	private GameObject[] tagList;
	private int playerLayer, hideLayer;

	// Use this for initialization
	void Start () {
		tagList = GameObject.FindGameObjectsWithTag ("DoorsWalls");
		diffuseShader = Shader.Find ("Diffuse");
		transAppShader = Shader.Find ("TransApproach/Diffuse");
		playerLayer = 8;
		hideLayer = 9;
		transVar = 0.3f;
	}
	
	// Update is called once per frame
	void Update () {
		if (PossessionMaster.AstralForm ) {
			//Camera.main.renderingPath = RenderingPath.Forward;
			foreach(GameObject i in tagList) {
				i.renderer.material.shader = transAppShader;
				i.renderer.material.SetFloat("_Opacity", transVar);
			}
			Physics.IgnoreLayerCollision(playerLayer, hideLayer, true);
		}
		else {
			//Camera.main.renderingPath = RenderingPath.DeferredLighting;
			foreach(GameObject i in tagList) {
				i.renderer.material.shader = diffuseShader;
			}
			Physics.IgnoreLayerCollision(playerLayer, hideLayer, false);
		}
	}
}
