using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using UnityEngine.UI;

namespace DA_Assets.FCU.Drawers.CanvasDrawers
{
    [Serializable]
    public class FittersDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public ContentSizeFitter Draw(FObject fobject, bool fitWidth, bool fitHeight)
        {
            fobject.Data.GameObject.TryAddComponent(out ContentSizeFitter contentSizeFitter);

            if (fitWidth)
            {
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
            }

            if (fitHeight)
            {
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
            }

            return contentSizeFitter;
        }
    }
}
