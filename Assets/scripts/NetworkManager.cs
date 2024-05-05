using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class NetworkManager : MonoBehaviour
{
    // 发送GET请求
    public IEnumerator SendGET(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                Debug.Log("Received: " + request.downloadHandler.text);
            }
        }
    }

    // 发送POST请求，发送数据需要通过WWWForm
    
    public IEnumerator SendPOST(string url, WWWForm formData, Action<string> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest request = UnityWebRequest.Post(url, formData))
        {
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError("Error: " + request.error);
                onError?.Invoke(request.error);
            }
            else
            {
                Debug.Log("Received: " + request.downloadHandler.text);
                onSuccess?.Invoke(request.downloadHandler.text);
            }
        }
    }

}
