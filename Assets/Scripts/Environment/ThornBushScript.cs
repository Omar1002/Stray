using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class ThornBushScript : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<Player>().m_isInThornBush = true;
            other.gameObject.GetComponent<FirstPersonController>().m_RunSpeed = 3.75f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<Player>().m_isInThornBush = false;
            other.gameObject.GetComponent<FirstPersonController>().m_RunSpeed = 5.0f;
        }
    }
}
