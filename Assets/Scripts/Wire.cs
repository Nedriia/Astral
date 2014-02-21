using UnityEngine;
using System.Collections;

public class Wire : MonoBehaviour {
	public int distanceFade; //The distance it takes to make the wire appear; the higher the closer

	private Material wireMaterial; //The material of the wire; we will set the shader's alpha to max to make it glow and set it to 0 to turn off the glow
	
	private Shader glowShader;
	private Shader noGlowShader;
	
	//Use this for initialization
	private void Start () {
		//Get the shaders
		glowShader = Shader.Find("Glow 11/Unity/Self-Illumin/Diffuse");
		noGlowShader = Shader.Find("Transparent/Diffuse");
		
		Renderer wireRenderer = this.gameObject.GetComponentInChildren<Renderer>();
		
		//Set the material
		if (wireRenderer != null) {
			wireMaterial = wireRenderer.material;
		}
	}
	
	//Changes the wire's shader from Transparent/Diffuse to a glow and vice versa
	public void changeWireShader(Light lightAffected, bool into) {
		if (wireMaterial != null && lightAffected != null) {
			if (into == true) {
				wireMaterial.shader = glowShader;
				wireMaterial.SetColor("_GlowColor", lightAffected.color);
				wireMaterial.color = lightAffected.color;
			}
			else {
				wireMaterial.shader = noGlowShader;
				wireMaterial.color = Color.black;
			}
		}
	}
	
	//Update is called once per frame
	private void Update () {
		//Change the transparency of the wire depending on the distance the light prisoner is from it
		if (PossessionMaster.CurrentlyPossesing != null) {
			//Use just the X and Z positions so it doesn't depend on the Y value you are from it
			Vector2 wireDist = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.z);
			Vector2 prisonerDist = new Vector2(PossessionMaster.CurrentlyPossesing.gameObject.transform.position.x, PossessionMaster.CurrentlyPossesing.gameObject.transform.position.z);
			
			float distWire = Vector2.Distance(wireDist, prisonerDist);
			if (distanceFade > 0) wireMaterial.color = new Color(wireMaterial.color.r, wireMaterial.color.g, wireMaterial.color.b, 1f - (distWire / distanceFade));
		}
		else if (wireMaterial.color.a != 0f) wireMaterial.color = new Color(wireMaterial.color.r, wireMaterial.color.g, wireMaterial.color.b, 0f);
	}
}
