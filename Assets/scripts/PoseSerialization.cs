using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PoseSerialization : MonoBehaviour
{
    [SerializeField]
    private string FolderName;
    [SerializeField]
    private string ModelSuffix;
    [SerializeField]
    private Transform ARScene;

    //char sysSep = System.IO.Path.DirectorySeparatorChar;
    static char sysSep = '/';

    private void Start()
    {
        StartRecord();
    }
    void StartRecord()
    {
        Debug.Log("开始持久化场景位姿");
        if (!ARScene)
            ARScene = this.transform;
        serializeScenePose(ARScene);
        Debug.Log("持久化结束");
    }


    void serializeScenePose(Transform scene)
    {
        ARTools.ModelInfoList infoList = new ARTools.ModelInfoList();
        //Transform[] transes = gameObject.GetComponentsInChildren<Transform>();
        List<Transform> transes = new List<Transform>();
        List<ARTools.ModelInfo> model_config = new List<ARTools.ModelInfo>();
        foreach (Transform child in scene)
        {
            Debug.Log(child.name);
            transes.Add(child);
        }

        int length = transes.Count;
        for (int i = 0; i < length; i++)
        {
            int id = i;
            string filepath = FolderName + sysSep + transes[i].name;
            string suffix = ModelSuffix;
            Vector3 position = transes[i].position;
            Quaternion rotation = transes[i].rotation;
            Vector3 scale = transes[i].localScale;
            bool rightHand = false;

            model_config.Add(new ARTools.ModelInfo(id, filepath, suffix, position, rotation, scale, rightHand));
        }

        infoList.model_config = model_config;
        string strData = JsonUtility.ToJson(infoList);
        System.IO.File.WriteAllText(Application.dataPath + sysSep + "Resources" + sysSep + FolderName + sysSep + "model_config.json",strData);
    }
}
