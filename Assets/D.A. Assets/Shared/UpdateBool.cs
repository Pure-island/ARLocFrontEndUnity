using System;
using UnityEngine;

namespace DA_Assets.Shared
{
    [Serializable]
    public class UpdateBool
    {
        [SerializeField] bool value;
        [SerializeField] bool temp;
        public bool Value { get; set; }
        public bool Temp { get; set; }
    }
}