using DA_Assets.FCU.Model;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DA_Assets.FCU.Extensions
{
    public static class FObjectExtensions
    {
        public static List<GradientColorKey> ToGradientColorKeys(this Paint gradient)
        {
            List<GradientColorKey> gradientColorKeys = new List<GradientColorKey>();

            foreach (GradientStop gradientStop in gradient.GradientStops)
            {
                gradientColorKeys.Add(new GradientColorKey
                {
                    color = gradientStop.Color,
                    time = gradientStop.Position
                });
            }

            return gradientColorKeys;
        }

        public static float ToAngle(this List<Vector2> gradientHandlePositions)
        {
            float radians = Mathf.Atan2(
                gradientHandlePositions[2].y - gradientHandlePositions[0].y,
                gradientHandlePositions[2].x - gradientHandlePositions[0].x
            );

            float degrees = radians * (180 / Mathf.PI);
            degrees = (degrees + 360) % 360; // Normalize angle between 0 and 360 degrees

            return degrees;
        }

        public static string GetText(this FObject fobject)
        {
            return fobject.Characters.Replace("\\r", " ").Replace("\\n", Environment.NewLine); 
        }

        public static bool IsInsideDownloadable(this FObject fobject, FigmaConverterUnity fcu, out FObject downloadableFObject)
        {
            downloadableFObject = default;

            bool get = fcu.CurrentProject.TryGetByIndex(fobject.Data.ParentIndex, out FObject parent);

            if (get == false)
                return false;

            if (fcu.ImageTypeSetter.DownloadableIds.Contains(parent.Id))
            {
                downloadableFObject = fobject;
                return true;
            }

            return IsInsideDownloadable(parent, fcu, out downloadableFObject);
        }

        public static bool IsShadowType(this Effect effect) => effect.Type.Contains("SHADOW");
        public static bool IsTextType(this FObject fobject) => fobject.Type == "TEXT";
        public static bool IsInstanceType(this FObject fobject) => fobject.Type == "INSTANCE";
        public static bool IsVectorType(this FObject fobject) => fobject.Type == "VECTOR";

        public static bool IsAnyMask(this FObject fobject) => fobject.IsObjectMask() || fobject.IsClipMask() || fobject.IsFrameMask();
        public static bool IsFrameMask(this FObject fobject) => fobject.ContainsTag(FcuTag.Frame);
        public static bool IsClipMask(this FObject fobject) => fobject.ClipsContent.ToBoolNullFalse();
        public static bool IsObjectMask(this FObject fobject) => fobject.IsMask.ToBoolNullFalse();
        public static bool IsGenerativeType(this FObject fobject) => fobject.Data.FcuImageType == FcuImageType.Generative;
        public static bool IsDrawableType(this FObject fobject) => fobject.Data.FcuImageType == FcuImageType.Drawable;
        public static bool IsDownloadableType(this FObject fobject) => fobject.Data.FcuImageType == FcuImageType.Downloadable;
        public static bool IsMaskType(this FObject fobject) => fobject.Data.FcuImageType == FcuImageType.Mask;

        public static bool IsSupportedRenderSize(this Vector2 sourceSize, FigmaConverterUnity fcu, out Vector2Int spriteSize, out Vector2Int renderSize)
        {
            spriteSize = (sourceSize * fcu.Settings.MainSettings.ImageScale).ToVector2Int();

            int maxRenderSize = FcuConfig.Instance.MaxRenderSize;
            int renderUpscaleFactor = FcuConfig.Instance.RenderUpscaleFactor;

            renderSize = spriteSize * renderUpscaleFactor;

            if (renderSize.x <= maxRenderSize && renderSize.y <= maxRenderSize)
            {
                return true;
            }

            return false;
        }
        public static bool IsGenerativeInstance(this FObject fobject)
        {
            bool v1 = string.IsNullOrWhiteSpace(fobject.PrimaryAxisSizingMode);
            bool v2 = string.IsNullOrWhiteSpace(fobject.CounterAxisSizingMode);
            bool v3 = string.IsNullOrWhiteSpace(fobject.PrimaryAxisAlignItems);
            bool v4 = string.IsNullOrWhiteSpace(fobject.CounterAxisAlignItems);

            if (v1 == false || v2 == false || v3 == false || v4 == false)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void SetFixedName(this FObject fobject)
        {
            string newName = fobject.Name.ReplaceInvalidFileNameChars(FcuConfig.Instance.RealTagSeparator);

            if (newName.IsEmpty())
            {
                if (fobject.Data.Parent.Equals(default(FObject)))
                {
                    newName = $"unnamed - {fobject.Id}";
                }
                else
                {
                    newName = $"unnamed - {fobject.Data.Parent.Id} {fobject.Id}";
                }
            }

            fobject.Data.FormattedName = newName.SubstringSafe(32);
        }

        public static bool IsAllPaintsDisabled(this FObject fobject)
        {
            bool allFillsDisabled = fobject.Fills
                .Where(x => x.Visible.ToBoolNullTrue() == false)
                .Count() == fobject.Fills.Count();

            bool allStrokesDisabled = fobject.Strokes
                .Where(x => x.Visible.ToBoolNullTrue() == false)
                .Count() == fobject.Strokes.Count();

            if (allFillsDisabled && allStrokesDisabled)
                return true;
            else
                return false;
        }

        public static bool TryGetRectTranformPosition(this FObject fobject, out Vector3 rtPos)
        {
            try
            {
                rtPos = new Vector3(fobject.RelativeTransform[0][2].ToFloat(), -fobject.RelativeTransform[1][2].ToFloat(), 0);
                return true;
            }
            catch
            {
                rtPos = new Vector3(0, 0, 0);
                return false;
            }
        }
    }
}
