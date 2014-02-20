#pragma strict

@script ExecuteInEditMode()

var laserMaxDistance : float = 100;
private var particles : PlaygroundParticles;

function Start () {
	particles = GetComponent(PlaygroundParticles);
}

function Update () {

	// Send a Raycast from particle system's source transform forward
	var hit : RaycastHit;
	if (Physics.Raycast(particles.sourceTransform.position, particles.sourceTransform.forward, hit, laserMaxDistance)) {
		
		// Set overflow offset z to hit distance (divide by particle count which by default is 1000)
		particles.overflowOffset.z = Vector3.Distance(particles.sourceTransform.position, hit.point)/(1+particles.particleCount);
		
	} else {
	
		// Render laser to laserMaxDistance on clear sight
		particles.overflowOffset.z = laserMaxDistance/(1+particles.particleCount);
	}
}