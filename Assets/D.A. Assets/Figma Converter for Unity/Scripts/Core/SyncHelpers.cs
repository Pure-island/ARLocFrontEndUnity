using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DA_Assets.FCU
{
    [Serializable]
    public class SyncHelpers : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public IEnumerator DestroySyncHelpers()
        {
            SyncHelper[] syncHelpers = GetAllSyncHelpers();

            for (int i = 0; i < syncHelpers.Length; i++)
            {
                syncHelpers[i].Destroy();
                yield return null;
            }

            DALogger.Log(FcuLocKey.log_current_canvas_metas_destroy.Localize(
                monoBeh.GetInstanceID(),
                syncHelpers.Length,
                nameof(SyncHelper)));
        }

        public bool IsExistsOnCurrentCanvas(FObject fobject, out SyncHelper syncObject)
        {
            SyncHelper[] syncHelpers = GetAllSyncHelpers();

            foreach (SyncHelper sh in syncHelpers)
            {
                if (sh.Data.Id == fobject.Id)
                {
                    syncObject = sh;
                    return true;
                }
            }

            syncObject = null;
            return false;
        }


        public SyncHelper[] GetAllSyncHelpers()
        {
            try
            {
                SyncHelper[] onSceneSyncHelpers = null;
#if UNITY_2020_1_OR_NEWER
                onSceneSyncHelpers = MonoBehaviour.FindObjectsOfType<SyncHelper>(true);
#else
                onSceneSyncHelpers = Resources.FindObjectsOfTypeAll<SyncHelper>();
#endif
                SyncHelper[] currentInstanceHelpers = onSceneSyncHelpers
                    .Where(x => x.Data.FigmaConverterUnity.GetInstanceID() == monoBeh.GetInstanceID())
                    .ToArray();

                return currentInstanceHelpers;
            }
            catch
            {
                return new List<SyncHelper>().ToArray();
            }
        }

        public IEnumerator SetFcuToAllSyncHelpers()
        {
            int counter = 0;
            SetFcuToAllChilds(monoBeh.gameObject, ref counter);

            yield return WaitFor.Delay01();

            DALogger.Log(FcuLocKey.log_fcu_assigned.Localize(
                counter,
                nameof(FigmaConverterUnity),
                monoBeh.GetInstanceID()));
        }

        public void SetFcuToAllChilds(GameObject @object, ref int counter)
        {
            if (@object == null)
                return;

            foreach (Transform child in @object.transform)
            {
                if (child == null)
                    continue;

                if (child.TryGetComponent(out SyncHelper syncObject))
                {
                    counter++;
                    syncObject.Data.FigmaConverterUnity = monoBeh;
                }

                SetFcuToAllChilds(child.gameObject, ref counter);
            }
        }
    }
}