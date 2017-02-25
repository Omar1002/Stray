using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampFireLightIncrease : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<Player>().m_lightPool += 50;
        }
    }
}
