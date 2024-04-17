using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using UnityEngine;

namespace DA_Assets.FCU
{
    internal class HeaderSection : ScriptableObjectBinder<FcuEditor, FigmaConverterUnity>
    {
        public void Draw()
        {
            gui.TopProgressBar(monoBeh.RequestSender.PbarProgress);

            GUILayout.BeginVertical(gui.Resources.FcuLogo, gui.GetStyle(GuiStyle.Logo));
            gui.Space60();
            GUILayout.EndVertical();

            gui.DrawGroup(new Group
            {
                GroupType = GroupType.Vertical,
                Body = () =>
                {
                    gui.SpecificSpace(gui.SPACE_30 * -0.8f);

                    UpdateChecker.DrawVersionLine(AssetType.fcu);
                    DrawImportInfoLine();
                    DrawCurrentProjectName();
                }
            });
        }

        public void DrawSmallHeader()
        {
            gui.DrawGroup(new Group
            {
                GroupType = GroupType.Vertical,
                Body = () =>
                {
                    DrawImportInfoLine();
                    DrawCurrentProjectName();
                }
            });
        }

        private void DrawImportInfoLine()
        {
            gui.DrawGroup(new Group
            {
                GroupType = GroupType.Horizontal,
                Body = () =>
                {
                    gui.FlexibleSpace();

                    gui.Label(new GUIContent($"{Mathf.Round(monoBeh.RequestSender.PbarBytes / 1024)} kB", FcuLocKey.label_kilobytes.Localize()), WidthType.Option, GuiStyle.Label10px);
                    gui.Space5();
                    gui.Label(new GUIContent("—"), WidthType.Option, GuiStyle.Label10px);

                    string userId = monoBeh.FigmaSession.CurrentFigmaUser.Id.SubstringSafe(10);
                    string userName = monoBeh.FigmaSession.CurrentFigmaUser.Name;

                    if (string.IsNullOrWhiteSpace(userName) == false)
                    {
                        gui.Space5();
                        gui.Label(new GUIContent(userName, FcuLocKey.label_user_name.Localize()), WidthType.Option, GuiStyle.Label10px);
                        gui.Space5();
                        gui.Label(new GUIContent("—"), WidthType.Option, GuiStyle.Label10px);
                    }
                    else if (string.IsNullOrWhiteSpace(userId) == false)
                    {
                        gui.Space5();
                        gui.Label(new GUIContent(userId, FcuLocKey.tooltip_user_id.Localize()), WidthType.Option, GuiStyle.Label10px);
                        gui.Space5();
                        gui.Label(new GUIContent("—"), WidthType.Option, GuiStyle.Label10px);
                    }

                    gui.Space5();
                    gui.Label(new GUIContent(monoBeh.Guid, FcuLocKey.tooltip_asset_instance_id.Localize()), WidthType.Option, GuiStyle.Label10px);
                }
            });
        }
        private void DrawCurrentProjectName()
        {
            string currentProjectName = monoBeh.CurrentProject.FigmaProject.Name;

            if (currentProjectName != null)
            {
                gui.DrawGroup(new Group
                {
                    GroupType = GroupType.Horizontal,
                    Body = () =>
                    {
                        gui.FlexibleSpace();
                        gui.Label10px(currentProjectName, widthType: WidthType.Option);
                    }
                });
            }
        }
    }
}
