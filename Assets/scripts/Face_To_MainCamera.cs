using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face_To_MainCamera : MonoBehaviour
{
    Transform facing_camera_transform;
    Vector3 facing_camera_localpos;

    public bool reverse = false;
    // Start is called before the first frame update
    void Start()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("MainCamera");
        if (obj)
        {
            facing_camera_transform = obj.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        facing_camera_localpos = this.transform.worldToLocalMatrix.MultiplyPoint3x4(facing_camera_transform.position);
        facing_camera_localpos.y = 0;
        Quaternion q;
        //Debug.Log(facing_camera_localpos);
        if (reverse)
        {
            q = Quaternion.LookRotation(this.transform.position - this.transform.localToWorldMatrix.MultiplyPoint3x4(facing_camera_localpos), this.transform.up);
        }
        else
        {
            q = Quaternion.LookRotation(this.transform.localToWorldMatrix.MultiplyPoint3x4(facing_camera_localpos) - this.transform.position, this.transform.up);
        }
        
        this.transform.rotation = q;


    }
}
