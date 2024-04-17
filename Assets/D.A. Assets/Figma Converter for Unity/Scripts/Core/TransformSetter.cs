using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DA_Assets.FCU
{
    [Serializable]
    public class TransformSetter : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public IEnumerator SetFigmaTransform(List<FObject> fobjects)
        {
            DALogger.Log(FcuLocKey.log_start_setting_transform.Localize());

            if (monoBeh.UsingPUI())
            {
                yield return monoBeh.RefreshTransform2023();
            }

            yield return DACycles.ForEach(fobjects, fobject =>
            {
                if (fobject.Data.GameObject == null)
                    return;

                RectTransform rt = fobject.Data.GameObject.GetComponent<RectTransform>();

                Rect rect = GetRect(fobject);
                fobject.Data.Size = rect.size;
                fobject.Data.Position = rect.position;

                rt.SetSmartAnchor(AnchorType.TopLeft);
                rt.SetSmartPivot(PivotType.TopLeft);

                rt.sizeDelta = fobject.Data.Size;
                rt.position = fobject.Data.Position;

                fobject.SetFigmaRotation(monoBeh);

                if (fobject.IsNeedRotate())
                {
                    if (fobject.TryGetRectTranformPosition(out Vector3 rtPos))
                    {
                        rt.localPosition = rtPos;
                    }
                }

                fobject.Data.GameObject.transform.localScale = Vector3.one;

                fobject.Data.GameObject.SetActive(fobject.IsVisible());

            }, WaitFor.Delay01().WaitTimeF, 500);

            yield return DACycles.ForEach(fobjects, fobject =>
            {
                if (fobject.Data.GameObject == null)
                    return;

                RectTransform rt = fobject.Data.GameObject.GetComponent<RectTransform>();

                rt.SetSmartPivot(PivotType.MiddleCenter);

                //The anchors of the child components of a AutoLayoutGroup are controlled by the AutoLayoutGroup
                if (fobject.ContainsTag(FcuTag.Frame))
                {
                    rt.SetSmartAnchor(AnchorType.TopLeft);
                }
                else if (fobject.Data.Parent.ContainsTag(FcuTag.AutoLayoutGroup) == false)
                {
                    rt.SetSmartAnchor(fobject.GetFigmaAnchor());
                }

            }, WaitFor.Delay01().WaitTimeF, 500);
        }


        public Rect GetRect(FObject fobject)
        {
            Rect rect = new Rect();
            Vector2 position = new Vector2();
            Vector2 size = new Vector2();

            bool hasBoundingSize = fobject.GetBoundingSize(out Vector2 bSize);
            bool hasBoundingPos = fobject.GetBoundingPosition(out Vector2 bPos);

            bool hasRenderSize = fobject.GetRenderSize(out Vector2 rSize);
            bool hasRenderPos = fobject.GetRenderPosition(out Vector2 rPos);

            int state = 0;

            if (fobject.IsNeedRotate())
            {
                state = 1;

                position.x = bPos.x;
                position.y = monoBeh.IsUGUI() ? -bPos.y : bPos.y;

                size.x = fobject.Size.x;
                size.y = fobject.Size.y;
            }
            else if (bSize == rSize)
            {
                state = 2;

                position.x = bPos.x;
                position.y = monoBeh.IsUGUI() ? -bPos.y : bPos.y;

                size.x = bSize.x;
                size.y = bSize.y;
            }
            else if (fobject.Data.FcuImageType == FcuImageType.Downloadable)
            {
                bool hasScaleInName = fobject.Data.SpritePath.TryParseSpriteName(out float scale, out System.Numerics.BigInteger hash);

                if (hasScaleInName)
                {
                    state = 5;

                    if (hasRenderPos)
                    {
                        position.x = rPos.x;
                        position.y = monoBeh.IsUGUI() ? -rPos.y : rPos.y;
                    }
                    else
                    {
                        position.x = bPos.x;
                        position.y = monoBeh.IsUGUI() ? -bPos.y : bPos.y;
                    }

                    size.x = fobject.Data.SpriteSize.x / scale;
                    size.y = fobject.Data.SpriteSize.y / scale;
                }
            }
            else if (monoBeh.CurrentProject.TryGetByIndex(fobject.Data.ParentIndex, out FObject parent) && monoBeh.IsUGUI())
            {
                if (parent.ContainsTag(FcuTag.AutoLayoutGroup))
                {
                    if (parent.Data.GameObject.TryGetComponent(out HorizontalOrVerticalLayoutGroup layoutGroup))
                    {
                        int leftp = layoutGroup.padding.left;
                        int rightp = layoutGroup.padding.right;

                        float newX = bSize.x;
                        float newY = bSize.y;

                        float parentSize = parent.Data.Size.x;

                        if (leftp + rightp + newX > parentSize)
                        {
                            float excess = (leftp + rightp + newX) - parentSize;
                            float totalPadding = leftp + rightp;

                            float leftFactor = leftp / totalPadding;
                            float rightFactor = rightp / totalPadding;

                            int newLeft = leftp - (int)Math.Floor(excess * leftFactor);
                            int newRight = rightp - (int)Math.Floor(excess * rightFactor);

                            if (newLeft > 0 && newRight > 0)
                            {
                                state = 3;

                                position.x = bPos.x;
                                position.y = monoBeh.IsUGUI() ? -bPos.y : bPos.y;

                                size.x = newX;
                                size.y = newY;

                                layoutGroup.padding.left = newLeft;
                                layoutGroup.padding.right = newRight;
                            }
                        }
                    }
                }
            }

            if (state == 0)
            {
                position.x = bPos.x;
                position.y = monoBeh.IsUGUI() ? -bPos.y : bPos.y;

                size.x = bSize.x;
                size.y = bSize.y;
            }

            if (size.y == 0 && fobject.Strokes.IsEmpty() == false)
            {
                size.y = fobject.StrokeWeight;
            }

            monoBeh.Log($"{nameof(GetRect)} | {fobject.Data.Hierarchy} | state: {state} | {size}", FcuLogType.Transform);

            rect.size = size;
            rect.position = position;

            return rect;
        }
    }
}