using DA_Assets.Shared;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace DA_Assets.Shared
{
    public class DAInspectorWindow<T1, T2, T3> : EditorWindow
        where T1 : DAInspectorWindow<T1, T2, T3>
        where T2 : Editor
        where T3 : MonoBehaviour
    {
        private int repaintTimeMS = 10;

        protected DAInspector gui => DAInspector.Instance;

        protected T2 inspector;
        public T2 Inspector { get => inspector; set => inspector = value; }

        protected T3 monoBeh;
        public T3 MonoBeh { get => monoBeh; set => monoBeh = value; }

        protected SerializedObject serializedObject;
        public SerializedObject SerializedObject { get => serializedObject; set => serializedObject = value; }

        private Vector2 windowSize = new Vector2(800, 600);
        public Vector2 WindowSize { get => windowSize; set => windowSize = value; }

        private bool init;
        public bool Init { get => init; set => init = value; }

        private bool onEnable;

        public void OnEnable()
        {
            onEnable = true;

            StartRepaint();
        }

        public new void Show()
        {
            Show(immediateDisplay: false);

            this.position = new Rect(
                (Screen.currentResolution.width - windowSize.x * 2) / 2,
                (Screen.currentResolution.height - windowSize.y * 2) / 2,
                windowSize.x,
                windowSize.y);

            OnShow();
        }

        public virtual void OnShow() { }

        public virtual void DrawGUI() { }

        void OnGUI()
        {
            if (onEnable)
            {
                if (monoBeh == null)
                {
                    this.Close();
                    return;
                }
                else
                {
                    if (serializedObject == null)
                    {
                        serializedObject = new SerializedObject(monoBeh);
                    }

                    init = true;

                    OnShow();
                }
            }

            gui.DrawGroup(new Group
            {
                GroupType = GroupType.Vertical,
                Style = GuiStyle.WindowRootBg,
                Body = () =>
                {
                    DrawGUI();
                }
            });
        }

        private async void StartRepaint()
        {
            while (true)
            {
#if UNITY_2020_3_OR_NEWER
                if (this.hasFocus)
#endif
                    this.Repaint();

                await Task.Delay(repaintTimeMS);
            }
        }
    }
}
