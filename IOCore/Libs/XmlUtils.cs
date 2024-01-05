using System.IO;
using System.Text;
using System.Xml;

namespace IOCore.Libs
{
    public class XmlUtils
    {
        public static XmlDocument LoadXmlDocument(string filePath)
        {
            var text = File.ReadAllText(filePath);
            var bytes = Encoding.UTF8.GetBytes(text);

            using var memoryStream = new MemoryStream(bytes);
            memoryStream.Flush();
            memoryStream.Position = 0;

            var document = new XmlDocument();
            document.Load(memoryStream);

            return document;
        }

        public static void Clear(string filePath)
        {
            Utils.DeleteFileOrDirectory(filePath);
        }
    }
}