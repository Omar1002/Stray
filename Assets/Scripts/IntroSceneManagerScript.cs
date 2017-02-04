using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class IntroSceneManagerScript : MonoBehaviour {

    #region member variables

    private GameObject m_pData;

    #endregion

    // Use this for initialization
    void Start ()
    {
	    if (!GameObject.Find("PersistentDataGO"))
        {
            m_pData = new GameObject("PersistentDataGO");
            m_pData.AddComponent<PersistentData>();
        }
	}
	
    public void OnPlayClicked()
    {
        m_pData.GetComponent<PersistentData>().ChangeToScene("NewArtProtoScene"); 
    }

    public void OnOptionsClicked()
    {
        m_pData.GetComponent<PersistentData>().ChangeToScene("OptionsScene");
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
