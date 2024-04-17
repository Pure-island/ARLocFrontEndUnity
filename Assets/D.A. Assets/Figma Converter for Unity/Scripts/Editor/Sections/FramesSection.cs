using DA_Assets.Shared;
using System.Linq;
using UnityEngine;

namespace DA_Assets.FCU
{
    internal class FramesSection : ScriptableObjectBinder<FcuEditor, FigmaConverterUnity>
    {
        public void Draw()
        {
            int framesCount = monoBeh.InspectorDrawer.SelectableFrames.Count();
            int framesToDownloadCount = monoBeh.InspectorDrawer.SelectableFrames.Where(x => x.Selected == true).Count();

            int index = monoBeh.InspectorDrawer.SelectableHamburgerItems.FindIndex(row => row.Id == HamburgerMenuId.FrameListKey.ToString());

            if (index == -1)
            {

            }
            else if (framesToDownloadCount == 0)
            {
                monoBeh.InspectorDrawer.SelectableHamburgerItems[index].CheckBoxValue.Value = false;
            }
            else if (framesToDownloadCount == framesCount)
            {
                monoBeh.InspectorDrawer.SelectableHamburgerItems[index].CheckBoxValue.Value = true;
            }
            else if (framesToDownloadCount != framesCount)
            {
                monoBeh.InspectorDrawer.SelectableHamburgerItems[index].CheckBoxValue.Value = false;
                monoBeh.InspectorDrawer.SelectableHamburgerItems[index].CheckBoxValue.Temp = false;
            }
        
            gui.DrawMenu(monoBeh.InspectorDrawer.SelectableHamburgerItems, new HamburgerItem
            {
                Id = HamburgerMenuId.FrameListKey.ToString(),
                GUIContent = new GUIContent(FcuLocKey.label_frames_to_import.Localize(framesToDownloadCount, framesCount), ""),
                Body = () =>
                {
                    var pages = monoBeh.InspectorDrawer.SelectableFrames.GroupBy(x => new { x.ParentId, x.ParentName });

                    foreach (var page in pages)
                    {
                        int currentPageFramesSelectedCount = monoBeh.InspectorDrawer.SelectableFrames
                            .Where(x => x.ParentId == page.Key.ParentId)
                            .Where(x => x.Selected == true)
                            .Count();

                        int _index = monoBeh.InspectorDrawer.SelectableHamburgerItems.FindIndex(row => row.Id == page.Key.ParentId);

                        if (_index == -1)
                        {

                        }
                        else if (currentPageFramesSelectedCount == 0)
                        {
                            monoBeh.InspectorDrawer.SelectableHamburgerItems[_index].CheckBoxValue.Value = false;
                        }
                        else if (framesToDownloadCount == framesCount)
                        {
                            monoBeh.InspectorDrawer.SelectableHamburgerItems[_index].CheckBoxValue.Value = true;
                        }
                    
                        gui.DrawMenu(monoBeh.InspectorDrawer.SelectableHamburgerItems, new HamburgerItem
                        {
                            GUIContent = new GUIContent($"{page.Key.ParentName} ({currentPageFramesSelectedCount}/{page.Count()})", ""),
                            Id = page.Key.ParentId,
                            Body = () =>
                            {
                                for (int i = 0; i < page.Count(); i++)
                                {
                                    page.ToList()[i].Selected =
                                        gui.CheckBox(new GUIContent(page.ToList()[i].Name), page.ToList()[i].Selected);
                                }
                            },
                            CheckBoxValueChanged = (menuId, value) =>
                            {
                                for (int i = 0; i < monoBeh.InspectorDrawer.SelectableFrames.Count(); i++)
                                {
                                    if (monoBeh.InspectorDrawer.SelectableFrames[i].ParentId == menuId)
                                    {
                                        monoBeh.InspectorDrawer.SelectableFrames[i].Selected = value;
                                    }
                                }
                            }
                        });
                    }
                },
                CheckBoxValueChanged = (menuId, value) =>
                {
                    for (int i = 0; i < monoBeh.InspectorDrawer.SelectableFrames.Count(); i++)
                    {
                        monoBeh.InspectorDrawer.SelectableFrames[i].Selected = value;
                    }
                }
            });
        }
    }
}