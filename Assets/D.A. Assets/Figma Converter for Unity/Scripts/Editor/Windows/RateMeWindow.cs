using DA_Assets.Shared;
using UnityEditor;
using UnityEngine;

#pragma warning disable CS0162

namespace DA_Assets.FCU
{
    internal class RateMeWindow : EditorWindow
    {
        private static Vector2 windowSize = new Vector2(400, 260);
        private static int dayCount;
        private static int maxPercent = 1;

        private DAInspector gui => DAInspector.Instance;

        public static void Show(int errorCount, int componentCount)
        {
            return;
            float percent = (float)errorCount / (float)componentCount * 100f;

            //Debug.Log($"RateMeWindow | {errorCount} | {componentCount} | {percent}");

            if (percent > maxPercent)
                return;

            bool shown = EditorPrefs.GetBool(FcuConfig.Instance.RateMePrefsKey, false);

            if (shown)
                return;

            RateMeWindow win = GetWindow<RateMeWindow>(FcuLocKey.label_rateme.Localize());

            dayCount = GetFirstVersionDaysCount();

            if (dayCount == -1)
                return;

            win.maxSize = windowSize;
            win.minSize = windowSize;
            win.position = new Rect(
                (Screen.currentResolution.width - windowSize.x * 2) / 2,
                (Screen.currentResolution.height - windowSize.y * 2) / 2,
                windowSize.x,
                windowSize.y);

            win.Show();

            EditorPrefs.SetBool(FcuConfig.Instance.RateMePrefsKey, true);
        }

        private void OnGUI()
        {
            gui.DrawGroup(new Group
            {
                GroupType = GroupType.Vertical,
                Style = GuiStyle.TabBg1,
                Body = () =>
                {
                    GUILayout.BeginVertical(gui.Resources.FcuLogo, gui.GetStyle(GuiStyle.Logo));
                    gui.Space60();
                    GUILayout.EndVertical();
                    gui.SpecificSpace(-19);

                    gui.DrawGroup(new Group
                    {
                        GroupType = GroupType.Horizontal,
                        Body = () =>
                        {
                            gui.FlexibleSpace();

                            for (int i = 0; i < 5; i++)
                            {
                                DrawStar();

                                if (i != 5)
                                {
                                    gui.Space5();
                                }
                            }

                            gui.FlexibleSpace();
                        }
                    });

                    gui.Space10();

                    //gui.Label(new GUIContent(FcuLocKey.label_rateme_desc.Localize(dayCount)), WidthType.Expand, GuiStyle.LabelCentered12px);

                    gui.Space15();

                    gui.DrawGroup(new Group
                    {
                        GroupType = GroupType.Horizontal,
                        Body = () =>
                        {
                            gui.FlexibleSpace();

                            if (gui.OutlineButton(FcuLocKey.label_rate.Localize()))
                            {
                                Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/198134#reviews");
                            }

                            gui.FlexibleSpace();
                        }
                    });
                }
            });
        }


        private static int GetFirstVersionDaysCount()
        {
            return -1;
            /*try
            {
                Asset assetInfo = UpdateChecker.WebConfig.Assets.First(x => x.Type == AssetType.fcu);
                AssetVersion last = assetInfo.Versions.First();

                DateTime lastDt = DateTime.ParseExact(last.ReleaseDate, "MMM d, yyyy", new System.Globalization.CultureInfo("en-US"));

                int dc = (int)Mathf.Abs((float)(DateTime.Now - lastDt).TotalDays);



                return dc;
            }
            catch
            {
                return -1;
            }*/
        }

        private void DrawStar()
        {
            GUILayout.Box(gui.Resources.ImgStar, gui.GetStyle(GuiStyle.ImgStar));
        }
    }
}