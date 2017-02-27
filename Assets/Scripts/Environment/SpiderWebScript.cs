using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
public class SpiderWebScript : MonoBehaviour {
    public GameObject player;
    public bool inWeb = false;
    public float timer = 0;

	
	// Update is called once per frame
	void Update ()
    {
        if (inWeb)
        {
            timer += Time.deltaTime;
        }
        if (timer >= 5.0f)
        {
            player.GetComponent<FirstPersonController>().m_WalkSpeed = 5.0f;
            Destroy(this.gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            inWeb = true;
            player = other.gameObject;
            other.gameObject.GetComponent<FirstPersonController>().m_WalkSpeed = 2.5f;
        }
    }
}
