using UnityEngine;
using System.Collections;

public class GuiManager : MonoBehaviour {

    public EasyButton enterAstral;
    public PossessionMaster possMaster;
	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        if (possMaster.AstralForm) {
            enterAstral.isActivated = false;
        } else {
            enterAstral.isActivated = true;
        }
	}
}
