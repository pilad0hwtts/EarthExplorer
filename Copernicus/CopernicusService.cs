using System;
using System.IO;
using System.Net;

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Microsoft.Extensions.Logging;


namespace EarthExplorer.Copernicus {

    public class WebClientBasedExecutor : IHttpRequestExecutor {
        private readonly WebHeaderCollection _headers;
        public WebClientBasedExecutor(WebHeaderCollection headers) {
            this._headers = headers;
        }
        public Stream GetWebData(Uri uri) {
            using (var client = new WebClient()) {
                client.Headers = _headers;
                return client.OpenRead(uri);
            }
        }
    }
    public interface IHttpRequestExecutor {
        Stream GetWebData(Uri uri);
    }
    public class CopernicusService {
        protected readonly ILogger _log;
        protected readonly IHttpRequestExecutor _webExecutor;
        public CopernicusService(ILogger<CopernicusService> logger, IHttpRequestExecutor webExecutor) {
            this._webExecutor = webExecutor;
            this._log = logger;
        }

        public XElement[] GetOpenSearchEntries(OpenSearchRequest request) {
            Func<string, IEnumerable<XElement>> parseEntriesAlias = (string input) => {
                var doc = XDocument.Parse(input);
                var root = Utils.RemoveAllNamespaces(doc.Root);
                var el1 = root.Elements("entry");
                return el1;
            };

            IEnumerable<XElement> result = new List<XElement>();
            try {
                var response = GetAsString(new Uri("https://scihub.copernicus.eu/" + request.ToString()));
                var root = Utils.RemoveAllNamespaces(XDocument.Parse(response).Root);
                var totalResults = Convert.ToInt32(root.Element("totalResults").Value);
                var itemsPerPage = Convert.ToInt32(root.Element("itemsPerPage").Value);

                result = result.Concat(parseEntriesAlias(response));

                for (int current = itemsPerPage; current < totalResults; current += itemsPerPage) {
                    request.start = current;
                    response = GetAsString(new Uri("https://scihub.copernicus.eu/" + request.ToString()));
                    result = result.Concat(parseEntriesAlias(response));
                }
            } catch (Exception ex) {
                _log.LogError(ex, "Loading entries failed with error!");
            }
            return result.ToArray();
        }

        public Uri[] GetImagesOfProduct(Guid guid) {
            try {
                var data = GetAsString(new Uri($"https://scihub.copernicus.eu/dhus/odata/v1/Products('{guid}')/Nodes"));
                var root = Utils.RemoveAllNamespaces(XDocument.Parse(data).Root);

                var title = root.Element("entry").Element("title").Value;
                var snapshotData = GetAsString(new Uri($"https://scihub.copernicus.eu/dhus/odata/v1/Products('{guid}')/Nodes('{title}')/Nodes('MTD_MSIL2A.xml')/Nodes('Level-2A_User_Product')/Nodes('General_Info')/Nodes('Product_Info')/Nodes('Product_Organisation')/Nodes('Granule_List')/Nodes('Granule')/Nodes"));
                root = Utils.RemoveAllNamespaces(XDocument.Parse(snapshotData).Root);

                var paths = root.Elements("entry").Elements("properties").Elements("Value").Select(x => x.Value + ".jp2")
            .Select(x => x.Split("/").Select(y => $"Nodes('{y}')").
            Aggregate((concat, str) => $"{concat}/{str}")).
            Select(x => new Uri($"https://scihub.copernicus.eu/dhus/odata/v1/Products('{guid}')/Nodes('{title}')/" + x + "/$value"));

                return paths.ToArray();
            } catch (Exception ex) {
                _log.LogError(ex, "Loading images list failed with error");
                return new Uri[] { };
            }

        }


        #region Checks
        public bool CheckInternetConnection() {
            try {
                GetAsString(new Uri("http://google.com"));
                return true;
            } catch (Exception ex) {
                _log.LogError(ex, "Checking internet connection fault with error");
                return false;
            }
        }

        public bool CheckCopernicusConnection() {
            try {
                GetAsString(new Uri("https://scihub.copernicus.eu/dhus/odata/v1/$metadata"));
                return true;
            } catch (Exception e) {
                _log.LogError(e, "Check copernicus connection fault with error");
                return false;
            }
        }
        #endregion

        private string GetAsString(Uri uri) {
            using (var stream = _webExecutor.GetWebData(uri)) {
                using (var reader = new StreamReader(stream)) {
                    return reader.ReadToEnd();
                }
            }
        }
    }

    internal class Utils {
        public static XElement RemoveAllNamespaces(XElement e) {
            return new XElement(e.Name.LocalName,
              (from n in e.Nodes()
               select ((n is XElement) ? RemoveAllNamespaces(n as XElement) : n)),
                  (e.HasAttributes) ?
                    (from a in e.Attributes()
                     where (!a.IsNamespaceDeclaration)
                     select new XAttribute(a.Name.LocalName, a.Value)) : null);
        }
    }
}