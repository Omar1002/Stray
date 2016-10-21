﻿using UnityEngine;
using System.Collections;

public class Beacon : MonoBehaviour {

    #region member variables

    private PersistentData m_pData;
    private float m_lightToActivate;
    private bool m_isActive = false;

    public GameObject m_monolith;

    #endregion

    void Start ()
    {
        m_pData = (PersistentData)FindObjectOfType(typeof(PersistentData));
        m_lightToActivate = m_pData.m_lightToActivateBeacon;
	}
	
	void OnTriggerEnter (Collider other)
    {
	    if (other.tag == "AI")
        {
            other.GetComponent<NavmeshAI>().SwitchGoals();
        }

        if (other.tag == "Player")
        {
            if (other.GetComponent<Player>() != null && other.GetComponent<Player>().m_lightPool > m_lightToActivate && !m_isActive)
            {
                m_isActive = true;
                //activate the beacon!
                other.GetComponent<Player>().m_lightPool -= m_lightToActivate;
                //light's up!
                foreach (Transform go in GetComponentsInChildren<Transform>(true))
                {
                    go.gameObject.SetActive(true);
                }
                //notify monolith
                m_monolith.GetComponent<MonolithTrigger>().BeaconLit();
            }
        }
	}
}