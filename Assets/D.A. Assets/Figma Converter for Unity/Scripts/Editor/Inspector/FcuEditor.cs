using DA_Assets.FCU.Extensions;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#pragma warning disable IDE0003
#pragma warning disable CS0649

namespace DA_Assets.FCU
{
    [CustomEditor(typeof(FigmaConverterUnity)), CanEditMultipleObjects]
    internal class FcuEditor : DaiEditor<FcuEditor, FigmaConverterUnity>
    {
        public override void OnShow()
        {
            monoBeh.DelegateHolder.ShowRateMe = RateMeWindow.Show;
            monoBeh.FigmaSession.TryRestoreSession();
            monoBeh.ProjectCacher.TryRestoreProjectFromCache(monoBeh.Settings.MainSettings.ProjectUrl);
        }

        public void DrawBaseOnInspectorGUI()
        {
            gui.DrawGroup(new Group
            {
                GroupType = GroupType.Vertical,
                DarkBg = true,
                Body = () => base.OnInspectorGUI()
            });
        }


        public override void OnInspectorGUI()
        {
            if (monoBeh.Settings.MainSettings.WindowMode)
            {
                DrawWindowedGUI();
            }
            else
            {
                DrawGUI(GuiStyle.Background);
            }
        }

        public void DrawWindowedGUI()
        {
            gui.DrawGroup(new Group
            {
                GroupType = GroupType.Vertical,
                Style = GuiStyle.Background,
                Body = () =>
                {
                    gui.TopProgressBar(monoBeh.RequestSender.PbarProgress);

                    gui.DrawGroup(new Group
                    {
                        GroupType = GroupType.Horizontal,
                        Body = () =>
                        {
                            this.Header.DrawSmallHeader();

                            gui.Space15();

                            if (gui.SquareButton30x30(new GUIContent(gui.Resources.IconOpen, FcuLocKey.tooltip_open_fcu_window.Localize())))
                            {
                                this.SettingsWindow.Show();
                            }

                            gui.Space5();

                            if (gui.SquareButton30x30(new GUIContent(gui.Resources.IconExpandWindow, FcuLocKey.tooltip_change_window_mode.Localize())))
                            {
                                if (monoBeh.Settings.MainSettings.WindowMode)
                                {
                                    monoBeh.Settings.MainSettings.WindowMode = false;
                                }
                            }
                        }
                    });
                }
            });
        }

        public void DrawGUI(GuiStyle customStyle)
        {
            gui.DrawGroup(new Group
            {
                GroupType = GroupType.Vertical,
                Style = customStyle,
                Body = () =>
                {
                    this.Header.Draw();

                    gui.Space15();

                    gui.DrawGroup(new Group
                    {
                        GroupType = GroupType.Horizontal,
                        Body = () =>
                        {
                            monoBeh.Settings.MainSettings.ProjectUrl = gui.BigTextField(monoBeh.Settings.MainSettings.ProjectUrl);

                            gui.Space5();

                            Group gr = new Group();

                            if (monoBeh.Settings.MainSettings.WindowMode)
                            {
                                gr.Style = GuiStyle.Group5Buttons;
                            }
                            else
                            {
                                gr.Style = GuiStyle.Group6Buttons;
                            }

                            gr.GroupType = GroupType.Horizontal;
                            gr.Body = () =>
                            {
                                if (gui.SquareButton30x30(new GUIContent(gui.Resources.ImgViewRecent, FcuLocKey.tooltip_recent_projects.Localize())))
                                {
                                    ShowRecentProjectsPopup_OnClick();
                                }

                                gui.Space5();
                                if (gui.SquareButton30x30(new GUIContent(gui.Resources.IconDownload, FcuLocKey.tooltip_download_project.Localize())))
                                {
                                    if (monoBeh.FigmaSession.IsAuthed() == false)
                                    {
                                        DALogger.Log(FcuLocKey.log_not_authorized.Localize());
                                    }
                                    else if (monoBeh.Settings.MainSettings.ProjectUrl.IsEmpty())
                                    {
                                        DALogger.Log(FcuLocKey.log_incorrent_project_url.Localize());
                                    }
                                    else
                                    {
                                        monoBeh.EventHandlers.DownloadProject_OnClick();
                                    }
                                }

                                gui.Space5();
                                if (gui.SquareButton30x30(new GUIContent(gui.Resources.IconImport, FcuLocKey.tooltip_import_frames.Localize())))
                                {
                                    monoBeh.EventHandlers.ImportSelectedFrames_OnClick();
                                }

                                gui.Space5();
                                if (gui.SquareButton30x30(new GUIContent(gui.Resources.IconStop, FcuLocKey.tooltip_stop_import.Localize())))
                                {
                                    monoBeh.EventHandlers.StopImport_OnClick();
                                    DALogger.Log(FcuLocKey.label_import_stoped_manually.Localize());
                                }

                                if (monoBeh.Settings.MainSettings.WindowMode == false)
                                {
                                    gui.Space5();
                                    if (gui.SquareButton30x30(new GUIContent(gui.Resources.IconSettings, FcuLocKey.tooltip_open_settings_window.Localize())))
                                    {
                                        this.SettingsWindow.Show();
                                    }
                                }

                                gui.Space5();
                                if (gui.SquareButton30x30(new GUIContent(gui.Resources.IconExpandWindow, FcuLocKey.tooltip_change_window_mode.Localize())))
                                {
                                    if (monoBeh.Settings.MainSettings.WindowMode)
                                    {
                                        monoBeh.Settings.MainSettings.WindowMode = false;
                                        this.SettingsWindow.CreateTabs();
                                    }
                                    else
                                    {
                                        monoBeh.Settings.MainSettings.WindowMode = true;
                                        this.SettingsWindow.Show();
                                    }
                                }
                            };

                            gui.DrawGroup(gr);
                        }
                    });

                    gui.Space6();

                    if (monoBeh.CurrentProject.FigmaProject.IsProjectEmpty() == false && monoBeh.InspectorDrawer.SelectableFrames.IsEmpty() == false)
                    {
                        this.FrameList.Draw();
                    }

                    Footer.DrawFooter();
                }
            });
        }
        private void ShowRecentProjectsPopup_OnClick()
        {
            List<CacheMeta> recentProjects = monoBeh.ProjectCacher.GetAll();

            List<GUIContent> options = new List<GUIContent>();

            if (recentProjects.IsEmpty())
            {
                options.Add(new GUIContent(FcuLocKey.label_no_recent_projects.Localize()));
            }
            else
            {
                foreach (CacheMeta project in recentProjects)
                {
                    options.Add(new GUIContent(project.Name));
                }
            }

            EditorUtility.DisplayCustomMenu(new Rect(11, 150, 0, 0), options.ToArray(), -1, (userData, ops, selected) =>
            {
                monoBeh.Settings.MainSettings.ProjectUrl = recentProjects[selected].Url;
                monoBeh.ProjectCacher.TryRestoreProjectFromCache(recentProjects[selected].Url);
            }, null);
        }

        private FcuSettingsWindow settingsWindow;
        internal FcuSettingsWindow SettingsWindow => settingsWindow.InitWindow(this, monoBeh, new Vector2(800, 600));

        private HeaderSection headerSection;
        internal HeaderSection Header => headerSection.Bind(this, monoBeh);

        private FramesSection frameListSection;
        internal FramesSection FrameList => frameListSection.Bind(this, monoBeh);
    }

    public enum HamburgerMenuId
    {
        MainSettingsKey,
        UnityTextSettingsKey,
        TextMeshSettingsKey,
        PuiSettingsKey,
        MPUIKitSettingsKey,
        FrameListKey,
        AssetsConfigKey,
        AssetToolsKey,
        DebugToolsKey,
        RemoveUnusedSpritesKey,
        ImportEventsKey,
        UnityImageSettingsKey,
        Shapes2DSettingsKey,
        TMFontsConverterKey
    }
}
