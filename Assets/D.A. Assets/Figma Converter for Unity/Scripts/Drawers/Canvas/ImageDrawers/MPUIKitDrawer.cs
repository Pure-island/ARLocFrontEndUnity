using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DA_Assets.Shared.Extensions;
using DA_Assets.FCU.Extensions;
using System.Reflection;

#pragma warning disable CS0649

#if MPUIKIT_EXISTS
using MPUIKIT;
#endif


namespace DA_Assets.FCU.Drawers.CanvasDrawers
{
    public class MPUIKitDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void Draw(FObject fobject, Sprite sprite, GameObject target)
        {
#if MPUIKIT_EXISTS
            target.TryAddGraphic(out MPImage img);
            SetCorners(fobject, img);

            SetColor(fobject, img);

            img.sprite = sprite;
            img.type = monoBeh.Settings.MPUIKitSettings.Type;
            img.raycastTarget = monoBeh.Settings.MPUIKitSettings.RaycastTarget;
            img.preserveAspect = monoBeh.Settings.MPUIKitSettings.PreserveAspect;
            img.FalloffDistance = monoBeh.Settings.MPUIKitSettings.FalloffDistance;
#if UNITY_2020_1_OR_NEWER
            img.raycastPadding = monoBeh.Settings.MPUIKitSettings.RaycastPadding;
#endif

            MethodInfo initMethod = typeof(MPImage).GetMethod("Init", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            initMethod.Invoke(img, null);
#endif
        }

#if MPUIKIT_EXISTS
        public void SetColor(FObject fobject, MPImage img)
        {
            bool hasFills = fobject.TryGetFills(monoBeh, out Paint solidFill, out Paint gradientFill);
            bool hasStroke = fobject.TryGetStrokes(monoBeh, out Paint solidStroke, out Paint gradientStroke);

            monoBeh.Log($"SetUnityImageColor | {fobject.Data.Hierarchy} | {fobject.Data.FcuImageType} | hasFills: {hasFills} | hasStroke: {hasStroke}");

            img.GradientEffect = new GradientEffect
            {
                Enabled = false,
                GradientType = MPUIKIT.GradientType.Linear,
                Gradient = null
            };

            if (fobject.IsDrawableType())
            {
                if (hasFills)
                {
                    if (solidFill.IsDefault() == false)
                    {
                        Color c = solidFill.Color.SetFigmaAlpha(solidFill.Opacity);
                        img.color = c;
                    }
                    else
                    {
                        Color c = Color.white;
                        img.color = c;
                    }

                    if (gradientFill.IsDefault() == false)
                    {
                        AddGradient(gradientFill, img);
                    }
                }
                else
                {
                    Color c = Color.white;
                    c.a = 0;
                    img.color = c;
                }

                if (hasStroke)
                {
                    if (fobject.StrokeAlign == "INSIDE")
                    {
                        img.OutlineWidth = fobject.StrokeWeight;

                        if (solidStroke.IsDefault() == false)
                        {
                            img.OutlineColor = solidStroke.Color.SetFigmaAlpha(solidStroke.Opacity);
                        }
                        else if (gradientStroke.IsDefault() == false)
                        {
                            List<GradientColorKey> gradientColorKeys = gradientStroke.ToGradientColorKeys();
                            img.OutlineColor = gradientColorKeys.First().color;
                        }
                        else
                        {
                            img.OutlineColor = default;
                        }
                    }
                    else if (fobject.StrokeAlign == "OUTSIDE")
                    {
                        monoBeh.CanvasDrawer.ImageDrawer.UnityImageDrawer.AddUnityOutline(fobject, img.gameObject, solidStroke, gradientStroke);
                    }
                    else
                    {
                        img.OutlineWidth = 0;
                    }
                }
                else
                {
                    img.OutlineWidth = 0;
                }
            }
            else
            {
                monoBeh.CanvasDrawer.ImageDrawer.UnityImageDrawer.SetColor(fobject, img);
            }
        }

        public void AddGradient(Paint gradientColor, MPImage img)
        {
            Gradient gradient = new Gradient
            {
                mode = GradientMode.Blend,
            };

            img.GradientEffect = new GradientEffect
            {
                Enabled = true,
                GradientType = MPUIKIT.GradientType.Linear,
                Gradient = gradient,
                Rotation = gradientColor.GradientHandlePositions.ToAngle()
            };

            List<GradientColorKey> gradientColorKeys = gradientColor.ToGradientColorKeys();
            gradient.colorKeys = gradientColorKeys.ToArray();
        }

        private void SetCorners(FObject fobject, MPImage img)
        {
            if (fobject.Type == "ELLIPSE")
            {
                img.DrawShape = DrawShape.Circle;
                img.Circle = new Circle
                {
                    FitToRect = true
                };
            }
            else
            {
                img.DrawShape = DrawShape.Rectangle;

                img.Rectangle = new Rectangle
                {
                    CornerRadius = fobject.GetCornerRadius(ImageComponent.MPImage)
                };
            }
        }
#endif
    }
}
