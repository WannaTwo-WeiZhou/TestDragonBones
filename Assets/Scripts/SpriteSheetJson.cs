using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameModel.Data
{
    [Serializable]
    public class SpriteSheetJson
    {
        public SpriteSheetFrame[] frames;

        public static SpriteSheetJson CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<SpriteSheetJson>(jsonString);
        }
    }

    [Serializable]
    public class SpriteSheetFrame
    {
        public string filename;
        public SpriteSheetFrameRect frame;
    }
    [Serializable]
    public class SpriteSheetFrameRect
    {
        public int x;
        public int y;
        public int w;
        public int h;
    }
}
