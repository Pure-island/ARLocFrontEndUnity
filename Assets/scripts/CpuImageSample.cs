using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
// Adds XRCameraConfigurationExtensions extension methods to XRCameraConfiguration.
// This is for the Android platform only.
using Google.XR.ARCoreExtensions;

/// <summary>测试获取CPU图像</summary>
public class CpuImageSample : MonoBehaviour
{
    public ARSceneTransformer arTransformer;
    private Transform recordTranform;
    public Transform arcamera;

    private int img_idx = 0;

    private ARCameraManager m_CameraManager;
    public Image rawCameraImage;
    public Text imageInfo;
    //private Texture2D m_CameraTexture; 定义为局部变量，否则更新纹理时导致内存溢出
    private int icount = 0;
    private ARSession session;
    private ARCameraBackground mARCameraBackground;
    private string BaseURL = "";
    //控件
    public Button btn_justimage;
    public Button btn_IAndP;
    public Button btn_pause;
    public Text text_result;
    public InputField input_url;
    
    //标记 1 仅上传图片  2 上传图像与位姿
    private int flag=0;

    private bool netRequst = true;
    private bool sendImg = false;

    private void SetUrl(string baseurl)
    {
        BaseURL = baseurl;
    }
    private void Awake()
    {        
        m_CameraManager = FindObjectOfType<ARCameraManager>();
        var configs = m_CameraManager.GetConfigurations(Allocator.Temp);
        //Debug.Log("CheckRedMiRes" + configs);
        //var oneKResolution = new Vector2(1080, 1920);
        //foreach (var c in configs)
        //{
        //    Debug.Log("CheckRedMiRes" + c);
        //    //if(c.resolution == oneKResolution)
        //    //{
        //    //    m_CameraManager.currentConfiguration = c;
        //    //    break;
        //    //}
        //}
        session = FindObjectOfType<ARSession>();
        mARCameraBackground = FindObjectOfType<ARCameraBackground>();

        //先创建一个文件夹 用来存储位姿信息
        StartCoroutine(CreatFolder("ImageAndPose"));

        //m_CameraManager.enabled = false;

    }
    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;//不锁屏设置
        initView();
    }

    private void initView()
    {
        btn_justimage.onClick.AddListener(upJustImage);
        btn_IAndP.onClick.AddListener(getPoseOnce);
        btn_pause.onClick.AddListener(ARpause);
        if (input_url.text != "")
        {
            BaseURL = input_url.text;
        }
        input_url.onEndEdit.AddListener(SetUrl);

        //9.20修改，隐藏连续定位的按钮，将连续定位功能改为自动触发
        upJustImage();
    }

    private void ARpause()
    {
        Debug.Log("测试日志");
        //m_CameraManager.enabled = false;
        sendImg = false;
    }

    private void getPoseOnce()
    {
        //m_CameraManager.enabled = true;
        sendImg = true;
        flag = 2;
        arTransformer.ResetFirstSetSceneObjectPose();
    }

    private void upJustImage()
    {
        //m_CameraManager.enabled = true;
        sendImg = true;
        flag = 1;
    }

    private void OnEnable()
    {
        if (m_CameraManager != null)
        {
            m_CameraManager.frameReceived += onCameraFrameReceived;
        }

    }



    [Obsolete]
    private void OnDisable()
    {
        if (m_CameraManager != null)
        {
            m_CameraManager.frameReceived -= onCameraFrameReceived;
        }
    }

    [Obsolete]
    private void onCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (sendImg)
        {
            updateCameraImage(eventArgs);
            if (flag == 2)
                sendImg = false;
        }
    }

    [Obsolete]
    private unsafe void updateCameraImage(ARCameraFrameEventArgs eventArgs)
    {
        if (!m_CameraManager.TryAcquireLatestCpuImage(out XRCpuImage image)) { return; }
        //保存位姿 Transform
        recordTranform = arcamera.transform;

        
        var format = TextureFormat.RGBA32;
        Texture2D m_CameraTexture = new Texture2D(image.width, image.height, format, false);//在方法最后，清理纹理
        //if (m_CameraTexture == null || m_CameraTexture.width != image.width || m_CameraTexture.height != image.height)
        //{
        //    Destroy(m_CameraTexture);
        //    m_CameraTexture = new Texture2D(image.width, image.height, format, false);
        //    //StartCoroutine(DestoryUnusedTexture());
        //}

        var conversionParams = new XRCpuImage.ConversionParams(image, format, XRCpuImage.Transformation.MirrorY);

        var rawTextureData = m_CameraTexture.GetRawTextureData<byte>();
        try
        {
            image.Convert(conversionParams, new IntPtr(rawTextureData.GetUnsafePtr()), rawTextureData.Length);
        }
        finally
        {
            image.Dispose();
        }
        //m_CameraTexture = TextureCut(m_CameraTexture, 0.5f);
        Texture2D rotatedTexture = RotateTextureAntiClock90(m_CameraTexture);
        if (Screen.orientation == ScreenOrientation.LandscapeLeft)
        {
            rotatedTexture = RotateTextureAntiClock90(rotatedTexture);
        }
        else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
        {
            rotatedTexture = RotateTextureAntiClock90(rotatedTexture);
            rotatedTexture = RotateTextureAntiClock90(rotatedTexture);
        }
        else if (Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            rotatedTexture = RotateTextureAntiClock90(rotatedTexture);
            rotatedTexture = RotateTextureAntiClock90(rotatedTexture);
            rotatedTexture = RotateTextureAntiClock90(rotatedTexture);
        }

        //texture转bytes存储图片 

        //var bytes = rotatedTexture.EncodeToPNG();
        var bytes = rotatedTexture.EncodeToJPG(95);                             //###################

        string timestamp1 = image.timestamp.ToString();
        //string fileName = "image.png";
        //string filePath_image = Path.Combine(Application.persistentDataPath + "/" + timestamp1, fileName);
        /*string directoryPath = Path.GetDirectoryName(filePath_image);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }*/
        //保存图片
        //System.IO.File.WriteAllBytes(filePath_image, bytes);
        //Debug.Log(filePath_image);

        //显示小窗口
        rawCameraImage.sprite = Sprite.Create(rotatedTexture, new Rect(0, 0, rotatedTexture.width, rotatedTexture.height), new Vector2(0.5f, 0.5f));
        rawCameraImage.preserveAspect = true;

        // 获取相机的位置和旋转
        Vector3 position1 = m_CameraManager.transform.position;
        Quaternion rotation = m_CameraManager.transform.rotation;


        imageInfo.text = string.Format(
               "Image info:\n\twidth: {0}\n\theight: {1}\n\tplaneCount: {2}\n\ttimestamp: {3}\n\tformat: {4}\n\tposition: {5}\n\trotation: {6}",
               image.width, image.height, image.planeCount, timestamp1, image.format, position1, rotation);



        string content_pose = position1.x.ToString() + " " + position1.y.ToString() + " " + position1.z.ToString() + " " + rotation.x.ToString() + " " + rotation.y.ToString() + " " + rotation.z.ToString() + " " + rotation.w.ToString();
       // string filePath_pose = Path.Combine(Application.persistentDataPath + "/" + timestamp1, "pose.txt");


        //上传相关信息
        if (netRequst)
        {

            netRequst = false;
            StartCoroutine(UploadJustImage(bytes, timestamp1,recordTranform));
        }
        StartCoroutine(DestoryUnusedTexture());//防止内存溢出，清理一下未使用的纹理
    }

    private Texture2D RotateTextureAntiClock90(Texture2D m_CameraTexture)
    {
        //逆时针旋转90度
        int width = m_CameraTexture.width;
        int height = m_CameraTexture.height;
        /*int width = m_CameraTexture.height;
        int height = m_CameraTexture.width;*/

        Texture2D rotatedTexture = new Texture2D(height, width, m_CameraTexture.format, false);
        Color[] originalPixels = m_CameraTexture.GetPixels();
        Color[] rotatedPixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int originalIndex = x + y * width;

                // 计算旋转后的坐标
                int rotatedX = height - y - 1;
                int rotatedY = x;
                /*int rotatedX = width - x - 1;
                int rotatedY = height - y - 1;*/
                int rotatedIndex = rotatedX + rotatedY * height;

                rotatedPixels[rotatedIndex] = originalPixels[originalIndex];
            }
        }

        rotatedTexture.Reinitialize(height, width);
        rotatedTexture.SetPixels(rotatedPixels);
        rotatedTexture.Apply();
        m_CameraTexture.Apply();
        return rotatedTexture;
    }

    private Texture2D TextureCut(Texture2D tex, float ratio = 0.8f)
    {
        if (tex == null)
            return null;
        if (ratio <= 0)
            return tex;
        Color color;
        int width = (int)(tex.width * ratio);
        int height = (int)(tex.height * ratio);
        Texture2D newTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        float newWidthGap = width * ratio;
        float newHieghtGap = height * ratio;
        for (int i = 0; i < newTexture.height; i++)
        {
            for (int j = 0; j < newTexture.width; j++)
            {
                color = tex.GetPixel((int)(j * (1 / ratio)), (int)(i * (1 / ratio)));
                newTexture.SetPixel(j, i, color);
            }
        }
        return newTexture;
    }


    //创建文件夹
    IEnumerator CreatFolder(string FolderName)
    {
        WWWForm wForm = new WWWForm();
        wForm.AddField("FolderName", FolderName);
        WWW w = new WWW(BaseURL+ "/creatfolder", wForm);
        yield return w;
        if (w.isDone)
        {
            Debug.Log("创建文件夹完成");
        }
        w.Dispose();
    }

    [Obsolete]
    private IEnumerator UploadJustImage(byte[] imagedata, string name, Transform recordTranform)
    {

        // M1
        //Pose origin_pose = new Pose(recordTranform.position, recordTranform.rotation);
        //Debug.Log($"jjj origin_pose {origin_pose.position.x} {origin_pose.position.y} {origin_pose.position.z} {origin_pose.rotation.x} {origin_pose.rotation.y} {origin_pose.rotation.z} {origin_pose.rotation.w}");

        //Matrix4x4 matrix = ARTools.pose2Matrix(origin_pose, true);  //相机pose为世界坐标在相机坐标系下的位姿（取反）
        //Pose recal_pose = ARTools.matrix2Pose(matrix, true);
        //Debug.Log($"jjj recal_pose {recal_pose.position.x} {recal_pose.position.y} {recal_pose.position.z} {recal_pose.rotation.x} {recal_pose.rotation.y} {recal_pose.rotation.z} {recal_pose.rotation.w}");

        //matrix = matrix.inverse;
        //Pose reverse_pose = ARTools.matrix2Pose(matrix, true);
        //Debug.Log($"jjj reverse_pose {reverse_pose.position.x} {reverse_pose.position.y} {reverse_pose.position.z} {reverse_pose.rotation.x} {reverse_pose.rotation.y} {reverse_pose.rotation.z} {reverse_pose.rotation.w}");

        // M2
        //Matrix4x4 matrix = recordTranform.localToWorldMatrix;
        //Matrix4x4 reverse_matrix = matrix.inverse;
        //Pose reverse_pose = ARTools.matrix2Pose(reverse_matrix, true);

        //M3
        Pose origin_pose = new Pose(recordTranform.position, recordTranform.rotation);
        Pose reverse_pose = ARTools.getInversePose(origin_pose,true);
        arTransformer.addQueryList(img_idx,reverse_pose);


        BitArray myBA = new BitArray(imagedata);
        int c = myBA.Length;

        //Debug.Log("长度:" + c);
        WWWForm formData = new WWWForm();
        //formData.AddBinaryData("image", imagedata, name, "image/png");//图片              //#########################
        formData.AddBinaryData("image", imagedata, name, "image/jpg");//图片              //#########################
        formData.AddField("img_idx", img_idx.ToString());//图像序号
        img_idx += 1;

        UnityWebRequest request = UnityWebRequest.Post(BaseURL + "/upimage", formData);
        request.timeout = 10;
        yield return request.SendWebRequest();
        //使用www容易卡死，测试unityWebRequest
        //WWW www = new WWW(BaseURL + "/upimage", formData);
        //icount = icount + 1;
        ////Debug.Log("发送请求次数：" + icount+ "     icount值：" + icount);
        //text_result.text += "\n\t等待服务器返回数据...";
        //yield return www;
        
        //Debug.Log("xxx 返回数据：" + www.text);
        if (request.error != null)
        {
            //m_info = www.error;
            text_result.text = "Net Error:\n\t" + request.error;
            yield return null;
        }
        else
        {
            //m_info = www.text;
            text_result.text = "Net Result:\n\t" + request.downloadHandler.text;

            string receiveMsg = request.downloadHandler.text;



            string sendingMsg = arTransformer.arSceneTransform(ref receiveMsg);

            request.Dispose();//释放


            //返回信息
            WWWForm formData2 = new WWWForm();
            formData2.AddField("msg", sendingMsg);//位姿
            Debug.Log(sendingMsg);
            UnityWebRequest request2 = UnityWebRequest.Post(BaseURL + "/upmsg", formData2);
            request2.timeout = 10;
            yield return request2.SendWebRequest();
            if (request2.error != null)
            {
                yield return null;
            }
            request.Dispose();
        }       

        netRequst = true;
        //WWW www2 = new WWW(BaseURL + "/upmsg", formData2);
        //yield return www2;
        //if (www2.error != null)
        //{
        //    yield return null;
        //}
        //www2.Dispose();
    }

    //上传图片到指定的文件夹
    [Obsolete]
    private IEnumerator UploadimageAndpose(byte[] imagedata, string pose, string name)
    {
        //Debug.Log(pose + "andname" + name);
      
        BitArray myBA = new BitArray(imagedata);
        int c = myBA.Length;
       
        //Debug.Log("长度:"+c);

        // Create form data
        WWWForm formData = new WWWForm();
        formData.AddBinaryData("image",imagedata,name, "image/png");//图片
        formData.AddField("pose", pose);//位姿

        
        WWW www = new WWW(BaseURL+"/upload", formData);
        icount=icount+1;

        
        //Debug.Log("------------------------------------------------");
        //Debug.Log(icount);


        if (icount % 5 == 0)
        {
            //Debug.Log("发送请求次数：" + icount/5+"     icount值："+icount);
            yield return www;
           
        }
        // Send request

        if (www.error != null)
        {
            //m_info = www.error;
            text_result.text = "Net Result:\n\t" + www.error;
            yield return null;
        }
        //m_info = www.text;
        text_result.text = "Net Result:\n\t" + www.text;
        www.Dispose();//释放


    }

    IEnumerator DestoryUnusedTexture()
    {
        yield return new WaitForSeconds(0.1f);
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }


}