using UnityEngine;
using System.Collections;

public class ToggleDoorsWalls : MonoBehaviour {

	public float transVar;

	private Shader diffuseShader, transAppDiffShader;		//Diffuse shaders
	private Shader bumpDiffShader, transAppBumpDiffShader;	//Bumped diffuse shaders
	private Shader bumpSpecShader, transAppBumpSpecShader;	//Bumped specular shaders
	private GameObject[] tagList;
	private int playerLayer, hideLayer;

	// Use this for initialization
	void Start () {
		tagList = GameObject.FindGameObjectsWithTag ("DoorsWalls");
		diffuseShader = Shader.Find ("Diffuse");
		transAppDiffShader = Shader.Find ("TransApproach/Diffuse");
		bumpDiffShader = Shader.Find ("Bumped Diffuse");
		transAppBumpDiffShader = Shader.Find ("TransApproach/Bumped");
		bumpSpecShader = Shader.Find ("Bumped Specular");
		transAppBumpSpecShader = Shader.Find ("TransApproach/BumpedSpecular");
		playerLayer = 8;
		hideLayer = 9;
		transVar = 0.3f;
	}
	
	// Update is called once per frame
	void Update () {
		if (PossessionMaster.AstralForm ) {
			//Camera.main.renderingPath = RenderingPath.Forward;
			foreach(GameObject i in tagList) {
				if (i.renderer.material.shader == diffuseShader) 
					i.renderer.material.shader = transAppDiffShader;
				else if (i.renderer.material.shader == bumpDiffShader)
					i.renderer.material.shader = transAppBumpDiffShader;
				else if (i.renderer.material.shader == bumpSpecShader)
					i.renderer.material.shader = transAppBumpSpecShader;
				i.renderer.material.SetFloat("_Opacity", transVar);
			}
			Physics.IgnoreLayerCollision(playerLayer, hideLayer, true);
		}
		else {
			//Camera.main.renderingPath = RenderingPath.DeferredLighting;
			foreach(GameObject i in tagList) {
				if (i.renderer.material.shader == transAppDiffShader) 
					i.renderer.material.shader = diffuseShader;
				else if (i.renderer.material.shader == transAppBumpDiffShader)
					i.renderer.material.shader = bumpDiffShader;
				else if (i.renderer.material.shader == transAppBumpSpecShader)
					i.renderer.material.shader = bumpSpecShader;

			}
			Physics.IgnoreLayerCollision(playerLayer, hideLayer, false);
		}
	}
}
