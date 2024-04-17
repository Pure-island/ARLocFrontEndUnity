﻿using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DA_Assets.FCU
{
    [Serializable]
    public class NameHumanizer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public string GetHumanizedTextPrefabName(FObject fobject)
        {
            string fontName = fobject.FontNameToString();
            string fontSize = fobject.Style.FontSize.ToString();

            string howAlig = fobject.Style.TextAlignHorizontal.ToPascalCase();
            string vertAlig = fobject.Style.TextAlignVertical.ToPascalCase();
            string aligment = $"{howAlig}{vertAlig}";
            string fontColor = GetColorString(fobject);

            string finalName = $"{fontName}-{fontSize}-{aligment}";

            if (fontColor.IsEmpty() == false)
                finalName += $"-{fontColor}";

            return $"{monoBeh.Settings.ComponentSettings.TextComponent} {finalName}";
        }

        private string GetColorString(FObject fobject)
        {
            bool hasFills = fobject.TryGetFills(monoBeh, out Paint solidFill, out Paint gradientFill);
            bool hasStroke = fobject.TryGetStrokes(monoBeh, out Paint solidStroke, out Paint gradientStroke);

            string fontColor = "";

            try
            {
                if (hasFills && hasStroke)
                {
                    string fillName = "";

                    if (solidFill.IsDefault() == false)
                    {
                        fillName = GetSolidName(solidFill);
                    }
                    else if (gradientFill.IsDefault() == false)
                    {
                        fillName = GetGradientName(gradientFill);
                    }

                    string strokeName = "";

                    if (solidStroke.IsDefault() == false)
                    {
                        strokeName = GetSolidName(solidStroke);
                    }
                    else if (gradientStroke.IsDefault() == false)
                    {
                        strokeName = GetGradientName(gradientStroke);
                    }

                    fontColor = $"fill-{fillName}_stroke-{strokeName}";
                }
                else if (hasFills)
                {
                    string fillName = "";

                    if (solidFill.IsDefault() == false)
                    {
                        fillName = GetSolidName(solidFill);
                    }
                    else if (gradientFill.IsDefault() == false)
                    {
                        fillName = GetGradientName(gradientFill);
                    }

                    fontColor = fillName;
                }
                else if (hasStroke)
                {
                    string strokeName = "";

                    if (solidStroke.IsDefault() == false)
                    {
                        strokeName = GetSolidName(solidStroke);
                    }
                    else if (gradientStroke.IsDefault() == false)
                    {
                        strokeName = GetGradientName(gradientStroke);
                    }

                    fontColor = strokeName;
                }
            }
            catch
            {

            }

            return fontColor;
        }

        private string GetSolidName(Paint paint)
        {
            Color32 color32 = paint.Color;

            switch (monoBeh.Settings.PrefabSettings.TextPrefabNameType)
            {
                case TextPrefabNameType.HumanizedColorString:
                    {
                        string nearestColorStr = GetNearestColor(paint.Color).Key;
                        return $"{nearestColorStr}{color32.a}";
                    }
                default:
                case TextPrefabNameType.HumanizedColorHEX:
                    {
                        string colorStr = ColorUtility.ToHtmlStringRGB(color32);
                        return colorStr;
                    }
            }
        }

        private string GetGradientName(Paint paint)
        {
            Color32 startColor = paint.GradientStops.First().Color;
            Color32 endColor = paint.GradientStops.Last().Color;

            switch (monoBeh.Settings.PrefabSettings.TextPrefabNameType)
            {
                case TextPrefabNameType.HumanizedColorString:
                    {
                        string startColorName = GetNearestColor(startColor).Key;
                        string endColorName = GetNearestColor(endColor).Key;
                        return $"{startColorName}{startColor.a}-{endColorName}{endColor.a}";
                    }
                default:
                case TextPrefabNameType.HumanizedColorHEX:
                    {
                        string startColorHex = ColorUtility.ToHtmlStringRGB(startColor);
                        string endColorHex = ColorUtility.ToHtmlStringRGB(endColor);
                        return $"{startColorHex}-{endColorHex}";
                    }
            }
        }

        private KeyValuePair<string, Color> GetNearestColor(Color inputColor)
        {
            KeyValuePair<NetKnownColor, Color32> nearestColor = default;
            float distance = float.MaxValue;

            foreach (KeyValuePair<NetKnownColor, Color32> kvp in NetKnownColors.KnownColors)
            {
                float redDiff = Mathf.Pow(kvp.Value.r - inputColor.r, 2f);
                float greenDiff = Mathf.Pow(kvp.Value.g - inputColor.g, 2f);
                float blueDiff = Mathf.Pow(kvp.Value.b - inputColor.b, 2f);

                float temp = Mathf.Sqrt(redDiff + greenDiff + blueDiff);

                if (temp == 0f)
                {
                    nearestColor = kvp;
                    break;
                }
                else if (temp < distance)
                {
                    distance = temp;
                    nearestColor = kvp;
                }
            }

            if (nearestColor.IsDefault())
            {
                return new KeyValuePair<string, Color>("UnknownColor", Color.black);
            }
            else
            {
                return new KeyValuePair<string, Color>(nearestColor.Key.ToString(), nearestColor.Value);
            }
        }
    }

}
