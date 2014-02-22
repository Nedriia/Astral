using UnityEngine;
using System.Collections;

public class ToggleDoorsWalls : MonoBehaviour {

	public float trans = 0.5f;
	private Shader diffuseShader;
	private Shader transAppShader;
	private GameObject[] tagList;
	private int playerLayer, hideLayer;

	// Use this for initialization
	void Start () {
		tagList = GameObject.FindGameObjectsWithTag ("DoorsWalls");
		diffuseShader = Shader.Find ("Diffuse");
		transAppShader = Shader.Find ("TransApproach");
		playerLayer = 8;
		hideLayer = 9;
	}
	
	// Update is called once per frame
	void Update () {
		Shader.SetGlobalFloat ("transVar", trans);
		if (PossessionMaster.AstralForm ) {
			//Camera.main.renderingPath = RenderingPath.Forward;
			foreach(GameObject i in tagList) {
				if (i.renderer != null) i.renderer.material.shader = transAppShader;
			}
			Physics.IgnoreLayerCollision(playerLayer, hideLayer, true);
		}
		else {
			//Camera.main.renderingPath = RenderingPath.DeferredLighting;
			foreach(GameObject i in tagList) {
				if (i.renderer != null) i.renderer.material.shader = diffuseShader;
			}
			Physics.IgnoreLayerCollision(playerLayer, hideLayer, false);
		}
	}
}
