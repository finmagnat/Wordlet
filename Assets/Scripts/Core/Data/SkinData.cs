using System;
using Core.Config;
using Core.Generated;

namespace Core.Data
{
    [Serializable]
    public struct SkinData
    {
        public SkinType SkinType;
        public AssetKey GameBackgroundAlias;
        public AssetKey SelectableLetterAlias;
        public AssetKey DragableLetterAlias;
        public AssetKey ListBackgroundAlias;
        public AssetKey BacklightLetterAlias;
        public AssetKey PreviewAlias;
    }
}