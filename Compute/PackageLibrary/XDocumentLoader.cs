using System.Xml.Linq;

namespace Compute
{
    public class XDocumentLoader : IXDocumentLoader
    {
        public XDocument Load(string uri)
        {
            return XDocument.Load(uri);
        }
    }
}