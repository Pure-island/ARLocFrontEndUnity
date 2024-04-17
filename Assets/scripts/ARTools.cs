using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARTools : MonoBehaviour
{
    public static bool printLogMsg = true;
    public static bool printErrorMsg = true;

    public static void PrintError(string msg)
    {
        //输出错误信息
        if (printErrorMsg)
        {
            Debug.LogError(msg);
        }
    }

    public static void PrintLog(string msg)
    {
        //输出调试信息
        if (printLogMsg)
        {
            Debug.Log(msg);
        }
    }

    public static Pose matrix2Pose(Matrix4x4 mat, bool rightHand = true)
    {
        Pose pose = new Pose();
        pose.position.x = mat.m03;
        pose.position.y = mat.m13;
        pose.position.z = mat.m23;

        pose.rotation.w = Mathf.Sqrt(mat.m00 + mat.m11 + mat.m22 + 1) / 2;
        if (rightHand)
        {
            pose.rotation.x = (mat.m21 - mat.m12) / (4 * pose.rotation.w);
            pose.rotation.y = (mat.m02 - mat.m20) / (4 * pose.rotation.w);
            pose.rotation.z = (mat.m10 - mat.m01) / (4 * pose.rotation.w);
        }
        //else      ==》存在错误
        //{
        //    pose.rotation.x = (mat.m12 - mat.m21) / (4 * pose.rotation.w);
        //    pose.rotation.y = (mat.m20 - mat.m02) / (4 * pose.rotation.w);
        //    pose.rotation.z = (mat.m01 - mat.m10) / (4 * pose.rotation.w);
        //}

        return pose;
    }

    public static Matrix4x4 pose2Matrix(Pose pose, bool rightHand = true)
    {
        Matrix4x4 mat = new Matrix4x4();

        float tx = pose.position.x;
        float ty = pose.position.y;
        float tz = pose.position.z;
        float qx = pose.rotation.x;
        float qy = pose.rotation.y;
        float qz = pose.rotation.z;
        float qw = pose.rotation.w;

        //右手系变换
        if (rightHand)
        {
            mat.m00 = 1 - 2 * qy * qy - 2 * qz * qz;
            mat.m01 = 2 * qx * qy - 2 * qw * qz;
            mat.m02 =  2 * qx * qz + 2 * qw * qy;
            mat.m03 = tx;

            mat.m10 = 2 * qx * qy + 2 * qw * qz;
            mat.m11 = 1 - 2 * qx * qx - 2 * qz * qz;
            mat.m12 = 2 * qy * qz - 2 * qw * qx;
            mat.m13 = ty;

            mat.m20 = 2 * qx * qz - 2 * qw * qy;
            mat.m21 = 2 * qy * qz + 2 * qw * qx;
            mat.m22 = 1 - 2 * qx * qx - 2 * qy * qy;
            mat.m23 = tz;

            mat.m30 = 0;
            mat.m31 = 0;
            mat.m32 = 0;
            mat.m33 = 1;
        }
        //左手系变换         ==》存在错误
        //else
        //{
        //    mat.m00 = 1 - 2 * qy * qy - 2 * qz * qz;
        //    mat.m01 = 2 * qx * qy + 2 * qw * qz;
        //    mat.m02 = 2 * qx * qz - 2 * qw * qy;
        //    mat.m03 = tx;

        //    mat.m10 = 2 * qx * qy - 2 * qw * qz;
        //    mat.m11 = 1 - 2 * qx * qx - 2 * qz * qz;
        //    mat.m12 = 2 * qy * qz + 2 * qw * qx;
        //    mat.m13 = ty;

        //    mat.m20 = 2 * qx * qz + 2 * qw * qy;
        //    mat.m21 = 2 * qy * qz - 2 * qw * qx;
        //    mat.m22 = 1 - 2 * qx * qx - 2 * qy * qy;
        //    mat.m23 = tz;

        //    mat.m30 = 0;
        //    mat.m31 = 0;
        //    mat.m32 = 0;
        //    mat.m33 = 1;
        //}

        return mat;
    }

    public static UnityEngine.Matrix4x4 systemMat2UnityMat(System.Numerics.Matrix4x4 systemMat)
    {
        UnityEngine.Matrix4x4 unityMat = UnityEngine.Matrix4x4.identity;
        unityMat.m00 = systemMat.M11;
        unityMat.m01 = systemMat.M12;
        unityMat.m02 = systemMat.M13;
        unityMat.m03 = systemMat.M14;

        unityMat.m10 = systemMat.M21;
        unityMat.m11 = systemMat.M22;
        unityMat.m12 = systemMat.M23;
        unityMat.m13 = systemMat.M24;

        unityMat.m20 = systemMat.M31;
        unityMat.m21 = systemMat.M32;
        unityMat.m22 = systemMat.M33;
        unityMat.m23 = systemMat.M34;

        unityMat.m30 = systemMat.M41;
        unityMat.m31 = systemMat.M42;
        unityMat.m32 = systemMat.M43;
        unityMat.m33 = systemMat.M44;

        return unityMat;
    }
    //右手系位姿转左手系表示
    public static float[] str2Vec(string str, char splitChar = ' ',int certain_length = 0)
    {
        string[] vecStr = str.Trim().Split(splitChar);
        if (certain_length > 1 && certain_length != vecStr.Length)
        {
            ARTools.PrintError("FAULT: certain_length != vec_str.Length");
            return new float[] { };
        }

        float[] vecFloat = new float[vecStr.Length];
        for (int i = 0; i < vecStr.Length; i++)
        {
            if (!float.TryParse(vecStr[i], out vecFloat[i]))
            {
                vecFloat[i] = 0.0f;
                ARTools.PrintError("FAULT: float.TryParse(vec_str[" + i + "]");
            }
   
        }
        return vecFloat;
    }

    public static string bytes2Str(ref byte[] dataBytes)
    {
        return System.Text.Encoding.ASCII.GetString(dataBytes, 0, dataBytes.Length);
    }
    public static Pose vec2Pose(float[] vec, bool changeChirality = false)
    {
        //位姿vec:(tx, ty, tz, qx, qy, qz, qw)
        if (vec.Length != 7)
        {
            ARTools.PrintError("FAULT: vec.Length != 7");
            return Pose.identity;
        }

        Vector3 pos = new Vector3(vec[0], vec[1], vec[2]);
        Quaternion quat = new Quaternion(vec[3], vec[4], vec[5], vec[6]);

        Pose pose = new Pose(pos, quat);
        if (changeChirality)
        {
            pose = poseChangeChirality(pose);
        }
        return pose;
    }

    public static string matrix2Str(ref Matrix4x4 mat)
    {
        string str = "";
        str += mat.m00 + " " + mat.m01 + " " + mat.m02 + " " + mat.m03 + " ";
        str += mat.m10 + " " + mat.m11 + " " + mat.m12 + " " + mat.m13 + " ";
        str += mat.m20 + " " + mat.m21 + " " + mat.m22 + " " + mat.m23 + " ";
        str += mat.m30 + " " + mat.m31 + " " + mat.m32 + " " + mat.m33;

        return str;
    }

    public static string pose2Str(ref Pose pose)
    {
        return $"{pose.position.x} {pose.position.y} {pose.position.z} {pose.rotation.x} {pose.rotation.y} {pose.rotation.z} {pose.rotation.w}";
    }

    //public static Pose getInversePose(Pose pose,bool rightHand=true)      # 可逆，但不可与手性变换交叉使用
    //{
    //    Matrix4x4 mat = ARTools.pose2Matrix(pose, rightHand);
    //    mat = mat.inverse;
    //    return ARTools.matrix2Pose(mat, rightHand);
    //}

    //完全使用右手系进行变换
    public static Pose getInversePose(Pose pose, bool rightHand = true)
    {
        Matrix4x4 mat = ARTools.pose2Matrix(pose, true);
        mat = mat.inverse;
        return ARTools.matrix2Pose(mat, true);
    }

    public static Pose poseChangeChirality(Pose pose)
    {
        return new Pose(
            ARTools.positionChangeChiralityByReverseY(pose.position),
            ARTools.quaternionChangeChiralityByReverseY(pose.rotation));
    }

    //ChangeChiralityByY
    public static Matrix4x4 matrixChangeChirality(Matrix4x4 mat)
    {
        Matrix4x4 new_mat = mat;
        //rotation change
        new_mat.m01 *= -1;
        new_mat.m10 *= -1;
        new_mat.m12 *= -1;
        new_mat.m21 *= -1;
        //position change
        new_mat.m13 *= -1;

        return new_mat;
    }

    public static Vector3 positionChangeChiralityByReverseY(Vector3 Pos)
    {
        return new Vector3(Pos.x, -Pos.y, Pos.z);
    }

    public static Quaternion quaternionChangeChiralityByReverseY(Quaternion Quat)
    {
        return new Quaternion(-Quat.x, Quat.y, -Quat.z, Quat.w);
    }

    public static Vector3 vector3ComponentMultiply(Vector3 vec1,Vector3 vec2)
    {
        return new Vector3(vec1.x * vec2.x, vec1.y * vec2.y, vec1.z * vec2.z);
    }

    public class QueryNode
    {

        public QueryNode(int img_id, Pose img_pose)
        {
            pose = img_pose;
            id = img_id;
            timestamp = 0f;
        }

        public QueryNode(int img_id, Pose img_pose, float time_stamp)
        {
            pose = img_pose;
            id = img_id;
            timestamp = time_stamp;
        }

        public Pose pose { get; set; }
        public int id { get; set; }
        public float timestamp { get; set; }
    }


    [System.Serializable]
    public class ModelInfo
    {
        public int id;
        public string filepath;
        public string suffix;
        public float[] position;
        public float[] rotation;
        public float[] scale;
        public bool rightHand;

        public ModelInfo(int _id,string _filepath,string _suffix, Vector3 _position,Quaternion _rotation,Vector3 _scale, bool _rightHand)
        {
            id = _id;
            filepath = _filepath;
            suffix = _suffix;
            position = new float[] { _position.x, _position.y, _position.z };
            rotation = new float[] { _rotation.x, _rotation.y, _rotation.z, _rotation.w };
            scale = new float[] { _scale.x, _scale.y, _scale.z };
            rightHand = _rightHand;
        }

    }

    [System.Serializable]
    public class ModelInfoList
    {
        public List<ModelInfo> model_config;
    }

}
