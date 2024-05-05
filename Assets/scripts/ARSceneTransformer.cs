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
    private float preComputeScale = 0.115f; //716沙盘模型:0.115f
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
    private Transform debugLineOriginPoint;   //Debug连接线起始点，建议绑定在相机前方
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
            /*逐帧更新*/
            // 参照计算的VOTargetPose按一定系数（poseChangeFactor）更新物体Pose (后面改为只更新识别点附近物体)
            for (int i = 0; i < VOList.Count; i++)
            {
                float distance = Vector3.Distance(FaList[i].position, VOTargetPose[i].position);
                float deltaDistance = distance * poseChangeFactor;
                FaList[i].position = Vector3.MoveTowards(FaList[i].position, VOTargetPose[i].position, deltaDistance);
                
                float angle = Quaternion.Angle(FaList[i].rotation, VOTargetPose[i].rotation);
                float deltaAngle = angle * poseChangeFactor;
                FaList[i].rotation = Quaternion.RotateTowards(FaList[i].rotation, VOTargetPose[i].rotation, deltaAngle);

                FaList[i].localScale = poseChangeFactor * VOInitScale[i] * predictScale + (1 - poseChangeFactor) * FaList[i].localScale;
                

                // 叠加操控变形偏移偏移量
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

        //更新指示线条
        if (debugMode)
        {
            for (int i = 0; i < debug_lines.Count; i++)
            {
                debug_lines[i].SetPosition(0, debugLineOriginPoint.position);  // 设置索引 0 位置的端点为 p
                debug_lines[i].SetPosition(1, VOList[i].transform.position);  // 设置索引 1 位置的端点为 p
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
                    pose = ARTools.poseChangeChirality(pose);   //后端位姿，右->左
                    //pose = ARTools.getInversePose(pose);
                    //Matrix4x4 Tcw1 = Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one);
                    Matrix4x4 Tcw1 = ARTools.pose2Matrix(pose,true);  //转为矩阵进行操作

                    Pose frontPose = queryWaitList[imgId % queryWaitListLength].pose; //前端位姿
                    //Matrix4x4 Tcw2 = Matrix4x4.TRS(frontPose.position, frontPose.rotation, Vector3.one);
                    Matrix4x4 Tcw2 = ARTools.pose2Matrix(frontPose, true);

                    
                    Matrix4x4 matInv = transformMatrix.inverse;
                    Pose frontPoseInversed = ARTools.getInversePose(frontPose, true);
                    Pose serverFrontPose = transformPose(ref matInv, frontPoseInversed);//前端坐标转到定位坐标

                    computeTransMatrix(ref Tcw1, ref Tcw2, predictScale);   // Change
                    transformSceneObject();

                    returnMsg += ";";
                    returnMsg += "frontPose " + ARTools.pose2Str(ref frontPoseInversed);
                    returnMsg += ";";
                    returnMsg += "transMatrix " + ARTools.matrix2Str(ref transformMatrix);
                    returnMsg += ";";
                    returnMsg += "predictScale " + predictScale.ToString();

                    // 返回current_camera
                    //transServerPose = ARTools.getInversePose(transServerPose, true);
                    //transServerPose = transformPose(ref transformMatrix, transServerPose);  //变换后端位姿


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
        /* 接受数据格式
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
        //读取模型信息
        TextAsset infoJson = Resources.Load<TextAsset>(configPath);
        ARTools.ModelInfoList infoList = JsonUtility.FromJson<ARTools.ModelInfoList>(infoJson.text);
        List<ARTools.ModelInfo> modelInfos = infoList.model_config;

        int model_num = modelInfos.Count;
        for (int i = 0; i < model_num; i++)
        {
            //读取模型位姿
            Vector3 pos = new Vector3(modelInfos[i].position[0], modelInfos[i].position[1], modelInfos[i].position[2]);
            Quaternion rot = new Quaternion(modelInfos[i].rotation[0], modelInfos[i].rotation[1], modelInfos[i].rotation[2], modelInfos[i].rotation[3]);
            Vector3 scale = new Vector3(modelInfos[i].scale[0], modelInfos[i].scale[1], modelInfos[i].scale[2]);
            Pose pose = new Pose(pos, rot);

            //读取模型文件
            string absolutePath = Application.dataPath + "/Resources/" + modelInfos[i].filepath + "." + modelInfos[i].suffix;
            GameObject prefab_go = Resources.Load<GameObject>(modelInfos[i].filepath);
            GameObject go;

            if (prefab_go)
            {
                VOPrefabList.Add(prefab_go);
                go = Instantiate<GameObject>(prefab_go);
                go.name = prefab_go.name;

                // 对于右手系模型，Unity默认翻转X轴转换手性，对z轴旋转180°后等价于翻转Y轴
                if (modelInfos[i].rightHand)
                {
                    pose.position = ARTools.positionChangeChiralityByReverseY(pose.position);
                    pose.rotation = pose.rotation * (new Quaternion(0, 0, 1, 0));
                }
                if (i > 0 && i <= 12)//给部分物体添加碰撞
                {
                    go.AddComponent<BoxCollider>();
                    go.GetComponent<BoxCollider>().size = new Vector3(colliderSize / scale[0], colliderSize / scale[1], colliderSize / scale[2]);
                }
               


            }
            else
            {
                //如果缺失模型，生成立方体替代显示
                VOPrefabList.Add(new GameObject());
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //go.GetComponent<BoxCollider>().enabled = false; //经测试，在红米k60上产生bug，原因未知
                go.name = "Cube_" + i.ToString();

                // 翻转Y轴变换手性
                if (modelInfos[i].rightHand)
                    pose = ARTools.poseChangeChirality(pose);
            }

            // 记录初始位姿
            VOPose.Add(pose);
            VOTargetPose.Add(pose);
            VOInitScale.Add(scale);

            // 触控交互导致的临时物体改变
            objName2Id.Add(go.name, i);//记录id对应的名字
            //undriftPos.Add(pose.position);
            //undriftRot.Add(pose.rotation);
            //undriftSca.Add(scale);
            //偏移量
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
                go.SetActive(false);  //定位前关闭显示
            VOList.Add(go);




            //添加追踪线条组件
            if (debugMode)
            {
                go.AddComponent<LineRenderer>();
                LineRenderer line = go.GetComponent<LineRenderer>();
                line.startWidth = 0.02f;   // 设置线宽度
                line.positionCount = 2;  // 设置线渲染器端点数量
                line.startColor = Color.black;   // 设置起点颜色
                line.endColor = Color.black;     // 设置终点颜色
                debug_lines.Add(line);
            }
            
        }
    }

    //计算变换矩阵
    void computeTransMatrix(ref Matrix4x4 Tcw1, ref Matrix4x4 Tcw2, float scale21)
    {
        //w1后端世界坐标系
        //w2前端世界坐标系
        //相机的位姿Tcw，世界坐标到相机坐标的变换
        //相机的坐标Twc，是相机坐标到世界坐标的变换，col(3)即为相机在世界坐标系下的坐标（Twc*原点=世界坐标系下的相机原点坐标）
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
        transformMatrix = Tw2c * Tscale21 * Tcw1;   //Tw2w1，场景坐标到世界坐标

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

    //估计前后端地图的尺度比例，Scale = WorldDist/SceneDist
    void updateScale(ref Vector3 currentScenePos, ref Vector3 currentWorldPos, int updateMethod=0)
    {
        // 用最近两个相机位姿的距离估计比例
        float localSceneDist = Vector3.Distance(lastScenePos, currentScenePos);
        float localWorldDist = Vector3.Distance(lastWorldPos, currentWorldPos);
        if (localWorldDist < minSceneDistanceToUpdateScale)
            return;


        localComputeScale = localWorldDist / localSceneDist;
        //if(localWorldDist > minSceneDistanceToUpdateScale)
        //    localComputeScale = localWorldDist / localSceneDist;

        // 用First和Last两个相机位姿的距离估计比例
        float fGlobalSceneDist = Vector3.Distance(firstScenePos, currentScenePos);
        float fGlobalWorldDist = Vector3.Distance(firstWorldPos, currentWorldPos);
        if (fGlobalSceneDist > minSceneDistanceToUpdateScale)
            globalComputeScale = fGlobalWorldDist / fGlobalSceneDist;

        lastScenePos = currentScenePos;
        lastWorldPos = currentWorldPos;

        // 更新数据（当前使用局部距离计算,0.1更新率）
        if (lockUpdateScale)
            return;
        float computeScale = localComputeScale;

        if (!usePreComputeScale)  // 无预计算尺度比例时，用第一次估算的比例初始化predictScale
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

    //更新坐标
    void transformSceneObject()
    {
        //计算目标位置
        for (int i = 0; i < VOList.Count; i++)
        {
            VOTargetPose[i] = transformPose(ref transformMatrix, VOPose[i]);
        }
        

        // 第一次识别：布置场景位置+显示物体
        if (firstSetSceneObjectPose)
        {
            firstSetSceneObjectPose = false;      //设为实时更新计算结果
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

        // 后续识别：平滑更新位姿
        if (!poseUpdatePerFrame)
        {
            /*每次定位时更新*/
            // 参照计算的VOTargetPose按一定系数（poseChangeFactor）更新物体Pose (后面改为只更新识别点附近物体)
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

                // 叠加操控变形偏移偏移量
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
