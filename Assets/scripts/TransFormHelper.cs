using UnityEngine;

public class TransFormHelper
{
    //(������任���������������)
    public static Transform GetChild(Transform TF, string childName)
    {
        Transform childTF = TF.Find(childName);//��������ΪchildName��������
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