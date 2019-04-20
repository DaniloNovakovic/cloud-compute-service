using System.Xml.Linq;

namespace PackageLibrary
{
    public class XDocumentLoader : IXDocumentLoader
    {
        public XDocument Load(string uri)
        {
            return XDocument.Load(uri);
        }
    }
}