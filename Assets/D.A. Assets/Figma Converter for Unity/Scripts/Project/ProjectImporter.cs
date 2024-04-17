using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DA_Assets.FCU
{
    [Serializable]
    public class ProjectImporter : MonoBehaviourBinder<FigmaConverterUnity>
    {
        private int importErrorCount = 0;
        public int ImportErrorCount { get => importErrorCount; set => importErrorCount = value; }

        public IEnumerator StartImport()
        {
            monoBeh.CurrentProject.CurrentPage.Clear();
            monoBeh.CurrentProject.LastImportedFrames.Clear();

            SceneBackuper.BackupActiveScene();

            monoBeh.CanvasDrawer.AddCanvasComponent();
            monoBeh.Events.OnImportStart?.Invoke(monoBeh);

            //yield return monoBeh.ComponentsParser.Download();

            List<string> selectedIds = GetSelectedFrameIds();


            DAResult<List<FObject>> result = default;
            yield return monoBeh.ProjectDownloader.DownloadAllNodes(selectedIds, x => result = x);

            FObject virtualPage = new FObject
            {
                Children = result.Object,
                Data = new SyncData
                {
                    GameObject = monoBeh.gameObject,
                    FormattedName = FcuTag.Page.ToString(),
                    Tags = new List<FcuTag>
                    {
                        FcuTag.Page
                    }
                }
            };

            monoBeh.TagSetter.SetTags(virtualPage);

            ConvertTreeToList(virtualPage, monoBeh.CurrentProject.CurrentPage);
            monoBeh.HashGenerator.SetHashes(monoBeh.CurrentProject.CurrentPage);

            if (monoBeh.IsUGUI())
            {
                yield return monoBeh.CurrentProject.LoadLocalPrefabs();

                yield return monoBeh.CanvasDrawer.GameObjectDrawer.Draw(virtualPage, monoBeh.CurrentProject.CurrentPage);
                monoBeh.TagSetter.CountTags(monoBeh.CurrentProject.CurrentPage);
                monoBeh.CurrentProject.SetRootFrames(monoBeh.CurrentProject.CurrentPage);

                yield return monoBeh.ImageTypeSetter.SetImageTypes(monoBeh.CurrentProject.CurrentPage);
                monoBeh.SpriteColorizer.SetSingleColors(monoBeh.CurrentProject.CurrentPage);

                yield return monoBeh.DuplicateFinder.SetDuplicateFlags(monoBeh.CurrentProject.CurrentPage);
                yield return monoBeh.SpritePathSetter.SetSpritePaths(monoBeh.CurrentProject.CurrentPage);
                yield return monoBeh.SpriteDownloader.DownloadSprites(monoBeh.CurrentProject.CurrentPage);
                yield return monoBeh.SpriteGenerator.GenerateSprites(monoBeh.CurrentProject.CurrentPage);
                yield return monoBeh.SpriteColorizer.ColorizeSprites(monoBeh.CurrentProject.CurrentPage);
                yield return monoBeh.FontDownloader.DownloadFonts(monoBeh.CurrentProject.CurrentPage);

                yield return monoBeh.SpriteWorker.MarkAsSprites(monoBeh.CurrentProject.CurrentPage);

                yield return monoBeh.TransformSetter.SetFigmaTransform(monoBeh.CurrentProject.CurrentPage);

                yield return monoBeh.CanvasDrawer.DrawToCanvas(monoBeh.CurrentProject.CurrentPage);
                yield return monoBeh.TransformSetter.SetFigmaTransform(monoBeh.CurrentProject.CurrentPage);
            }
            else
            {
                monoBeh.CurrentProject.SetRootFrames(monoBeh.CurrentProject.CurrentPage);

                yield return monoBeh.ImageTypeSetter.SetImageTypes(monoBeh.CurrentProject.CurrentPage);
                monoBeh.SpriteColorizer.SetSingleColors(monoBeh.CurrentProject.CurrentPage);

                yield return monoBeh.DuplicateFinder.SetDuplicateFlags(monoBeh.CurrentProject.CurrentPage);
                yield return monoBeh.SpritePathSetter.SetSpritePaths(monoBeh.CurrentProject.CurrentPage);
                yield return monoBeh.SpriteDownloader.DownloadSprites(monoBeh.CurrentProject.CurrentPage);
                yield return monoBeh.SpriteGenerator.GenerateSprites(monoBeh.CurrentProject.CurrentPage);
                yield return monoBeh.SpriteColorizer.ColorizeSprites(monoBeh.CurrentProject.CurrentPage);
                yield return monoBeh.FontDownloader.DownloadFonts(monoBeh.CurrentProject.CurrentPage);
                yield return monoBeh.SpriteWorker.MarkAsSprites(monoBeh.CurrentProject.CurrentPage);
#if UITKPLUGIN_EXISTS
                monoBeh.UITK_Converter.Convert(virtualPage);
#endif
            }

            if (monoBeh.Settings.DebugSettings.DebugMode == false)
            {
                ClearAfterImport();
            }

            monoBeh.Events.OnImportComplete?.Invoke(monoBeh);

            ShowRateMe();

            DALogger.LogSuccess(FcuLocKey.log_import_complete.Localize());
        }


        private void ShowRateMe()
        {
            try
            {
                monoBeh.DelegateHolder.ShowRateMe(importErrorCount, monoBeh.TagSetter.TagsCounter[FcuTag.Image]);
            }
            catch
            {

            }
        }

        private List<string> GetSelectedFrameIds()
        {
            List<FObject> docChilds = monoBeh.CurrentProject.FigmaProject.Document.Children;

            List<FObject> childsOfChilds = docChilds
                .Select(x => x.Children)
                .FromChunks();

            List<string> selected = monoBeh.InspectorDrawer.SelectableFrames
                .Where(si => si.Selected)
                .Select(si => si.Id)
                .ToList();

            return selected;
        }

        public void ConvertTreeToList(FObject parent, List<FObject> fobjects)
        {
            foreach (FObject child in parent.Children)
            {
                int parentIndex = fobjects.IndexOf(parent);
                child.Data.ParentIndex = parentIndex;

                fobjects.Add(child);

                if (child.Data.Parent.ContainsTag(FcuTag.Page) == false)
                {
                    fobjects[child.Data.ParentIndex].Data.ChildIndexes.Add(fobjects.Count() - 1);
                }

                if (child.Children.IsEmpty())
                    continue;

                if (child.Data.ForceImage)
                {
                    SetFlagToAllChilds(child);
                    continue;
                }

                ConvertTreeToList(child, fobjects);
            }
        }

        public void SetFlagToAllChilds(FObject parent)
        {
            if (parent.IsDefault())
                return;

            if (parent.Children.IsEmpty())
                return;

            foreach (FObject child in parent.Children)
            {
                child.Data.InsideForceImage = true;
                SetFlagToAllChilds(child);
            }
        }

        private void ClearAfterImport()
        {
            Parallel.ForEach(monoBeh.CurrentProject.CurrentPage, fobject =>
            {
                fobject.Data.SpritePath = null;
                fobject.Data.Link = null;
            });
        }
    }
}