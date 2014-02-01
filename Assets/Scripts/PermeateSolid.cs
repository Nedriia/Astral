using UnityEngine;
using System.Collections;

public class PermeateSolid : MonoBehaviour {
	
	private GameObject[] tagList;
	private int playerLayer;
	private int doorLayer;
	private float distFrom;
	private float alpha;
	private float lerp;
	private Color temp;
	private float dist;
	private string tagName;
	private Shader shaderDiff;
	private Shader shaderTransDiff;
    private PossessionMaster possMaster;

	// Use this for initialization
	private void Start () {
		playerLayer = 8;
		doorLayer = 9;
		distFrom = 5.0f;
		tagName = "Door";
		shaderDiff = Shader.Find("Diffuse");
		shaderTransDiff = Shader.Find("Transparent/Diffuse");
	}
	
	// Update is called once per frame
	private void Update () {
		permeateSolid();
	}
	
	//If in Astral form, fade opacity as you approach and disable layer collision. Else, solid
	private void permeateSolid() {
		tagList = GameObject.FindGameObjectsWithTag(tagName);
		//If in Astral form, shade opacity as you near object and disable layer collision
		if(possMaster.AstralForm) { //YO CHANGE THIS LINE!
			foreach(GameObject i in tagList) {
				i.renderer.material.shader = shaderTransDiff;
				dist = Vector3.Distance(Camera.main.transform.position, i.transform.position);
				if(dist <= distFrom) {
					lerp =  dist/distFrom;
					alpha = Mathf.Lerp (0.0f, 0.8f, lerp);
					temp = i.renderer.material.color;
					i.renderer.material.color = new Color(temp.r, temp.g, temp.b, alpha);
				}
				Physics.IgnoreLayerCollision(doorLayer, playerLayer, true);
			}
		}
		//If not in Astral form, turn off transparency and enable layer collision
		else {
			foreach(GameObject i in tagList) {
				i.renderer.material.shader = shaderDiff;
			}
			Physics.IgnoreLayerCollision(doorLayer, playerLayer, false);
		}
	}
}