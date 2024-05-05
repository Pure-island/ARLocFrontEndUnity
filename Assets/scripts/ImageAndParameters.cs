using UnityEngine;
using UnityEngine.UI;
using Rokid.UXR.Native;
using Rokid.UXR.Utility;
using System.Collections;
using Rokid.UXR.Module;

public class ImageAndParameters : MonoBehaviour
{
    public Camera ArCamera;
    public ARSceneTransformer arTransformer;
    private NetworkManager networkManager;
    public Text text_result;
    public Text image_info;
    private bool isInit;
    private bool isReadyToSendNextImage = true;
    private Texture2D previewTex;
    public RawImage rawImage;
    private int width, height;
    private byte[] data;
    private string URL="http://219.224.167.242:5018";
    float[] focalLength = new float[2];
    float[] principalPoint = new float[2];
    float[] distortion = new float[5]; // ����ϵ��
    private int img_idx = 0;
    public Camera mainCamera;

   
    public void Init()
    {
        // Configures the app to not shut down the screen
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        NativeInterface.NativeAPI.Recenter();  // reset glass 3dof
        RKVirtualController.Instance.Change(ControllerType.NORMAL);
        if (mainCamera == null)
        {
            mainCamera = MainCameraCache.mainCamera;
        }
        width = NativeInterface.NativeAPI.GetPreviewWidth();
        height = NativeInterface.NativeAPI.GetPreivewHeight();
        if (NativeInterface.NativeAPI.GetGlassName().Equals("Rokid Max Plus"))
        {
            NativeInterface.NativeAPI.SetCameraPreviewDataType(3);
            previewTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        }
        else
        {
            NativeInterface.NativeAPI.SetCameraPreviewDataType(1);
            previewTex = new Texture2D(width, height, TextureFormat.BGRA32, false);
        }
        data = new byte[width * height * 4];
        GetCameraIntrinsicParameters(); 
        NativeInterface.NativeAPI.OnCameraDataUpdate += OnCameraDataUpdate;
        isInit = true;
        
        networkManager = gameObject.AddComponent<NetworkManager>();  // �������ģ��
    }
    //��ȡ����ڲ������ϵ��
    private void GetCameraIntrinsicParameters()
    {
        

        NativeInterface.NativeAPI.GetFocalLength(focalLength);
        NativeInterface.NativeAPI.GetPrincipalPoint(principalPoint);
        NativeInterface.NativeAPI.GetDistortion(distortion);
        Debug.Log($"����ڲΣ�focalLength: {focalLength[0]}, {focalLength[1]}, principalPoint: {principalPoint[0]}, {principalPoint[1]}, ����ϵ��: {distortion[0]}, {distortion[1]}, {distortion[2]}, {distortion[3]}, {distortion[4]}");
    
        /*// ��������ڲξ���
        cameraMatrix = new Mat(3, 3, MatType.CV_64FC1);
        cameraMatrix.Set<double>(0, 0, focalLength[0]);
        cameraMatrix.Set<double>(1, 1, focalLength[1]);
        cameraMatrix.Set<double>(0, 2, principalPoint[0]);
        cameraMatrix.Set<double>(1, 2, principalPoint[1]);
        cameraMatrix.Set<double>(2, 2, 1);

        // ��������ϵ������
        distortionCoefficients = new Mat(1, 5, MatType.CV_64FC1);
        for (int i = 0; i < 5; i++)
        {
            distortionCoefficients.Set<double>(0, i, distortion[i]);
        }*/
    }
    private void OnCameraDataUpdate(int width, int height, byte[] data, long timestamp)
    {
        Transform recordTransform;
        
        //Debug.Log("���λ�ˣ�" + recordTransform.position.ToString());
        Loom.QueueOnMainThread(() =>
        {
            recordTransform = mainCamera.transform;
            previewTex.LoadRawTextureData(data);
            previewTex.Apply();
            // ���� 'previewTex' �����Ѿ����������ݵ�ԭʼ����
           // Texture2D rotatedTex = RotateTexture180(previewTex);
            // �� Texture2D ���ݱ���Ϊ JPEG ��ʽ
            byte[] jpegData = previewTex.EncodeToJPG(70);
            
            rawImage.texture = previewTex;
           // Debug.Log("JPEG data length: " + jpegData.Length);
            
            if (mainCamera == null)
            {
                mainCamera = MainCameraCache.mainCamera;
            }
            image_info.text = string.Format("Position:{0}\r\nEuler:{1}\r\nRotation:{2}", mainCamera.transform.position.ToString("f3"), mainCamera.transform.rotation.eulerAngles.ToString(), mainCamera.transform.rotation.ToString("f3"));
            /* image_info.text = string.Format(
               "Image info:\n\twidth: {0}\n\theight: {1}\n\tposition: {2}\n\trotation: {3}",
               width, height, recordTransform.position, recordTransform.rotation);*/
            /* image_info.text = string.Format(
               "Image info:\n\twidth: {0}\n\theight: {1}\n\tposition: {2}\n\trotation: {3}",
               width, height, "111111", "222222");*/
            // Check if ready to send next image
            if (isReadyToSendNextImage)
            {
               StartCoroutine(UploadImageData(jpegData, recordTransform, timestamp));
                Debug.Log("���λ�ˣ�" + mainCamera.transform.position.ToString());
                isReadyToSendNextImage = false;  // Set flag to false until image is successfully uploaded
            }
        });
    }
    Texture2D RotateTexture180(Texture2D originalTexture)
    {
        Texture2D rotatedTexture = new Texture2D(originalTexture.width, originalTexture.height);
        int width = originalTexture.width;
        int height = originalTexture.height;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // �����ش�ԭʼλ��(i, j)ת�Ƶ���λ�ã�ʵ��180����ת
                rotatedTexture.SetPixel(width - i - 1, height - j - 1, originalTexture.GetPixel(i, j));
            }
        }
        rotatedTexture.Apply();  // Ӧ�ø��ĵ�����
        return rotatedTexture;  // ������ת�������
    }
    IEnumerator UploadImageData(byte[] imageData,Transform recordTransform,long timestamp)
    {
        Debug.Log("��ʼ����ͼƬ...............");
        Pose origin_pose = new Pose(recordTransform.position, recordTransform.rotation);
        Pose reverse_pose = ARTools.getInversePose(origin_pose, true);
        arTransformer.addQueryList(img_idx, reverse_pose);
        BitArray myBA = new BitArray(imageData);
        int c = myBA.Length;


        WWWForm form = new WWWForm();
        //���ͼƬ���ڲΡ�����ϵ����
        form.AddBinaryData("image", imageData, timestamp.ToString(), "image/jpeg");
        form.AddField("img_idx", img_idx.ToString());//ͼ�����
        // �������ڲ�
        form.AddField("focalLengthX", focalLength[0].ToString());
        form.AddField("focalLengthY", focalLength[1].ToString());
        form.AddField("principalPointX", principalPoint[0].ToString());
        form.AddField("principalPointY", principalPoint[1].ToString());

        // ��ӻ���ϵ��
        form.AddField("distortionK1", distortion[0].ToString());
        form.AddField("distortionK2", distortion[1].ToString());
        form.AddField("distortionK3", distortion[2].ToString());
        form.AddField("distortionK4", distortion[3].ToString());
        form.AddField("distortionK5", distortion[4].ToString());

        img_idx += 1;

        float startTime = Time.realtimeSinceStartup; // ��¼����ʼ��ʱ��
        yield return StartCoroutine(networkManager.SendPOST(URL + "/upimageandcanshu", form,
            response => {
                float endTime = Time.realtimeSinceStartup; // ��¼��Ӧ�����ʱ��
                float rtt = (endTime - startTime) * 1000f; // ��������ʱ�ӣ���λΪ����
                text_result.text = "Net Result:\n\t" + response + "\n\tͼƬ����:" + c + "bit" + "\n\t����ʱ��RTT: " + rtt + " ms";
                Debug.Log("Upload Success: " + response);
                Debug.Log($"RTT: {rtt} ms");
                isReadyToSendNextImage = true;  // Allow next image to be sent

                string receiveMsg = response;
                string sendingMsg = arTransformer.arSceneTransform(ref receiveMsg);

               
            },
            error => {
                float endTime = Time.realtimeSinceStartup; // ��¼��Ӧ�����ʱ��
                float rtt = (endTime - startTime) * 1000f; // ��������ʱ�ӣ���λΪ����
                text_result.text = "Net Error:\n\t" + "���粻�ɴ" + error + "\n\tͼƬ����:" + c + "bit" + "\n\t����ʱ��RTT: " + rtt + " ms";
                Debug.LogError("Upload Failed: " + error);
                Debug.Log($"RTT: {rtt} ms");
                isReadyToSendNextImage = true;  // Even on error, allow next attempt
                
                string receiveMsg = error;
                string sendingMsg = arTransformer.arSceneTransform(ref receiveMsg);

            }
        ));
        Debug.Log("���ͽ���.........");
        
    }

    public void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            Api.UpdateScreenParams();
        }

        RKVirtualController.Instance.Change(ControllerType.NORMAL);
#if !UNITY_EDITOR
        rawImage.color = Color.white;
#endif
        // Configures the app to not shut down the screen
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        NativeInterface.NativeAPI.Recenter();  // reset glass 3dof
        RKVirtualController.Instance.Change(ControllerType.NORMAL);
        if (mainCamera == null)
        {
            mainCamera = MainCameraCache.mainCamera;
        }
        NativeInterface.NativeAPI.StartCameraPreview();
    }
/*    private void start()
    {
#if !UNITY_EDITOR
        rawImage.color = Color.white;
#endif
        // Configures the app to not shut down the screen
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        NativeInterface.NativeAPI.Recenter();  // reset glass 3dof
        RKVirtualController.Instance.Change(ControllerType.NORMAL);
        if (mainCamera == null)
        {
            mainCamera = MainCameraCache.mainCamera;
        }
        NativeInterface.NativeAPI.StartCameraPreview();
    }*/

    public void Release()
    {
        if (isInit)
        {
            NativeInterface.NativeAPI.OnCameraDataUpdate -= OnCameraDataUpdate;
            NativeInterface.NativeAPI.StopCameraPreview();
            NativeInterface.NativeAPI.ClearCameraDataUpdate();
            isInit = false;
        }
    }

    private void OnDestroy()
    {
        Release();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Release();
        }
        else
        {
            NativeInterface.NativeAPI.StartCameraPreview();
        }
    }

    private void Update()
    {
        if (isInit == false && NativeInterface.NativeAPI.IsPreviewing())
        {
            Init();
        }
    }
}
