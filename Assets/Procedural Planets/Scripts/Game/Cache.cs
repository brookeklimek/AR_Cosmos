using UnityEngine;
using System.IO;

namespace ProceduralPlanets
{
    public class Cache : MonoBehaviour
    {
        public string guid;
        public string textureName;
        public Texture2D texture;

        public Texture2D LoadFromCache()
        {
            byte[] _fileData;
            string _filePath = Application.persistentDataPath + "/" + guid + "_" + textureName + ".png";

            if (File.Exists(_filePath))
            {
                _fileData = File.ReadAllBytes(_filePath);
                texture = new Texture2D(2, 2);
                texture.LoadImage(_fileData);
            }
            return texture;
        }

        public void SaveToCache()
        {
            File.WriteAllBytes(Application.persistentDataPath + "/" + guid + "_" + textureName + ".png", texture.EncodeToPNG());
        }
    }
}
