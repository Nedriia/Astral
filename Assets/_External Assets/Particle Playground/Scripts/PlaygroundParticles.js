#pragma strict

import System.Collections.Generic;

@script RequireComponent (ParticleSystem)
@script ExecuteInEditMode()

class PlaygroundParticles extends MonoBehaviour {

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// PlaygroundParticles variables
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Playground variables
	var source : SOURCE;											// The particle source
	var activeState : int;											// Current active state (when using state as source)
	var transition : TRANSITION;									// The type of transition to use
	var transitionTime : float = 1.0;								// The time it takes to complete a transition
	var emit : boolean = true;										// If emission of particles is active on this PlaygroundParticles
	var updateRate : int = 1;										// The rate to update this PlaygroundParticles
	var calculate : boolean = true;									// Calculate forces on this PlaygroundParticles (can be overrided by Playground.calculate)
	var calculateDeltaMovement : boolean = true;					// Calculate the delta movement force of this particle system
	var deltaMovementStrength : float = 10.0;						// The strength to multiply delta movement with
	var worldObjectUpdateNormals : boolean = false;					// The current world object will change its vertices over time
	var nearestNeighborOrigin : int = 0;							// The initial source position when using lifetime sorting of Nearest Neighbor / Nearest Neighbor Reversed
	
	// Particle variables
	var particleCount : int;										// The amount of particles within this PlaygroundParticles object
	var emissionRate : float = 1.0;									// The percentage to emit of particleCount in bursts from this PlaygroundParticles
	var overflowMode : OVERFLOWMODE = OVERFLOWMODE.SourceTransform;	// The method to calculate overflow with
	var overflowOffset : Vector3;									// Offset when particle count exceeds source count
	var sorting : SORTING = SORTING.Scrambled;						// Sort mode for particle lifetime
	var sizeMin : float;											// Minimum size
	var sizeMax : float;											// Maximum size
	var initialRotationMin : float;									// Minimum initial rotation
	var initialRotationMax : float;									// Maximum initial rotation
	var rotationSpeedMin : float;									// Minimum amount to rotate
	var rotationSpeedMax : float;									// Maximum amount to rotate
	var rotateTowardsDirection : boolean = false;					// Should the particles rotate towards their movement direction
	var rotationNormal : Vector3 = -Vector3.forward;				// The rotation direction normal when rotating towards direction (always normalized value)
	var lifetime : float;											// The life of a particle in seconds
	var lifetimeSize : AnimationCurve;								// The size over lifetime of each particle
	var onlySourcePositioning : boolean = false;					// Should the particles only position on their source (and not apply any forces)?
	var applyLifetimeVelocity : boolean = false;					// Should lifetime velocity affect particles?
	var lifetimeVelocity : Vector3AnimationCurve;					// The velocity over lifetime of each particle
	var applyInitialVelocity : boolean = false;						// Should initial velocity affect particles?
	var initialVelocityMin : Vector3;								// The minimum starting velocity of each particle
	var initialVelocityMax : Vector3;								// The maximum starting velocity of each particle
	var applyInitialLocalVelocity : boolean = false;				// Should initial local velocity affect particles?
	var initialLocalVelocityMin : Vector3;							// The minimum starting velocity of each particle with normal or transform direction
	var initialLocalVelocityMax : Vector3;							// The maximum starting velocity of each particle with normal or transform direction
	var applyVelocityBending : boolean;								// Should bending affect particles velocity?
	var velocityBending : Vector3;									// The amount to bend velocity of each particle
	var gravity : Vector3;											// The constant force towards gravitational vector
	var damping : float;											// Particles inertia over time
	var lifetimeColor : Gradient;									// The color over lifetime
	var colorSource : COLORSOURCE = COLORSOURCE.Source;				// The source to read color from (fallback on Lifetime Color if no source color is available)
	var sourceUsesLifetimeAlpha : boolean;							// Should the source color use alpha from Lifetime Color instead of the source's original alpha?
	
	// Collision detection
	var collision : boolean = true;									// Can particles collide?
	var affectRigidbodies : boolean = true;							// Should particles affect rigidbodies?
	var mass : float = .01;											// The mass of a particle (calculated in collision with rigidbodies)
	var collisionRadius : float = 1.0;								// The spherical radius of a particle
	var collisionMask : LayerMask;									// The layers these particles will collide with
	var bounciness : float = .5;									// The amount a particle will bounce on collision
	
	// States (source)
	var states : List.<ParticleState>;								// The states of this PlaygroundParticles

	// Scene objects (source)
	var worldObject : WorldObject;									// A mesh calculated within the scene
	var skinnedWorldObject : SkinnedWorldObject;					// A skinned mesh calculated within the scene
	var sourceTransform : Transform;								// A transform calculated within the scene
	
	// Paint
	var paint : PaintObject;										// The paint source of this PlaygroundParticles

	// Cache
	var playgroundCache : PlaygroundCache;							// Data for each particle
	var particleCache : ParticleCache;								// Particle pool

	// Components
	@HideInInspector var shurikenParticleSystem : ParticleSystem;	// This ParticleSystem (Shuriken) component
	@HideInInspector var particleSystemId : int;					// The id of this PlaygroundParticles object
	@HideInInspector var particleSystemGameObject : GameObject;		// This GameObject
	@HideInInspector var particleSystemTransform : Transform;		// This Transform
	@HideInInspector var particleSystemRenderer : Renderer;			// This Renderer
	
	// Internally used variables
	@HideInInspector var inTransition : boolean = false;			
	@HideInInspector var previousActiveState : int;
	@HideInInspector var previousParticleCount : int = -1;
	@HideInInspector var previousEmissionRate : float = 1.0;
	@HideInInspector var cameFromNonCalculatedFrame : boolean = false;
	@HideInInspector var previousSizeMin : float;
	@HideInInspector var previousSizeMax : float;
	@HideInInspector var previousInitialRotationMin : float;
	@HideInInspector var previousInitialRotationMax : float;
	@HideInInspector var previousRotationSpeedMin : float;
	@HideInInspector var previousRotationSpeedMax : float;
	@HideInInspector var previousVelocityMin : Vector3;
	@HideInInspector var previousVelocityMax : Vector3;
	@HideInInspector var previousLocalVelocityMin : Vector3;
	@HideInInspector var previousLocalVelocityMax : Vector3;
	@HideInInspector var previousLifetime : float;
	@HideInInspector var previousEmission : boolean = true;
	@HideInInspector var isYieldRefreshing : boolean = false;
	@HideInInspector var isPainting : boolean = false;
	
	@HideInInspector var particleTimescale : float = 1.0;
	
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// PlaygroundParticles functions
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Create a new PlaygroundParticles object
	static function CreatePlaygroundParticles (images:Texture2D[], name:String, position:Vector3, rotation:Quaternion, offset : Vector3, particleSize : float, scale : float, material:Material) : PlaygroundParticles {
		var playgroundParticles : PlaygroundParticles = CreateParticleObject(name,position,rotation,particleSize,material);

		var quantityList : int[] = new int[images.Length];
		for (var i = 0; i<images.Length; i++)
			quantityList[i] = images[i].width*images[i].height;
		playgroundParticles.particleCache = new ParticleCache();
		playgroundParticles.particleCache.particles = new ParticleSystem.Particle[quantityList[Playground.Largest(quantityList)]];
		OnCreatePlaygroundParticles(playgroundParticles);	
		
		for (i = 0; i<images.Length; i++) {
			playgroundParticles.states.Add(new ParticleState());
			playgroundParticles.states[playgroundParticles.states.Count-1].ConstructParticles(images[i],scale,offset,"State 0",null);
		}
		
		return playgroundParticles;
	}
	
	// Set default settings for PlaygroundParticles object
	static function OnCreatePlaygroundParticles (playgroundParticles : PlaygroundParticles) {
		playgroundParticles.playgroundCache = new PlaygroundCache();
		playgroundParticles.paint = new PaintObject();
		playgroundParticles.states = new List.<ParticleState>();
		playgroundParticles.particleSystemId = Playground.particlesQuantity;
		
		playgroundParticles.playgroundCache.size = new float[playgroundParticles.particleCount];
		playgroundParticles.playgroundCache.size = Playground.RandomFloat(playgroundParticles.playgroundCache.size.Length, playgroundParticles.sizeMin, playgroundParticles.sizeMax);
		
		playgroundParticles.previousParticleCount = playgroundParticles.particleCount;
		playgroundParticles.lifetimeSize = new AnimationCurve(Keyframe(0,1), Keyframe(1,1));
		
		playgroundParticles.shurikenParticleSystem.Emit(playgroundParticles.particleCount);
		playgroundParticles.shurikenParticleSystem.GetParticles(playgroundParticles.particleCache.particles);
		for (var p = 0; p<playgroundParticles.particleCache.particles.Length; p++) {
			playgroundParticles.particleCache.particles[p].size = playgroundParticles.playgroundCache.size[p];
		}
		
		PlaygroundParticles.SetParticleCount(playgroundParticles, playgroundParticles.particleCount);
		
		if (Playground.reference!=null) {
			Playground.particlesQuantity++;
			Playground.reference.particleSystems.Add(playgroundParticles);
			playgroundParticles.particleSystemId = Playground.particlesQuantity;
		}
	}
	
	// Create a Shuriken Particle System
	static function CreateParticleObject (name : String, position : Vector3, rotation : Quaternion, particleSize : float, material : Material) : PlaygroundParticles {
		var go = Playground.ResourceInstantiate("Particle Playground System");
		var playgroundParticles : PlaygroundParticles = go.GetComponent(PlaygroundParticles);
		playgroundParticles.particleSystemGameObject = go;
		playgroundParticles.particleSystemGameObject.name = name;
		playgroundParticles.shurikenParticleSystem = playgroundParticles.particleSystemGameObject.GetComponent(ParticleSystem);
		playgroundParticles.particleSystemRenderer = playgroundParticles.shurikenParticleSystem.renderer;
		playgroundParticles.particleSystemTransform = playgroundParticles.particleSystemGameObject.transform;
		playgroundParticles.sourceTransform = playgroundParticles.particleSystemTransform;
		playgroundParticles.source = SOURCE.Transform;
		playgroundParticles.particleSystemTransform.position = position;
		playgroundParticles.particleSystemTransform.rotation = rotation;
		if (Playground.reference==null)
			Playground.ResourceInstantiate("Playground Manager");
		if (Playground.reference.autoGroup && playgroundParticles.particleSystemTransform.parent==null)
			playgroundParticles.particleSystemTransform.parent = Playground.referenceTransform;
		
		if (playgroundParticles.particleSystemRenderer.sharedMaterial==null)
			playgroundParticles.particleSystemRenderer.sharedMaterial = material;
		playgroundParticles.shurikenParticleSystem.startSize = particleSize;
		
		return playgroundParticles;
	}
	
	// Create a new WorldObject
	static function NewWorldObject (playgroundParticles : PlaygroundParticles, meshTransform : Transform) : WorldObject {
		var worldObject : WorldObject = new WorldObject();
		if (meshTransform.GetComponentInChildren(MeshFilter)) {
			worldObject.transform = meshTransform;
			worldObject.gameObject = meshTransform.gameObject;
			worldObject.rigidbody = meshTransform.rigidbody;
			worldObject.renderer = meshTransform.GetComponentInChildren(Renderer);
			worldObject.meshFilter = meshTransform.GetComponentInChildren(MeshFilter);
			worldObject.mesh = worldObject.meshFilter.sharedMesh;
			worldObject.vertexPositions = new Vector3[worldObject.mesh.vertexCount];
			worldObject.normals = worldObject.mesh.normals;
			worldObject.cachedId = worldObject.gameObject.GetInstanceID();
		
			playgroundParticles.worldObject = worldObject;
				
		} else Debug.Log("Could not find a mesh in "+meshTransform.name+".");
		
		return worldObject;
	}
	
	// Create a new SkinnedWorldObject
	static function NewSkinnedWorldObject (playgroundParticles : PlaygroundParticles, meshTransform : Transform) : SkinnedWorldObject {
		var worldObject : SkinnedWorldObject = new SkinnedWorldObject();
		if (meshTransform.GetComponent(SkinnedMeshRenderer)) {
			worldObject.transform = meshTransform;
			worldObject.gameObject = meshTransform.gameObject;
			worldObject.rigidbody = meshTransform.rigidbody;
			worldObject.renderer = meshTransform.GetComponent(SkinnedMeshRenderer);
			worldObject.mesh = worldObject.renderer.sharedMesh;
			worldObject.vertexPositions = new Vector3[worldObject.mesh.vertexCount];
			worldObject.normals = worldObject.mesh.normals;
			worldObject.cachedId = worldObject.gameObject.GetInstanceID();
			
			playgroundParticles.skinnedWorldObject = worldObject;
			
			SetParticleCount(playgroundParticles, worldObject.mesh.vertexCount);
		
		} else Debug.Log("Could not find a skinned mesh in "+meshTransform.name+".");
		
		return worldObject;
	}
	
	// Create a new PaintObject
	static function NewPaintObject (playgroundParticles : PlaygroundParticles) : PaintObject {
		var paintObject : PaintObject = new PaintObject();
		playgroundParticles.paint = paintObject;
		playgroundParticles.paint.Initialize();
		return paintObject;
	}
	
	// Create a new ManipulatorObject
	static function NewManipulatorObject (type : MANIPULATORTYPE, affects : LayerMask, manipulatorTransform : Transform, size : float, strength : float) : ManipulatorObject {
		var manipulatorObject : ManipulatorObject = new ManipulatorObject();
		manipulatorObject.type = type;
		manipulatorObject.affects = affects;
		manipulatorObject.transform = manipulatorTransform;
		manipulatorObject.size = size;
		manipulatorObject.strength = strength;
		manipulatorObject.bounds = new Bounds(Vector3.zero, Vector3(size, size, size));
		
		Playground.reference.manipulators.Add(manipulatorObject);
		
		return manipulatorObject;
	}
	
	// Lerp to specified state in this PlaygroundParticles
	static function Lerp (playgroundParticles : PlaygroundParticles, to : int, time : float, lerpType : LERPTYPE) {
		if (to<0) {to=playgroundParticles.states.Count;} to=to%playgroundParticles.states.Count;
		if(time<0) time = .0;
		var color : Color = new Color();
		for (var i = 0; i<playgroundParticles.particleCache.particles.Length; i++) {
			if (lerpType==LERPTYPE.PositionColor||lerpType==LERPTYPE.Position) 
				playgroundParticles.particleCache.particles[i].position = Vector3.Lerp(playgroundParticles.particleCache.particles[i].position, playgroundParticles.states[to].GetPosition(i%playgroundParticles.states[to].positionLength), time);
			if (lerpType==LERPTYPE.PositionColor||lerpType==LERPTYPE.Color) {
				color = playgroundParticles.states[to].GetColor(i%playgroundParticles.states[to].colorLength);
				playgroundParticles.particleCache.particles[i].color = Color.Lerp(playgroundParticles.particleCache.particles[i].color, color, time);
			}
		}
		Update(playgroundParticles);
	}
	
	// Lerp to state object
	static function Lerp (playgroundParticles : PlaygroundParticles, state : ParticleState, time : float, lerpType : LERPTYPE) {
		if(time<0) time = .0;
		var color : Color = new Color();
		for (var i = 0; i<playgroundParticles.particleCache.particles.Length; i++) {
			if (lerpType==LERPTYPE.PositionColor||lerpType==LERPTYPE.Position) 
				playgroundParticles.particleCache.particles[i].position = Vector3.Lerp(playgroundParticles.particleCache.particles[i].position, state.GetPosition(i%state.positionLength), time);
			if (lerpType==LERPTYPE.PositionColor||lerpType==LERPTYPE.Color) {
				color = state.GetColor(i%state.colorLength);
				playgroundParticles.particleCache.particles[i].color = Color.Lerp(playgroundParticles.particleCache.particles[i].color, color, time);
			}
		}
	}
	
	// Lerp to a Skinned World Object
	static function Lerp (playgroundParticles : PlaygroundParticles, particleStateWorldObject : SkinnedWorldObject, time : float) {
		if(time<0) time = .0;
		var vertices : Vector3[] = particleStateWorldObject.mesh.vertices.Clone() as Vector3[];
		var weights : BoneWeight[] = particleStateWorldObject.mesh.boneWeights;
		var boneMatrices : Matrix4x4[] = new Matrix4x4[particleStateWorldObject.renderer.bones.Length];

		for (var i = 0; i<boneMatrices.Length; i++)
            boneMatrices[i] = particleStateWorldObject.renderer.bones[i].localToWorldMatrix * particleStateWorldObject.mesh.bindposes[i];
		
		var vertexMatrix : Matrix4x4 = new Matrix4x4();
		for (i = 0; i<playgroundParticles.particleCache.particles.Length; i++) {
			var weight : BoneWeight = weights[i];
			var m0 : Matrix4x4 = boneMatrices[weight.boneIndex0];
			var m1 : Matrix4x4 = boneMatrices[weight.boneIndex1];
			var m2 : Matrix4x4 = boneMatrices[weight.boneIndex2];
			var m3 : Matrix4x4 = boneMatrices[weight.boneIndex3];

			for(var n=0;n<16;n++){
				vertexMatrix[n] =
                    m0[n] * weight.weight0 +
                    m1[n] * weight.weight1 +
                    m2[n] * weight.weight2 +
                    m3[n] * weight.weight3;
			}
		    
			playgroundParticles.particleCache.particles[i].position = Vector3.Lerp(playgroundParticles.particleCache.particles[i].position, vertexMatrix.MultiplyPoint3x4(vertices[i%vertices.Length]), time);
		}
	}
	
	// Set position from PixelParticle object
	static function SetPosition (playgroundParticles : PlaygroundParticles, to : int, runUpdate : boolean) {
		
		for (var i = 0; i<playgroundParticles.particleCache.particles.Length; i++)
			playgroundParticles.particleCache.particles[i].position = playgroundParticles.states[to].GetPosition(i%playgroundParticles.states[to].positionLength)+GetOverflowOffset(playgroundParticles, i, playgroundParticles.states[to].positionLength);
		if (runUpdate)
			Update(playgroundParticles);
	}
	
	// Set color from PixelParticle object
	static function SetColor (playgroundParticles : PlaygroundParticles, to : int) {
		var color : Color = new Color();
		for (var i = 0; i<playgroundParticles.particleCache.particles.Length; i++) {
			color = playgroundParticles.states[to].GetColor(i%playgroundParticles.states[to].colorLength);
			playgroundParticles.particleCache.particles[i].color = color;
		}
	}
	
	// Set color from Color
	static function SetColor (playgroundParticles : PlaygroundParticles, color : Color) {
		for (var i = 0; i<playgroundParticles.particleCache.particles.Length; i++) {
			playgroundParticles.particleCache.particles[i].color = color;
		}
	}
	
	// Set position from Mesh World Object
	static function SetPosition (playgroundParticles : PlaygroundParticles, particleStateWorldObject : WorldObject) {
		if (!playgroundParticles.worldObject || playgroundParticles.worldObject.mesh==null) {
			Debug.Log("There is no mesh assigned to "+playgroundParticles.particleSystemGameObject.name+"'s worlObject.");
			return;
		}
		var meshVertices : Vector3[] = particleStateWorldObject.mesh.vertices.Clone() as Vector3[];
		for (var i = 0; i<playgroundParticles.particleCache.particles.Length; i++)
			playgroundParticles.particleCache.particles[i].position = particleStateWorldObject.transform.TransformPoint(meshVertices[i%meshVertices.Length])+GetOverflowOffset(playgroundParticles, i, meshVertices.Length);
	}
	
	// Set position from Skinned Mesh World Object
	static function SetPosition (playgroundParticles : PlaygroundParticles, particleStateWorldObject : SkinnedWorldObject) {
		if (!playgroundParticles.skinnedWorldObject || playgroundParticles.skinnedWorldObject.mesh==null) {
			Debug.Log("There is no skinned mesh assigned to "+playgroundParticles.particleSystemGameObject.name+"'s skinnedWorlObject.");
			return;
		}
		var vertices : Vector3[] = particleStateWorldObject.mesh.vertices.Clone() as Vector3[];
		var weights : BoneWeight[] = particleStateWorldObject.mesh.boneWeights;
		var bindposes : Matrix4x4[] = particleStateWorldObject.mesh.bindposes;
		var boneMatrices : Matrix4x4[] = new Matrix4x4[particleStateWorldObject.renderer.bones.Length];
		
		for (var i = 0; i<boneMatrices.Length; i++)
            boneMatrices[i] = particleStateWorldObject.renderer.bones[i].localToWorldMatrix * bindposes[i];
		
		var vertexMatrix : Matrix4x4 = new Matrix4x4();
		for (i = 0; i<playgroundParticles.particleCache.particles.Length; i++) {
			var weight : BoneWeight = weights[i];
			var m0 : Matrix4x4 = boneMatrices[weight.boneIndex0];
			var m1 : Matrix4x4 = boneMatrices[weight.boneIndex1];
			var m2 : Matrix4x4 = boneMatrices[weight.boneIndex2];
			var m3 : Matrix4x4 = boneMatrices[weight.boneIndex3];

			for(var n=0;n<16;n++){
				vertexMatrix[n] =
                    m0[n] * weight.weight0 +
                    m1[n] * weight.weight1 +
                    m2[n] * weight.weight2 +
                    m3[n] * weight.weight3;
			}
		    
			playgroundParticles.particleCache.particles[i].position = vertexMatrix.MultiplyPoint3x4(vertices[i%vertices.Length])+GetOverflowOffset(playgroundParticles, i, vertices.Length);
		}
	}
	
	// Get vertices from a skinned world object in a Vector3-array
	static function GetPosition (v3 : Vector3[], norm : Vector3[], particleStateWorldObject : SkinnedWorldObject) {
		var vertices : Vector3[] = particleStateWorldObject.mesh.vertices.Clone() as Vector3[];
		norm = particleStateWorldObject.mesh.normals.Clone() as Vector3[];
		var weights : BoneWeight[] = particleStateWorldObject.mesh.boneWeights;
		var bindPoses : Matrix4x4[] = particleStateWorldObject.mesh.bindposes;
		var boneMatrices : Matrix4x4[] = new Matrix4x4[particleStateWorldObject.renderer.bones.Length];
		if (v3.Length!=vertices.Length) v3 = new Vector3[vertices.Length];
		for (var i = 0; i<boneMatrices.Length; i++)
            boneMatrices[i] = particleStateWorldObject.renderer.bones[i].localToWorldMatrix * bindPoses[i];
		
		var vertexMatrix : Matrix4x4 = new Matrix4x4();
		for (i = 0; i<v3.Length; i++) {
			var weight : BoneWeight = weights[i];
			var m0 : Matrix4x4 = boneMatrices[weight.boneIndex0];
			var m1 : Matrix4x4 = boneMatrices[weight.boneIndex1];
			var m2 : Matrix4x4 = boneMatrices[weight.boneIndex2];
			var m3 : Matrix4x4 = boneMatrices[weight.boneIndex3];

			for(var n=0;n<16;n++){
				vertexMatrix[n] =
                    m0[n] * weight.weight0 +
                    m1[n] * weight.weight1 +
                    m2[n] * weight.weight2 +
                    m3[n] * weight.weight3;
			}
		    
			v3[i] = vertexMatrix.MultiplyPoint3x4(vertices[i%vertices.Length]);
		}
	}
	
	// Get position from Mesh World Object
	static function GetPosition (v3 : Vector3[], particleStateWorldObject : WorldObject) {
		if (particleStateWorldObject.meshFilter.sharedMesh!=particleStateWorldObject.mesh)
			particleStateWorldObject.mesh = particleStateWorldObject.meshFilter.sharedMesh;
		var vertices : Vector3[] = particleStateWorldObject.mesh.vertices.Clone() as Vector3[];
		if (v3.Length!=vertices.Length) v3 = new Vector3[vertices.Length];
		for (var i = 0; i<v3.Length; i++) {
			v3[i] = particleStateWorldObject.transform.TransformPoint(vertices[i%vertices.Length]);
		}
	}
	
	// Get normals from Mesh World Object
	static function GetNormals (v3 : Vector3[], particleStateWorldObject : WorldObject) {
		v3 = particleStateWorldObject.mesh.normals.Clone() as Vector3[];
	}
	
	// Returns the offset as a remainder using the particleCount towards maxVal
	static function GetOverflowOffset (playgroundParticles : PlaygroundParticles, currentVal : int, maxVal : int) : Vector3 {
		var iteration : float = (currentVal/maxVal);
		
		// Trying to use the transform of the current source
		if (playgroundParticles.overflowMode == OVERFLOWMODE.SourceTransform) {
			
			// State
			if (playgroundParticles.source == SOURCE.State && playgroundParticles.states[playgroundParticles.activeState].stateTransform) {
				return playgroundParticles.states[playgroundParticles.activeState].stateTransform.TransformPoint(
					playgroundParticles.overflowOffset.x*iteration,
					playgroundParticles.overflowOffset.y*iteration,
					playgroundParticles.overflowOffset.z*iteration
				)-playgroundParticles.states[playgroundParticles.activeState].stateTransform.position;
			} else
			
			// World Object
			if (playgroundParticles.source == SOURCE.WorldObject && playgroundParticles.worldObject.transform) {
				return playgroundParticles.worldObject.transform.TransformPoint(
					playgroundParticles.overflowOffset.x*iteration,
					playgroundParticles.overflowOffset.y*iteration,
					playgroundParticles.overflowOffset.z*iteration
				)-playgroundParticles.worldObject.transform.position;
			} else
			
			// Skinned World Object
			if (playgroundParticles.source == SOURCE.SkinnedWorldObject && playgroundParticles.skinnedWorldObject.transform) {
				return playgroundParticles.skinnedWorldObject.transform.TransformPoint(
					playgroundParticles.overflowOffset.x*iteration,
					playgroundParticles.overflowOffset.y*iteration,
					playgroundParticles.overflowOffset.z*iteration
				)-playgroundParticles.skinnedWorldObject.transform.position;
			} else
			
			// Transform
			if (playgroundParticles.source == SOURCE.Transform && playgroundParticles.sourceTransform) {
				return playgroundParticles.sourceTransform.TransformPoint(
					playgroundParticles.overflowOffset.x*iteration,
					playgroundParticles.overflowOffset.y*iteration,
					playgroundParticles.overflowOffset.z*iteration
				)-playgroundParticles.sourceTransform.position;
			} else
			
			// Paint
			if (playgroundParticles.source == SOURCE.Paint) {
				var paintParent : Transform = playgroundParticles.paint.GetParent(currentVal);
				return paintParent.TransformPoint(
					playgroundParticles.overflowOffset.x*iteration,
					playgroundParticles.overflowOffset.y*iteration,
					playgroundParticles.overflowOffset.z*iteration
				)-paintParent.position;
			}
			
		// Using the transform of the Particle System
		} else if (playgroundParticles.overflowMode == OVERFLOWMODE.ParticleSystemTransform) {
			return playgroundParticles.particleSystemTransform.TransformPoint(
				playgroundParticles.overflowOffset.x*iteration,
				playgroundParticles.overflowOffset.y*iteration,
				playgroundParticles.overflowOffset.z*iteration
			)-playgroundParticles.particleSystemTransform.position;
		
		// Using the source point
		} else if (playgroundParticles.overflowMode == OVERFLOWMODE.SourcePoint) {
			
			// State
			if (playgroundParticles.source == SOURCE.State && playgroundParticles.states[playgroundParticles.activeState].stateTransform) {
				var statePos : Vector3 = playgroundParticles.states[playgroundParticles.activeState].stateTransform.TransformPoint(playgroundParticles.states[playgroundParticles.activeState].GetPosition(currentVal));
				var stateNorm : Vector3 = playgroundParticles.states[playgroundParticles.activeState].stateTransform.TransformDirection(playgroundParticles.states[playgroundParticles.activeState].GetNormal(currentVal));
				
				return Vector3(
					statePos.x+(stateNorm.x*playgroundParticles.overflowOffset.z*iteration),
					statePos.y+(stateNorm.y*playgroundParticles.overflowOffset.z*iteration),
					statePos.z+(stateNorm.z*playgroundParticles.overflowOffset.z*iteration)
				)-statePos;
			} else
			
			// World Object
			if (playgroundParticles.source == SOURCE.WorldObject && playgroundParticles.worldObject.transform) {
				var woPos : Vector3 = playgroundParticles.worldObject.vertexPositions[currentVal%playgroundParticles.worldObject.vertexPositions.Length];
				var woNorm : Vector3 = playgroundParticles.worldObject.transform.TransformDirection(playgroundParticles.worldObject.normals[currentVal%playgroundParticles.worldObject.normals.Length]);
				
				return Vector3(
					woPos.x+(woNorm.x*playgroundParticles.overflowOffset.z*iteration),
					woPos.y+(woNorm.y*playgroundParticles.overflowOffset.z*iteration),
					woPos.z+(woNorm.z*playgroundParticles.overflowOffset.z*iteration)
				)-woPos;
			} else
			
			// Skinned World Object
			if (playgroundParticles.source == SOURCE.SkinnedWorldObject && playgroundParticles.skinnedWorldObject.transform) {
				var swoPos : Vector3 = playgroundParticles.skinnedWorldObject.vertexPositions[currentVal%playgroundParticles.skinnedWorldObject.vertexPositions.Length];
				var swoNorm : Vector3 = playgroundParticles.skinnedWorldObject.normals[currentVal%playgroundParticles.skinnedWorldObject.normals.Length];
				
				return Vector3(
					swoPos.x+(swoNorm.x*playgroundParticles.overflowOffset.z*iteration),
					swoPos.y+(swoNorm.y*playgroundParticles.overflowOffset.z*iteration),
					swoPos.z+(swoNorm.z*playgroundParticles.overflowOffset.z*iteration)
				)-swoPos;
			} else
			
			// Transform
			if (playgroundParticles.source == SOURCE.Transform && playgroundParticles.sourceTransform) {
				return (playgroundParticles.sourceTransform.forward*playgroundParticles.overflowOffset.z*iteration);
			} else
			
			// Paint
			if (playgroundParticles.source == SOURCE.Paint) {
				var paintPos : Vector3 = playgroundParticles.paint.GetPosition(currentVal);
				var paintNorm : Vector3 = playgroundParticles.paint.GetNormal(currentVal);
				
				return Vector3(
					paintPos.x+(paintNorm.x*playgroundParticles.overflowOffset.z*iteration),
					paintPos.y+(paintNorm.y*playgroundParticles.overflowOffset.z*iteration),
					paintPos.z+(paintNorm.z*playgroundParticles.overflowOffset.z*iteration)
				)-paintPos;
			}
		}
		
		// Return world position if all else fails
		return Vector3(
			playgroundParticles.overflowOffset.x*iteration,
			playgroundParticles.overflowOffset.y*iteration,
			playgroundParticles.overflowOffset.z*iteration
		);
	}
	
	// Returns the offset as a remainder from a specified transform
	static function GetOverflowOffset (playgroundParticles : PlaygroundParticles, currentVal : int, maxVal : int, overflowTransform : Transform) : Vector3 {
		var iteration : float = (currentVal/maxVal);
		return overflowTransform.TransformPoint(
			playgroundParticles.overflowOffset.x*iteration,
			playgroundParticles.overflowOffset.y*iteration,
			playgroundParticles.overflowOffset.z*iteration
		)-overflowTransform.position;
	}
	
	// Set size for particles
	static function SetSize (playgroundParticles:PlaygroundParticles, size:float) {
		for (var i = 0; i<playgroundParticles.particleCache.particles.Length; i++) {
			playgroundParticles.playgroundCache.size[i] = size;
			playgroundParticles.particleCache.particles[i].size = size;
		}
	}
	
	// Set random size for particles within sizeMinimum- and sizeMaximum range 
	static function SetSizeRandom (playgroundParticles:PlaygroundParticles, sizeMinimum : float, sizeMaximum : float) {
		playgroundParticles.playgroundCache.size = Playground.RandomFloat(playgroundParticles.particleCache.particles.Length, sizeMinimum, sizeMaximum);
		for (var i = 0; i<playgroundParticles.particleCache.particles.Length; i++) {
			playgroundParticles.particleCache.particles[i].size = playgroundParticles.playgroundCache.size[i];
		}
		playgroundParticles.sizeMin = sizeMinimum;
		playgroundParticles.sizeMax = sizeMaximum;
		playgroundParticles.previousSizeMin = playgroundParticles.sizeMin;
		playgroundParticles.previousSizeMax = playgroundParticles.sizeMax;
	}
	
	// Set random rotation for particles within rotationMinimum- and rotationMaximum range
	static function SetRotationRandom (playgroundParticles:PlaygroundParticles, rotationMinimum : float, rotationMaximum : float) {
		playgroundParticles.playgroundCache.rotationSpeed = Playground.RandomFloat(playgroundParticles.particleCache.particles.Length, rotationMinimum, rotationMaximum);
		for (var i = 0; i<playgroundParticles.particleCache.particles.Length; i++) {
			playgroundParticles.particleCache.particles[i].rotation = playgroundParticles.playgroundCache.initialRotation[i];
		}
		playgroundParticles.rotationSpeedMin = rotationMinimum;
		playgroundParticles.rotationSpeedMax = rotationMaximum;
		playgroundParticles.previousRotationSpeedMin = playgroundParticles.rotationSpeedMin;
		playgroundParticles.previousRotationSpeedMax = playgroundParticles.rotationSpeedMax;
	}
	
	// Set random initial rotation for particles within rotationMinimum- and rotationMaximum range
	static function SetInitialRotationRandom (playgroundParticles:PlaygroundParticles, rotationMinimum : float, rotationMaximum : float) {
		playgroundParticles.playgroundCache.initialRotation = Playground.RandomFloat(playgroundParticles.particleCache.particles.Length, rotationMinimum, rotationMaximum);
		for (var i = 0; i<playgroundParticles.particleCache.particles.Length; i++) {
			playgroundParticles.particleCache.particles[i].rotation = playgroundParticles.playgroundCache.initialRotation[i];
		}
		playgroundParticles.initialRotationMin = rotationMinimum;
		playgroundParticles.initialRotationMax = rotationMaximum;
		playgroundParticles.previousInitialRotationMin = playgroundParticles.initialRotationMin;
		playgroundParticles.previousInitialRotationMax = playgroundParticles.initialRotationMax;
	}
	
	// Set initial random velocity for particles within velocityMinimum- and velocityMaximum range
	static function SetVelocityRandom (playgroundParticles:PlaygroundParticles, velocityMinimum : Vector3, velocityMaximum : Vector3) {
		playgroundParticles.playgroundCache.initialVelocity = Playground.RandomVector3(playgroundParticles.particleCache.particles.Length, velocityMinimum, velocityMaximum);
		
		playgroundParticles.initialVelocityMin = velocityMinimum;
		playgroundParticles.initialVelocityMax = velocityMaximum;
		playgroundParticles.previousVelocityMin = playgroundParticles.initialVelocityMin;
		playgroundParticles.previousVelocityMax = playgroundParticles.initialVelocityMax;
	}
	
	// Set initial random local velocity for particles within velocityMinimum- and velocityMaximum range
	static function SetLocalVelocityRandom (playgroundParticles:PlaygroundParticles, velocityMinimum : Vector3, velocityMaximum : Vector3) {
		playgroundParticles.playgroundCache.initialLocalVelocity = Playground.RandomVector3(playgroundParticles.particleCache.particles.Length, velocityMinimum, velocityMaximum);
		
		playgroundParticles.initialLocalVelocityMin = velocityMinimum;
		playgroundParticles.initialLocalVelocityMax = velocityMaximum;
		playgroundParticles.previousLocalVelocityMin = playgroundParticles.initialLocalVelocityMin;
		playgroundParticles.previousLocalVelocityMax = playgroundParticles.initialLocalVelocityMax;
	}
	
	// Set material for particle system
	static function SetMaterial (playgroundParticles : PlaygroundParticles, particleMaterial : Material) {
		playgroundParticles.particleSystemRenderer.sharedMaterial = particleMaterial;
	}
	
	// Set alphas for particles
	static function SetAlpha (playgroundParticles:PlaygroundParticles, alpha : float) {
		for (var i = 0; i<playgroundParticles.particleCache.particles.Length; i++)
			playgroundParticles.particleCache.particles[i].color.a = alpha;
	}
	
	// Set particle random particle positions within min- and max range
	static function Random (playgroundParticles : PlaygroundParticles, min : Vector3, max : Vector3) {
		for (var p : ParticleSystem.Particle in playgroundParticles.particleCache.particles) {
			p.position = Vector3(
				UnityEngine.Random.Range(min.x, max.x),
				UnityEngine.Random.Range(min.y, max.y),
				UnityEngine.Random.Range(min.z, max.z)
			);
		}
		Update(playgroundParticles);
	}
	
	// Move all particles in direction
	static function Translate (playgroundParticles:PlaygroundParticles, direction:Vector3) {
		for (var i = 0; i<playgroundParticles.particleCache.particles.Length; i++)
			playgroundParticles.particleCache.particles[i].position += direction;
	}
	
	// Add new state from state
	static function Add (playgroundParticles : PlaygroundParticles, state : ParticleState) {
		playgroundParticles.states.Add(state);
		state.Initialize();
	}
	
	// Add new state from image
	static function Add (playgroundParticles : PlaygroundParticles, image : Texture2D, scale : float, offset : Vector3, stateName : String, stateTransform : Transform) {
		playgroundParticles.states.Add(new ParticleState());
		playgroundParticles.states[playgroundParticles.states.Count-1].ConstructParticles(image,scale,offset,stateName,stateTransform);
	}
	
	// Add new state from image with depthmap
	static function Add (playgroundParticles:PlaygroundParticles, image:Texture2D, depthmap:Texture2D, depthmapStrength:float, scale:float, offset:Vector3, stateName:String, stateTransform:Transform) {
		playgroundParticles.states.Add(new ParticleState());
		playgroundParticles.states[playgroundParticles.states.Count-1].ConstructParticles(image,scale,offset,stateName,stateTransform);
		playgroundParticles.states[playgroundParticles.states.Count-1].stateDepthmap = depthmap;
		playgroundParticles.states[playgroundParticles.states.Count-1].stateDepthmapStrength = depthmapStrength;
	}
	
	// Destroy a PlaygroundParticles object
	static function Destroy (playgroundParticles : PlaygroundParticles) {
		Clear(playgroundParticles);
		MonoBehaviour.DestroyImmediate(playgroundParticles.particleSystemGameObject);
		playgroundParticles = null;
	}
	
	// Sorts the particles in lifetime
	static function SetLifetime (playgroundParticles : PlaygroundParticles, time : float) {
		playgroundParticles.lifetime = time;
		playgroundParticles.playgroundCache.lifetimeOffset = new float[playgroundParticles.particleCount];
		var currentCount : int = playgroundParticles.particleCount;
		switch (playgroundParticles.sorting) {
			case SORTING.Scrambled:
				playgroundParticles.playgroundCache.lifetimeOffset = Playground.RandomFloat(playgroundParticles.particleCount, .0, playgroundParticles.lifetime);
			break;
			case SORTING.ScrambledLinear:
				var slPerc : float;
				for (var sl = 0; sl<playgroundParticles.particleCount; sl++) {
					if (currentCount!=playgroundParticles.particleCount) return;
					slPerc = (sl*1.0)/(playgroundParticles.particleCount*1.0);
					playgroundParticles.playgroundCache.lifetimeOffset[sl] = playgroundParticles.lifetime*slPerc;
				}
				Playground.ShuffleFloat(playgroundParticles.playgroundCache.lifetimeOffset);
			break;
			case SORTING.Burst:
				// No action needed for spawning all particles at once
			break;
			case SORTING.Reversed:
				var lPerc : float;
				for (var l = 0; l<playgroundParticles.particleCount; l++) {
					if (currentCount!=playgroundParticles.particleCount) return;
					lPerc = (l*1.0)/(playgroundParticles.particleCount*1.0);
					playgroundParticles.playgroundCache.lifetimeOffset[l] = playgroundParticles.lifetime*lPerc;
				}
			break;
			case SORTING.Linear:
				var rPerc : float;
				var rInc : int;
				for (var r = playgroundParticles.particleCount-1; r>=0; r--) {
					if (currentCount!=playgroundParticles.particleCount) return;
					rPerc = (rInc*1.0)/(playgroundParticles.particleCount*1.0);
					rInc++;
					playgroundParticles.playgroundCache.lifetimeOffset[r] = playgroundParticles.lifetime*rPerc;
				}
			break;
			case SORTING.NearestNeighbor:
				playgroundParticles.nearestNeighborOrigin = Mathf.Clamp(playgroundParticles.nearestNeighborOrigin, 0, playgroundParticles.particleCount-1);
				var nnDist : float[] = new float[playgroundParticles.particleCount];
				var nnHighest : float;
				for (var nn = 0; nn<playgroundParticles.particleCount; nn++) {
					if (currentCount!=playgroundParticles.particleCount) return;
					nnDist[nn%playgroundParticles.particleCount] = Vector3.Distance(playgroundParticles.playgroundCache.targetPosition[playgroundParticles.nearestNeighborOrigin%playgroundParticles.particleCount], playgroundParticles.playgroundCache.targetPosition[nn%playgroundParticles.particleCount]);
					if (nnDist[nn%playgroundParticles.particleCount]>nnHighest)
						nnHighest = nnDist[nn%playgroundParticles.particleCount];
				}
				for (nn = 0; nn<playgroundParticles.particleCount; nn++) {
					if (currentCount!=playgroundParticles.particleCount) return;
					playgroundParticles.playgroundCache.lifetimeOffset[nn%playgroundParticles.particleCount] = Mathf.Lerp(playgroundParticles.lifetime, 0, (nnDist[nn%playgroundParticles.particleCount]/nnHighest));
				}
			break;
			case SORTING.NearestNeighborReversed:
				playgroundParticles.nearestNeighborOrigin = Mathf.Clamp(playgroundParticles.nearestNeighborOrigin, 0, playgroundParticles.particleCount-1);
				var nnrDist : float[] = new float[playgroundParticles.particleCount];
				var nnrHighest : float;
				for (var nnr = 0; nnr<playgroundParticles.particleCount; nnr++) {
					if (currentCount!=playgroundParticles.particleCount) return;
					nnrDist[nnr%playgroundParticles.particleCount] = Vector3.Distance(playgroundParticles.playgroundCache.targetPosition[playgroundParticles.nearestNeighborOrigin], playgroundParticles.playgroundCache.targetPosition[nnr%playgroundParticles.particleCount]);
					if (nnrDist[nnr%playgroundParticles.particleCount]>nnrHighest)
						nnrHighest = nnrDist[nnr%playgroundParticles.particleCount];
				}
				for (nnr = 0; nnr<playgroundParticles.particleCount; nnr++) {
					if (currentCount!=playgroundParticles.particleCount) return;
					playgroundParticles.playgroundCache.lifetimeOffset[nnr%playgroundParticles.particleCount] = Mathf.Lerp(0, playgroundParticles.lifetime, (nnrDist[nnr%playgroundParticles.particleCount]/nnrHighest));
				}
			break;
		}
		playgroundParticles.previousLifetime = time;
		
		SetEmissionRate(playgroundParticles);
		SetParticleTimeNow(playgroundParticles);
	}
	
	// Set emission rate percentage of particle count
	static function SetEmissionRate (playgroundParticles : PlaygroundParticles) {
		var rateCount : float = playgroundParticles.lifetime*playgroundParticles.emissionRate;
		var currentCount : int = playgroundParticles.particleCount;
		for (var p = 0; p<playgroundParticles.particleCount; p++) {
			if (currentCount!=playgroundParticles.particleCount || playgroundParticles.isYieldRefreshing) return;
			if (playgroundParticles.emissionRate!=0)
				playgroundParticles.playgroundCache.emission[p] = (playgroundParticles.playgroundCache.lifetimeOffset[p]>=playgroundParticles.lifetime-rateCount);
			else playgroundParticles.playgroundCache.emission[p] = false;
			if (playgroundParticles.playgroundCache.emission[p])
				playgroundParticles.playgroundCache.rebirth[p] = true;
		}
		playgroundParticles.previousEmissionRate = playgroundParticles.emissionRate;
	}
	
	// Set life and death of particles
	static function SetParticleTimeNow (playgroundParticles : PlaygroundParticles) {
		playgroundParticles.playgroundCache.birth = new float[playgroundParticles.particleCount];
		playgroundParticles.playgroundCache.death = new float[playgroundParticles.particleCount];  
		playgroundParticles.playgroundCache.life = new float[playgroundParticles.particleCount];
		var currentTime : float = Playground.globalTime;
		var p : int;
		if (playgroundParticles.sorting!=SORTING.Burst) {
			for (p = 0; p<playgroundParticles.particleCount; p++) {		 
				playgroundParticles.playgroundCache.life[p] = playgroundParticles.lifetime-(playgroundParticles.lifetime-playgroundParticles.playgroundCache.lifetimeOffset[p]);
				playgroundParticles.playgroundCache.birth[p] = currentTime-playgroundParticles.playgroundCache.life[p];
				playgroundParticles.playgroundCache.death[p] = currentTime+(playgroundParticles.lifetime-playgroundParticles.playgroundCache.lifetimeOffset[p]);
			}
		} else {
			// Force recalculation for burst mode to initiate sequence directly
			for (p = 0; p<playgroundParticles.particleCount; p++) {		 
				playgroundParticles.playgroundCache.life[p] = playgroundParticles.lifetime;
				playgroundParticles.playgroundCache.birth[p] = currentTime-playgroundParticles.lifetime;
				playgroundParticles.playgroundCache.death[p] = currentTime;
			}
		}
	}
	
	// Set life and death of particles after emit has changed
	static function SetParticleTimeNowWithRestEmission (playgroundParticles : PlaygroundParticles) {
		var currentTime : float = Playground.globalTime;
		for (var p = 0; p<playgroundParticles.particleCount; p++) {
			if (!playgroundParticles.playgroundCache.rebirth[p]) {	 
				playgroundParticles.playgroundCache.life[p] = playgroundParticles.lifetime-(playgroundParticles.lifetime-playgroundParticles.playgroundCache.lifetimeOffset[p]);
				playgroundParticles.playgroundCache.birth[p] = currentTime-playgroundParticles.playgroundCache.life[p];
				playgroundParticles.playgroundCache.death[p] = currentTime+(playgroundParticles.lifetime-playgroundParticles.playgroundCache.lifetimeOffset[p]);
			}
		}
	}
	
	// Get color from evaluated lifetime color value where time is normalized
	static function GetColorAtLifetime (playgroundParticles : PlaygroundParticles, time : float) : Color32 {
		return playgroundParticles.lifetimeColor.Evaluate(time/playgroundParticles.lifetime);
	}
	
	// Color all particles from evaluated lifetime color value where time is normalized
	static function SetColorAtLifetime (playgroundParticles : PlaygroundParticles, time : float) {
		var c : Color32 = playgroundParticles.lifetimeColor.Evaluate(time/playgroundParticles.lifetime);
		for (var p = 0; p<playgroundParticles.particleCount; p++)
			playgroundParticles.particleCache.particles[p].color = c;
		playgroundParticles.shurikenParticleSystem.SetParticles(playgroundParticles.particleCache.particles, playgroundParticles.particleCache.particles.Length);
	}
	
	// Color all particles from lifetime span with sorting
	static function SetColorWithLifetimeSorting (playgroundParticles : PlaygroundParticles) {
		SetLifetime(playgroundParticles, playgroundParticles.lifetime);
		var c : Color32;
		for (var p = 0; p<playgroundParticles.particleCount; p++) {
			c = playgroundParticles.lifetimeColor.Evaluate(playgroundParticles.playgroundCache.life[p]/playgroundParticles.lifetime);
			playgroundParticles.particleCache.particles[p].color = c;
		}
		playgroundParticles.shurikenParticleSystem.SetParticles(playgroundParticles.particleCache.particles, playgroundParticles.particleCache.particles.Length);
	}
	
	// Set position of emission when using a Transform together with Overflow Offset
	static function SetTransformPositionWithOffset (playgroundParticles : PlaygroundParticles) {
		for (var p = 0; p<playgroundParticles.particleCount; p++)
			playgroundParticles.particleCache.particles[p].position = playgroundParticles.sourceTransform.position+GetOverflowOffset(playgroundParticles, p, 1);
		playgroundParticles.shurikenParticleSystem.SetParticles(playgroundParticles.particleCache.particles, playgroundParticles.particleCache.particles.Length);
	}
	
	// Sets the amount of particles and initiates the necessary arrays
	static function SetParticleCount (playgroundParticles : PlaygroundParticles, amount : int) {
		if (amount<0) amount = 0;
		
		// Null Cache
		playgroundParticles.particleCache.particles = null;
		playgroundParticles.playgroundCache.velocity = null;
		playgroundParticles.playgroundCache.targetPosition = null;
		playgroundParticles.playgroundCache.previousTargetPosition = null;
		playgroundParticles.playgroundCache.life = null;
		playgroundParticles.playgroundCache.birth = null;
		playgroundParticles.playgroundCache.death = null;
		playgroundParticles.playgroundCache.size = null;
		playgroundParticles.playgroundCache.rebirth = null;
		playgroundParticles.playgroundCache.emission = null;
		playgroundParticles.playgroundCache.changedByProperty = null;
		playgroundParticles.playgroundCache.changedByPropertyColor = null;
		playgroundParticles.playgroundCache.changedByPropertyColorLerp = null;
		playgroundParticles.playgroundCache.changedByPropertySize = null;
		playgroundParticles.inTransition = false;
		
		// Create Particles
		playgroundParticles.particleCache.particles = new ParticleSystem.Particle[amount];
		playgroundParticles.shurikenParticleSystem.Emit(amount);
		playgroundParticles.shurikenParticleSystem.GetParticles(playgroundParticles.particleCache.particles);
		
		// Rebuild Cache
		playgroundParticles.playgroundCache.rebirth = new boolean[amount];
		playgroundParticles.playgroundCache.emission = new boolean[amount];
		playgroundParticles.playgroundCache.changedByProperty = new boolean[amount];
		playgroundParticles.playgroundCache.changedByPropertyColor = new boolean[amount];
		playgroundParticles.playgroundCache.changedByPropertyColorLerp = new boolean[amount];
		playgroundParticles.playgroundCache.changedByPropertySize = new boolean[amount];
		playgroundParticles.playgroundCache.targetPosition = new Vector3[amount];
		playgroundParticles.playgroundCache.previousTargetPosition = new Vector3[amount];
		playgroundParticles.playgroundCache.velocity = new Vector3[amount];
		playgroundParticles.particleCount = amount;
		playgroundParticles.previousParticleCount = playgroundParticles.particleCount;
		
		// Set sizes
		SetSizeRandom(playgroundParticles, playgroundParticles.sizeMin, playgroundParticles.sizeMax);
		playgroundParticles.previousSizeMin = playgroundParticles.sizeMin;
		playgroundParticles.previousSizeMax = playgroundParticles.sizeMax;
		
		// Set rotations
		playgroundParticles.playgroundCache.initialRotation = Playground.RandomFloat(amount, playgroundParticles.initialRotationMin, playgroundParticles.initialRotationMax);
		playgroundParticles.playgroundCache.rotationSpeed = Playground.RandomFloat(amount, playgroundParticles.rotationSpeedMin, playgroundParticles.rotationSpeedMax);
		playgroundParticles.previousInitialRotationMin = playgroundParticles.initialRotationMin;
		playgroundParticles.previousInitialRotationMax = playgroundParticles.initialRotationMax;
		playgroundParticles.previousRotationSpeedMin = playgroundParticles.rotationSpeedMin;
		playgroundParticles.previousRotationSpeedMax = playgroundParticles.rotationSpeedMax;
		
		// Set velocities
		SetVelocityRandom(playgroundParticles, playgroundParticles.initialVelocityMin, playgroundParticles.initialVelocityMax);
		SetLocalVelocityRandom(playgroundParticles, playgroundParticles.initialLocalVelocityMin, playgroundParticles.initialLocalVelocityMax);
		
		// Garbage Collect
		if (Playground.reference!=null && Playground.reference.garbageCollectOnResize)
			GC.Collect();
		
		// Set Lifetime
		SetLifetime(playgroundParticles, playgroundParticles.lifetime);
		
		// Set Emission
		Emission(playgroundParticles, playgroundParticles.emit);
		
		// Update initial target position
		SetInitialTargetPosition(playgroundParticles, Playground.initialTargetPosition);
				
		// Update source if calculation is off
		if (!playgroundParticles.calculate) {
			if (playgroundParticles.source==SOURCE.Transform && playgroundParticles.overflowOffset!=Vector3.zero) {
				SetTransformPositionWithOffset(playgroundParticles);
				SetColorWithLifetimeSorting(playgroundParticles);
			} else if (playgroundParticles.source==SOURCE.WorldObject && playgroundParticles.worldObject!=null) {
				SetPosition(playgroundParticles, playgroundParticles.worldObject);
				SetColorWithLifetimeSorting(playgroundParticles);
			} else if (playgroundParticles.source==SOURCE.SkinnedWorldObject && playgroundParticles.skinnedWorldObject!=null) {
				SetPosition(playgroundParticles, playgroundParticles.skinnedWorldObject);
				SetColorWithLifetimeSorting(playgroundParticles);
			}
		}
		
		// Misc
		playgroundParticles.isPainting = false;
	}
	
	// Updates a PlaygroundParticles object (called from Playground)
	static function Update (playgroundParticles : PlaygroundParticles) {
		
		// Particle count
		if (playgroundParticles.particleCount!=playgroundParticles.previousParticleCount) {
			SetParticleCount(playgroundParticles, playgroundParticles.particleCount);
			playgroundParticles.Start();
		}
		
		// Particle emission
		if (playgroundParticles.emit!=playgroundParticles.previousEmission)
			Emission(playgroundParticles, playgroundParticles.emit);
		
		// Particle size
		if (playgroundParticles.sizeMin!=playgroundParticles.previousSizeMin || playgroundParticles.sizeMax!=playgroundParticles.previousSizeMax)
			SetSizeRandom(playgroundParticles, playgroundParticles.sizeMin, playgroundParticles.sizeMax);
		
		// Particle rotation
		if (playgroundParticles.initialRotationMin!=playgroundParticles.previousInitialRotationMin || playgroundParticles.initialRotationMax!=playgroundParticles.previousInitialRotationMax)
			SetInitialRotationRandom(playgroundParticles, playgroundParticles.initialRotationMin, playgroundParticles.initialRotationMax);
		if (playgroundParticles.rotationSpeedMin!=playgroundParticles.previousRotationSpeedMin || playgroundParticles.rotationSpeedMax!=playgroundParticles.previousRotationSpeedMax)
			SetRotationRandom(playgroundParticles, playgroundParticles.rotationSpeedMin, playgroundParticles.rotationSpeedMax);
			
		// Particle velocity
		if (playgroundParticles.applyInitialVelocity)
			if (playgroundParticles.initialVelocityMin!=playgroundParticles.previousVelocityMin || playgroundParticles.initialVelocityMax!=playgroundParticles.previousVelocityMax || !playgroundParticles.playgroundCache.initialVelocity || playgroundParticles.playgroundCache.initialVelocity.Length!=playgroundParticles.particleCount)
				SetVelocityRandom(playgroundParticles, playgroundParticles.initialVelocityMin, playgroundParticles.initialVelocityMax);
			
		// Particle local velocity
		if (playgroundParticles.applyInitialLocalVelocity)
			if (playgroundParticles.initialLocalVelocityMin!=playgroundParticles.previousLocalVelocityMin || playgroundParticles.initialLocalVelocityMax!=playgroundParticles.previousLocalVelocityMax || !playgroundParticles.playgroundCache.initialLocalVelocity || playgroundParticles.playgroundCache.initialLocalVelocity.Length!=playgroundParticles.particleCount)
				SetLocalVelocityRandom(playgroundParticles, playgroundParticles.initialLocalVelocityMin, playgroundParticles.initialLocalVelocityMax);
		
		// Particle life
		if (playgroundParticles.previousLifetime!=playgroundParticles.lifetime)
			SetLifetime(playgroundParticles, playgroundParticles.lifetime);
			
		// Particle emission rate
		if (playgroundParticles.previousEmissionRate!=playgroundParticles.emissionRate)
			SetEmissionRate(playgroundParticles);
		
		// Particle state change transition
		if (playgroundParticles.source==SOURCE.State && playgroundParticles.activeState!=playgroundParticles.previousActiveState) {
			if (playgroundParticles.states[playgroundParticles.activeState].positionLength>playgroundParticles.particleCount)
				SetParticleCount(playgroundParticles, playgroundParticles.states[playgroundParticles.activeState].positionLength);
			playgroundParticles.InitTransition(playgroundParticles);
			playgroundParticles.previousActiveState = playgroundParticles.activeState;
		}
		
		// Particle calculation
		if (Playground.reference.calculate && playgroundParticles.calculate && playgroundParticles.source!=SOURCE.Script && !playgroundParticles.inTransition)
			Calculate(playgroundParticles);
		else playgroundParticles.cameFromNonCalculatedFrame = true;
		
		// Assign all particles into the particle system
		if (!playgroundParticles.inTransition && playgroundParticles.particleCache!=null && playgroundParticles.particleCache.particles.Length>0)
			playgroundParticles.shurikenParticleSystem.SetParticles(playgroundParticles.particleCache.particles, playgroundParticles.particleCache.particles.Length);
	
		// Make sure that variables are reset till next frame
		playgroundParticles.isPainting = false;		
	}
	
	// Initial target position
	static function SetInitialTargetPosition (playgroundParticles : PlaygroundParticles, position : Vector3) {
		for (var p = 0; p<playgroundParticles.particleCache.particles.Length; p++) {
			playgroundParticles.playgroundCache.targetPosition[p] = position;
			playgroundParticles.particleCache.particles[p].position = position;
		}
	}
	
	// Transition event on activeState change
	function InitTransition (playgroundParticles : PlaygroundParticles) {
		playgroundParticles.Transition(playgroundParticles);
	}
	function Transition (playgroundParticles : PlaygroundParticles) {
		
		playgroundParticles.inTransition = true;
		
		var p : int;
		var t : float = .0;
		var timeTransitionStarted : float = Time.realtimeSinceStartup;
		var timeTransitionFinishes : float = timeTransitionStarted+playgroundParticles.transitionTime;
		var thisState : int = playgroundParticles.activeState;
		var currentPosition : Vector3[];
		var currentColor : Color32[];
		var targetColor : Color32;
		var calculatedoverflowOffset : Vector3;
		
		if (playgroundParticles.states[playgroundParticles.activeState].stateMesh!=null && playgroundParticles.states[playgroundParticles.activeState].stateTexture==null)
			targetColor = GetColorAtLifetime(playgroundParticles, 0);
		
		switch (playgroundParticles.transition) {
		
			case TRANSITION.None:
				// Set all particles directly
				for (p = 0; p<playgroundParticles.particleCache.particles.Length; p++) {
				
					// overflow offset
					calculatedoverflowOffset = GetOverflowOffset(playgroundParticles, p, playgroundParticles.states[playgroundParticles.activeState].positionLength);
				
					if (playgroundParticles.states[playgroundParticles.activeState].stateTransform!=null) 
						playgroundParticles.playgroundCache.targetPosition[p%playgroundParticles.particleCache.particles.Length] = playgroundParticles.states[playgroundParticles.activeState].GetParentedPosition(p%playgroundParticles.particleCache.particles.Length)+calculatedoverflowOffset;
					else
						playgroundParticles.playgroundCache.targetPosition[p%playgroundParticles.particleCache.particles.Length] = playgroundParticles.states[playgroundParticles.activeState].GetPosition(p%playgroundParticles.particleCache.particles.Length)+calculatedoverflowOffset;
					
					if (playgroundParticles.states[playgroundParticles.activeState].stateMesh!=null && playgroundParticles.states[playgroundParticles.activeState].stateTexture==null)
						playgroundParticles.particleCache.particles[p%playgroundParticles.particleCache.particles.Length].color = targetColor;
					else
						playgroundParticles.particleCache.particles[p%playgroundParticles.particleCache.particles.Length].color = playgroundParticles.states[playgroundParticles.activeState%playgroundParticles.states.Count].GetColor(p%playgroundParticles.particleCache.particles.Length);
					playgroundParticles.particleCache.particles[p%playgroundParticles.particleCache.particles.Length].position = playgroundParticles.playgroundCache.targetPosition[p%playgroundParticles.particleCache.particles.Length];
				}
			break;
			
			case TRANSITION.Lerp:
				currentPosition = new Vector3[playgroundParticles.particleCache.particles.Length];
				currentColor = new Color32[playgroundParticles.particleCache.particles.Length];
				
				// Set target, current positions and colors
				for (p = 0; p<playgroundParticles.particleCache.particles.Length; p++) {
					
					// Overflow offset
					calculatedoverflowOffset = GetOverflowOffset(playgroundParticles, p, playgroundParticles.states[playgroundParticles.activeState].positionLength);
					
					// Set current position and color, target position (with overflowOffset)
					currentPosition[p%playgroundParticles.particleCache.particles.Length] = playgroundParticles.particleCache.particles[p%playgroundParticles.particleCache.particles.Length].position;
					
					if (playgroundParticles.states[playgroundParticles.activeState].stateTransform!=null) 
						playgroundParticles.playgroundCache.targetPosition[p%playgroundParticles.particleCache.particles.Length] = playgroundParticles.states[playgroundParticles.activeState].GetParentedPosition(p%playgroundParticles.particleCache.particles.Length)+calculatedoverflowOffset;
					else
						playgroundParticles.playgroundCache.targetPosition[p%playgroundParticles.particleCache.particles.Length] = playgroundParticles.states[playgroundParticles.activeState].GetPosition(p%playgroundParticles.particleCache.particles.Length)+calculatedoverflowOffset;
					
					currentColor[p%playgroundParticles.particleCache.particles.Length] = playgroundParticles.particleCache.particles[p%playgroundParticles.particleCache.particles.Length].color;
				}
								
				// Lerp towards target
				timeTransitionStarted = Time.realtimeSinceStartup;
				while (timeTransitionStarted+t<timeTransitionFinishes && playgroundParticles.activeState==thisState && playgroundParticles.inTransition) {
					
					// Update time
					t = (Time.realtimeSinceStartup-timeTransitionStarted);
					
					// Iterate particles
					for (p = 0; p<playgroundParticles.particleCache.particles.Length; p++) {
						
						// Position over time
						playgroundParticles.particleCache.particles[p%playgroundParticles.particleCache.particles.Length].position = Vector3.Lerp(
							currentPosition[p%playgroundParticles.particleCache.particles.Length],
							playgroundParticles.playgroundCache.targetPosition[p%playgroundParticles.particleCache.particles.Length],
							t/playgroundParticles.transitionTime
						);
						
						// Color over time
						if (playgroundParticles.states[playgroundParticles.activeState].stateTexture!=null)
							targetColor = playgroundParticles.states[playgroundParticles.activeState%playgroundParticles.states.Count].GetColor(p%playgroundParticles.particleCache.particles.Length);
						
						playgroundParticles.particleCache.particles[p%playgroundParticles.particleCache.particles.Length].color = Color.Lerp(
							currentColor[p%playgroundParticles.particleCache.particles.Length],
							targetColor,
							t/playgroundParticles.transitionTime
						);
						
					}
					
					// Assign the particles back
					playgroundParticles.shurikenParticleSystem.SetParticles(playgroundParticles.particleCache.particles, playgroundParticles.particleCache.particles.Length);
					
					yield;
				}
			break;
			
			case TRANSITION.Fade: case TRANSITION.Fade2:
				
				var isFade2 : boolean = (playgroundParticles.transition==TRANSITION.Fade2);
				
				// Set colors (and positions if Fade2)
				currentColor = new Color32[playgroundParticles.particleCache.particles.Length];
				if (isFade2)
					currentPosition = new Vector3[playgroundParticles.particleCache.particles.Length];
				for (p = 0; p<playgroundParticles.particleCache.particles.Length; p++) {
					currentColor[p%playgroundParticles.particleCache.particles.Length] = playgroundParticles.particleCache.particles[p%playgroundParticles.particleCache.particles.Length].color;
					if (isFade2) {
						currentPosition[p%playgroundParticles.particleCache.particles.Length] = playgroundParticles.particleCache.particles[p%playgroundParticles.particleCache.particles.Length].position;
						
						calculatedoverflowOffset = GetOverflowOffset(playgroundParticles, p, playgroundParticles.states[playgroundParticles.activeState].positionLength);
						if (playgroundParticles.states[playgroundParticles.activeState].stateTransform!=null) 
							playgroundParticles.playgroundCache.targetPosition[p%playgroundParticles.particleCache.particles.Length] = playgroundParticles.states[playgroundParticles.activeState].GetParentedPosition(p%playgroundParticles.particleCache.particles.Length)+calculatedoverflowOffset;
						else
							playgroundParticles.playgroundCache.targetPosition[p%playgroundParticles.particleCache.particles.Length] = playgroundParticles.states[playgroundParticles.activeState].GetPosition(p%playgroundParticles.particleCache.particles.Length)+calculatedoverflowOffset;
					}
				}
				
				var transitionColor : Color;
				var transitionAlpha : float;
				var fadeOut : boolean = true;
				var setPositions : boolean = true;
				
				timeTransitionStarted = Time.realtimeSinceStartup;
				
				while (timeTransitionStarted+t<timeTransitionFinishes && playgroundParticles.activeState==thisState && playgroundParticles.inTransition) {
					
					// Update time
					t = (Time.realtimeSinceStartup-timeTransitionStarted);

					for (p = 0; p<playgroundParticles.particleCache.particles.Length; p++) {
						
						// Get color and set alpha
						if (fadeOut) {
							transitionColor = currentColor[p%playgroundParticles.particleCache.particles.Length];
							transitionAlpha = Mathf.Lerp(transitionColor.a, 0, t/(playgroundParticles.transitionTime*.5));
							if (t/(playgroundParticles.transitionTime*.5)>=1.0) {
								transitionAlpha = 0;
								fadeOut=false;
							}
						} else {
							if (playgroundParticles.states[playgroundParticles.activeState].stateMesh==null && playgroundParticles.states[playgroundParticles.activeState].stateTexture!=null)
								transitionColor = playgroundParticles.states[playgroundParticles.activeState%playgroundParticles.states.Count].GetColor(p%playgroundParticles.particleCache.particles.Length);
							else
								transitionColor = targetColor;
							transitionAlpha = Mathf.Lerp(0, transitionColor.a, -1.0+(t/(playgroundParticles.transitionTime*.5)));
						}
						transitionColor.a = transitionAlpha;
						
						// Assign color to the particle
						playgroundParticles.particleCache.particles[p%playgroundParticles.particleCache.particles.Length].color = transitionColor;
						
						// Position if Transition is Fade2
						if (isFade2)
							playgroundParticles.particleCache.particles[p%playgroundParticles.particleCache.particles.Length].position = Vector3.Lerp(
								currentPosition[p%playgroundParticles.particleCache.particles.Length],
								playgroundParticles.playgroundCache.targetPosition[p%playgroundParticles.particleCache.particles.Length],
								t/playgroundParticles.transitionTime
							);
					}
					
					if (!fadeOut && setPositions) {
						setPositions = false;
						SetPosition(playgroundParticles, thisState, false);
					}
					
					// Assign the particles back
					playgroundParticles.shurikenParticleSystem.SetParticles(playgroundParticles.particleCache.particles, playgroundParticles.particleCache.particles.Length);
					
					yield;
				}
				
			break;
		}
		
		// No longer in transition
		if (playgroundParticles.activeState==thisState) {
			SetParticleTimeNow(playgroundParticles);
			playgroundParticles.inTransition = false;
		}
	}
	
	// Set emission of PlaygroundParticles object
	static function Emission (playgroundParticles : PlaygroundParticles, emission : boolean) {
		playgroundParticles.previousEmission = emission;
		if (emission) {
			for (var p = 0; p<playgroundParticles.playgroundCache.rebirth.Length; p++)
				playgroundParticles.playgroundCache.rebirth[p] = true;
			SetParticleTimeNowWithRestEmission(playgroundParticles);
		}
	}
	
	// Returns the angle between a and b with normal direction
	static function SignedAngle (a : Vector3, b : Vector3, n : Vector3) {
	    return (Vector3.Angle(a, b)*Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b))));
	}
	
	// Run all calculations for this PlaygroundParticles object
	static function Calculate (playgroundParticles : PlaygroundParticles) {
		
		if (!playgroundParticles.enabled) return;
		
		var distance : float;
		var t : float;
		var prevP : int;
		var deltaVelocity : Vector3;
		var hitInfo : RaycastHit;
		var currentSource : SOURCE = playgroundParticles.source;
		var currentState = playgroundParticles.activeState;
		var lifetimeColor : Color;
		var calculatedoverflowOffset : Vector3;
		var m : int;
		
		// Get data from source
		if (playgroundParticles.emit)
			switch (playgroundParticles.source) {
				case SOURCE.State:
					if (playgroundParticles.states.Count==0)
						return;
				break;
				case SOURCE.Transform:
					if (playgroundParticles.sourceTransform==null)
						return;
				break;
				case SOURCE.WorldObject:
					// Handle vertex data in active World Object
					if (playgroundParticles.worldObject.gameObject!=null && playgroundParticles.worldObject.mesh!=null) {
						if (playgroundParticles.worldObject.gameObject.GetInstanceID()!=playgroundParticles.worldObject.cachedId) {
							NewWorldObject(playgroundParticles, playgroundParticles.worldObject.gameObject.transform);
						}
						GetPosition(playgroundParticles.worldObject.vertexPositions, playgroundParticles.worldObject);
						if (playgroundParticles.worldObjectUpdateNormals)
							GetNormals(playgroundParticles.worldObject.normals, playgroundParticles.worldObject);
					} else return;
				break;
				case SOURCE.SkinnedWorldObject:
					// Handle vertex data in active Skinned World Object
					if (playgroundParticles.skinnedWorldObject.gameObject!=null && playgroundParticles.skinnedWorldObject.mesh!=null) {
						if (playgroundParticles.skinnedWorldObject.gameObject.GetInstanceID()!=playgroundParticles.skinnedWorldObject.cachedId)
							NewSkinnedWorldObject(playgroundParticles, playgroundParticles.skinnedWorldObject.gameObject.transform);
						if (Time.frameCount%Playground.skinnedUpdateRate==0)
							GetPosition(playgroundParticles.skinnedWorldObject.vertexPositions, playgroundParticles.skinnedWorldObject.normals, playgroundParticles.skinnedWorldObject);
					} else return;
				break;
				case SOURCE.Paint:
					if (playgroundParticles.paint==null)
						return;
				break;
			}
		
		var evaluatedLife : float;
		
		if (playgroundParticles.cameFromNonCalculatedFrame) {
			SetLifetime(playgroundParticles, playgroundParticles.lifetime);
			playgroundParticles.cameFromNonCalculatedFrame = false;
		}
		
		t = (Playground.globalDeltaTime*playgroundParticles.particleTimescale)*(playgroundParticles.updateRate*1.0);
				
		// Calculate each particle
		for (var p = 0; p<playgroundParticles.particleCache.particles.Length; p++) {
			if (currentSource!=playgroundParticles.source || playgroundParticles.particleCache.particles.Length!=playgroundParticles.particleCount) return;
			
			// If this particle is set to rebirth
			if (playgroundParticles.playgroundCache.rebirth[p]) {
				
				// Calculate lifetime
				evaluatedLife = (Playground.globalTime-playgroundParticles.playgroundCache.birth[p])/playgroundParticles.lifetime;
				 
				// Previous particle
				prevP = p>0?p-1:playgroundParticles.particleCache.particles.Length-1;
				
				// Lifetime
				if (playgroundParticles.playgroundCache.life[p]<playgroundParticles.lifetime) {
					playgroundParticles.playgroundCache.life[p] = playgroundParticles.lifetime*evaluatedLife;
				} else {
					playgroundParticles.playgroundCache.life[p] = 0;
					playgroundParticles.playgroundCache.birth[p] = playgroundParticles.playgroundCache.death[p]; 
					playgroundParticles.playgroundCache.death[p] += playgroundParticles.lifetime;
					playgroundParticles.playgroundCache.rebirth[p] = (playgroundParticles.emit && playgroundParticles.playgroundCache.emission[p]);
					if (playgroundParticles.applyInitialVelocity)
						playgroundParticles.playgroundCache.velocity[p] = playgroundParticles.playgroundCache.initialVelocity[p];
					else
						playgroundParticles.playgroundCache.velocity[p] = Vector3.zero;
					playgroundParticles.particleCache.particles[p].rotation = playgroundParticles.playgroundCache.initialRotation[p];
					if (!playgroundParticles.playgroundCache.rebirth[p]) {
						playgroundParticles.playgroundCache.targetPosition[p] = Playground.initialTargetPosition;
						playgroundParticles.playgroundCache.previousTargetPosition[p] = Playground.initialTargetPosition;
					}
					playgroundParticles.particleCache.particles[p].position = playgroundParticles.playgroundCache.targetPosition[p];
					playgroundParticles.playgroundCache.changedByProperty[p] = false;
					playgroundParticles.playgroundCache.changedByPropertyColor[p] = false;
					playgroundParticles.playgroundCache.changedByPropertyColorLerp[p] = false;
					playgroundParticles.playgroundCache.changedByPropertySize[p] = false;
				}
				
				// Lifetime size
				if (playgroundParticles.lifetime>0) {
					if (!playgroundParticles.playgroundCache.changedByPropertySize[p])
						playgroundParticles.particleCache.particles[p].size = playgroundParticles.playgroundCache.size[p]*playgroundParticles.lifetimeSize.Evaluate(playgroundParticles.playgroundCache.life[p]/playgroundParticles.lifetime);
				} else playgroundParticles.particleCache.particles[p].size = playgroundParticles.playgroundCache.size[p];
				
				// Rotation
				if (!playgroundParticles.rotateTowardsDirection)
					playgroundParticles.particleCache.particles[p].rotation += playgroundParticles.playgroundCache.rotationSpeed[p]*t;
				else {
					playgroundParticles.particleCache.particles[p].rotation = playgroundParticles.playgroundCache.initialRotation[p]+SignedAngle(
						Vector3.up, 
						playgroundParticles.playgroundCache.velocity[p],
						playgroundParticles.rotationNormal
					);
				}
				
				// Source for particle target
				switch (playgroundParticles.source) {
					case SOURCE.State:
					
						// Calculate State
						if (playgroundParticles.activeState!=currentState)
							return;
						if (playgroundParticles.states.Count>0 && playgroundParticles.states[playgroundParticles.activeState]!=null && playgroundParticles.states[playgroundParticles.activeState].initialized) {
							
							// Previous target position (for delta movement, available when having a transform assigned)
							if (playgroundParticles.states[playgroundParticles.activeState].stateTransform) {
								if (playgroundParticles.overflowOffset!=Vector3.zero)
									calculatedoverflowOffset = GetOverflowOffset(playgroundParticles, prevP, playgroundParticles.states[playgroundParticles.activeState].positionLength);
								playgroundParticles.playgroundCache.previousTargetPosition[prevP] = playgroundParticles.states[playgroundParticles.activeState].GetParentedPosition(prevP)+calculatedoverflowOffset;
							} else {
								playgroundParticles.playgroundCache.previousTargetPosition[prevP] = playgroundParticles.playgroundCache.targetPosition[prevP];
							}
							
							// Overflow offset this particle
							if (playgroundParticles.overflowOffset!=Vector3.zero)
								calculatedoverflowOffset = GetOverflowOffset(playgroundParticles, p, playgroundParticles.states[playgroundParticles.activeState].positionLength);
							
							// Position and color
							if (playgroundParticles.states[playgroundParticles.activeState].stateTransform!=null) 
								playgroundParticles.playgroundCache.targetPosition[p] = playgroundParticles.states[playgroundParticles.activeState].GetParentedPosition(p)+calculatedoverflowOffset;
							else
								playgroundParticles.playgroundCache.targetPosition[p] = playgroundParticles.states[playgroundParticles.activeState].GetPosition(p)+calculatedoverflowOffset;
							
							if (playgroundParticles.colorSource==COLORSOURCE.Source) 
								playgroundParticles.particleCache.particles[p].color = playgroundParticles.states[playgroundParticles.activeState].GetColor(p);
							
							// Transform direction velocity
							if (playgroundParticles.applyInitialLocalVelocity && playgroundParticles.playgroundCache.life[p]==0 && playgroundParticles.states[playgroundParticles.activeState].stateTransform!=null)
								playgroundParticles.playgroundCache.velocity[p] += playgroundParticles.states[playgroundParticles.activeState].stateTransform.TransformDirection(
									Vector3(playgroundParticles.playgroundCache.initialLocalVelocity[p].x,
											playgroundParticles.playgroundCache.initialLocalVelocity[p].y,
											playgroundParticles.playgroundCache.initialLocalVelocity[p].z
									)
								);
						} else return;
					break;
					case SOURCE.Transform:
					
						// Calculate Transform
						if (playgroundParticles.sourceTransform!=null) {
							
							// Overflow offset previous particle
							if (playgroundParticles.overflowOffset!=Vector3.zero)
								calculatedoverflowOffset = GetOverflowOffset(playgroundParticles, prevP, 1);
							
							// Previous target position (for delta movement)
							playgroundParticles.playgroundCache.previousTargetPosition[prevP] = playgroundParticles.sourceTransform.position+calculatedoverflowOffset;
							
							// Overflow offset this particle
							if (playgroundParticles.overflowOffset!=Vector3.zero)
								calculatedoverflowOffset = GetOverflowOffset(playgroundParticles, p, 1);
							
							// Position on transform
							playgroundParticles.playgroundCache.targetPosition[p] = playgroundParticles.sourceTransform.position+GetOverflowOffset(playgroundParticles, p, 1);
							
							// Normal direction velocity
							if (playgroundParticles.applyInitialLocalVelocity && playgroundParticles.playgroundCache.life[p]==0)
								playgroundParticles.playgroundCache.velocity[p] += playgroundParticles.sourceTransform.TransformDirection(
									Vector3(playgroundParticles.playgroundCache.initialLocalVelocity[p].x,
											playgroundParticles.playgroundCache.initialLocalVelocity[p].y,
											playgroundParticles.playgroundCache.initialLocalVelocity[p].z
									)
								);
						}
					break;
					case SOURCE.WorldObject:
					
						// Calculate World Object
						if (playgroundParticles.worldObject.gameObject!=null) {
							
							// Overflow offset previous particle
							if (playgroundParticles.overflowOffset!=Vector3.zero)
								calculatedoverflowOffset = GetOverflowOffset(playgroundParticles, prevP, playgroundParticles.worldObject.vertexPositions.Length);
							
							// Previous target position (for delta movement)
							playgroundParticles.playgroundCache.previousTargetPosition[prevP] = playgroundParticles.worldObject.vertexPositions[prevP%playgroundParticles.worldObject.vertexPositions.Length]+calculatedoverflowOffset;
							
							// Overflow offset this particle
							if (playgroundParticles.overflowOffset!=Vector3.zero)
								calculatedoverflowOffset = GetOverflowOffset(playgroundParticles, p, playgroundParticles.worldObject.vertexPositions.Length);
							
							// Position towards vertices
							playgroundParticles.playgroundCache.targetPosition[p] = playgroundParticles.worldObject.vertexPositions[p%playgroundParticles.worldObject.vertexPositions.Length]+calculatedoverflowOffset;
							
							// Normal direction velocity
							if (playgroundParticles.applyInitialLocalVelocity && playgroundParticles.playgroundCache.initialLocalVelocity[p]!=Vector3.zero && playgroundParticles.playgroundCache.life[p]==0)
								playgroundParticles.playgroundCache.velocity[p] += playgroundParticles.worldObject.transform.TransformDirection(
									Vector3(playgroundParticles.worldObject.normals[p%playgroundParticles.worldObject.normals.Length].x*playgroundParticles.playgroundCache.initialLocalVelocity[p].x,
											playgroundParticles.worldObject.normals[p%playgroundParticles.worldObject.normals.Length].y*playgroundParticles.playgroundCache.initialLocalVelocity[p].y,
											playgroundParticles.worldObject.normals[p%playgroundParticles.worldObject.normals.Length].z*playgroundParticles.playgroundCache.initialLocalVelocity[p].z
									)
								);
						}
					break;
					case SOURCE.SkinnedWorldObject:
					
						// Calculate Skinned World Object
						if (playgroundParticles.skinnedWorldObject.gameObject!=null) {
							
							// Overflow offset previous particle
							if (playgroundParticles.overflowOffset!=Vector3.zero)
								calculatedoverflowOffset = GetOverflowOffset(playgroundParticles, prevP, playgroundParticles.skinnedWorldObject.vertexPositions.Length);
							
							// Previous target position (for delta movement)
							playgroundParticles.playgroundCache.previousTargetPosition[prevP] = playgroundParticles.skinnedWorldObject.vertexPositions[prevP%playgroundParticles.skinnedWorldObject.vertexPositions.Length]+calculatedoverflowOffset;
							
							// Overflow offset this particle
							if (playgroundParticles.overflowOffset!=Vector3.zero)
								calculatedoverflowOffset = GetOverflowOffset(playgroundParticles, p, playgroundParticles.skinnedWorldObject.vertexPositions.Length);
							
							// Position towards vertices
							playgroundParticles.playgroundCache.targetPosition[p] = playgroundParticles.skinnedWorldObject.vertexPositions[p%playgroundParticles.skinnedWorldObject.vertexPositions.Length]+calculatedoverflowOffset;
							
							// Normal direction velocity
							if (playgroundParticles.applyInitialLocalVelocity && playgroundParticles.playgroundCache.initialLocalVelocity[p]!=Vector3.zero && playgroundParticles.playgroundCache.life[p]==0)
								playgroundParticles.playgroundCache.velocity[p] += playgroundParticles.skinnedWorldObject.transform.TransformDirection(
									Vector3(playgroundParticles.skinnedWorldObject.normals[p%playgroundParticles.skinnedWorldObject.normals.Length].x*playgroundParticles.playgroundCache.initialLocalVelocity[p].x,
											playgroundParticles.skinnedWorldObject.normals[p%playgroundParticles.skinnedWorldObject.normals.Length].y*playgroundParticles.playgroundCache.initialLocalVelocity[p].y,
											playgroundParticles.skinnedWorldObject.normals[p%playgroundParticles.skinnedWorldObject.normals.Length].z*playgroundParticles.playgroundCache.initialLocalVelocity[p].z
									)
								);
							
						}
					break;
					case SOURCE.Paint:
					
						// Calculate Paint
						if (playgroundParticles.paint!=null && playgroundParticles.paint.positionLength>0) {
							
							// Update current paint position regarding its transform parent
							playgroundParticles.paint.Update(p);
							
							// Overflow offset previous particle
							if (playgroundParticles.overflowOffset!=Vector3.zero)
								calculatedoverflowOffset = GetOverflowOffset(playgroundParticles, prevP, playgroundParticles.paint.positionLength);
							
							// Previous target position (for delta movement)
							playgroundParticles.playgroundCache.previousTargetPosition[prevP] = playgroundParticles.paint.GetPosition(prevP)+calculatedoverflowOffset;
							
							// Overflow offset this particle
							if (playgroundParticles.overflowOffset!=Vector3.zero)
								calculatedoverflowOffset = GetOverflowOffset(playgroundParticles, p, playgroundParticles.paint.positionLength);
							
							// Position and color
							playgroundParticles.playgroundCache.targetPosition[p] = playgroundParticles.paint.GetPosition(p)+calculatedoverflowOffset;
							if (playgroundParticles.colorSource==COLORSOURCE.Source) 
								playgroundParticles.particleCache.particles[p].color = playgroundParticles.paint.GetColor(p);
							
							// Normal direction velocity
							if (playgroundParticles.applyInitialLocalVelocity && playgroundParticles.playgroundCache.initialLocalVelocity[p]!=Vector3.zero && playgroundParticles.playgroundCache.life[p]==0) {
								var normal : Vector3 = playgroundParticles.paint.GetNormal(p);
								playgroundParticles.playgroundCache.velocity[p] += playgroundParticles.paint.GetParent(p).TransformDirection(
									Vector3(normal.x*playgroundParticles.playgroundCache.initialLocalVelocity[p].x,
											normal.y*playgroundParticles.playgroundCache.initialLocalVelocity[p].y,
											normal.z*playgroundParticles.playgroundCache.initialLocalVelocity[p].z
									)
								);
							}
						} else {
							playgroundParticles.playgroundCache.previousTargetPosition[prevP] = Playground.initialTargetPosition;
							playgroundParticles.playgroundCache.targetPosition[p] = Playground.initialTargetPosition;
						}
					break;
				}
				
				// Lifetime coloring
				if (!playgroundParticles.playgroundCache.changedByPropertyColor[p] && !playgroundParticles.playgroundCache.changedByPropertyColorLerp[p]) {
					if (playgroundParticles.colorSource!=COLORSOURCE.Source || playgroundParticles.colorSource==COLORSOURCE.Source && playgroundParticles.source!=SOURCE.State && playgroundParticles.source!=SOURCE.Paint || playgroundParticles.source==SOURCE.State && playgroundParticles.states[playgroundParticles.activeState].stateMesh!=null && playgroundParticles.states[playgroundParticles.activeState].stateTexture==null)
						playgroundParticles.particleCache.particles[p].color = playgroundParticles.lifetimeColor.Evaluate(playgroundParticles.lifetime>0?playgroundParticles.playgroundCache.life[p]/playgroundParticles.lifetime:0);
					else if (playgroundParticles.colorSource==COLORSOURCE.Source && playgroundParticles.sourceUsesLifetimeAlpha) {
						lifetimeColor = playgroundParticles.particleCache.particles[p].color;
						lifetimeColor.a = Mathf.Clamp(playgroundParticles.lifetimeColor.Evaluate(playgroundParticles.playgroundCache.life[p]/playgroundParticles.lifetime).a, 0, lifetimeColor.a);
						playgroundParticles.particleCache.particles[p].color = lifetimeColor;
					}
				}
				
				if (!playgroundParticles.onlySourcePositioning) {
				
					// Velocity bending
					if (playgroundParticles.applyVelocityBending && playgroundParticles.playgroundCache.velocity[p]!=Vector3.zero) {
						playgroundParticles.playgroundCache.velocity[p] += Vector3.Reflect(
							Vector3(
								playgroundParticles.playgroundCache.velocity[p].x*playgroundParticles.velocityBending.x,
								playgroundParticles.playgroundCache.velocity[p].y*playgroundParticles.velocityBending.y, 
								playgroundParticles.playgroundCache.velocity[p].z*playgroundParticles.velocityBending.z
							),
							(playgroundParticles.playgroundCache.targetPosition[p]-playgroundParticles.particleCache.particles[p].position).normalized
						)*t;
					}
					
					// Delta velocity
					if (playgroundParticles.calculateDeltaMovement && playgroundParticles.playgroundCache.life[p]==0 && !playgroundParticles.isPainting) {
						deltaVelocity = (playgroundParticles.playgroundCache.targetPosition[p]-playgroundParticles.playgroundCache.previousTargetPosition[p]);
						playgroundParticles.playgroundCache.velocity[p] += deltaVelocity*playgroundParticles.deltaMovementStrength;
						playgroundParticles.playgroundCache.previousTargetPosition[p] = playgroundParticles.playgroundCache.targetPosition[p];
					}
					
				}
					
				// Manipulators
				for (m = 0; m<Playground.reference.manipulators.Count; m++) {
					if (Playground.reference.manipulators[m].enabled && Playground.reference.manipulators[m].transform!=null && Playground.reference.manipulators[m].strength!=0 && (Playground.reference.manipulators[m].affects.value & 1<<playgroundParticles.particleSystemGameObject.layer)!=0) {
					
						if (!playgroundParticles.onlySourcePositioning) {
							// Attractors
							if (Playground.reference.manipulators[m].type==MANIPULATORTYPE.Attractor) {
								distance = Vector3.Distance(Playground.reference.manipulators[m].transform.position, playgroundParticles.particleCache.particles[p].position);
								if (Playground.reference.manipulators[m].shape == MANIPULATORSHAPE.Sphere && distance<=Playground.reference.manipulators[m].size || Playground.reference.manipulators[m].shape == MANIPULATORSHAPE.Box && Playground.reference.manipulators[m].Contains(playgroundParticles.particleCache.particles[p].position)) {
									playgroundParticles.playgroundCache.velocity[p] = Vector3.Lerp(playgroundParticles.playgroundCache.velocity[p], (Playground.reference.manipulators[m].transform.position-playgroundParticles.particleCache.particles[p].position)*(Playground.reference.manipulators[m].strength/distance), t*(Playground.reference.manipulators[m].strength/distance));
								}
							} else
												
							// Attractors Gravitational
							if (Playground.reference.manipulators[m].type==MANIPULATORTYPE.AttractorGravitational) {
								distance = Vector3.Distance(Playground.reference.manipulators[m].transform.position, playgroundParticles.particleCache.particles[p].position);
								if (Playground.reference.manipulators[m].shape == MANIPULATORSHAPE.Sphere && distance<=Playground.reference.manipulators[m].size || Playground.reference.manipulators[m].shape == MANIPULATORSHAPE.Box && Playground.reference.manipulators[m].Contains(playgroundParticles.particleCache.particles[p].position)) {
									playgroundParticles.playgroundCache.velocity[p] = Vector3.Lerp(playgroundParticles.playgroundCache.velocity[p], (Playground.reference.manipulators[m].transform.position-playgroundParticles.particleCache.particles[p].position)*Playground.reference.manipulators[m].strength/distance, t);
								}
							} else
							
							// Repellents 
							if (Playground.reference.manipulators[m].type==MANIPULATORTYPE.Repellent) {
								distance = Vector3.Distance(Playground.reference.manipulators[m].transform.position, playgroundParticles.particleCache.particles[p].position);
								if (Playground.reference.manipulators[m].shape == MANIPULATORSHAPE.Sphere && distance<=Playground.reference.manipulators[m].size || Playground.reference.manipulators[m].shape == MANIPULATORSHAPE.Box && Playground.reference.manipulators[m].Contains(playgroundParticles.particleCache.particles[p].position)) {
									playgroundParticles.playgroundCache.velocity[p] = Vector3.Lerp(playgroundParticles.playgroundCache.velocity[p], (playgroundParticles.particleCache.particles[p].position-Playground.reference.manipulators[m].transform.position)*(Playground.reference.manipulators[m].strength/distance), t*(Playground.reference.manipulators[m].strength/distance));
								}
							}
						}
						
						// Properties
						if (Playground.reference.manipulators[m].type==MANIPULATORTYPE.Property) {
							distance = Vector3.Distance(Playground.reference.manipulators[m].transform.position, playgroundParticles.particleCache.particles[p].position);
							if (Playground.reference.manipulators[m].shape == MANIPULATORSHAPE.Sphere && distance<=Playground.reference.manipulators[m].size || Playground.reference.manipulators[m].shape == MANIPULATORSHAPE.Box && Playground.reference.manipulators[m].Contains(playgroundParticles.particleCache.particles[p].position)) {
								
								switch (Playground.reference.manipulators[m].property.type) {
									
									// Velocity Property
									case MANIPULATORPROPERTYTYPE.Velocity:
										if (Playground.reference.manipulators[m].property.transition == MANIPULATORPROPERTYTRANSITION.None)
											playgroundParticles.playgroundCache.velocity[p] = Playground.reference.manipulators[m].property.useLocalRotation? Playground.reference.manipulators[m].transform.TransformPoint(Playground.reference.manipulators[m].property.velocity)-Playground.reference.manipulators[m].transform.position : Playground.reference.manipulators[m].property.velocity;
										else
											playgroundParticles.playgroundCache.velocity[p] = Vector3.Lerp(playgroundParticles.playgroundCache.velocity[p], Playground.reference.manipulators[m].property.useLocalRotation? Playground.reference.manipulators[m].transform.TransformPoint(Playground.reference.manipulators[m].property.velocity)-Playground.reference.manipulators[m].transform.position : Playground.reference.manipulators[m].property.velocity, t*Playground.reference.manipulators[m].strength);
									break;
									
									// Additive Velocity Property
									case MANIPULATORPROPERTYTYPE.AdditiveVelocity:
										if (Playground.reference.manipulators[m].property.transition == MANIPULATORPROPERTYTRANSITION.None)
											playgroundParticles.playgroundCache.velocity[p] += Playground.reference.manipulators[m].property.useLocalRotation? Playground.reference.manipulators[m].transform.TransformPoint(Playground.reference.manipulators[m].property.velocity)-Playground.reference.manipulators[m].transform.position : Playground.reference.manipulators[m].property.velocity*(t*Playground.reference.manipulators[m].strength);
										else
											playgroundParticles.playgroundCache.velocity[p] += Vector3.Lerp(playgroundParticles.playgroundCache.velocity[p], Playground.reference.manipulators[m].property.useLocalRotation? Playground.reference.manipulators[m].transform.TransformPoint(Playground.reference.manipulators[m].property.velocity)-Playground.reference.manipulators[m].transform.position : Playground.reference.manipulators[m].property.velocity, t*Playground.reference.manipulators[m].strength)*(t*Playground.reference.manipulators[m].strength);
									break;
									
									// Color Property
									case MANIPULATORPROPERTYTYPE.Color:
										if (Playground.reference.manipulators[m].property.transition == MANIPULATORPROPERTYTRANSITION.None) {
											if (Playground.reference.manipulators[m].property.keepColorAlphas) {
												lifetimeColor = Playground.reference.manipulators[m].property.color;
												lifetimeColor.a = Mathf.Clamp(playgroundParticles.lifetimeColor.Evaluate(playgroundParticles.playgroundCache.life[p]/playgroundParticles.lifetime).a, 0, lifetimeColor.a);
												playgroundParticles.particleCache.particles[p].color = lifetimeColor;
											} else playgroundParticles.particleCache.particles[p].color = Playground.reference.manipulators[m].property.color;
										} else {
											if (Playground.reference.manipulators[m].property.keepColorAlphas) {
												lifetimeColor = Playground.reference.manipulators[m].property.color;
												lifetimeColor.a = Mathf.Clamp(playgroundParticles.lifetimeColor.Evaluate(playgroundParticles.playgroundCache.life[p]/playgroundParticles.lifetime).a, 0, lifetimeColor.a);
												playgroundParticles.particleCache.particles[p].color = Color.Lerp(playgroundParticles.particleCache.particles[p].color, lifetimeColor, t*Playground.reference.manipulators[m].strength);
											} else playgroundParticles.particleCache.particles[p].color = Color.Lerp(playgroundParticles.particleCache.particles[p].color, Playground.reference.manipulators[m].property.color, t*Playground.reference.manipulators[m].strength);
											playgroundParticles.playgroundCache.changedByPropertyColorLerp[p] = true;
										}
										if (!Playground.reference.manipulators[m].property.onlyColorInRange)
											playgroundParticles.playgroundCache.changedByPropertyColor[p] = true;
									break;
									
									// Size Property
									case MANIPULATORPROPERTYTYPE.Size:
										if (Playground.reference.manipulators[m].property.transition == MANIPULATORPROPERTYTRANSITION.None)
											playgroundParticles.particleCache.particles[p].size = Playground.reference.manipulators[m].property.size;
										else
											playgroundParticles.particleCache.particles[p].size = Mathf.Lerp(playgroundParticles.particleCache.particles[p].size, Playground.reference.manipulators[m].property.size, t*Playground.reference.manipulators[m].strength);
										playgroundParticles.playgroundCache.changedByPropertySize[p] = true;
									break;
								}
								
								
								playgroundParticles.playgroundCache.changedByProperty[p] = true;
							
							} else {
								// Particle is outside of property and is set to only color in range (try to lerp back color)
								if (playgroundParticles.playgroundCache.changedByPropertyColorLerp[p] && Playground.reference.manipulators[m].property.transition == MANIPULATORPROPERTYTRANSITION.Lerp && Playground.reference.manipulators[m].property.onlyColorInRange)
									playgroundParticles.particleCache.particles[p].color = Color.Lerp(playgroundParticles.particleCache.particles[p].color, playgroundParticles.lifetimeColor.Evaluate(playgroundParticles.playgroundCache.life[p]/playgroundParticles.lifetime), t*Playground.reference.manipulators[m].strength);
							}
						} 
					}
				}
				
				if (!playgroundParticles.onlySourcePositioning) {
				
					// Gravity
					playgroundParticles.playgroundCache.velocity[p] -= playgroundParticles.gravity*t;
					
					// Lifetime additive velocity
					if (playgroundParticles.applyLifetimeVelocity) 
						playgroundParticles.playgroundCache.velocity[p] += playgroundParticles.lifetimeVelocity.Evaluate(playgroundParticles.playgroundCache.life[p]/playgroundParticles.lifetime)*t;
					
					// Collision detection 
					if (playgroundParticles.collision && playgroundParticles.playgroundCache.velocity[p].magnitude>Playground.collisionSleepVelocity) {
						if (Physics.SphereCast(playgroundParticles.particleCache.particles[p].position, playgroundParticles.collisionRadius, playgroundParticles.playgroundCache.velocity[p], hitInfo, playgroundParticles.collisionRadius, playgroundParticles.collisionMask)) {
							playgroundParticles.particleCache.particles[p].position = hitInfo.point+((playgroundParticles.particleCache.particles[p].position-hitInfo.point));
							if (playgroundParticles.playgroundCache.velocity[p].magnitude>Playground.collisionSleepVelocity) {
								if (playgroundParticles.affectRigidbodies && hitInfo.rigidbody)
									 hitInfo.rigidbody.AddForceAtPosition(playgroundParticles.playgroundCache.velocity[p]*playgroundParticles.mass, playgroundParticles.particleCache.particles[p].position);
								playgroundParticles.playgroundCache.velocity[p] = Vector3.Reflect(playgroundParticles.playgroundCache.velocity[p], hitInfo.normal)*playgroundParticles.bounciness;
							} else playgroundParticles.playgroundCache.velocity[p] = Vector3.zero;
						}
					}
					
					// Damping and final positioning
					if (playgroundParticles.playgroundCache.velocity[p]!=Vector3.zero) {
						playgroundParticles.playgroundCache.velocity[p] = Vector3.Lerp(playgroundParticles.playgroundCache.velocity[p], Vector3.zero, playgroundParticles.damping*t);
						playgroundParticles.particleCache.particles[p].position += playgroundParticles.playgroundCache.velocity[p]*t;
					}
					
				} else {
					// Only Source Positioning
					playgroundParticles.playgroundCache.previousTargetPosition[p] = playgroundParticles.playgroundCache.targetPosition[p];
					playgroundParticles.particleCache.particles[p].position = playgroundParticles.playgroundCache.targetPosition[p];
				}
				
			}
		}
	}
	
	// Delete a state from states list
	function RemoveState (i : int) {
		var newState : int = this.activeState;
		newState = (newState%this.states.Count)-1;
		if (newState<0) newState = 0;
			
		this.states[newState].Initialize();
		this.activeState = newState;
		this.states.RemoveAt(i);
	}
	
	// Wipe out particles in current PlaygroundParticles object
	static function Clear (playgroundParticles:PlaygroundParticles) {
		playgroundParticles.inTransition = false;
		playgroundParticles.particleCache.particles = new ParticleSystem.Particle[0];
		playgroundParticles.playgroundCache.velocity = null;
		playgroundParticles.playgroundCache.targetPosition = null;
		playgroundParticles.playgroundCache.previousTargetPosition = null;
		playgroundParticles.playgroundCache.life = null;
		playgroundParticles.playgroundCache.size = null;
		playgroundParticles.shurikenParticleSystem.SetParticles(playgroundParticles.particleCache.particles,0);
		playgroundParticles.shurikenParticleSystem.Clear();
	}
	
	// YieldedRefresh makes sure that Playground Manager and simulation time is ready before this particle system
	function YieldedRefresh () {
		isYieldRefreshing = true;
		yield;
		SetLifetime(this, this.lifetime);
		isYieldRefreshing = false;
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// MonoBehaviours
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	function OnEnable () {
		// Make sure we have a reference to Playground
		if (Playground.reference==null)
			Playground.reference = FindObjectOfType(Playground);
		
		// Make sure that the components are cached
		if (this.particleSystemGameObject==null) {
			this.particleSystemGameObject = gameObject;
			this.particleSystemTransform = transform;
			this.particleSystemRenderer = renderer;
			this.shurikenParticleSystem = particleSystemGameObject.GetComponent(ParticleSystem);
		}
		
		// Make sure that state data is initialized
		for (var x = 0; x<this.states.Count; x++) {
			this.states[x].Initialize();
		}
		
		// Initiate all arrays by setting particle count
		if (this.particleCache.particles==null) {
			SetParticleCount(this, this.particleCount);
		}
		
	}
	
	function Start () {
		YieldedRefresh();
	}
	
	function OnDestroy () {
		// Remove this PlaygroundParticles object from Particle Systems list
		Playground.reference.particleSystems.Remove(this);
	}
	
}	

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MeshParticles - Extension class for PlaygroundParticles which creates mesh states. 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

class MeshParticles extends PlaygroundParticles {

	static function CreateMeshParticles (meshes : Mesh[], textures:Texture2D[], heightMap : Texture2D[], name : String, position : Vector3, rotation : Quaternion, particleScale : float, offsets:Vector3[], material : Material) {
		var meshParticles : PlaygroundParticles = PlaygroundParticles.CreateParticleObject(name,position,rotation,particleScale,material);
		meshParticles.states = new List.<ParticleState>();
		
		var quantityList : int[] = new int[meshes.Length];
		for (var i = 0; i<textures.Length; i++)
			quantityList[i] = meshes[i].vertexCount;
		meshParticles.particleCache.particles = new ParticleSystem.Particle[quantityList[Playground.Largest(quantityList)]];
		meshParticles.shurikenParticleSystem.Emit(meshParticles.particleCache.particles.Length);
		meshParticles.shurikenParticleSystem.GetParticles(meshParticles.particleCache.particles);
		for (i = 0; i<meshes.Length; i++) {
			meshParticles.states.Add(new ParticleState());
			meshParticles.states[0].ConstructParticles(meshes[i],textures[i],particleScale,offsets[i], "State "+i,null);
		}
		
		Playground.Update(meshParticles);
		Playground.particlesQuantity++;
		
		return meshParticles;
	}
	
	static function Add (meshParticles : PlaygroundParticles, mesh : Mesh, scale : float, offset : Vector3, stateName : String, stateTransform : Transform) {
		meshParticles.states.Add(new ParticleState());
		meshParticles.states[meshParticles.states.Count-1].ConstructParticles(mesh,scale,offset,stateName,stateTransform);
	}
	
	static function Add (meshParticles : PlaygroundParticles, mesh : Mesh, texture : Texture2D, scale : float, offset : Vector3, stateName : String, stateTransform : Transform) {
		meshParticles.states.Add(new ParticleState());
		meshParticles.states[meshParticles.states.Count-1].ConstructParticles(mesh,texture,scale,offset,stateName,stateTransform);
	}
}


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Structs (non-serializable)
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

class PlaygroundCache extends System.ValueType {
	var size : float[];							// The size of each particle
	var life : float[];							// The life time of each particle
	var birth : float[];						// The time of birth for each particle
	var death : float[];						// The time of death for each particle
	var emission : boolean[];					// The emission for each particle (controlled by emission rate)
	var rebirth : boolean[];					// The rebirth for each particle
	var lifetimeOffset : float[];				// The offset in birth-death (sorting)
	var velocity : Vector3[];					// The velocity of each particle in this PlaygroundParticles
	var initialVelocity : Vector3[];			// The initial velocity of each particle in this PlaygroundParticles
	var initialLocalVelocity : Vector3[];		// The initial local velocity of each particle in this PlaygroundParticles
	var targetPosition : Vector3[];				// The base position for each particle
	var previousTargetPosition : Vector3[]; 	// The previous base position for each particle (used to calculate delta movement)
	var initialRotation : float[];				// The initial rotation of each particle in this PlaygroundParticles
	var rotationSpeed : float[];				// The rotation speed of each particle in this PlaygroundParticles
	var changedByProperty : boolean[];			// The interaction with property manipulators of each particle
	var changedByPropertyColor : boolean[];		// The interaction with property manipulators that change color of each particle
	var changedByPropertyColorLerp : boolean[]; // The interaction with property manipulators that change color over time of each particle
	var changedByPropertySize : boolean[];		// The interaction with property manipulators that change size of each particle
	
	// Copy this PlaygroundCache struct
	function Clone () : PlaygroundCache {
		var playgroundCache : PlaygroundCache = new PlaygroundCache();
		playgroundCache.size = this.size.Clone() as float[];
		playgroundCache.life = this.life.Clone() as float[];
		playgroundCache.birth = this.birth.Clone() as float[];
		playgroundCache.death = this.death.Clone() as float[];
		playgroundCache.emission = this.birth.Clone() as boolean[];
		playgroundCache.rebirth = this.rebirth.Clone() as boolean[];
		playgroundCache.lifetimeOffset = this.lifetimeOffset.Clone() as float[];
		playgroundCache.velocity = this.velocity.Clone() as Vector3[];
		playgroundCache.targetPosition = this.targetPosition.Clone() as Vector3[];
		playgroundCache.previousTargetPosition = this.previousTargetPosition.Clone() as Vector3[];
		playgroundCache.initialRotation = this.initialRotation.Clone() as float[];
		playgroundCache.rotationSpeed = this.rotationSpeed.Clone() as float[];
		playgroundCache.changedByProperty = this.changedByProperty.Clone() as boolean[];
		playgroundCache.changedByPropertyColor = this.changedByPropertyColor.Clone() as boolean[];
		playgroundCache.changedByPropertyColorLerp = this.changedByPropertyColorLerp.Clone() as boolean[];
		playgroundCache.changedByPropertySize = this.changedByPropertySize.Clone() as boolean[];
		return playgroundCache;
	}
}

class ParticleCache extends System.ValueType {
	var particles : ParticleSystem.Particle[];	// The particle pool of this PlaygroundParticles object
}