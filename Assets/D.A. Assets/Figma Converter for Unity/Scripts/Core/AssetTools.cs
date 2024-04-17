using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections;
using UnityEngine;

namespace DA_Assets.FCU
{
    [Serializable]
    public class AssetTools : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public IEnumerator DestroyChilds()
        {
            int count = monoBeh.transform.ClearChilds();
            DALogger.Log(FcuLocKey.log_current_canvas_childs_destroy.Localize(monoBeh.Guid, count));
            yield return null;
        }

        public IEnumerator DestroyLastImportedFrames()
        {
            foreach (var item in monoBeh.CurrentProject.LastImportedFrames)
            {
                item.GameObject.Destroy();
            }

            monoBeh.CurrentProject.LastImportedFrames.Clear();
            yield return null;
        }

        public static void CreateFcuOnScene()
        {
            GameObject go = MonoBehExtensions.CreateEmptyGameObject();

            go.TryAddComponent(out FigmaConverterUnity fcu);
            go.name = string.Format(FcuConfig.Instance.CanvasGameObjectName, fcu.Guid);

            fcu.CanvasDrawer.AddCanvasComponent();
        }

        public void StopImport()
        {
            monoBeh.StopDARoutines();
        }
    }
}