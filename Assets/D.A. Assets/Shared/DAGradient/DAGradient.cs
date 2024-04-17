using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace DA_Assets.DAG
{
    [AddComponentMenu("UI/Effects/D.A. Gradient")]
    public class DAGradient : BaseMeshEffect
    {
        [SerializeField] Gradient gradient = new Gradient();
        [SerializeField] DAColorBlendMode blendMode = DAColorBlendMode.Overlay;
        [SerializeField, Range(0, 1)] float intensity = 1f;
        [SerializeField, Range(0, 360)] float angle = 0f;

        [SerializeProperty(nameof(blendMode))]
        public DAColorBlendMode BlendMode
        {
            get => blendMode;
            set
            {
                blendMode = value;
                graphic?.SetVerticesDirty();
            }
        }

        [SerializeProperty(nameof(intensity))]
        public float Intensity
        {
            get => intensity;
            set
            {
                intensity = Mathf.Clamp01(value);
                graphic?.SetVerticesDirty();
            }
        }

        [SerializeProperty(nameof(angle))]
        public float Angle
        {
            get => angle;
            set
            {
                angle = value.NormalizeAngle();
                graphic?.SetVerticesDirty();
            }
        }

        [SerializeProperty(nameof(gradient))]
        public Gradient Gradient
        {
            get => gradient;
            set
            {
                gradient = value;
                graphic?.SetVerticesDirty();
            }
        }

        public override void ModifyMesh(VertexHelper vertexHelper)
        {
            if (enabled == false)
                return;

            if (vertexHelper == null)
                return;

            RectTransform rectTransform = transform.GetComponent<RectTransform>();

            UIVertex uiVertex = new UIVertex();
            Vector2 rectSize = rectTransform.rect.size;
            Vector2 rectCenter = rectTransform.rect.center;

            for (int i = 0; i < vertexHelper.currentVertCount; i++)
            {
                vertexHelper.PopulateUIVertex(ref uiVertex, i);

                float a = Mathf.Deg2Rad * angle;
                float scale = Mathf.Abs(Mathf.Sin(a)) + Mathf.Abs(Mathf.Cos(a));
                float x = (uiVertex.position.x - rectCenter.x) / rectSize.x;
                float y = (uiVertex.position.y - rectCenter.y) / rectSize.y;

                Vector2 relativePosition = new Vector2(x, y);
                Vector2 rotatedPosition = relativePosition.Rotate(angle);
                Vector2 scaledPosition = rotatedPosition / scale;
                Vector2 centeredPosition = new Vector2(0.5f, 0.5f);
                Vector2 gradientPosition = scaledPosition + centeredPosition;

                Color gradientColor = gradient.Evaluate(gradientPosition.y);
                uiVertex.color = DAColorBlender.Blend(uiVertex.color, gradientColor, blendMode, intensity);

                vertexHelper.SetUIVertex(uiVertex, i);
            }
        }
    }
}