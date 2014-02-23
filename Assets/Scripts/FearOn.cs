using UnityEngine;
using System.Collections;

public class FearOn : MonoBehaviour {
    public GameObject pearl;
    public UIPanel panel;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if(PossessionMaster.CurrentlyPossesing == pearl.GetComponent<Prisoner>()){
            panel.enabled = true;
        } else {
            panel.enabled = false;
        }
	}
}
