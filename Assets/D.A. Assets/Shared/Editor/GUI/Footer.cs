using DA_Assets.FCU;
using UnityEngine;

namespace DA_Assets.Shared
{
    internal class Footer
    {
        public static DAInspector gui => DAInspector.Instance;
        public static void DrawFooter()
        {
            gui.Space30();

            gui.DrawGroup(new Group
            {
                GroupType = GroupType.Horizontal,
                Flexible = true,    
                Body = () =>
                {
                    if (gui.LinkButton(FcuLocKey.label_made_by.Localize(DAConstants.Publisher)))
                    {
                        Application.OpenURL(DAConstants.SiteLink);
                    }
                }
            });
        }
    }
}
