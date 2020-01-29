using System;
using ReactUnity;
using UnityEngine;

namespace Models
{
    [Serializable]
    public class ItemModel : IModel
    {
        public string Name;
        public string Binary;
        public string[] Labels;
        public string Preview;
        public string Description;

        public Texture2D PreviewTexture { get; set; }
        public bool BinaryExists { get; set; }
    }
}