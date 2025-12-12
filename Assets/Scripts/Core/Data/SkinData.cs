using System;
using Core.Config;
using Core.Generated;

namespace Core.Data
{
    [Serializable]
    public struct SkinData
    {
        public SkinType SkinType;
        public string GameBackgroundAlias;
        public string SelectableLetterAlias;
        public string DragableLetterAlias;
        public string ListBackgroundAlias;
        public string BacklightLetterAlias;
        public string PreviewAlias;
    }
}