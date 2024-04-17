using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEditor.Callbacks;
using UnityEngine;

#pragma warning disable CS0649

namespace DA_Assets.Shared
{
    internal class DAWebConfig
    {
        internal static WebConfig WebConfig => webConfig;
        private static WebConfig webConfig = default;

        internal static bool HasWebConfig => hasWebConfig;
        private static bool hasWebConfig = false;

        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            GetWebConfig();
        }

        private static void GetWebConfig()
        {
            try
            {
                Thread t = new Thread(() =>
                {
                    string url = "https://da-assets.github.io/site/files/webConfig.json";
                    string json = new WebClient().DownloadString(url);

                    webConfig = JsonUtility.FromJson<WebConfig>(json);
                    hasWebConfig = true;
                });

                t.Start();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }
    }

    [Serializable]
    internal struct WebConfig
    {
        [SerializeField] List<Asset> assets;
        internal List<Asset> Assets => assets;
    }

    [Serializable]
    internal struct Asset
    {
        [SerializeField] string name;
        [SerializeField] AssetType type;
        [SerializeField] int oldVersionDaysCount;
        [SerializeField] List<AssetVersion> versions;

        internal string Name => name;
        internal AssetType Type => type;
        internal int OldVersionDaysCount => oldVersionDaysCount;
        internal List<AssetVersion> Versions => versions;
    }

    [Serializable]
    internal struct AssetVersion
    {
        [SerializeField] string version;
        [SerializeField] VersionType versionType;
        [SerializeField] string releaseDate;
        [SerializeField] string description;

        internal string Version => version;
        internal VersionType VersionType => versionType;
        internal string ReleaseDate => releaseDate;
        internal string Description => description;
    }

    internal enum AssetType
    {
        fcu = 0,
        dab = 1,
    }

    internal enum VersionType
    {
        stable = 0,
        beta = 1,
        buggy = 2
    }
}
