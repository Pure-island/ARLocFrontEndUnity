using DA_Assets.FCU.Model;
using DA_Assets.Shared.Extensions;
using System.Linq;
using UnityEngine;

namespace DA_Assets.FCU.Extensions
{
    public static class TransformExtensions
    {
        public static bool IsNeedRotate(this FObject fobject)
        {
            return fobject.GetFigmaRotationAngle() != 0 && 
                (fobject.Data.ChildIndexes.Count() > 0 || fobject.ContainsTag(FcuTag.Text) || fobject.Data.FcuImageType != FcuImageType.Downloadable);
        }
        public static void SetFigmaRotation(this FObject fobject, FigmaConverterUnity fcu)
        {
            float rotationAngle;

            RectTransform rect = fobject.Data.GameObject.GetComponent<RectTransform>();

            if (fobject.IsNeedRotate())
            {
                rotationAngle = fobject.GetFigmaRotationAngle();
            }
            else
            {
                rotationAngle = 0;
            }

            rect.SetRotation(rotationAngle);
        }
        public static float GetFigmaRotationAngle(this FObject fobject)
        {
            if (fobject.RelativeTransform.IsEmpty())
            {
                return 0;
            }

            if (fobject.RelativeTransform[0].IsEmpty())
            {
                return 0;
            }

            if (fobject.RelativeTransform[0].Count() < 2)
            {
                return 0;
            }

            bool isNull = fobject.RelativeTransform[0][0] == null || fobject.RelativeTransform[0][1] == null;

            Vector2 fangle = new Vector2(fobject.RelativeTransform[0][0].ToFloat(), fobject.RelativeTransform[0][1].ToFloat());

            if (isNull == false && fangle != new Vector2(1.0f, 0.0f))
            {
                float rotationAngle = Mathf.Atan2(fangle.y, fangle.x) * (180 / Mathf.PI);
                return rotationAngle;
            }
            else
            {
                return 0;
            }
        }

        public static AnchorType GetFigmaAnchor(this FObject fobject)
        {
            string anchor = fobject.Constraints.Vertical + " " + fobject.Constraints.Horizontal;

            AnchorType anchorPreset;

            switch (anchor)
            {
                ////////////////LEFT////////////////
                case "TOP LEFT":
                    anchorPreset = AnchorType.TopLeft;
                    break;
                case "BOTTOM LEFT":
                    anchorPreset = AnchorType.BottomLeft;
                    break;
                case "TOP_BOTTOM LEFT":
                    anchorPreset = AnchorType.VertStretchLeft;
                    break;
                case "CENTER LEFT":
                    anchorPreset = AnchorType.MiddleLeft;
                    break;
                case "SCALE LEFT":
                    anchorPreset = AnchorType.VertStretchLeft;
                    break;
                ////////////////RIGHT////////////////
                case "TOP RIGHT":
                    anchorPreset = AnchorType.TopRight;
                    break;
                case "BOTTOM RIGHT":
                    anchorPreset = AnchorType.BottomRight;
                    break;
                case "TOP_BOTTOM RIGHT":
                    anchorPreset = AnchorType.VertStretchRight;
                    break;
                case "CENTER RIGHT":
                    anchorPreset = AnchorType.MiddleRight;
                    break;
                case "SCALE RIGHT":
                    anchorPreset = AnchorType.VertStretchRight;
                    break;
                ////////////////LEFT_RIGHT////////////////
                case "TOP LEFT_RIGHT":
                    anchorPreset = AnchorType.HorStretchTop;
                    break;
                case "BOTTOM LEFT_RIGHT":
                    anchorPreset = AnchorType.HorStretchBottom;
                    break;
                case "TOP_BOTTOM LEFT_RIGHT":
                    anchorPreset = AnchorType.StretchAll;
                    break;
                case "CENTER LEFT_RIGHT":
                    anchorPreset = AnchorType.HorStretchMiddle;
                    break;
                case "SCALE LEFT_RIGHT":
                    anchorPreset = AnchorType.HorStretchMiddle;
                    break;
                ////////////////CENTER////////////////
                case "TOP CENTER":
                    anchorPreset = AnchorType.TopCenter;
                    break;
                case "BOTTOM CENTER":
                    anchorPreset = AnchorType.BottomCenter;
                    break;
                case "TOP_BOTTOM CENTER":
                    anchorPreset = AnchorType.VertStretchCenter;
                    break;
                case "CENTER CENTER":
                    anchorPreset = AnchorType.MiddleCenter;
                    break;
                case "SCALE CENTER":
                    anchorPreset = AnchorType.StretchAll;
                    break;
                ////////////////SCALE////////////////
                case "TOP SCALE":
                    anchorPreset = AnchorType.HorStretchTop;
                    break;
                case "BOTTOM SCALE":
                    anchorPreset = AnchorType.HorStretchBottom;
                    break;
                case "TOP_BOTTOM SCALE":
                    anchorPreset = AnchorType.VertStretchCenter;
                    break;
                case "CENTER SCALE":
                    anchorPreset = AnchorType.StretchAll;
                    break;
                case "SCALE SCALE":
                    anchorPreset = AnchorType.StretchAll;
                    break;
                ////////////////DEFAULT////////////////
                default:
                    anchorPreset = AnchorType.MiddleCenter;
                    break;
            }

            return anchorPreset;
        }
    }
}