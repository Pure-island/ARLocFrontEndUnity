using DG.Tweening.Plugins.Core.PathCore;
using Google.XR.ARCoreExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;
using static UnityEngine.Networking.UnityWebRequest;

public class ARSceneRecorder : MonoBehaviour
{
    public string editorMP4Uri;
    ARRecordingManager recordingManager;
    ARPlaybackManager playbackManager;
    ARCoreRecordingConfig recordingConfig;
    ARSession session;
    Uri datasetUri;
    Uri datasetUri4Test;
    string AssetsPath;
    // Start is called before the first frame update
    void Start()
    {
        Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        Permission.RequestUserPermission(Permission.ExternalStorageRead);

        session = GameObject.FindObjectOfType<ARSession>();
        recordingManager = GetComponent<ARRecordingManager>();
        playbackManager = GetComponent<ARPlaybackManager>();

        recordingConfig = ScriptableObject.CreateInstance<ARCoreRecordingConfig>();
        AssetsPath = "/storage/emulated/0/DCIM/ARproject/videos/";
        if (!Directory.Exists(AssetsPath))
            Directory.CreateDirectory(AssetsPath);
        if(System.Uri.TryCreate($"file://{AssetsPath}{DateTime.Now.ToString("yyyy-MM-dd-HH-mm")}.mp4",UriKind.Absolute ,out datasetUri))
        {
            Debug.LogError("saving data to: " + datasetUri.ToString());
        }
        if (System.Uri.TryCreate($"file://{AssetsPath}test.mp4", UriKind.Absolute, out datasetUri4Test))
        {
            Debug.LogError("测试文件地址: " + datasetUri4Test.ToString());
        }
        recordingConfig.Mp4DatasetUri = datasetUri;        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartOrStopRecord()
    {
        if(recordingManager == null)
        {
            recordingManager = GameObject.FindObjectOfType<ARRecordingManager>();
        }

        if(recordingManager.RecordingStatus == RecordingStatus.None)
        {
            recordingManager.StartRecording(recordingConfig);
        }
        else if(recordingManager.RecordingStatus == RecordingStatus.IOError)
        {
            Debug.LogError("录制出现IOError");
        }
        else
        {
            recordingManager.StopRecording();
        }    
    }

    public void Playback(bool test)
    {
        StartCoroutine(PlaybackRecord(test));
    }

    private IEnumerator PlaybackRecord(bool test)
    {
#if UNITY_EDITOR
        //Editor暂时没法用
        System.Uri.TryCreate($"file:///{editorMP4Uri}", UriKind.Absolute, out datasetUri);
#endif
        if (!session)
        {
            session = GameObject.FindObjectOfType<ARSession>();
        }
        if (!playbackManager)
        {
            playbackManager = GameObject.FindObjectOfType<ARPlaybackManager>();
        }
        // Disable the ARSession to pause the current AR session.
        session.enabled = false;
        yield return new WaitForSeconds(0.1f);
        // In the next frame, provide a URI for the dataset you wish to play back.
        if(test)
        {
            while (playbackManager.SetPlaybackDatasetUri(datasetUri4Test) != PlaybackResult.OK)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            while (playbackManager.SetPlaybackDatasetUri(datasetUri) != PlaybackResult.OK)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        yield return new WaitForSeconds(0.1f);
        // In the frame after that, re-enable the ARSession to resume the session from
        // the beginning of the dataset.
        session.enabled = true;
    }
}
