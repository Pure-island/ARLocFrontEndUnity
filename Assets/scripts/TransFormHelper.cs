using UnityEngine;

public class TransFormHelper
{
    //(父物体变换组件，子物体名称)
    public static Transform GetChild(Transform TF, string childName)
    {
        Transform childTF = TF.Find(childName);//查找名字为childName的子物体
        if (childTF != null)
        {
            return childTF;
        }
        if (TF.childCount == 0)
        {
            return null;
        }
        for (int i = 0; i < TF.childCount; i++)
        {
            childTF = TF.GetChild(i);
            childTF = GetChild(childTF, childName);

            if (childTF != null)
            {
                return childTF;
            }
        }
        return null;
    }



}