using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

using System.IO;

using YamlDotNet.RepresentationModel;

public class PersistentData : MonoBehaviour {

    #region member variables

    public string m_file = "Settings.yaml";

    //fountains
    public bool m_wellsDeplete = false;
    public float m_wellLightAmount = 100F;
    public float m_wellLightRecharge = 100F;

    //player
    public float m_playerLightPool = 100F;
    public float m_playerLightConsumption = 0.5F;
    public bool m_playerCanGhost = false;

    //beacon
    public float m_lightToActivateBeacon;

    //monolyth
    public float m_timeToEndLevel;

    //screen fader
    private GameObject m_screenFaderAsset;
    private ScreenFader m_fader;

    #endregion

    void Awake ()
    {
        //add delegate listener to the scene changed event  from SceneManager
        SceneManager.activeSceneChanged += SceneChanged;

        m_screenFaderAsset = Resources.Load("ScreenFaderPrefab") as GameObject;

        //load all the various settings from the YAML file
        StringReader input = new StringReader(System.IO.File.ReadAllText(m_file));

        YamlStream yaml = new YamlStream();
        yaml.Load(input);

        YamlMappingNode mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
        
        foreach (var setting in mapping.Children)
        {
            switch(((YamlScalarNode)setting.Key).Value)
            {
                case "WellsDeplete":
                    if (((YamlScalarNode)setting.Value).Value == "true")
                        m_wellsDeplete = true;
                    else
                        m_wellsDeplete = false;
                break;

                case "WellsLightAmount":
                    m_wellLightAmount = float.Parse(((YamlScalarNode)setting.Value).Value);
                break;

                case "WellsLightRecharge":
                    m_wellLightRecharge = float.Parse(((YamlScalarNode)setting.Value).Value);
                break;

                case "PlayerLightPool":
                    m_playerLightPool = float.Parse(((YamlScalarNode)setting.Value).Value);
                break;

                case "PlayerLightConsumption":
                    m_playerLightConsumption = float.Parse(((YamlScalarNode)setting.Value).Value);
                break;

                case "PlayerCanGhost":
                    if (((YamlScalarNode)setting.Value).Value == "true")
                        m_playerCanGhost = true;
                    else
                        m_playerCanGhost = false;
                break;

                case "LightToActivateBeacon":
                    m_lightToActivateBeacon = float.Parse(((YamlScalarNode)setting.Value).Value);
                break;

                case "TimeToEndLevel":
                    m_timeToEndLevel = float.Parse(((YamlScalarNode)setting.Value).Value);
                break;
            }
        }
    }

	void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        if (!FindObjectOfType<ScreenFader>())
            Instantiate(m_screenFaderAsset);
    }

    void SceneChanged(Scene previosScene, Scene newScene)
    {
        Instantiate(m_screenFaderAsset);
        FindObjectOfType<ScreenFader>().FadeIn();
    }

    public void ChangeToScene(string scene)
    {
        StartCoroutine(ChangeSceneCO(scene));
        FindObjectOfType<ScreenFader>().FadeOut();
    }

    IEnumerator ChangeSceneCO(string scene)
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(scene);
    }
}
