using System.Xml.Linq;

namespace PackageLibrary
{
    public interface IXDocumentLoader
    {
        XDocument Load(string uri);
    }
}