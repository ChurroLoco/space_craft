using UnityEngine;
using System.Collections;

public class ParticleAutoDestory : MonoBehaviour 
{	
	void Start() 
	{
		Invoke("Kill", particleSystem.duration);
	}

	void Kill()
	{
		Destroy(this.gameObject);
	}
}
