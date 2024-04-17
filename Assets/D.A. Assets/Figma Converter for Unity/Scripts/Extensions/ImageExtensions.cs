using DA_Assets.FCU.Model;
using DA_Assets.Shared.Extensions;
#if MPUIKIT_EXISTS
#endif
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DA_Assets.FCU.Extensions
{
    public static class ImageExtensions
    {
        public static bool GetBoundingSize(this FObject fobject, out Vector2 size)
        {
            size = new Vector2(fobject.AbsoluteBoundingBox.Width.ToFloat(), fobject.AbsoluteBoundingBox.Height.ToFloat());
            return !fobject.AbsoluteBoundingBox.IsSizeNull();
        }
        public static bool GetBoundingPosition(this FObject fobject, out Vector2 position)
        {
            position = new Vector2(fobject.AbsoluteBoundingBox.X.ToFloat(), fobject.AbsoluteBoundingBox.Y.ToFloat());
            return !fobject.AbsoluteBoundingBox.IsPositionNull();
        }
        public static bool GetRenderSize(this FObject fobject, out Vector2 size)
        {
            size = new Vector2(fobject.AbsoluteRenderBounds.Width.ToFloat(), fobject.AbsoluteRenderBounds.Height.ToFloat());
            return !fobject.AbsoluteRenderBounds.IsSizeNull();
        }
        public static bool GetRenderPosition(this FObject fobject, out Vector2 position)
        {
            position = new Vector2(fobject.AbsoluteRenderBounds.X.ToFloat(), fobject.AbsoluteRenderBounds.Y.ToFloat());
            return !fobject.AbsoluteRenderBounds.IsPositionNull();
        }
        public static bool IsSizeNull(this BoundingBox bb)
        {
            return bb.Width == null || bb.Height == null;
        }
        public static bool IsPositionNull(this BoundingBox bb)
        {
            return bb.X == null || bb.X == null;
        }
        public static bool IsSizeEqual(this BoundingBox bb1, BoundingBox bb2, FObject f)
        {
            Vector2 s1 = new Vector2(bb1.Width.ToFloat(), bb1.Height.ToFloat());
            Vector2 s2 = new Vector2(bb2.Width.ToFloat(), bb2.Height.ToFloat());

            return s1 == s2;
        }

        public static Vector4 GetCornerRadius(this FObject fobject, ImageComponent imageComponent)
        {
            if (fobject.CornerRadiuses.IsEmpty())
            {
                return new Vector4
                {
                    x = fobject.CornerRadius.ToFloat(),
                    y = fobject.CornerRadius.ToFloat(),
                    z = fobject.CornerRadius.ToFloat(),
                    w = fobject.CornerRadius.ToFloat()
                };
            }
            else
            {
                if (imageComponent == ImageComponent.ProceduralImage)
                {
                    return new Vector4
                    {
                        x = fobject.CornerRadiuses[0],
                        y = fobject.CornerRadiuses[1],
                        z = fobject.CornerRadiuses[2],
                        w = fobject.CornerRadiuses[3]
                    };
                }
                else
                {
                    return new Vector4
                    {
                        x = fobject.CornerRadiuses[3],
                        y = fobject.CornerRadiuses[2],
                        z = fobject.CornerRadiuses[1],
                        w = fobject.CornerRadiuses[0]
                    };
                }
            }
        }
        public static Color GetColor(this Paint fill)
        {
            if (fill.Opacity != null)
            {
                Color _color = fill.Color;
                _color.a = (float)fill.Opacity;
                return _color;
            }
            else
            {
                return fill.Color;
            }
        }
        /// <summary>
        /// So far, importing gradients is not supported due to the lack of an algorithm for getting the angle of a gradient.
        /// </summary>
        public static Color SimplifyGradient(this Paint fill)
        {
            if (fill.Opacity != null)
            {
                Color _color = fill.GradientStops.First().Color;
                _color.a = (float)fill.Opacity;
                return _color;
            }
            else
            {
                return fill.GradientStops.First().Color;
            }
        }
        public static bool IsZeroSize(this FObject fobject)
        {
            if (fobject.AbsoluteBoundingBox.Width == 0 || fobject.AbsoluteBoundingBox.Height == 0)
            {
                return true;
            }

            return false;
        }
        public static bool IsVisible(this FObject fobject)
        {
            if (fobject.Visible != null && fobject.Visible == false)
                return false;

            return true;
        }
        public static bool IsVisible(this Paint paint)
        {
            if (paint.Visible != null && paint.Visible == false)
                return false;

            return true;
        }
        public static bool IsVisible(this Effect effect)
        {
            if (effect.Visible != null && effect.Visible == false)
                return false;

            return true;
        }


        public static bool IsSingleColor(this FObject fobject, out Color color)
        {
            Dictionary<Color, float?> values = new Dictionary<Color, float?>();
            List<bool> flags = new List<bool>();

            IsSingleColorRecursive(fobject, flags, values);

            if (flags.Count > 0)
            {
                color = default;
                return false;
            }

            if (values.Count == 1)
            {
                var singleColorItem = values.First();

                Color singleColor = singleColorItem.Key.SetFigmaAlpha(singleColorItem.Value);

                color = singleColor;
                return true;
            }
            else
            {
                color = default;
                return false;
            }
        }

        public static bool HasImageOrGifRef(this FObject fobject)
        {
            if (fobject.Fills.IsEmpty())
                return false;

            foreach (Paint item in fobject.Fills)
            {
                if (item.Visible.ToBoolNullTrue() == false)
                    continue;

                if (item.ImageRef.IsEmpty() == false || item.GifRef.IsEmpty() == false)
                    return true;
            }

            return false;
        }

        private static void IsSingleColorRecursive(FObject fobject, List<bool> flags, Dictionary<Color, float?> values)
        {
            if (fobject.Fills.IsEmpty() == false)
            {
                foreach (var item in fobject.Fills)
                {
                    if (item.ImageRef.IsEmpty() == false || item.GifRef.IsEmpty() == false)
                    {
                        flags.Add(true);
                        return;
                    }

                    if (item.Type.Contains("SOLID") == false)
                    {
                        flags.Add(true);
                        return;
                    }

                    if (item.IsVisible())
                    {
                        values.TryAddValue<Color, float?>(item.Color, item.Opacity);
                    }
                }
            }

            if (fobject.Strokes.IsEmpty() == false)
            {
                foreach (var item in fobject.Strokes)
                {
                    if (item.ImageRef.IsEmpty() == false || item.GifRef.IsEmpty() == false)
                    {
                        flags.Add(true);
                        return;
                    }

                    if (item.Type.Contains("SOLID") == false)
                    {
                        flags.Add(true);
                        return;
                    }

                    if (item.IsVisible())
                    {
                        values.TryAddValue<Color, float?>(item.Color, item.Opacity);
                    }
                }
            }

            if (fobject.Effects.IsEmpty() == false)
            {
                foreach (var item in fobject.Effects)
                {
                    if (item.Type.Contains("SOLID") == false)
                    {
                        flags.Add(true);
                        return;
                    }

                    if (item.IsVisible())
                    {
                        values.TryAddValue<Color, float?>(item.Color, item.Opacity);
                    }
                }
            }

            if (fobject.Children.IsEmpty())
                return;

            foreach (var item in fobject.Children)
            {
                if (item.ContainsTag(FcuTag.Text))
                    continue;

                IsSingleColorRecursive(item, flags, values);
            }
        }

        public static bool ContainsRoundedCorners(this FObject fobject)
        {
            return fobject.CornerRadius > 0 || (fobject.CornerRadiuses?.Any(radius => radius > 0)).ToBoolNullFalse();
        }

        public static bool IsArcDataFilled(this FObject fobject)
        {
            if (fobject.ArcData.Equals(default(ArcData)))
            {
                return false;
            }

            return fobject.ArcData.EndingAngle < 6.28f;
        }

        public static bool IsGradient(this Paint paint)
        {
            return paint.Type.Contains("GRADIENT");
        }

        public static bool TryGetFirstGradient(this FObject fobject, out Paint gradient)
        {
            if (fobject.Fills.IsEmpty())
            {
                gradient = default;
                return false;
            }

            foreach (Paint _fill in fobject.Fills)
            {
                if (_fill.Visible == false)
                    continue;


            }

            gradient = default;
            return false;
        }

        public static bool IsDefault<T>(this T color)
        {
            if (color == null)
            {
                return true;
            }

            return color.Equals(default(T));
        }

        public static bool TryGetFills(this FObject fobject, FigmaConverterUnity fcu, out Paint solidFill, out Paint gradientFill)
        {
            return fobject.Fills.TryGetColors(fcu, out solidFill, out gradientFill);
        }

        public static bool TryGetStrokes(this FObject fobject, FigmaConverterUnity fcu, out Paint solidFill, out Paint gradientFill)
        {
            if (fobject.StrokeWeight <= 0)
            {
                solidFill = default;
                gradientFill = default;
                return false;
            }

            return fobject.Strokes.TryGetColors(fcu, out solidFill, out gradientFill);
        }

        public static bool TryGetColors(this List<Paint> paints, FigmaConverterUnity fcu, out Paint solidFill, out Paint gradientFill)
        {
            Paint _solidFill = default(Paint);

            Paint _gradientFill = default(Paint);

            foreach (Paint _fill in paints)
            {
                if (_fill.IsVisible() == false)
                    continue;

                if (_fill.Type.Contains("SOLID"))
                {
                    if (_solidFill.IsDefault())
                    {
                        _solidFill = _fill;
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (_fill.Type.Contains("GRADIENT"))
                {
                    if (_gradientFill.IsDefault())
                    {
                        if (_fill.Type.Contains("LINEAR"))
                        {
                            _gradientFill = _fill;
                        }
                        else if (fcu.Settings.ComponentSettings.ImageComponent == ImageComponent.Shape)
                        {
                            if (_fill.Type.Contains("RADIAL"))
                            {
                                _gradientFill = _fill;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            solidFill = _solidFill;
            gradientFill = _gradientFill;

            if (solidFill.IsDefault() && gradientFill.IsDefault())
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}