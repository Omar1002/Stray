using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampFireLightIncrease : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player" && other.gameObject.GetComponent<Player>().m_hasReceivedLightFromFire == false)
        {
            //USING THIS SYSTEM, WE NEED TO RESET THIS BOOL TO FALSE BEFORE THE NEXT TIME THE PLAYERS GO TO THE CAMPFIRE.
            other.gameObject.GetComponent<Player>().m_hasReceivedLightFromFire = true;
            other.gameObject.GetComponent<Player>().m_lightPool += 50;
        }
    }
}
