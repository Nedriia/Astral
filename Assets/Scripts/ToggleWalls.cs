using UnityEngine;
using System.Collections;

public class ToggleWalls : MonoBehaviour {

	private Shader diffuseShader;
	private Shader transAppShader;
	public PossessionMaster possMaster;

	// Use this for initialization
	private void Start () {
		diffuseShader = Shader.Find ("Diffuse");
		transAppShader = Shader.Find ("TransApproach");

		if (possMaster.AstralForm) {
			renderer.material.shader = transAppShader;
			Camera.main.renderingPath = RenderingPath.Forward;
		}
		else {
			renderer.material.shader = diffuseShader;
			Camera.main.renderingPath = RenderingPath.DeferredLighting;
		}
	}
	
	// Update is called once per frame
	private void Update () {
		if (possMaster.AstralForm && Input.GetKey(KeyCode.G)) {
			renderer.material.shader = transAppShader;
			Camera.main.renderingPath = RenderingPath.Forward;
		}
		else {
			renderer.material.shader = diffuseShader;
			Camera.main.renderingPath = RenderingPath.DeferredLighting;
		}
	}
}
