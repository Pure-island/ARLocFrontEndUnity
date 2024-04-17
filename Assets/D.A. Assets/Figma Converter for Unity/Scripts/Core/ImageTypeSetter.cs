using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0649

namespace DA_Assets.FCU
{
    [Serializable]
    public class ImageTypeSetter : MonoBehaviourBinder<FigmaConverterUnity>
    {
        [SerializeField] List<string> downloadableIds = new List<string>();
        [SerializeField] List<string> generativeIds = new List<string>();
        [SerializeField] List<string> drawableIds = new List<string>();
        [SerializeField] List<string> noneIds = new List<string>();

        public List<string> DownloadableIds => downloadableIds;

        public IEnumerator SetImageTypes(List<FObject> fobjects)
        {
            downloadableIds.Clear();
            generativeIds.Clear();
            drawableIds.Clear();
            noneIds.Clear();

            foreach (FObject fobject in fobjects)
            {
                if (fobject.ContainsTag(FcuTag.Image) == false)
                {
                    continue;
                }

                bool isGenerative = IsGenerative(fobject);
                bool isDownloadable = IsDownloadable(fobject);
                bool isDrawable = IsDrawable(fobject);

                if (isGenerative)
                {
                    fobject.Data.FcuImageType = FcuImageType.Generative;
                    generativeIds.Add(fobject.Id);
                }
                else if (isDownloadable)
                {
                    fobject.Data.FcuImageType = FcuImageType.Downloadable;
                    downloadableIds.Add(fobject.Id);
                }
                else if (isDrawable)
                {
                    fobject.Data.FcuImageType = FcuImageType.Drawable;
                    drawableIds.Add(fobject.Id);
                }
                else
                {
                    fobject.Data.FcuImageType = FcuImageType.None;
                    noneIds.Add(fobject.Id);
                }

                monoBeh.Log($"SetImageType | {fobject.Data.Hierarchy} | {fobject.Data.FcuImageType}", FcuLogType.IsDownloadable);
            }

            monoBeh.Log($"SetImageType | {downloadableIds.Count} | {generativeIds.Count} | {drawableIds.Count} | {noneIds.Count}", FcuLogType.IsDownloadable);

            yield return null;
        }

        private bool IsDrawable(FObject fobject)
        {
            bool result = true;
            string reason = "drawable";

            monoBeh.Log($"{nameof(IsDrawable)} | {result} | {fobject.Data.Hierarchy} | {reason}", FcuLogType.IsDownloadable);

            return result;
        }

        private bool IsGenerative(FObject fobject)
        {
            bool result = true;
            string reason = "generative";

            if (monoBeh.IsUGUI() == false)
            {
                reason = "using uitk";
                result = false;
            }
            else if (monoBeh.UsingMPUIKit() || monoBeh.UsingPUI() || monoBeh.UsingShapes2D())
            {
                reason = "custom image asset";
                result = false;
            }
            else if (fobject.Size.IsSupportedRenderSize(monoBeh, out Vector2Int spriteSize, out Vector2Int _renderSize) == false)
            {
                reason = $"render size is big: {spriteSize} x {_renderSize}";
                result = false;
            }
            else if (fobject.Type != "RECTANGLE" && fobject.Type != "FRAME" && fobject.IsGenerativeInstance() == false)
            {
                reason = $"is not rectangle: {fobject.Type} {fobject.IsGenerativeInstance()}";
                result = false;
            }
            else if (fobject.Data.ChildIndexes.Count == 0)
            {
                reason = $"has no childs";
                result = false;
            }
            else
            {
                bool hasFills = fobject.TryGetFills(monoBeh, out Paint solidFill, out Paint gradientColor);
                bool hasStrokes = fobject.TryGetStrokes(monoBeh, out Paint solidStroke, out Paint gradientStroke);

                if (hasFills && hasStrokes)
                {
                    if (gradientColor.IsDefault() == false || gradientStroke.IsDefault() == false)
                    {
                        reason = $"fill + stroke + gradient is not supported";
                        result = false;
                    }
                }
                else if (hasFills == false && hasStrokes == false)
                {
                    reason = $"no fills no strokes";
                    result = false;
                }
            }

            monoBeh.Log($"{nameof(IsGenerative)} | {result} | {fobject.Data.Hierarchy} | {reason}", FcuLogType.IsDownloadable);

            return result;
        }

        private bool IsDownloadable(FObject fobject)
        {
            bool result = true;
            string reason = "unknown";

            if (IsDownloadableByTags(fobject) == false)
            {
                reason = "IsDownloadableByTags == false";
                result = false;
            }
            else if (fobject.Data.IsEmpty)
            {
                reason = "IsEmpty";
                result = false;
            }
            else if (fobject.IsArcDataFilled())
            {
                reason = "fobject.IsFilled()";
                result = true;
            }
            else if (fobject.Effects.IsEmpty() == false)
            {
                if (fobject.Data.Tags.Contains(FcuTag.Shadow))
                {
                    if (monoBeh.Settings.ComponentSettings.ShadowComponent == ShadowComponent.Figma)
                    {
                        reason = "shadowComponent.Figma";
                        result = true;
                    }
                }
                else
                {
                    reason = "contains other effects";
                    result = true;
                }
            }
            else if (monoBeh.UsingUnityImage() && monoBeh.IsUGUI())
            {
                if (fobject.Type != "RECTANGLE")
                {
                    reason = "fobject.Type != 'RECTANGLE'";
                    result = true;
                }
                else if (fobject.ContainsRoundedCorners())
                {
                    reason = "containsRoundedCorners";
                    result = true;
                }
                else if (fobject.Strokes.IsEmpty() == false)
                {
                    reason = "has strokes";
                    result = true;
                }
                else if (fobject.Type == "LINE" && fobject.StrokeCap == "ROUND")
                {
                    reason = "StrokeCap == ROUND";
                    result = true;
                }
            }
            else if (monoBeh.UsingMPUIKit() || monoBeh.UsingPUI() || monoBeh.UsingShapes2D() || monoBeh.IsUGUI() == false)
            {
                if (fobject.HasImageOrGifRef())
                {
                    reason = "HasImageOrGifRef";
                    result = true;
                }
                else if (fobject.Type == "RECTANGLE" || fobject.Type == "ELLIPSE" || fobject.Type == "LINE")
                {
                    reason = "fobject.Type == 'RECTANGLE' || fobject.Type == 'ELLIPSE'";
                    result = false;
                }

                if (monoBeh.UsingShapes2D())
                {
                    if (fobject.StrokeAlign == "CENTER")
                    {
                        reason = "has strokes";
                        result = true;
                    }
                }
                else if (monoBeh.UsingPUI())
                {
                    if (fobject.StrokeAlign == "INSIDE" || fobject.StrokeAlign == "CENTER")
                    {
                        reason = "has strokes";
                        result = true;
                    }
                }
                else if (fobject.StrokeAlign == "INSIDE" || fobject.StrokeAlign == "CENTER")
                {
                    reason = "has strokes";
                    result = true;
                }
            }

            monoBeh.Log($"{nameof(IsDownloadable)} | {result} | {fobject.Data.Hierarchy} | {reason}", FcuLogType.IsDownloadable);
            return result;
        }


        public bool IsDownloadableByTags(FObject fobject)
        {
            bool result = true;
            string reason = "downloadableByTags";

            if (fobject.ContainsTag(FcuTag.Image) == false)
            {
                reason = "fobject.ContainsTag(FcuTag.Image) == false)";
                result = false;
            }
            else
            {
                foreach (FcuTag fcuTag in fobject.Data.Tags)
                {
                    TagConfig tc = fcuTag.GetTagConfig();

                    if (tc.CanBeDownloaded == false)
                    {
                        reason = $"{fobject.Data.Tags.ToLine()}";
                        result = false;
                        break;
                    }
                }
            }

            monoBeh.Log($"{nameof(IsDownloadableByTags)} | {result} | {fobject.Data.Hierarchy} | {reason}", FcuLogType.IsDownloadable);

            return result;
        }
    }
}
