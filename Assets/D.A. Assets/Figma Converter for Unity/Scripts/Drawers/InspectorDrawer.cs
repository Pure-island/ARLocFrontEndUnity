using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DA_Assets.FCU.Drawers
{
    [Serializable]
    public class InspectorDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public IEnumerator FillSelectableFramesArray(bool fromCache)
        {
            List<SelectableItem> newItems = new List<SelectableItem>();

            foreach (FObject page in monoBeh.CurrentProject.FigmaProject.Document.Children)
            {
                for (int i = 0; i < page.Children.Count; i++)
                {
                    FObject frame = page.Children[i];
                    frame.Data = new SyncData();

                    if (frame.Type == FcuTag.Frame.ToUpper())
                    {
                        frame.AddTag(FcuTag.Frame);

                        newItems.Add(new SelectableItem
                        {
                            Id = frame.Id,
                            Name = frame.Name,
                            Selected = true,
                            ParentId = $"{page.Id}_{page.Name}",
                            ParentName = page.Name,
                        });
                    }

                    page.Children[i] = frame;
                    yield return null;
                }
            }

            var newItemsIds = newItems.Select(x => x.Id);
            var oldItemsIds = selectableFrames.Select(x => x.Id);
            bool equals = newItemsIds.SequenceEqual(oldItemsIds);

            if (equals)
            {
                for (int i = 0; i < selectableFrames.Count(); i++)
                {
                    selectableFrames[i].Name = newItems[i].Name;
                    selectableFrames[i].ParentId = newItems[i].ParentId;
                    selectableFrames[i].ParentName = newItems[i].ParentName;
                }
            }
            else
            {
                selectableFrames = newItems;
            }

            if (fromCache == false && selectableFrames.Count() == 1)
            {
                selectableFrames[0].Selected = true;
                yield return monoBeh.ProjectImporter.StartImport();
            }
            else if (selectableFrames.Count > 0)
            {
                DALogger.Log(FcuLocKey.log_frames_finded.Localize(selectableFrames.Count));
            }
            else
            {
                DALogger.LogError(FcuLocKey.log_frames_not_found.Localize());
            }
        }

        public List<HamburgerItem> SelectableHamburgerItems = new List<HamburgerItem>();
        [SerializeField] List<SelectableItem> selectableFrames = new List<SelectableItem>();

        public List<SelectableItem> SelectableFrames { get => selectableFrames; set => SetValue(ref selectableFrames, value); }
    }
}
