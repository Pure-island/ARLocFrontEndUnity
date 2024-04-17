using DA_Assets.Shared;
using System;

namespace DA_Assets.FCU
{
    [Serializable]
    public class DelegateHolder : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public ShowRateMe ShowRateMe { get; set; }
    }

    public delegate void ShowRateMe(int errorCount, int componentCount);
}