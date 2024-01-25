//  This class is used for serializing and deserializing MazeFile objects to and from xml files.


using System.IO;
using System.Xml.Serialization;


namespace JesterwaySoftware.CustomMazeGenerator
{
    public static class Serialization
    {
        public static void SerializeFile(MazeFile mazeObject, string chosenPath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MazeFile));
            using (TextWriter writer = new StreamWriter(chosenPath))
            {
                serializer.Serialize(writer, mazeObject);
            }
        }


        public static MazeFile DeSerializeFile(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MazeFile));
            FileStream fileStream = new FileStream(path, FileMode.Open);
            MazeFile deserializedResult = (MazeFile)serializer.Deserialize(fileStream);
            fileStream.Close();
            return deserializedResult;
        }
    }
}