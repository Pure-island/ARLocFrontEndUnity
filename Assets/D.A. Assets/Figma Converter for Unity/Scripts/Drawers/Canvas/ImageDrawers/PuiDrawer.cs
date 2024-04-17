using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using UnityEngine;
using UnityEngine.UI;

#if PUI_EXISTS
using UnityEngine.UI.ProceduralImage;
#endif

namespace DA_Assets.FCU.Drawers.CanvasDrawers
{
    public class PuiDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void Draw(FObject fobject, Sprite sprite, GameObject target)
        {
#if PUI_EXISTS
            target.TryAddGraphic(out ProceduralImage img);

            img.sprite = sprite;
            img.type = monoBeh.Settings.PuiSettings.Type;
            img.raycastTarget = monoBeh.Settings.PuiSettings.RaycastTarget;
            img.preserveAspect = monoBeh.Settings.PuiSettings.PreserveAspect;
            img.FalloffDistance = monoBeh.Settings.PuiSettings.FalloffDistance;
#if UNITY_2020_1_OR_NEWER
            img.raycastPadding = monoBeh.Settings.PuiSettings.RaycastPadding;
#endif
            if (fobject.Type == "ELLIPSE")
            {
                target.TryAddComponent(out RoundModifier roundModifier);
            }
            else
            {
                if (fobject.CornerRadiuses != null)
                {
                    target.TryAddComponent(out FreeModifier freeModifier);
                    freeModifier.Radius = fobject.GetCornerRadius(ImageComponent.ProceduralImage);
                }
                else
                {
                    target.TryAddComponent(out UniformModifier uniformModifier);
                    uniformModifier.Radius = fobject.CornerRadius.ToFloat();
                }
            }

            SetColor(fobject, img);
#endif
        }

        public void SetColor(FObject fobject, Image img)
        {
            bool hasFills = fobject.TryGetFills(monoBeh, out Paint solidFill, out Paint gradientFill);
            bool hasStroke = fobject.TryGetStrokes(monoBeh, out Paint solidStroke, out Paint gradientStroke);

            monoBeh.Log($"SetUnityImageColor | {fobject.Data.Hierarchy} | {fobject.Data.FcuImageType} | hasFills: {hasFills} | hasStroke: {hasStroke}");

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
                        monoBeh.CanvasDrawer.ImageDrawer.UnityImageDrawer.AddGradient(gradientFill, img.gameObject);
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
                    if (fobject.StrokeAlign == "OUTSIDE")
                    {
                        monoBeh.CanvasDrawer.ImageDrawer.UnityImageDrawer.AddUnityOutline(fobject, img.gameObject, solidStroke, gradientStroke);
                    }
                }
            }
            else
            {
                monoBeh.CanvasDrawer.ImageDrawer.UnityImageDrawer.SetColor(fobject, img);
            }
        }
    }
}
