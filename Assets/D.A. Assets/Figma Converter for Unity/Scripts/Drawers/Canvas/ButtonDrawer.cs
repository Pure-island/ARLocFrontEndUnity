using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DA_Assets.FCU.Drawers.CanvasDrawers
{
    [Serializable]
    public class ButtonDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        [SerializeField] List<SyncData> buttons;

        public void Init()
        {
            buttons = new List<SyncData>();
        }

        public void Draw(FObject fobject)
        {
            /*bool hasCustomButtonBackgrounds = false;

            List<FObject> backgrounds = new List<FObject>();

            if (fobject.Data.ChildIndexes.IsEmpty() == false)
            {
                foreach (int cindex in fobject.Data.ChildIndexes)
                {
                    if (monoBeh.CurrentProject.GetByIndex(cindex).ContainsTag(FcuTag.Image))
                        backgrounds.Add(monoBeh.CurrentProject.GetByIndex(cindex));

                    if (monoBeh.CurrentProject.GetByIndex(cindex).ContainsAnyTag(
                        FcuTag.BtnDefault,
                        FcuTag.BtnDisabled,
                        FcuTag.BtnHover,
                        FcuTag.BtnPressed,
                        FcuTag.BtnSelected))
                    {
                        hasCustomButtonBackgrounds = true;
                        break;
                    }
                }
            }

            bool sameDownloadType = backgrounds.GroupBy(x => x.Data.FcuImageType == FcuImageType.Downloadable).Count() == 1;

            if (monoBeh.UsingDaButton() || hasCustomButtonBackgrounds)
            {

            }
            else
            {
                DrawDefaultButton(fobject);
            }*/

            fobject.Data.GameObject.TryAddComponent(out Button btn);
            buttons.Add(fobject.Data);
        }

        public IEnumerator SetTargetGraphics()
        {
            foreach (SyncData syncData in buttons)
            {
                SyncHelper[] syncHelpers = syncData.GameObject.GetComponentsInChildren<SyncHelper>(true).Skip(1).ToArray();

                if (syncHelpers.Length == 0)
                {
                    continue;
                }

                if (syncData.ButtonComponent == ButtonComponent.UnityButton)
                {
                    Button btn = syncData.GameObject.GetComponent<Button>();

                    SetButtonTargetGraphic(btn, syncHelpers);
                }
                /*  if (btnMeta.ButtonComponent == ButtonComponent.Default)
                  {
                     
                  }
                  else if (btnMeta.ButtonComponent == ButtonComponent.FcuButton)
                  {
                      DAButton fcuBtn = btnMeta.gameObject.GetComponent<DAButton>();

                      Set_FcuButtonTargetGraphic(fcuBtn, childMetas);
                  }
                */
                yield return WaitFor.Delay001();
            }

            buttons.Clear();
        }

        private void SetButtonTargetGraphic(Button btn, SyncHelper[] metas)
        {
            bool exists = metas.First().TryGetComponent(out Graphic gr1);

            //If the first element of the hierarchy can be used as a target graphic.
            if (exists)
            {
                btn.targetGraphic = gr1;
            }
            else
            {
                //If there is at least some image, assign it to the targetGraphic.
                foreach (SyncHelper meta in metas)
                {
                    if (meta.TryGetComponent(out Image gr2))
                    {
                        btn.targetGraphic = gr2;
                        return;
                    }
                }

                //If there is at least some graphic, assign it to the targetGraphic.
                foreach (SyncHelper meta in metas)
                {
                    if (meta.TryGetComponent(out Graphic gr3))
                    {
                        btn.targetGraphic = gr3;
                        return;
                    }
                }

                //If there is a graphic on the button itself, assign it to the targetGraphic.
                if (btn.TryGetComponent(out Graphic gr4))
                {
                    btn.targetGraphic = gr4;
                }
            }
        }

        public IEnumerator DrawDAButton(FObject fobject)
        {
#if DA_BUTTON_EXISTS
            /*controller.Log($"InstantiateButton | FcuButton | {fobject.FixedName}");

            fobject.Data.GameObject.TryAddComponent(out DAButton btn);

            DATargetGraphic targetGraphic = new DATargetGraphic();

            foreach (int cindex in fobject.Data.ChildIndexes)
            {
                foreach (var item in monoBeh.CurrentProject.GetByIndex(cindex).Data.Tags)
                {
                    if (monoBeh.CurrentProject.GetByIndex(cindex).ContainsTag(FcuTag.Text))
                    {
                        DAButtonEvent buttonEvent = DAButtonEvent.CLICK;

                        switch (item)
                        {
                            case FcuTag.BtnDefault:
                                targetGraphic.DefaultColor = monoBeh.CurrentProject.GetByIndex(cindex).GetTextColor();
                                break;
                            case FcuTag.BtnHover:
                                buttonEvent = DAButtonEvent.HOVER;
                                break;
                            case FcuTag.BtnPressed:
                                buttonEvent = DAButtonEvent.CLICK;
                                break;
                            case FcuTag.BtnSelected:
                                continue;
                                break;
                            case FcuTag.BtnDisabled:
                                continue;
                                break;
                        }

                        DAButtonAnimation currentAnimation = DAButtonConfig.Instance.DefaultAnimations.First(x => x.Event == buttonEvent).CopyClass();
                        currentAnimation.AnimationItems[0].Color = new SerializableTuple<bool, Color>(true, monoBeh.CurrentProject.GetByIndex(cindex).GetTextColor());

                        targetGraphic.Animations.Add(currentAnimation);
                    }
                }
            }*/
#endif
            yield break;
        }
    }
}
