using DG.Tweening;
using Google.XR.ARCoreExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARSceneTransformer : MonoBehaviour
{
    public InputField scaleField;
    public Transform arcamera_trans;
    // AR Param
    [SerializeField]
    private bool lockUpdateScale = false;
    [SerializeField]
    private bool usePreComputeScale = true;
    [SerializeField]
    private float preComputeScale = 0.115f; //716ɳ��ģ��:0.115f
    [SerializeField]
    private float scaleChangeFactor = 0.1f;
    [SerializeField]
    private float minSceneDistanceToUpdateScale = 0.1f;
    [SerializeField]
    private bool poseUpdatePerFrame = false;
    [SerializeField]
    private float poseChangeFactor = 0.5f;
    [SerializeField]
    private float scaleChangeSpeed = 0.2f;
    [SerializeField]
    private float colliderSize = 0.2f;

    // Signal
    private bool firstLocalize;
    private bool firstSetSceneObjectPose;

    // Pose&Scale Compute
    private float localComputeScale;
    private float globalComputeScale;
    private float predictScale;
    private Vector3 firstScenePos;
    private Vector3 firstWorldPos;
    private Vector3 lastScenePos;
    private Vector3 lastWorldPos;
    private Matrix4x4 transformMatrix;

    // Query List
    [SerializeField]
    private int queryWaitListLength = 128;
    private ARTools.QueryNode[] queryWaitList;

    // Virtual Object
    [SerializeField]
    private string configPath = "Models/model_config";
    private List<Pose> VOPose;
    private List<Pose> VOTargetPose;
    private List<Vector3> VOInitScale;
    private List<GameObject> VOPrefabList;
    public List<GameObject> VOList;
    private List<Transform> FaList;

    // Debug
    public bool debugMode = false;
    public Text receiveText;
    public Text logText;
    public Text debugText;
    [SerializeField]
    private Transform debugLineOriginPoint;   //Debug��������ʼ�㣬����������ǰ��
    private List<LineRenderer> debug_lines;
    private string returnMsg;

    // Text Parse
    private const char wordSpliter = ' ';
    private const char lineSpliter = ';';

    // Touch Functions
    public Dictionary<string, int> objName2Id;
    //private List<Vector3> undriftPos;
    //private List<Quaternion> undriftRot;
    //private List<Vector3> undriftSca;

    public List<Vector3> driftPosDelta;
    public List<Quaternion> driftRotDelta;
    public List<Vector3> driftScaDelta;



    private void Start()
    {
        Init();
        LoadModels(configPath);

        if(scaleField != null)
        {
            scaleField.onValueChanged.AddListener(SetScale);
        }
    }

    private void Update()
    {
        if (poseUpdatePerFrame)
        {
            /*��֡����*/
            // ���ռ����VOTargetPose��һ��ϵ����poseChangeFactor����������Pose (�����Ϊֻ����ʶ��㸽������)
            for (int i = 0; i < VOList.Count; i++)
            {
                float distance = Vector3.Distance(FaList[i].position, VOTargetPose[i].position);
                float deltaDistance = distance * poseChangeFactor;
                FaList[i].position = Vector3.MoveTowards(FaList[i].position, VOTargetPose[i].position, deltaDistance);
                
                float angle = Quaternion.Angle(FaList[i].rotation, VOTargetPose[i].rotation);
                float deltaAngle = angle * poseChangeFactor;
                FaList[i].rotation = Quaternion.RotateTowards(FaList[i].rotation, VOTargetPose[i].rotation, deltaAngle);

                FaList[i].localScale = poseChangeFactor * VOInitScale[i] * predictScale + (1 - poseChangeFactor) * FaList[i].localScale;
                

                // ���Ӳٿر���ƫ��ƫ����
                VOList[i].transform.position = FaList[i].position + driftPosDelta[i];
                VOList[i].transform.rotation = FaList[i].rotation * driftRotDelta[i];
                VOList[i].transform.localScale = driftScaDelta[i];
                //if (i == 0)
                //{
                //    Debug.Log("uuu predictScale " + predictScale.ToString());
                //    Debug.Log("uuu FaList[i].localScale" + FaList[i].localScale);
                //    Debug.Log("uuu VOList[i].transform.localScale" + VOList[i].transform.localScale);
                //}
                
            }
        }

        //����ָʾ����
        if (debugMode)
        {
            for (int i = 0; i < debug_lines.Count; i++)
            {
                debug_lines[i].SetPosition(0, debugLineOriginPoint.position);  // �������� 0 λ�õĶ˵�Ϊ p
                debug_lines[i].SetPosition(1, VOList[i].transform.position);  // �������� 1 λ�õĶ˵�Ϊ p
            }
        }
    }

    private void Init()
    {
        VOPrefabList = new List<GameObject>();
        VOList = new List<GameObject>();
        FaList = new List<Transform>();
        debug_lines = new List<LineRenderer>();

        VOPose = new List<Pose>();
        VOTargetPose = new List<Pose>();
        VOInitScale = new List<Vector3>();

        poseChangeFactor = Mathf.Clamp(poseChangeFactor, 0.001f, 1f);
        scaleChangeSpeed = Mathf.Clamp(scaleChangeSpeed, 0.001f, 1f);

        queryWaitList = new ARTools.QueryNode[queryWaitListLength];
        localComputeScale = 0.0f;
        globalComputeScale = 0.0f;
        predictScale = usePreComputeScale ? preComputeScale : 1.0f;

        firstLocalize = true;
        firstSetSceneObjectPose = true;

        firstScenePos = Vector3.zero;
        firstWorldPos = Vector3.zero;
        lastScenePos = Vector3.zero;
        lastWorldPos = Vector3.zero;
        transformMatrix = Matrix4x4.identity;

        returnMsg = "";

        if (debugLineOriginPoint==null)
        {
            debugLineOriginPoint = transform;
        }

        objName2Id = new Dictionary<string, int>();
        //undriftPos = new List<Vector3>();
        //undriftRot = new List<Quaternion>();
        //undriftSca = new List<Vector3>();
        driftPosDelta = new List<Vector3>();
        driftRotDelta = new List<Quaternion>();
        driftScaDelta = new List<Vector3>();

        if (poseUpdatePerFrame)
            poseChangeFactor /= 30;
    }

    public void addQueryList(int imgId, Pose imgPose)
    {
        queryWaitList[imgId % queryWaitListLength] = new ARTools.QueryNode(imgId, imgPose);
    }

    public string arSceneTransform(ref string receiveMsg)
    {
        returnMsg = "";
        returnMsg += receiveMsg;
        Debug.Log(receiveMsg);

        if (receiveMsg.StartsWith("success"))
        {
            int imgId;
            Pose pose;
            int inliners;
            float timestamp;
            if(parseReceiveMsg(ref receiveMsg, out imgId, out pose, out inliners, out timestamp,';',' '))
            {
                if (queryWaitList[imgId % queryWaitListLength].id == imgId)
                {
                    pose = ARTools.poseChangeChirality(pose);   //���λ�ˣ���->��
                    //pose = ARTools.getInversePose(pose);
                    //Matrix4x4 Tcw1 = Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one);
                    Matrix4x4 Tcw1 = ARTools.pose2Matrix(pose,true);  //תΪ������в���

                    Pose frontPose = queryWaitList[imgId % queryWaitListLength].pose; //ǰ��λ��
                    //Matrix4x4 Tcw2 = Matrix4x4.TRS(frontPose.position, frontPose.rotation, Vector3.one);
                    Matrix4x4 Tcw2 = ARTools.pose2Matrix(frontPose, true);

                    
                    Matrix4x4 matInv = transformMatrix.inverse;
                    Pose frontPoseInversed = ARTools.getInversePose(frontPose, true);
                    Pose serverFrontPose = transformPose(ref matInv, frontPoseInversed);//ǰ������ת����λ����

                    computeTransMatrix(ref Tcw1, ref Tcw2, predictScale);   // Change
                    transformSceneObject();

                    returnMsg += ";";
                    returnMsg += "frontPose " + ARTools.pose2Str(ref frontPoseInversed);
                    returnMsg += ";";
                    returnMsg += "transMatrix " + ARTools.matrix2Str(ref transformMatrix);
                    returnMsg += ";";
                    returnMsg += "predictScale " + predictScale.ToString();

                    // ����current_camera
                    //transServerPose = ARTools.getInversePose(transServerPose, true);
                    //transServerPose = transformPose(ref transformMatrix, transServerPose);  //�任���λ��


                    returnMsg += ";";
                    
                    returnMsg += "ARCameraPose " + ARTools.pose2Str(ref serverFrontPose);
                    returnMsg += ";";
                    Pose ServerPose = ARTools.getInversePose(pose, true);
                    returnMsg += "ServerPose " + ARTools.pose2Str(ref ServerPose);
                }

            }
        }

        return returnMsg;
    }

    private bool parseReceiveMsg(ref string msg, out int imgId, out Pose pose, out int inliners, out float timestamp,
        char lineSpliter=';', char wordSpliter = ' ')
    {
        /* �������ݸ�ʽ
         * success;
         * img_id 1;
         * pose tx ty tz qx qy qz qw;
         * inliners 25;
         * timestamp xxx
         */

        imgId = -1;
        pose = Pose.identity;
        inliners = -1;
        timestamp = -1;

        if (msg.StartsWith("success"))
        {
            string[] lines = msg.Trim().Split(lineSpliter);
            if (lines.Length != 5)
            {
                ARTools.PrintError("lines.Length!=5");
                return false;
            }

            int.TryParse(lines[1].Split(wordSpliter)[1],out imgId);
            //float[] vec = ARTools.str2Vec(lines[2]);
            float[] vec = ARTools.str2Vec(lines[2].Substring(5));
            pose = ARTools.vec2Pose(vec);
            int.TryParse(lines[3].Split(wordSpliter)[1], out inliners);
            float.TryParse(lines[4].Split(wordSpliter)[1], out timestamp);
            return true;
        }

        return false;
    }

    private void LoadModels(string configPath)
    {
        //��ȡģ����Ϣ
        TextAsset infoJson = Resources.Load<TextAsset>(configPath);
        ARTools.ModelInfoList infoList = JsonUtility.FromJson<ARTools.ModelInfoList>(infoJson.text);
        List<ARTools.ModelInfo> modelInfos = infoList.model_config;

        int model_num = modelInfos.Count;
        for (int i = 0; i < model_num; i++)
        {
            //��ȡģ��λ��
            Vector3 pos = new Vector3(modelInfos[i].position[0], modelInfos[i].position[1], modelInfos[i].position[2]);
            Quaternion rot = new Quaternion(modelInfos[i].rotation[0], modelInfos[i].rotation[1], modelInfos[i].rotation[2], modelInfos[i].rotation[3]);
            Vector3 scale = new Vector3(modelInfos[i].scale[0], modelInfos[i].scale[1], modelInfos[i].scale[2]);
            Pose pose = new Pose(pos, rot);

            //��ȡģ���ļ�
            string absolutePath = Application.dataPath + "/Resources/" + modelInfos[i].filepath + "." + modelInfos[i].suffix;
            GameObject prefab_go = Resources.Load<GameObject>(modelInfos[i].filepath);
            GameObject go;

            if (prefab_go)
            {
                VOPrefabList.Add(prefab_go);
                go = Instantiate<GameObject>(prefab_go);
                go.name = prefab_go.name;

                // ��������ϵģ�ͣ�UnityĬ�Ϸ�תX��ת�����ԣ���z����ת180���ȼ��ڷ�תY��
                if (modelInfos[i].rightHand)
                {
                    pose.position = ARTools.positionChangeChiralityByReverseY(pose.position);
                    pose.rotation = pose.rotation * (new Quaternion(0, 0, 1, 0));
                }
                if (i > 0 && i <= 12)//���������������ײ
                {
                    go.AddComponent<BoxCollider>();
                    go.GetComponent<BoxCollider>().size = new Vector3(colliderSize / scale[0], colliderSize / scale[1], colliderSize / scale[2]);
                }
               


            }
            else
            {
                //���ȱʧģ�ͣ����������������ʾ
                VOPrefabList.Add(new GameObject());
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //go.GetComponent<BoxCollider>().enabled = false; //�����ԣ��ں���k60�ϲ���bug��ԭ��δ֪
                go.name = "Cube_" + i.ToString();

                // ��תY��任����
                if (modelInfos[i].rightHand)
                    pose = ARTools.poseChangeChirality(pose);
            }

            // ��¼��ʼλ��
            VOPose.Add(pose);
            VOTargetPose.Add(pose);
            VOInitScale.Add(scale);

            // ���ؽ������µ���ʱ����ı�
            objName2Id.Add(go.name, i);//��¼id��Ӧ������
            //undriftPos.Add(pose.position);
            //undriftRot.Add(pose.rotation);
            //undriftSca.Add(scale);
            //ƫ����
            driftPosDelta.Add(Vector3.zero);
            driftRotDelta.Add(Quaternion.identity);
            driftScaDelta.Add(Vector3.one);


            GameObject f_go = new GameObject();
            f_go.name = "f_" + go.name;
            f_go.transform.parent = transform;
            f_go.transform.position = pose.position;
            f_go.transform.rotation = pose.rotation;
            f_go.transform.localScale = scale;
            FaList.Add(f_go.transform);

            go.transform.parent = f_go.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.layer = LayerMask.NameToLayer("Touchable");   //Layer: Touchable 
            if (!debugMode)
                go.SetActive(false);  //��λǰ�ر���ʾ
            VOList.Add(go);




            //���׷���������
            if (debugMode)
            {
                go.AddComponent<LineRenderer>();
                LineRenderer line = go.GetComponent<LineRenderer>();
                line.startWidth = 0.02f;   // �����߿��
                line.positionCount = 2;  // ��������Ⱦ���˵�����
                line.startColor = Color.black;   // ���������ɫ
                line.endColor = Color.black;     // �����յ���ɫ
                debug_lines.Add(line);
            }
            
        }
    }

    //����任����
    void computeTransMatrix(ref Matrix4x4 Tcw1, ref Matrix4x4 Tcw2, float scale21)
    {
        //w1�����������ϵ
        //w2ǰ����������ϵ
        //�����λ��Tcw���������굽�������ı任
        //���������Twc����������굽��������ı任��col(3)��Ϊ�������������ϵ�µ����꣨Twc*ԭ��=��������ϵ�µ����ԭ�����꣩
        Matrix4x4 Tw1c = Tcw1.inverse;
        Matrix4x4 Tw2c = Tcw2.inverse;

        Matrix4x4 Tscale21 = Matrix4x4.identity;
        Tscale21.m00 = scale21;
        Tscale21.m11 = scale21;
        Tscale21.m22 = scale21;

        Vector3 sceneCameraPos = Tw1c.GetColumn(3);
        Vector3 worldCameraPos = Tw2c.GetColumn(3);

        if (firstLocalize)
        {
            //StartCoroutine(ReLocateCameraPos(worldCameraPos));
            firstLocalize = false;
            firstScenePos = sceneCameraPos;
            firstWorldPos = worldCameraPos;
        }
        else
        {
            updateScale(ref sceneCameraPos, ref worldCameraPos);
        }
        transformMatrix = Tw2c * Tscale21 * Tcw1;   //Tw2w1���������굽��������

    }

    private IEnumerator ReLocateCameraPos(Vector3 worldCameraPos)
    {
        ARSession session = GameObject.FindObjectOfType<ARSession>();
        session.enabled = false;
        Camera camera = Camera.main;
        camera.GetComponent<ARPoseDriver>().enabled = false;
        camera.GetComponent<ARCameraManager>().enabled = false;
        yield return new WaitForSeconds(0.1f);
        Camera.main.transform.localPosition = worldCameraPos;
        yield return new WaitForSeconds(0.1f);
        camera.GetComponent<ARPoseDriver>().enabled = true;
        camera.GetComponent<ARCameraManager>().enabled = true;
        yield return new WaitForSeconds(0.1f);
        session.enabled = true;
        
    }

    //����ǰ��˵�ͼ�ĳ߶ȱ�����Scale = WorldDist/SceneDist
    void updateScale(ref Vector3 currentScenePos, ref Vector3 currentWorldPos, int updateMethod=0)
    {
        // ������������λ�˵ľ�����Ʊ���
        float localSceneDist = Vector3.Distance(lastScenePos, currentScenePos);
        float localWorldDist = Vector3.Distance(lastWorldPos, currentWorldPos);
        if (localWorldDist < minSceneDistanceToUpdateScale)
            return;


        localComputeScale = localWorldDist / localSceneDist;
        //if(localWorldDist > minSceneDistanceToUpdateScale)
        //    localComputeScale = localWorldDist / localSceneDist;

        // ��First��Last�������λ�˵ľ�����Ʊ���
        float fGlobalSceneDist = Vector3.Distance(firstScenePos, currentScenePos);
        float fGlobalWorldDist = Vector3.Distance(firstWorldPos, currentWorldPos);
        if (fGlobalSceneDist > minSceneDistanceToUpdateScale)
            globalComputeScale = fGlobalWorldDist / fGlobalSceneDist;

        lastScenePos = currentScenePos;
        lastWorldPos = currentWorldPos;

        // �������ݣ���ǰʹ�þֲ��������,0.1�����ʣ�
        if (lockUpdateScale)
            return;
        float computeScale = localComputeScale;

        if (!usePreComputeScale)  // ��Ԥ����߶ȱ���ʱ���õ�һ�ι���ı�����ʼ��predictScale
        {
            if(localSceneDist > minSceneDistanceToUpdateScale)
            {
                usePreComputeScale = true;
                predictScale = computeScale;
            }
        }
        else
        {
            predictScale = scaleChangeFactor * computeScale + (1.0f - scaleChangeFactor) * predictScale;
        }
        Debug.Log(predictScale.ToString());
        debugText.text = "";
        debugText.text += "\nlocalSceneDist " + localSceneDist.ToString();
        debugText.text += "\nlocalWorldDist " + localWorldDist.ToString();
        debugText.text += "\nlocalComputeScale " + localComputeScale.ToString();
        debugText.text += "\npredictScale " + predictScale.ToString();
        debugText.text += "\nFaList[0].localScale " + FaList[0].localScale.ToString();
        debugText.text += "\nVOList[0].transform.localScale " + VOList[0].transform.localScale.ToString();
        //if (i == 0)
        //{
        //    Debug.Log("uuu predictScale " + predictScale.ToString());
        //    Debug.Log("uuu FaList[i].localScale" + FaList[i].localScale);
        //    Debug.Log("uuu VOList[i].transform.localScale" + VOList[i].transform.localScale);
        //}
    }

    //��������
    void transformSceneObject()
    {
        //����Ŀ��λ��
        for (int i = 0; i < VOList.Count; i++)
        {
            VOTargetPose[i] = transformPose(ref transformMatrix, VOPose[i]);
        }
        

        // ��һ��ʶ�𣺲��ó���λ��+��ʾ����
        if (firstSetSceneObjectPose)
        {
            firstSetSceneObjectPose = false;      //��Ϊʵʱ���¼�����
            for (int i = 0; i < VOList.Count; i++)
            {
                FaList[i].position = VOTargetPose[i].position;
                FaList[i].rotation = VOTargetPose[i].rotation;
                FaList[i].localScale = VOInitScale[i] * predictScale;
                if (i >= 0 && i <= 12)
                {
                    VOList[i].SetActive(true);
                    //VOList[i].AddComponent<ARAnchor>();
                }
                
            }
            return;
        }

        // ����ʶ��ƽ������λ��
        if (!poseUpdatePerFrame)
        {
            /*ÿ�ζ�λʱ����*/
            // ���ռ����VOTargetPose��һ��ϵ����poseChangeFactor����������Pose (�����Ϊֻ����ʶ��㸽������)
            for (int i = 0; i < VOList.Count; i++)
            {
                //float distance = Vector3.Distance(FaList[i].position, VOTargetPose[i].position);
                //float deltaDistance = distance * poseChangeFactor;
                //FaList[i].position = Vector3.MoveTowards(FaList[i].position, VOTargetPose[i].position, deltaDistance);
                FaList[i].DOMove(VOTargetPose[i].position, 1f / poseChangeFactor);

                //float angle = Quaternion.Angle(FaList[i].rotation, VOTargetPose[i].rotation);
                //float deltaAngle = angle * poseChangeFactor;
                //FaList[i].rotation = Quaternion.RotateTowards(FaList[i].rotation, VOTargetPose[i].rotation, deltaAngle);
                FaList[i].DORotate(VOTargetPose[i].rotation.eulerAngles, 1f / poseChangeFactor);

                FaList[i].localScale = poseChangeFactor * VOInitScale[i] * predictScale + (1 - poseChangeFactor) * FaList[i].localScale;

                // ���Ӳٿر���ƫ��ƫ����
                VOList[i].transform.localPosition = driftPosDelta[i];
                VOList[i].transform.localRotation = driftRotDelta[i];
                VOList[i].transform.localScale = driftScaDelta[i];
            }
        }
    }

    Pose transformPose(ref Matrix4x4 mat,Pose pose)
    {
        Matrix4x4 res = mat * ARTools.pose2Matrix(pose,true);
        return new Pose(res.GetPosition(), res.rotation);
    }

    public void ResetFirstSetSceneObjectPose()
    {
        firstSetSceneObjectPose = true;
    }

    public int name2Id(string name)
    {
        return objName2Id[name];
    }
    public void setActiveById(int id, bool isset)
    {
        Debug.Log(id + "  setActiveById   "+isset);
        if (id <= 24)
        {
            VOList[id].SetActive(isset);
        }
        
    }

    public void SetScale(string scale)
    {
        float s = float.Parse(scale);
        predictScale = s;
    }
}
