using System.Xml.Linq;

namespace Compute
{
    public interface IXDocumentLoader
    {
        XDocument Load(string uri);
    }
}