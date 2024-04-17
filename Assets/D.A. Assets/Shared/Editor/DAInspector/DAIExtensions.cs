using DA_Assets.Shared;
using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace DA_Assets.Shared
{
    public class ScriptableObjectBinder<T1, T3>
        where T1 : ScriptableObject
        where T3 : MonoBehaviour
    {
        public DAInspector gui => DAInspector.Instance;

        protected T1 scriptableObject;
        public T1 ScriptableObject { get => scriptableObject; set => scriptableObject = value; }

        protected T3 monoBeh;
        public T3 MonoBeh { get => monoBeh; set => monoBeh = value; }

        public virtual void Init() { }
    }

    public static class DAIExtensions
    {
        public static T2 Bind<T1, T2, T3>(this T2 binder, T1 scriptableObject, T3 monoBeh)
            where T1 : ScriptableObject
            where T2 : ScriptableObjectBinder<T1, T3>
            where T3 : MonoBehaviour
        {
            bool needInit = false;

            if (binder == null)
            {
                binder = (T2)Activator.CreateInstance(typeof(T2));
                needInit = true;
            }

            if (binder.ScriptableObject == null)
                binder.ScriptableObject = scriptableObject;
            if (binder.MonoBeh == null)
                binder.MonoBeh = monoBeh;

            if (needInit)
            {
                binder.Init();
            }

            return binder;
        }

        public static T1 InitWindow<T1, T2, T3>(this T1 daiWindow, T2 inspector, T3 monoBeh, Vector2 windowSize)
            where T1 : DAInspectorWindow<T1, T2, T3>
            where T2 : Editor
            where T3 : MonoBehaviour
        {
            if (daiWindow == null)
            {
                daiWindow = ScriptableObject.CreateInstance<T1>();
            }

            if (daiWindow.Inspector == null)
            {
                daiWindow.Inspector = inspector;
            }

            if (daiWindow.MonoBeh == null)
            {
                daiWindow.MonoBeh = monoBeh;
            }

            if (daiWindow.SerializedObject == null)
            {
                daiWindow.SerializedObject = new SerializedObject(monoBeh);
            }

            daiWindow.WindowSize = windowSize;
            daiWindow.Init = true;

            return daiWindow;
        }

        public static async void StartUiRepaint(this UnityEditor.Editor editor)
        {
            while (true)
            {
                if (editor == null)
                    break;

                if (editor.target ==null)
                    break;

                if (UnityEditor.Selection.Contains((editor.target as MonoBehaviour).gameObject))
                    editor.Repaint();

                await Task.Delay((int)(WaitFor.Delay001().WaitTimeF * 1000));
            }
        }
    }
}
