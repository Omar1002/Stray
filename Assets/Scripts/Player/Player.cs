using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public enum PlayerState { alive, dead }

public class Player : MonoBehaviour {

    #region member variables

    public float m_lightPool;
    public PlayerState m_state = PlayerState.alive;


    private PersistentData m_pData;
    private float m_lightConsumption;
    private bool m_canBecomeGhost;
    private GameObject m_remLight;
    private Text m_remainingLightNumber;

    #endregion

    void Start ()
    {
        m_pData = (PersistentData)FindObjectOfType(typeof(PersistentData));

        m_lightPool = m_pData.m_playerLightPool;
        m_lightConsumption = m_pData.m_playerLightConsumption;
        m_canBecomeGhost = m_pData.m_playerCanGhost;

        m_remLight = GameObject.Find("RemainingLightNumber");
        m_remainingLightNumber = m_remLight.GetComponent<Text>();
        
    }

    void Update ()
    {
        m_remainingLightNumber.text = m_lightPool.ToString();

        if (m_state == PlayerState.alive)
        {
            if (GetComponent<CharacterController>().velocity != Vector3.zero)
            {
                m_lightPool -= Time.deltaTime * m_lightConsumption;
                UpdateLight();

                m_remainingLightNumber.text = m_lightPool.ToString();

                if (m_lightPool <= 0) //player is dead
                {
                    Debug.Log("Lightpool less than 0");
                        m_state = PlayerState.dead;
                        //TODO: refactor scene management into the persistent data object
                        //SceneManager.LoadScene("NEW INTRO SCENE");
                        transform.rotation.Set(90.0f, 0.0f, 0.0f, 1.0f);
                        transform.position = Vector3.MoveTowards(transform.position, GameObject.Find("DeathMoveTowardsObject").transform.position, 0.0f);

                    
                }
            }
        }
        else if (m_state == PlayerState.dead) //decide what happens here, coroutine??
        {
            transform.rotation.Set(90.0f, 0.0f, 0.0f, 1.0f);
            transform.position = Vector3.MoveTowards(transform.position, GameObject.Find("DeathMoveTowardsObject").transform.position, 0.0f);

        }
    }

    void DisablePlayerStuff()
    {
        foreach (Light li in GetComponentsInChildren<Light>())
        {
            li.enabled = false;
        }
        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
        {
            mr.enabled = false;
        }
        foreach (InteractScript Is in GetComponentsInChildren<InteractScript>())
        {
            Is.enabled = false;
        }
    }

    public void AddLight(float light)
    {
        if (m_lightPool < m_pData.m_playerLightPool)
        {
            m_lightPool += light;
        }
    }

    void UpdateLight()
    {
        Light light = GetComponentInChildren<Light>();
        light.intensity = m_lightPool / m_pData.m_playerLightPool;
    }
}
