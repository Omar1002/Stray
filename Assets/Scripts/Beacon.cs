using UnityEngine;
using System.Collections;
using Photon;

public class Beacon : Photon.PunBehaviour {

    #region member variables

    private PersistentData m_pData;
    private float m_lightToActivate;
    private bool m_isActive = false;

    private int m_PlayersClose = 0;

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
            m_PlayersClose++;
            if (other.GetComponent<Player>() != null && other.GetComponent<Player>().m_lightPool > m_lightToActivate && !m_isActive && m_PlayersClose >= 2)
            {
                other.GetComponent<Player>().m_lightPool -= m_lightToActivate;
                photonView.RPC("Activate", PhotonTargets.All);
            }
        }
	}

    void OnTriggerExit(Collider other)
    {
        m_PlayersClose--;   
    }




    [PunRPC]
    void Activate()
    {
        m_isActive = true;
        
        //light's up!
        foreach (Transform go in GetComponentsInChildren<Transform>(true))
        {
            go.gameObject.SetActive(true);
        }
        //notify monolith
        m_monolith.GetComponent<MonolithTrigger>().BeaconLit();
    }
}
