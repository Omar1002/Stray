using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

public enum PortalFunction { full, exit };
[DisallowMultipleComponent]
[ExecuteInEditMode]

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]


public class StrayPortalManager : MonoBehaviour
{
    //Public vars and correlated
    public GameObject ConnectedPortal;
    public Material ConnectedPortalSkybox;
    [HideInInspector]
    public Material NullConnectedPortalSkybox;

    [Serializable]
    public class ViewSettingsClass
    {
        [Serializable]
        public class ProjectionClass
        {
            public Vector2 Resolution = new Vector2(1280, 1024);

            public enum DepthQualityEnum { Fast, High };
            public DepthQualityEnum DepthQuality = DepthQualityEnum.High;
            [HideInInspector]
            public DepthQualityEnum[] CurrentDepthQuality = new DepthQualityEnum[0];
        }
        public ProjectionClass Projection;

        [Serializable]
        public class RecursionClass
        {
            [Range(1, 20)]
            public int Steps = 1;
            public Material CustomFinalStep;
        }
        public RecursionClass Recursion;

        [Serializable]
        public class DistorsionClass
        {
            public bool EnableDistorsion;

            public Texture2D Pattern;
            public Color Color = new Color(1, 1, 1, 1);
            [Range(1, 100)]
            public int Tiling = 1;
            [Range(-10, 10)]
            public float SpeedX = .01f;
            [Range(-10, 10)]
            public float SpeedY = 0;
        }
        public DistorsionClass Distorsion;
    }
    public ViewSettingsClass ViewSettings;

    [Serializable]
    public class PortalSettingsClass
    {
        public bool EnablePortalTrigger = true;
        public bool EnableMeshClipPlane = true;
    }
    public PortalSettingsClass PortalSettings;

    [Serializable]
    public class PortalFunctionalityClass
    {
        [Serializable]
        public class ExcludedObjsFromTriggerClass
        {
            public GameObject Obj;

            public bool OnlyForPortal;
        }

        public ExcludedObjsFromTriggerClass[] ExcludedObjsFromTrigger = new ExcludedObjsFromTriggerClass[0];

        [Serializable]
        public class ExcludedObjsFromRenderClass
        {
            public GameObject Obj;

            [Range(2, 31)]
            public int Layer = 2;
        }
        public ExcludedObjsFromRenderClass[] ExcludedObjsFromRender = new ExcludedObjsFromRenderClass[0];

        [Serializable]
        public class SceneAsyncLoadClass
        {
            public bool Enable;

            public int SceneIndex = 0;
        }
        public SceneAsyncLoadClass SceneAsyncLoad;
    }
    public PortalFunctionalityClass PortalFunctionality;
    //----------

    private Material[] GateMaterial;
    private RenderTexture[] RenTex;
    private Material ClipPlaneMaterial;
    private Material CloneClipPlaneMaterial;
    [HideInInspector]
    public GameObject ClipPlanePosObj;
    private Vector2[] CurrentProjectionResolution;
    [HideInInspector]
    public GameObject[] GateCamObjs;
    private int[] InitGateCamObjsCullingMask;
    private GameObject SceneviewRender;

    public PortalFunction m_portalType;

    void OnEnable()
    {
        #if UNITY_EDITOR
        RenTex = new RenderTexture[2];
        #else
			RenTex = new RenderTexture[1];
        #endif
        GateMaterial = new Material[RenTex.Length];
        CurrentProjectionResolution = new Vector2[RenTex.Length];
        ViewSettings.Projection.CurrentDepthQuality = new ViewSettingsClass.ProjectionClass.DepthQualityEnum[RenTex.Length];

        GateCamObjs = new GameObject[20];
        Array.Resize(ref GateCamObjs, GateCamObjs.Length + 1);
        InitGateCamObjsCullingMask = new int[GateCamObjs.Length];

        for (int i = 0; i < GateMaterial.Length; i++) //Generate "Portal" and "Clipping plane" materials
            if (!GateMaterial[i])
                GateMaterial[i] = new Material(Shader.Find("Gater/UV Remap"));

        Shader ClipPlaneShader = Shader.Find("Custom/StandardClippable");

        if (!NullConnectedPortalSkybox)
            NullConnectedPortalSkybox = new Material(Shader.Find("Standard"));

        if (!ClipPlaneMaterial)
        {
            ClipPlaneMaterial = new Material(Shader.Find("Standard"));
            ClipPlaneMaterial.shader = ClipPlaneShader;
        }
        if (!CloneClipPlaneMaterial)
        {
            CloneClipPlaneMaterial = new Material(Shader.Find("Standard"));
            ClipPlaneMaterial.shader = ClipPlaneShader;
        }

        for (int j = 0; j < CurrentProjectionResolution.Length; j++)
            CurrentProjectionResolution[j] = new Vector2(0, 0);

        //Apply custom settings to the portal components
        GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        GetComponent<MeshRenderer>().receiveShadows = false;
        GetComponent<MeshRenderer>().sharedMaterial = GateMaterial[0];
        GetComponent<Rigidbody>().mass = 1;
        GetComponent<Rigidbody>().drag = 0;
        GetComponent<Rigidbody>().angularDrag = 0;
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
        GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        if (GetComponent<MeshCollider>())
        {
            GetComponent<MeshCollider>().convex = true;
            GetComponent<MeshCollider>().sharedMaterial = null;
        }

        //Disable collision of walls behind portals, and check if the excluded objects from trigger have a collider component
        if (PortalFunctionality.ExcludedObjsFromTrigger.Length > 0)
            for (int k = 0; k < PortalFunctionality.ExcludedObjsFromTrigger.Length; k++)
                if (PortalFunctionality.ExcludedObjsFromTrigger[k].Obj)
                {
                    Physics.IgnoreCollision(transform.GetComponent<Collider>(), PortalFunctionality.ExcludedObjsFromTrigger[k].Obj.GetComponent<Collider>(), true);

                    if (!PortalFunctionality.ExcludedObjsFromTrigger[k].Obj.GetComponent<Collider>())
                        Debug.LogError("One excluded wall doesn't have a collider component");
                }

#if UNITY_EDITOR
        EditorApplication.update = Update;

        //Search already existing required objects for teleport, and fill the relative variables with
        int GateCamObjsSteps = 0;

        for (int l = 0; l < transform.GetComponentsInChildren<Transform>().Length; l++)
        {
            if (transform.GetComponentsInChildren<Transform>()[l].name == this.gameObject.name + " Camera " + GateCamObjsSteps)
            {
                GateCamObjs[GateCamObjsSteps] = transform.GetComponentsInChildren<Transform>()[l].gameObject;

                GateCamObjsSteps += 1;
            }

            if (transform.GetComponentsInChildren<Transform>()[l].name == transform.name + " SceneviewRender")
                SceneviewRender = transform.GetComponentsInChildren<Transform>()[l].gameObject;

            if (transform.GetComponentsInChildren<Transform>()[l].name == transform.name + " ClipPlanePosObj")
                ClipPlanePosObj = transform.GetComponentsInChildren<Transform>()[l].gameObject;
        }
        #endif
    }

    void Update()
    {
        #if UNITY_EDITOR
        SetGate();

        GateCamRepos();
        #endif
    }


    void FixedUpdate()
    {
        if (m_portalType == PortalFunction.full)
            SetGate();
    }

    void LateUpdate()
    {
        if (m_portalType == PortalFunction.full)
            GateCamRepos();
    }

    protected Camera InGameCamera;
    private RenderTexture TempRenTex;
    private Mesh GateMesh;

    void GateNotFound()
    {
        print("could not find gate, double check your portal connections");
        this.gameObject.SetActive(false);
    }

    void SetGate()
    {
        if (!ConnectedPortal || ConnectedPortal.GetComponent<StrayPortalManager>().ConnectedPortal != this.gameObject)
        {
            print(this.gameObject.name + " is looking for a gate");
            for (int i = 0; i < FindObjectsOfType<Transform>().Length; i++)
                if (FindObjectsOfType<Transform>()[i] != transform && FindObjectsOfType<Transform>()[i].GetComponent<StrayPortalManager>() && FindObjectsOfType<Transform>()[i].GetComponent<StrayPortalManager>().ConnectedPortal == gameObject)
                {
                    print("gate found");
                    ConnectedPortal = FindObjectsOfType<Transform>()[i].gameObject;
                }
            //we can at this point assume that a gate will not be found, let's take appropriate action
            GateNotFound();
        }
        else
        {
            if (!InGameCamera)
            {
                InGameCamera = Camera.main; //Fill empty "InGameCamera" variable with main camera
            }
            else
            {
                if (InGameCamera.nearClipPlane > .01f)
                    Debug.LogError("The nearClipPlane of 'Main Camera' is not equal to 0.01");

                GateMesh = GetComponent<MeshFilter>().sharedMesh; //Acquire current portal mesh

                //Generate render texture for the portal camera
                for (int i = 0; i < RenTex.Length; i++)
                {
                    if (CurrentProjectionResolution[i].x != ViewSettings.Projection.Resolution.x || CurrentProjectionResolution[i].y != ViewSettings.Projection.Resolution.y || ViewSettings.Projection.CurrentDepthQuality[i] != ViewSettings.Projection.DepthQuality)
                    {
                        if (RenTex[i])
                        {
                        #if UNITY_EDITOR
                            if (!EditorApplication.isPlaying)
                            {
                                DestroyImmediate(RenTex[i], false);

                                if (i == 0)
                                    DestroyImmediate(TempRenTex, false);
                            }
                            if (EditorApplication.isPlaying)
                            {
                                Destroy(RenTex[i]);

                                if (i == 0)
                                    Destroy(TempRenTex);
                            }
                            #else
								Destroy (RenTex [i]);

								if (i == 0)
									Destroy (TempRenTex);
                            #endif
                        }
                        if (!RenTex[i])
                        {
                            RenTex[i] = new RenderTexture(Convert.ToInt32(ViewSettings.Projection.Resolution.x), Convert.ToInt32(ViewSettings.Projection.Resolution.y), ViewSettings.Projection.DepthQuality == ViewSettingsClass.ProjectionClass.DepthQualityEnum.Fast ? 16 : 24);
                            RenTex[i].name = this.gameObject.name + " RenderTexture " + i;
                            if (i == 0)
                                TempRenTex = RenderTexture.GetTemporary(Convert.ToInt32(ViewSettings.Projection.Resolution.x), Convert.ToInt32(ViewSettings.Projection.Resolution.y), ViewSettings.Projection.DepthQuality == ViewSettingsClass.ProjectionClass.DepthQualityEnum.Fast ? 16 : 24);

                            CurrentProjectionResolution[i] = new Vector2(ViewSettings.Projection.Resolution.x, ViewSettings.Projection.Resolution.y);
                            ViewSettings.Projection.CurrentDepthQuality[i] = ViewSettings.Projection.DepthQuality;
                        }
                    }
                }

                #if UNITY_EDITOR
                LayerMask SceneTabLayerMask = Tools.visibleLayers;

                SceneTabLayerMask &= ~(1 << 1); //Disable SceneviewRender layer on Sceneview

                Tools.visibleLayers = SceneTabLayerMask;

                //Generate projection plane for Sceneview
                if (!SceneviewRender)
                {
                    SceneviewRender = new GameObject(transform.name + " SceneviewRender");

                    SceneviewRender.AddComponent<MeshFilter>();
                    SceneviewRender.AddComponent<MeshRenderer>();

                    SceneviewRender.transform.position = transform.position;
                    SceneviewRender.transform.rotation = transform.rotation;
                    SceneviewRender.transform.localScale = transform.localScale;
                    SceneviewRender.transform.parent = transform;
                }
                else
                {
                    if (SceneviewRender.name != transform.name + " SceneviewRender")
                        SceneviewRender.name = transform.name + " SceneviewRender";

                    SceneviewRender.layer = 4;

                    SceneviewRender.transform.localPosition = new Vector3(0, 0, .0001f);

                    SceneviewRender.GetComponent<MeshFilter>().sharedMesh = GateMesh;
                    SceneviewRender.GetComponent<MeshRenderer>().sharedMaterial = GateMaterial[1];

                    //Apply render texture to the scene portal material
                    if (GateMaterial.Length > 1)
                        SceneviewRender.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", InGameCamera && ConnectedPortal.GetComponent<StrayPortalManager>().RenTex[1] ? ConnectedPortal.GetComponent<StrayPortalManager>().RenTex[1] : null);
                }
                #endif

                //Apply render texture to the game portal material
                if (GateMaterial.Length > 0)
                    GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", InGameCamera && ConnectedPortal.GetComponent<StrayPortalManager>().RenTex[0] ? ConnectedPortal.GetComponent<StrayPortalManager>().RenTex[0] : null);

                //Manage distorstion pattern settings
                GetComponent<MeshRenderer>().sharedMaterial.SetInt("_EnableDistorsionPattern", ViewSettings.Distorsion.EnableDistorsion ? 1 : 0);
                GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_DistorsionPattern", ViewSettings.Distorsion.Pattern);
                GetComponent<MeshRenderer>().sharedMaterial.SetColor("_DistorsionPatternColor", ViewSettings.Distorsion.Color);
                GetComponent<MeshRenderer>().sharedMaterial.SetInt("_DistorsionPatternTiling", ViewSettings.Distorsion.Tiling);
                GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_DistorsionPatternSpeedX", -ViewSettings.Distorsion.SpeedX);
                GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_DistorsionPatternSpeedY", -ViewSettings.Distorsion.SpeedY);


                //Generate camera for the portal rendering
                for (int j = 0; j < GateCamObjs.Length; j++)
                {
                    if (j < ViewSettings.Recursion.Steps + 1)
                    {
                        if (!GateCamObjs[j])
                        {
                            GateCamObjs[j] = new GameObject(transform.name + " Camera " + j);

                            GateCamObjs[j].tag = "Untagged";

                            GateCamObjs[j].transform.parent = transform;
                            GateCamObjs[j].AddComponent<Camera>();
                            GateCamObjs[j].GetComponent<Camera>().enabled = false;
                            InitGateCamObjsCullingMask[j] = GateCamObjs[j].GetComponent<Camera>().cullingMask;
                            GateCamObjs[j].GetComponent<Camera>().nearClipPlane = .01f;

                            GateCamObjs[j].AddComponent<Skybox>();
                        }
                        else
                        {
                            if (GateCamObjs[j].name != transform.name + " Camera " + j)
                                GateCamObjs[j].name = transform.name + " Camera " + j;

                            if (GateCamObjs[j].GetComponent<Camera>().depth != InGameCamera.depth - 1)
                                GateCamObjs[j].GetComponent<Camera>().depth = InGameCamera.depth - 1;

                            //Acquire settings from Scene/Game camera, to apply on Portal camera
                            if (InGameCamera)
                            {
                                GateCamObjs[j].GetComponent<Camera>().renderingPath = InGameCamera.renderingPath;
                                GateCamObjs[j].GetComponent<Camera>().useOcclusionCulling = InGameCamera.useOcclusionCulling;
                                GateCamObjs[j].GetComponent<Camera>().hdr = InGameCamera.hdr;
                            }
                        }

                        if (ConnectedPortal.GetComponent<StrayPortalManager>().GateCamObjs[j])
                            ConnectedPortal.GetComponent<StrayPortalManager>().GateCamObjs[j].GetComponent<Skybox>().material = ViewSettings.Recursion.CustomFinalStep && (j > 0 && j == ViewSettings.Recursion.Steps) ? ViewSettings.Recursion.CustomFinalStep : (!ConnectedPortalSkybox && (j > 0 && j == ViewSettings.Recursion.Steps) ? NullConnectedPortalSkybox : ConnectedPortalSkybox);
                    }
                    else
                    {
                    #if UNITY_EDITOR
                        if (!EditorApplication.isPlaying)
                            DestroyImmediate(GateCamObjs[j], false);
                        if (EditorApplication.isPlaying)
                            Destroy(GateCamObjs[j]);
                    #else
							Destroy (GateCamObjs [j]);
                    #endif
                    }
                }

                //Generate mesh clip plane modificator object
                if (!ClipPlanePosObj)
                {
                    ClipPlanePosObj = new GameObject(transform.name + " ClipPlanePosObj");

                    ClipPlanePosObj.transform.position = transform.position;
                    ClipPlanePosObj.transform.rotation = transform.rotation;
                    ClipPlanePosObj.transform.parent = transform;
                }
                else
                {
                    ClipPlanePosObj.transform.localPosition = new Vector3(0, 0, .005f);

                    if (ClipPlanePosObj.name != transform.name + " ClipPlanePosObj")
                        ClipPlanePosObj.name = transform.name + " ClipPlanePosObj";
                }

                gameObject.layer = 1;

                //Apply current portal mesh to the mesh collider if exist
                if (GetComponent<MeshCollider>() && GetComponent<MeshCollider>().sharedMesh != GateMesh)
                    GetComponent<MeshCollider>().sharedMesh = GateMesh;
                //Disable trigger of portal collider
                if (GetComponent<Collider>())
                {
                    if (GetComponent<Collider>().isTrigger != (InGameCamera ? true : false))
                        GetComponent<Collider>().isTrigger = InGameCamera ? (PortalSettings.EnablePortalTrigger ? true : false) : false;
                }
                else
                    Debug.LogError("No collider component found");
            }
        }
        if (m_portalType != PortalFunction.full)
        {
            SceneviewRender.SetActive(false); //deactivate portal visuals
        }
    }

    void GateCamRepos()
    {
        if (InGameCamera && ConnectedPortal)
        {
            Vector3[] GateCamPos = new Vector3[GateCamObjs.Length];
            Quaternion[] GateCamRot = new Quaternion[GateCamObjs.Length];

            for (int i = 0; i < RenTex.Length; i++)
            {
                if (RenTex[i])
                {
                    for (int j = ViewSettings.Recursion.Steps; j >= 0; j--)
                    {
                        if (GateCamObjs[j])
                        {
                            //Move portal camera to position/rotation of Scene/Game camera
                            Camera SceneCamera = null;

#if UNITY_EDITOR
                            SceneCamera = SceneView.GetAllSceneCameras().Length > 0 ? SceneView.GetAllSceneCameras()[0] : null;
#endif

                            GateCamObjs[j].GetComponent<Camera>().aspect = (i == 1 && SceneCamera ? SceneCamera.aspect : InGameCamera.aspect);
                            GateCamObjs[j].GetComponent<Camera>().fieldOfView = (i == 1 && SceneCamera ? SceneCamera.fieldOfView : InGameCamera.fieldOfView);
                            GateCamObjs[j].GetComponent<Camera>().farClipPlane = (i == 1 && SceneCamera ? SceneCamera.farClipPlane : InGameCamera.farClipPlane);

                            GateCamPos[j] = ConnectedPortal.transform.InverseTransformPoint(i == 1 && SceneCamera ? SceneCamera.transform.position : InGameCamera.transform.position);

                            GateCamPos[j].x = -GateCamPos[j].x;
                            GateCamPos[j].z = -GateCamPos[j].z + j * (Vector3.Distance(transform.position, ConnectedPortal.transform.position) / 5);

                            GateCamRot[j] = Quaternion.Inverse(ConnectedPortal.transform.rotation) * (i == 1 && SceneCamera ? SceneCamera.transform.rotation : InGameCamera.transform.rotation);

                            GateCamRot[j] = Quaternion.AngleAxis(180.0f, new Vector3(0, 1, 0)) * GateCamRot[j];

                            GateCamObjs[j].transform.localPosition = GateCamPos[j];
                            GateCamObjs[j].transform.localRotation = GateCamRot[j];

                            //Render portal camera and recursion to render texture
                            if (ConnectedPortal.GetComponent<StrayPortalManager>().m_portalType == PortalFunction.full)
                            {
                                if (j > 0 && j == ViewSettings.Recursion.Steps)
                                    GateCamObjs[j].GetComponent<Camera>().cullingMask = 0;
                                else
                                {
                                    GateCamObjs[j].GetComponent<Camera>().cullingMask = InGameCamera.cullingMask;

                                    for (int k = 0; k < PortalFunctionality.ExcludedObjsFromRender.Length; k++)
                                        if (PortalFunctionality.ExcludedObjsFromRender[k].Obj)
                                            GateCamObjs[j].GetComponent<Camera>().cullingMask &= ~(1 << PortalFunctionality.ExcludedObjsFromRender[k].Layer);

                                    if (i == 0)
                                        GateCamObjs[j].GetComponent<Camera>().cullingMask &= ~(1 << 4);
                                    else
                                        GateCamObjs[j].GetComponent<Camera>().cullingMask &= ~(1 << 1);
                                }

                                GateCamObjs[j].GetComponent<Camera>().targetTexture = TempRenTex;

                                RenderTexture.active = GateCamObjs[j].GetComponent<Camera>().targetTexture;

                                GateCamObjs[j].GetComponent<Camera>().Render();

                                Graphics.Blit(TempRenTex, RenTex[i]);

                                RenderTexture.active = null;

                                GateCamObjs[j].GetComponent<Camera>().targetTexture = null;
                            }
                        }
                    }
                }
            }
        }
    }

    class InitMaterialsList { public Material[] Materials; }
    private GameObject[] CollidedObjs = new GameObject[0];
    private string[] CollidedObjsInitName = new string[0];
    [HideInInspector]
    public Vector3 CollidedObjsParentPreviousFirstPos;
    [HideInInspector]
    public Vector3 CollidedObjsParentPreviousSecondPos;
    private bool AcquireNextPos;
    private InitMaterialsList[] CollidedObjsInitMaterials = new InitMaterialsList[0];
    private bool[] StandardObjShader = new bool[0];
    private bool[] CollidedObjsAlwaysTeleport = new bool[0];
    private bool[] CollidedObjsFirstTrig = new bool[0];
    private float[] CollidedObjsFirstTrigDist = new float[0];
    private GameObject[] ProxDetCollidedObjs = new GameObject[0];
    private GameObject[] CloneCollidedObjs = new GameObject[0];
    private Vector3[] CollidedObjVelocity = new Vector3[0];
    private bool[] ContinueTriggerEvents = new bool[0];
    [HideInInspector]
    public bool CollidedObjsExternalParent;
    [HideInInspector]
    public int EnterTriggerTimes;

    public bool m_canTP = true;

    void OnTriggerEnter(Collider collision)
    {
        print("entering portal");

        //just tp the player to the final location...
        if (m_canTP)
        {
            collision.transform.position = ConnectedPortal.transform.position;
            //InGameCamera.transform.rotation = Quaternion.Euler(new Vector3(10.0f, 10.0f, 10.0f));//Quaternion.Inverse(ConnectedPortal.transform.rotation) * (InGameCamera.transform.rotation);
            //collision.transform.rotation = Quaternion.AngleAxis(180.0f, new Vector3(0, 1, 0)) * GateCamObjs[0].transform.rotation;
            ConnectedPortal.GetComponent<StrayPortalManager>().m_canTP = false;
            m_canTP = false;
        }
    }

    private GameObject[] ObjCollidedCamObj = new GameObject[2];
    private GameObject[] ObjCloneCollidedCamObj = new GameObject[2];

    private Vector3 PreviousCollidedObjsInternalParentPos;

    void OnTriggerExit(Collider collision)
    {
        print("exiting portal");
        m_canTP = true;
    }
}