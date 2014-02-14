#pragma strict

class PlaygroundCreatePresetWindow extends EditorWindow {

	static var particleSystemObject : GameObject;
	static var particleSystemIcon : Texture2D;
	static var particleSystemName : String;
	
	static var window : EditorWindow;
	static var scrollPosition : Vector2;
	
	var showError1 : boolean = false;
	
	static function ShowWindow () {
		window = EditorWindow.GetWindow(PlaygroundCreatePresetWindow, true, "Preset Wizard");
        window.Show();
	}
	
	function OnEnable () {
		Initialize();
	}
	
	function Initialize () {
		if (Selection.activeGameObject && Selection.activeGameObject.GetComponent(PlaygroundParticles)) {
			particleSystemObject = Selection.activeGameObject;
			particleSystemName = Selection.activeGameObject.name;
		} else if (!particleSystemName) {
			particleSystemName = "";
		}
	}
	
	function OnGUI () {
		EditorGUILayout.BeginVertical();
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false);
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Playground Preset Wizard", EditorStyles.largeLabel, GUILayout.Height(20));
		EditorGUILayout.Separator();
		
		GUILayout.BeginVertical("box");
		EditorGUILayout.HelpBox("Create a Particle Playground Preset by selecting a Particle Playground System and an icon (optional). The icon must be in png-format and preferably 32x32 pixels. All connected objects will be childed to the Particle Playground System.", MessageType.Info);
		EditorGUILayout.Separator();
		
		GUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Particle System");
		var selectedObj : GameObject = particleSystemObject;
		particleSystemObject = EditorGUILayout.ObjectField(particleSystemObject, GameObject, true) as GameObject;
		if (particleSystemObject!=selectedObj) {
			
			// Check if this is a Particle Playground System
			if (particleSystemObject && particleSystemObject.GetComponent(PlaygroundParticles)) {
			
				// Set new name if user hasn't specified one
				if (particleSystemName=="")
					particleSystemName = particleSystemObject.name;
					
				showError1 = false;
			} else {
				showError1 = true;
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Icon");
		particleSystemIcon = EditorGUILayout.ObjectField(particleSystemIcon, Texture2D, false) as Texture2D;
		GUILayout.EndHorizontal();
		particleSystemName = EditorGUILayout.TextField("Name", particleSystemName);
		
		EditorGUILayout.Separator();
		
		if(GUILayout.Button("Create", EditorStyles.toolbarButton, GUILayout.Width(50))){
			particleSystemName = particleSystemName.Trim();
			if (particleSystemObject && particleSystemObject.GetComponent(PlaygroundParticles) && particleSystemName!="") {
				var tmpName : String = particleSystemObject.name;
				particleSystemObject.name = particleSystemName;
				if (AssetDatabase.LoadAssetAtPath(PlaygroundParticleWindow.presetPath+particleSystemName+".prefab", GameObject)) {
					if (EditorUtility.DisplayDialog("Preset with same name found!", 
						"The preset "+particleSystemName+" already exists. Do you want to overwrite it?", 
						"Yes", 
						"No"))
							CreatePreset();
				} else CreatePreset();
				particleSystemObject.name = tmpName;
			}
		}
		GUILayout.EndVertical();
		
		// Space for error messages
		if (showError1 && particleSystemObject)
			EditorGUILayout.HelpBox("GameObject is not a Particle Playground System.", MessageType.Error);
		
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}
	
	function CreatePreset () {
		
		// Try to child all connected objects to the particle system
		var ppScript : PlaygroundParticles = particleSystemObject.GetComponent(PlaygroundParticles);
		
		for (var i = 0; i<ppScript.paint.paintPositions.Count; i++)
			if (ppScript.paint.paintPositions[i].parent)
				ppScript.paint.paintPositions[i].parent.parent = particleSystemObject.transform;
		for (i = 0; i<ppScript.states.Count; i++)
			if (ppScript.states[i].stateTransform)
				ppScript.states[i].stateTransform.parent = particleSystemObject.transform;
		if (ppScript.sourceTransform)
			ppScript.sourceTransform.parent = particleSystemObject.transform;
		if (ppScript.worldObject.transform)
			ppScript.worldObject.transform.parent = particleSystemObject.transform;
		if (ppScript.skinnedWorldObject.transform)
			ppScript.skinnedWorldObject.transform.parent = particleSystemObject.transform;
		
		// Save it as prefab in presetPath
		var particleSystemPrefab : GameObject = PrefabUtility.CreatePrefab(PlaygroundParticleWindow.presetPath+particleSystemObject.name+".prefab", particleSystemObject, ReplacePrefabOptions.Default);
		AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(particleSystemIcon as UnityEngine.Object), PlaygroundParticleWindow.iconPath+particleSystemPrefab.name+".png");
		AssetDatabase.ImportAsset(PlaygroundParticleWindow.iconPath+particleSystemPrefab.name+".png");
		window.Close();
	}
}