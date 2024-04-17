using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace DA_Assets.FCU.Model
{
    [Serializable]
    public class SyncData
    {
        [SerializeField] string id;
        [SerializeField] string formattedName;
        [SerializeField] string spriteName;

        [Space]

        [SerializeField] List<FcuTag> tags = new List<FcuTag>();
        [SerializeField] List<int> childIndexes = new List<int>();

        [SerializeField] FigmaConverterUnity fcuInstance;
        [SerializeField] GameObject gameObject;
        [SerializeField] SyncData text;
        [SerializeField] SyncData rootFrame;
        private XmlElement xmlElement;

        [Space]

        [SerializeField] FObject parent;
        [SerializeField] TransformData transformData;
        [SerializeField] Color singleColor;
        [SerializeField] Vector2Int spriteSize;
        [SerializeField] Vector2 size;
        [SerializeField] Vector2 position;
        [SerializeField] FComponent fComponent;

        [Space]

        [SerializeField] FcuImageType fcuImageType;
        [SerializeField] ButtonComponent buttonComponent;

        [Space]

        [SerializeField] int hash;
        [SerializeField] string hashDataTree;
        [SerializeField] int parentIndex;
        [SerializeField] string hierarchy;
        [SerializeField] string spritePath;
        [SerializeField] string link;
        [SerializeField] string humanizedTextPrefabName;
        [SerializeField] string tagReason;
        [SerializeField] int downloadAttempsCount;

        [Space]

        [SerializeField] bool isDownloadable;
        [SerializeField] bool isDuplicate;
        [SerializeField] bool isMutual;
        [SerializeField] bool isEmptyMask;
        [SerializeField] bool isEmpty;

        [Space]

        [SerializeField] bool needDownload;
        [SerializeField] bool needGenerate;
        [SerializeField] bool forceImage;
        [SerializeField] bool forceContainer;
        [SerializeField] bool isInsideDownloadable;
        [SerializeField] bool insideForceImage;
        [SerializeField] bool hasFontAsset;


        public XmlElement XmlElement { get => xmlElement; set => xmlElement = value; }

        public bool ForceImage { get => forceImage; set => forceImage = value; }
        public bool ForceContainer { get => forceContainer; set => forceContainer = value; }
        public int DownloadAttempsCount { get => downloadAttempsCount; set => downloadAttempsCount = value; }
        
        public string TagReason { get => tagReason; set => tagReason = value; }

        public string Id { get => id; set => id = value; }
        public TransformData TransformData { get => transformData; set => transformData = value; }

        public string HumanizedTextPrefabName { get => humanizedTextPrefabName; set => humanizedTextPrefabName = value; }
        public int Hash { get => hash; set => hash = value; }
        public string HashDataTree { get => hashDataTree; set => hashDataTree = value; }
        public int ParentIndex { get => parentIndex; set => parentIndex = value; }

        public List<int> ChildIndexes { get => childIndexes; set => childIndexes = value; }
        public List<FcuTag> Tags { get => tags; set => tags = value; }

        public FObject Parent { get => parent; set => parent = value; }
        public string SpritePath { get => spritePath; set => spritePath = value; }
        public string Link { get => link; set => link = value; }
        public string Hierarchy { get => hierarchy; set => hierarchy = value; }
        public string FormattedName { get => formattedName; set => formattedName = value; }
        public string SpriteName { get => spriteName; set => spriteName = value; }

        public FcuImageType FcuImageType { get => fcuImageType; set => fcuImageType = value; }
        public ButtonComponent ButtonComponent { get => buttonComponent; set => buttonComponent = value; }
        public Vector2Int SpriteSize { get => spriteSize; set => spriteSize = value; }
        public Vector2 Size { get => size; set => size = value; }
        public Vector2 Position { get => position; set => position = value; }
        public FComponent FComponent { get => fComponent; set => fComponent = value; }

        public Color SingleColor { get => singleColor; set => singleColor = value; }
        public bool IsDuplicate { get => isDuplicate; set => isDuplicate = value; }
        public bool NeedDownload { get => needDownload; set => needDownload = value; }
        public bool NeedGenerate { get => needGenerate; set => needGenerate = value; }
        public bool IsEmpty { get => isEmpty; set => isEmpty = value; }
        public bool IsMutual { get => isMutual; set => isMutual = value; }
        public GameObject GameObject { get => gameObject; set => gameObject = value; }
        public SyncData RootFrame { get => rootFrame; set => rootFrame = value; }
        public FigmaConverterUnity FigmaConverterUnity { get => fcuInstance; set => fcuInstance = value; }
        public SyncData Text { get => text; set => text = value; }
        public bool InsideDownloadable { get => isInsideDownloadable; set => isInsideDownloadable = value; }
        public bool InsideForceImage { get => insideForceImage; set => insideForceImage = value; }
        public bool HasFontAsset { get => hasFontAsset; set => hasFontAsset = value; }
    }

    public struct FComponent
    {
        [SerializeField] FObject component;
        public FObject Component { get => component; set => component = value; }

        [SerializeField] string projectUrl;
        public string ProjectUrl { get => projectUrl; set => projectUrl = value; }
    }
    public enum FcuImageType
    {
        None,
        Downloadable,
        Drawable,
        Generative,
        Mask
    }
}