using UnityEngine;
using System.Collections;

//The generator. Upon touching, the current prisoner "dies" and turns brown and into a ragdoll. Afterwards, the player is forced back into Astral form
public class Generator : MonoBehaviour {
	private PossessionMaster posMaster;
	
	//Use this for initialization
	private void Start() {
		posMaster = GameObject.Find("Possession Master").GetComponent<PossessionMaster>();
	}
	
	private void OnCollisionStay(Collision collision) {
		//Check if a prisoner touched the generator and the Z key was pressed
		if (collision.gameObject.CompareTag("Prisoner") == true && Input.GetKeyDown(KeyCode.Z) == true) {
			//Exit possession form
			StartCoroutine(posMaster.enterAstral());
			
			//Kill the prisoner
			collision.gameObject.GetComponent<Prisoner>().Die();
		}
	}
	
	//Update is called once per frame
	private void Update() {
	
	}
}
