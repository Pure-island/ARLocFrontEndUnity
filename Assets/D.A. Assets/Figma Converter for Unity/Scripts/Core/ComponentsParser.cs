using DA_Assets.Shared;
using System;

namespace DA_Assets.FCU
{
    [Serializable]
    public class ComponentsParser : MonoBehaviourBinder<FigmaConverterUnity>
    {
        /*Dictionary<string,FigmaProject> figmaProjects = new Dictionary<string, FigmaProject>();

        private List<FObject> components = new List<FObject>();
        public List<FObject> Components { get => components; set => components = value; }

        public string GetComponentIdByKey(string componentKey, out string projectId)
        {
            foreach (var figmaProject in figmaProjects)
            {
                foreach (var item in figmaProject.Value.Components)
                {
                    if (item.Value.Key == componentKey)
                    {
                        return item.Key;
                    }
                }
            }

            return null;
        }

        public FObject GetById(string id)
        {
            foreach (var item in components)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }

            return default;
        }

        public IEnumerator Download()
        {
            figmaProjects.Clear();

            foreach (var projectUrl in monoBeh.Settings.MainSettings.ComponentsUrls)
            {
                yield return monoBeh.ProjectDownloader.DownloadProject(projectUrl, @return =>
                {
                    if (@return.Success)
                    {
                        figmaProjects.Add(@return.Result);
                    }
                });
            }

            foreach (var item in figmaProjects)
            {
                FObject virtualPage = new FObject
                {
                    Children = item.Document.Children,
                    Data = new SyncData
                    {
                        GameObject = monoBeh.gameObject,
                        FormattedName = FcuTag.Page.ToString(),
                        Tags = new List<FcuTag>
                        {
                            FcuTag.Page
                        }
                    }
                };

                monoBeh.TagSetter.SetTags(virtualPage);
                monoBeh.ProjectImporter.ConvertTreeToList(virtualPage, components);
            }

            Debug.Log($"components.Count: {components.Count}");

        }*/
    }
}
