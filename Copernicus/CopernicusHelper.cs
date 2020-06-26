using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Net;
using System.Net.Http;
using EarthExplorer.Copernicus;


namespace EarthExplorer.Copernicus
{
    public static class Helper
    {

        public static WebClient SetupToCopernicus(this WebClient client, string login, string pass)
        {
            client.BaseAddress = "https://scihub.copernicus.eu";
            string encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("UTF-8").GetBytes(login + ":" + pass));
            //!!!!!
            client.Headers.Clear();
            client.Headers.Add(HttpRequestHeader.Authorization, $"Basic {encoded}");
            return client;
        }

        public static bool CheckCopernicusConnection(this WebClient client)
        {
            try
            {
                client.DownloadString("https://scihub.copernicus.eu/dhus/odata/v1/$metadata");
            }
            catch (WebException e)
            {
                var response = e.Response as HttpWebResponse;
                if (response == null)
                {
                    Console.WriteLine("No connection");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine("Unauthorized");
                }
                return false;
            }
            return true;
        }
        public static IEnumerable<XElement> LoadAllEntries(WebClient client, OpenSearchRequest request) => LoadAllEntries(client, request.ToString());

        public static IEnumerable<XElement> LoadAllEntries(WebClient client, String request)
        {
            
            
            IEnumerable<XElement> result = new List<XElement>();
            
            var response = client.DownloadString(request + "&start=0&rows=100");
            var root = RemoveAllNamespaces(XDocument.Parse(response).Root);

            var totalResults = Convert.ToInt32(root.Element("totalResults").Value);
            var itemsPerPage = Convert.ToInt32(root.Element("itemsPerPage").Value);

            result = result.Concat(ParseEntriesXml(response));

            for (int current = itemsPerPage; current < totalResults; current += itemsPerPage) {
                response = client.DownloadString(request + $"&start={current}&rows=100");
                result = result.Concat(ParseEntriesXml(response));
            }
            return result;
        }

        public static IEnumerable<XElement> ParseEntriesXml(string inputstring)
        {
            var doc = XDocument.Parse(inputstring);
            var root = RemoveAllNamespaces(doc.Root);
            var el1 = root.Elements("entry");
            return el1;
        }

        public static XElement RemoveAllNamespaces(XElement e)
        {
            return new XElement(e.Name.LocalName,
              (from n in e.Nodes()
               select ((n is XElement) ? RemoveAllNamespaces(n as XElement) : n)),
                  (e.HasAttributes) ?
                    (from a in e.Attributes()
                     where (!a.IsNamespaceDeclaration)
                     select new XAttribute(a.Name.LocalName, a.Value)) : null);
        }

        public static string GetFullDownloadUrl(WebClient connection, string uuid, string spatialRes, string band)
        {
            string path = $"/dhus/odata/v1/Products('{uuid}')/Nodes";
            var s23 = (connection.DownloadString(path));
            var element = RemoveAllNamespaces(XDocument.Parse(s23).Root);

            Func<string, string> findParamInEntry = (param) =>
            {
                return element.Element("entry").Element(param)?.Value;
            };
            path = path + $"('{findParamInEntry("title")}')/Nodes('GRANULE')/Nodes";
            element = RemoveAllNamespaces(XDocument.Parse(connection.DownloadString(path)).Root);
            path = path + $"('{findParamInEntry("title")}')/Nodes('IMG_DATA')/Nodes('{spatialRes}')/Nodes";
            element = RemoveAllNamespaces(XDocument.Parse(connection.DownloadString(path)).Root);

            var filename = element.Elements("entry").Where(x => x.Element("title").Value.Split("_").SkipLast(1).Last() == band).First().Element("title").Value;
            if (filename == null)
                return null;
            else return path + $"('{filename}')/$value";
        }
        public static IEnumerable<Uri> GetImageList(WebClient connection, Guid uuid)
        {
            connection.BaseAddress = "https://scihub.copernicus.eu";
            var data = connection.DownloadString($"/dhus/odata/v1/Products('{uuid}')/Nodes");
            var root = RemoveAllNamespaces(XDocument.Parse(data).Root);
            var title = root.Element("entry").Element("title").Value;
            var snapshotData = connection.DownloadString($"/dhus/odata/v1/Products('{uuid}')/Nodes('{title}')/Nodes('MTD_MSIL2A.xml')/Nodes('Level-2A_User_Product')/Nodes('General_Info')/Nodes('Product_Info')/Nodes('Product_Organisation')/Nodes('Granule_List')/Nodes('Granule')/Nodes");
            root = RemoveAllNamespaces(XDocument.Parse(snapshotData).Root);
            var paths = root.Elements("entry").Elements("properties").Elements("Value").Select(x => x.Value + ".jp2")
            .Select(x => x.Split("/").Select(y => $"Nodes('{y}')").
            Aggregate((concat, str) => $"{concat}/{str}")).
            Select(x => $"https://scihub.copernicus.eu/dhus/odata/v1/Products('{uuid}')/Nodes('{title}')/" + x + "/$value");
            foreach (var path in paths)
            {
                yield return new Uri(path);
            }
        }

        public static Product BuildProduct(XElement entry_)
        {            
            var uuid = Guid.Parse(entry_.Element("id").Value);
            var nameWords = entry_.Element("title").Value.Split("_");
            var tile = new Tile(nameWords.SkipLast(1).Last());
            
            PlatformName platform;
            Enum.TryParse(entry_.Elements("str").First(x => x.Attribute("name") != null && x.Attribute("name").Value == "platformname").Value.Replace("-", "_"), out platform);            
            var gmlFootprint = entry_.Elements("str").First(x => x.Attribute("name") != null && x.Attribute("name").Value == "gmlfootprint").Value;
            //Drop everythinf after .            
            var ingestionDate = System.TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(entry_.Elements("date").First(x => x.Attribute("name") != null && x.Attribute("name").Value == "ingestiondate").Value.Split(".").First().Split("T").First(), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));          
            return new Product {
                Uuid = uuid,
                Tile = tile,
                IngestionDate = ingestionDate,
                GmlFootprint = gmlFootprint,
                Platform = platform
            };
                   
        }
    }
}
