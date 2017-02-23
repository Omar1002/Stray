using UnityEngine;
using System.Collections;

public class NavmeshAI : MonoBehaviour {

    #region member variables

    public Transform m_beacon, m_lightPool, m_currentGoal;

    #endregion

    void Start()
    {
        /// TODO: sync position of npc across network
        m_currentGoal = m_beacon;
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.destination = m_currentGoal.position;
    }
	
	public void SwitchGoals ()
    {
	    if (m_currentGoal.position == m_beacon.position)
        {
            m_currentGoal = m_lightPool;
        }
        else
        {
            m_currentGoal = m_beacon;
        }
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.destination = m_currentGoal.position;
    }
}
