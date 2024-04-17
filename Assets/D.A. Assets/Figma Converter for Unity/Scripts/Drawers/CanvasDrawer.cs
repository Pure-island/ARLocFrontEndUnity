using DA_Assets.FCU.Drawers.CanvasDrawers;
using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#pragma warning disable IDE0003
#pragma warning disable CS0649

namespace DA_Assets.FCU.Drawers
{
    [Serializable]
    public class CanvasDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public IEnumerator DrawToCanvas(List<FObject> fobjects)
        {
            this.ButtonDrawer.Init();
            this.InputFieldDrawer.Init();
            this.I2LocalizationDrawer.AddLanguageSource();

            yield return DrawComponents(fobjects);

            yield return this.ButtonDrawer.SetTargetGraphics();
            yield return this.InputFieldDrawer.SetTargetGraphics();

        }

        public IEnumerator DrawComponents(List<FObject> fobjects)
        {
            Array fcuTags = Enum.GetValues(typeof(FcuTag));

            foreach (FcuTag tag in fcuTags)
            {
                if (tag.GetTagConfig().HasComponent == false)
                    continue;

                int drawnObjectsCount = 0;
                int objectsToDrawCount = monoBeh.TagSetter.TagsCounter[tag];

                if (objectsToDrawCount < 1)
                    continue;

                DACycles.ForEach(fobjects, fobject =>
                {
                    if (fobject.ContainsTag(tag) == false)
                        return;

                    Action onDraw = () =>
                    {
                        drawnObjectsCount++;
                        monoBeh.Events.OnAddComponent?.Invoke(monoBeh, fobject.Data, tag);
                    };

                    DrawByTag(fobject, tag, onDraw).StartDARoutine(monoBeh);

                }, WaitFor.Delay001().WaitTimeF, 150).StartDARoutine(monoBeh);

                int tempCount = -1;
                while (DALogger.WriteLogBeforeEqual(
                    ref drawnObjectsCount,
                    ref objectsToDrawCount,
                    FcuLocKey.log_drawn_count.Localize($"{tag}", drawnObjectsCount, objectsToDrawCount),
                    ref tempCount))
                {
                    yield return WaitFor.Delay1();
                }
            }
        }

        private IEnumerator DrawByTag(FObject fobject, FcuTag tag, Action onDraw)
        {
            try
            {
                if (fobject.Data.GameObject == null)
                {
                    yield break;
                }

                switch (tag)
                {
                    case FcuTag.Shadow:
                        this.ShadowDrawer.Draw(fobject);
                        break;
                    case FcuTag.AutoLayoutGroup:
                        this.AutoLayoutDrawer.Draw(fobject);
                        break;
                    case FcuTag.ContentSizeFitter:
                        this.ContentSizeFitterDrawer.Draw(fobject);
                        break;
                    case FcuTag.InputField:
                        this.InputFieldDrawer.Draw(fobject);
                        break;
                    case FcuTag.Button:
                        this.ButtonDrawer.Draw(fobject);
                        break;
                    case FcuTag.Mask:
                        this.MaskDrawer.Draw(fobject);
                        break;
                    case FcuTag.CanvasGroup:
                        this.CanvasGroupDrawer.Draw(fobject);
                        break;
                    case FcuTag.Placeholder:
                    case FcuTag.Text:
                        this.TextDrawer.Draw(fobject);
                        this.I2LocalizationDrawer.AddI2Localize(fobject);
                        break;
                    case FcuTag.Image:
                        this.ImageDrawer.Draw(fobject);
                        break;
                }
            }
            catch (Exception ex)
            {
                if (tag == FcuTag.Image)
                    monoBeh.ProjectImporter.ImportErrorCount++;

                DALogger.LogError(FcuLocKey.log_cant_draw_object.Localize(fobject.Data.Hierarchy, ex));
            }

            onDraw.Invoke();
            yield return null;
        }

        public void AddCanvasComponent()
        {
            monoBeh.gameObject.TryAddComponent(out Canvas c);
            c.renderMode = RenderMode.ScreenSpaceOverlay;

            if (monoBeh.gameObject.TryGetComponent(out CanvasScaler cs))
                cs.enabled = false;

            monoBeh.gameObject.TryAddComponent(out GraphicRaycaster gr);

            if (MonoBehExtensions.IsExistsOnScene<EventSystem>() == false)
            {
                GameObject go = MonoBehExtensions.CreateEmptyGameObject();
                go.AddComponent<EventSystem>();
                go.AddComponent<StandaloneInputModule>();
                go.name = nameof(EventSystem);
            }
        }

        [SerializeField] ImageDrawer imageDrawer;
        [SerializeProperty(nameof(imageDrawer))]
        public ImageDrawer ImageDrawer => imageDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] TextDrawer textDrawer;
        [SerializeProperty(nameof(textDrawer))]
        public TextDrawer TextDrawer => textDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] AutoLayoutDrawer autoLayoutDrawer;
        [SerializeProperty(nameof(autoLayoutDrawer))]
        public AutoLayoutDrawer AutoLayoutDrawer => autoLayoutDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] ContentSizeFitterDrawer contentSizeFitterDrawer;
        [SerializeProperty(nameof(contentSizeFitterDrawer))]
        public ContentSizeFitterDrawer ContentSizeFitterDrawer => contentSizeFitterDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] MaskDrawer maskDrawer;
        [SerializeProperty(nameof(maskDrawer))]
        public MaskDrawer MaskDrawer => maskDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] ButtonDrawer buttonDrawer;
        [SerializeProperty(nameof(buttonDrawer))]
        public ButtonDrawer ButtonDrawer => buttonDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] InputFieldDrawer inputFieldDrawer;
        [SerializeProperty(nameof(inputFieldDrawer))]
        public InputFieldDrawer InputFieldDrawer => inputFieldDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] I2LocalizationDrawer i2LocalizationDrawer;
        [SerializeProperty(nameof(i2LocalizationDrawer))]
        public I2LocalizationDrawer I2LocalizationDrawer => i2LocalizationDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] ShadowDrawer shadowDrawer;
        [SerializeProperty(nameof(shadowDrawer))]
        public ShadowDrawer ShadowDrawer => shadowDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] CanvasGroupDrawer canvasGroupDrawer;
        [SerializeProperty(nameof(canvasGroupDrawer))]
        public CanvasGroupDrawer CanvasGroupDrawer => canvasGroupDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] GameObjectDrawer gameObjectDrawer;
        [SerializeProperty(nameof(gameObjectDrawer))]
        public GameObjectDrawer GameObjectDrawer => gameObjectDrawer.SetMonoBehaviour(monoBeh);
    }
}
