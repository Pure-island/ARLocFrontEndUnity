using DA_Assets.FCU.Model;
using DA_Assets.Shared.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DA_Assets.FCU.Extensions
{
    public static class TagExtensions
    {
        public static void RemoveNotDownloadableTags(this FObject fobject)
        {
            List<FcuTag> newTags = new List<FcuTag>();

            foreach (FcuTag tag in fobject.Data.Tags)
            {
                TagConfig tc = tag.GetTagConfig();

                if (tc.CanBeDownloaded)
                {
                    newTags.Add(tag);
                }
            }

            fobject.Data.Tags = newTags;
        }
        public static void AddTag(this FObject fobject, FcuTag tag)
        {
            if (fobject.Data.Tags == null)
            fobject.Data.Tags = new List<FcuTag>();

            if (tag == FcuTag.Image)
                fobject.RemoveTag(FcuTag.Vector);

            if (fobject.Data.Tags.Contains(tag) == false)
                fobject.Data.Tags.Add(tag);
        }
        public static void RemoveTag(this FObject fobject, FcuTag tag)
        {
            List<FcuTag> tags = new List<FcuTag>();

            foreach (FcuTag item in fobject.Data.Tags)
            {
                if (item != tag)
                {
                    tags.Add(item);
                }
            }

            fobject.Data.Tags = tags;
        }
        public static bool ContainsAnyTag(this FObject fobject, params FcuTag[] tags)
        {
            if (fobject.Data.Tags.IsEmpty())
                return false;

            foreach (FcuTag tag in tags)
            {
                if (fobject.ContainsTag(tag))
                {
                    return true;
                }
            }

            return false;
        }
        public static bool ContainsTag(this FObject fobject, FcuTag tag)
        {
            if (fobject.Data.Tags.IsEmpty())
                return false;

            return fobject.Data.Tags.Contains(tag);
        }

        public static TagConfig GetTagConfig(this FcuTag fcuTag)
        {
            TagConfig tagConfig = FcuConfig.Instance.TagConfigs.FirstOrDefault(x => x.FcuTag == fcuTag);

            if (tagConfig.IsDefault())
            {
                Debug.LogError($"No tag config for '{fcuTag}' tag.");
                return new TagConfig();
            }

            return tagConfig;
        }
        public static string ToLine(this IList<FcuTag> tags)
        {
            return string.Join(", ", tags);
        }
    }
}