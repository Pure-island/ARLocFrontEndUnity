using DA_Assets.DAG;
using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace DA_Assets.FCU.Drawers.CanvasDrawers
{
    public class UnityImageDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void Draw(FObject fobject, Sprite sprite, GameObject target)
        {
            MaskableGraphic graphic;

            if (monoBeh.UsingRawImage())
            {
                target.TryAddGraphic(out RawImage img);
                graphic = img;

                img.texture = sprite.texture;
            }
            else
            {
                target.TryAddGraphic(out Image img);
                graphic = img;

                img.sprite = sprite;
                img.type = monoBeh.Settings.UnityImageSettings.Type;
                img.preserveAspect = monoBeh.Settings.UnityImageSettings.PreserveAspect;
            }

            graphic.raycastTarget = monoBeh.Settings.UnityImageSettings.RaycastTarget;
            graphic.maskable = monoBeh.Settings.UnityImageSettings.Maskable;
#if UNITY_2020_1_OR_NEWER
            graphic.raycastPadding = monoBeh.Settings.UnityImageSettings.RaycastPadding;
#endif
            SetColor(fobject, graphic);
        }

        public void SetColor(FObject fobject, MaskableGraphic gr)
        {
            bool hasFills = fobject.TryGetFills(monoBeh, out Paint solidFill, out Paint gradientFill);
            bool hasStroke = fobject.TryGetStrokes(monoBeh, out Paint solidStroke, out Paint gradientStroke);

            monoBeh.Log($"SetUnityImageColor | {fobject.Data.Hierarchy} | {fobject.Data.FcuImageType} | hasFills: {hasFills} | hasStroke: {hasStroke}");

            if (fobject.IsDrawableType())
            {
                if (hasFills && hasStroke)
                {
                    AddUnityOutline(fobject, gr.gameObject, solidStroke, gradientStroke);
                }

                if (hasFills)
                {
                    if (solidFill.IsDefault() == false)
                    {
                        Color c = solidFill.Color.SetFigmaAlpha(solidFill.Opacity);
                        gr.color = c;
                    }
                    else
                    {
                        Color c = Color.white;
                        gr.color = c;
                    }

                    if (gradientFill.IsDefault() == false)
                    {
                        AddGradient(gradientFill, gr.gameObject);
                    }
                }
                else if (hasStroke)
                {
                    if (solidStroke.IsDefault() == false)
                    {
                        Color c = solidStroke.Color.SetFigmaAlpha(solidFill.Opacity);
                        gr.color = c;
                    }
                    else
                    {
                        Color c = Color.white;
                        gr.color = c;
                    }

                    if (gradientStroke.IsDefault() == false)
                    {
                        AddGradient(gradientStroke, gr.gameObject);
                    }
                }
                else
                {
                    fobject.Data.GameObject.TryDestroyComponent<Outline>();
                }
            }
            else if (fobject.IsGenerativeType())
            {
                if (hasFills && hasStroke)//no need colorize
                {
                    if (fobject.StrokeAlign == "OUTSIDE")
                    {
                        AddUnityOutline(fobject, gr.gameObject, solidStroke, gradientStroke);
                    }
                }
                else if (hasFills)
                {
                    if (solidFill.IsDefault() == false)
                    {
                        Color c = solidFill.Color.SetFigmaAlpha(solidFill.Opacity);
                        gr.color = c;
                    }
                    else
                    {
                        Color c = Color.white;
                        gr.color = c;
                    }

                    if (gradientFill.IsDefault() == false)
                    {
                        AddGradient(gradientFill, gr.gameObject);
                    }
                }
                else if (hasStroke)
                {
                    if (solidStroke.IsDefault() == false)
                    {
                        Color c = solidStroke.Color.SetFigmaAlpha(solidFill.Opacity);
                        gr.color = c;
                    }
                    else
                    {
                        Color c = Color.white;
                        gr.color = c;
                    }

                    if (gradientStroke.IsDefault() == false)
                    {
                        AddGradient(gradientStroke, gr.gameObject);
                    }
                }
            }
            else if (fobject.IsDownloadableType())
            {
                if (fobject.Data.SingleColor.IsDefault() == false)
                {
                    Color c = fobject.Data.SingleColor;
                    gr.color = c;
                }
                else
                {
                    Color c = Color.white;
                    gr.color = c;
                }
            }
        }

        public void AddUnityOutline(FObject fobject, GameObject target, Paint solidStroke, Paint gradientStroke)
        {
            if (monoBeh.UsingUnityImage() == false)
            {
                if (fobject.StrokeAlign == "INSIDE")
                {
                    return;
                }
            }

            target.TryAddComponent(out Outline outline);
            outline.effectDistance = new Vector2(fobject.StrokeWeight, -fobject.StrokeWeight);

            if (solidStroke.IsDefault() == false)
            {
                outline.effectColor = solidStroke.Color.SetFigmaAlpha(solidStroke.Opacity);
            }
            else if (gradientStroke.IsDefault() == false)
            {
                List<GradientColorKey> gradientColorKeys = gradientStroke.ToGradientColorKeys();
                outline.effectColor = gradientColorKeys.First().color;
            }
            else
            {
                outline.effectColor = default;
            }
        }

        public void AddGradient(Paint gradientColor, GameObject go)
        {
            if (monoBeh.UsingMPUIKit())
                return;

            go.TryAddComponent(out DAGradient gradient);

            List<GradientColorKey> gradientColorKeys = gradientColor.ToGradientColorKeys();

            gradient.BlendMode = DAColorBlendMode.Multiply;
            gradient.Gradient.colorKeys = gradientColorKeys.ToArray();
            gradient.Angle = gradientColor.GradientHandlePositions.ToAngle();
        }
    }
}