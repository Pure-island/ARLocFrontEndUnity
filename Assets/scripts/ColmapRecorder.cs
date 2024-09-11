using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Experimental.Rendering;
using UnityEngine.XR.ARFoundation;
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARSubsystems;
using Newtonsoft.Json.Linq;

public class ColmapRecorder : MonoBehaviour
{

    [HideInInspector]
    public bool Recording { get; set; } = false;

    [HideInInspector]
    public bool CanRecord { get; set; } = true;

    ARCameraManager cameraManager;
    ARSession session;
    ARSessionOrigin sessionOrigin;
    XRCameraIntrinsics cameraIntrinsics; 
    private ARAnchor anchor;

    string AssetsPath;
    string ImagePath;
    string ImageTxtPath;
    string CamerasTxtPath ;
    string Points3DTxtPath;

    bool inited = false;

    int imgID = 1;

    // Start is called before the first frame update
    [Obsolete]
    void Start()
    {
        Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        Permission.RequestUserPermission(Permission.ExternalStorageRead);

        AssetsPath = Application.persistentDataPath + "/ARproject/colmap/";
        ImagePath = Application.persistentDataPath + "/ARproject/colmap/images/";
        ImageTxtPath = Application.persistentDataPath + "/ARproject/colmap/images.txt";
        CamerasTxtPath = Application.persistentDataPath + "/ARproject/colmap/cameras.txt"; 
        Points3DTxtPath = Application.persistentDataPath + "/ARproject/colmap/points3D.txt";


        if (!Directory.Exists(AssetsPath))
            Directory.CreateDirectory(AssetsPath);
        DirectoryInfo dir = new DirectoryInfo(AssetsPath);
        dir.Delete(true);//初始化清空文件

        if (!Directory.Exists(ImagePath))
            Directory.CreateDirectory(ImagePath);
        if (System.Uri.TryCreate(ImageTxtPath, UriKind.Absolute, out Uri datasetUri))
        {
            Debug.Log("saving data to: " + datasetUri.ToString());
        }
        if (System.Uri.TryCreate(CamerasTxtPath, UriKind.Absolute, out  datasetUri))
        {
            Debug.Log("saving data to: " + datasetUri.ToString());
        }
        if (System.Uri.TryCreate(Points3DTxtPath, UriKind.Absolute, out  datasetUri))
        {
            Debug.Log("saving data to: " + datasetUri.ToString());
        }
        File.WriteAllText(ImageTxtPath, "# Image");
        File.WriteAllText(CamerasTxtPath, "# Camera");
        File.WriteAllText(Points3DTxtPath, "# Points3D");

        session = FindObjectOfType<ARSession>();
        cameraManager = FindObjectOfType<ARCameraManager>();
        sessionOrigin = FindObjectOfType<ARSessionOrigin>();
        anchor = session.GetComponent<ARAnchor>();
    }

    public void RecordToColmap(Texture2D currentTexture, ARCameraFrameEventArgs eventArgs)
    {
        if (!inited)
        {
            inited = true;
            cameraManager.TryGetIntrinsics(out cameraIntrinsics);
            File.AppendAllText(CamerasTxtPath, $"{Environment.NewLine}1 PINHOLE {cameraIntrinsics.resolution.y} {cameraIntrinsics.resolution.x} {cameraIntrinsics.focalLength.y} {cameraIntrinsics.focalLength.x} {cameraIntrinsics.principalPoint.y} {cameraIntrinsics.principalPoint.x}");
        }
        SaveImage(currentTexture, eventArgs);
        //StartCoroutine(TryRecordToColmap(currentTexture));
    }

    private void SaveImage(Texture2D currentTexture, ARCameraFrameEventArgs eventArgs)
    {
        CanRecord = false;
        Debug.Log("saving img ");
        //Pose p = new Pose(cameraManager.transform.position - anchor.transform.position, cameraManager.transform.rotation * Quaternion.Inverse(anchor.transform.rotation));
        Pose p = new Pose(cameraManager.transform.position, cameraManager.transform.rotation);
        p = ARTools.poseChangeChirality(p);
        p = ARTools.getInversePose(p);
        File.AppendAllText(ImageTxtPath, $"{Environment.NewLine}{imgID} {p.rotation.w} {p.rotation.x} {p.rotation.y} {p.rotation.z} {p.position.x} {p.position.y} {p.position.z} 1 {imgID}.png");
        File.AppendAllText(ImageTxtPath, $"{Environment.NewLine}0 0 -1");
        Debug.Log($"img width:{currentTexture.width} img height:{currentTexture.height}");
        Task writeTask = File.WriteAllBytesAsync($"{ImagePath}{imgID++}.png", currentTexture.EncodeToPNG());
        StartCoroutine(Delay(writeTask));
    }
    IEnumerator Delay(Task t)
    {
        yield return new WaitForSeconds(0.25f);

        yield return new WaitUntil(() => t.IsCompleted);
        CanRecord = true;
    }

    /// <summary>写入图片</summary>有bug
    private void WriteImageAsync(string filePath, Color32[] array, int height, int width)
    {
        Task.Run(() =>
        {
            if (System.Uri.TryCreate(filePath, UriKind.Absolute, out Uri dataUri))
            {
                Debug.Log("saving img to: " + dataUri.ToString());
                Debug.Log($"img width:{width} img height:{height}");
                GraphicsFormat graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
                byte[] datas = ImageConversion.EncodeArrayToPNG(array, graphicsFormat, (uint)width, (uint)height);
                File.WriteAllBytes(dataUri.ToString(), datas);
            }
        });
    }
}
