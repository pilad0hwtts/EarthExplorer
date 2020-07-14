using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using CsvHelper;

namespace EarthExplorer.LandsatAws
{
    public static class LandsatAwsHelper
    {
        public class Landsat8CsvInfo
        {
            public string productId { get; set; }
            public string entityId { get; set; }
            public DateTime acquisitionDate { get; set; }
            public double cloudCover { get; set; }
            public string processingLevel { get; set; }
            public int path { get; set; }
            public int row { get; set; }
            public double min_lat { get; set; }
            public double min_lon { get; set; }
            public double max_lat { get; set; }
            public double max_lon { get; set; }
            public string download_url { get; set; }

        }

        public static Landsat8CsvInfo[] GetLastLandsat8()
        {
            string[] codes = { "132023", "132024", "137022", "137023", "136022", "136023", "134023", "134024", "137021", "137022", "137023", "136021", "136022", "136023", "134021", "134022", "134023", "136021", "136022", "137020", "137021", "137022", "139019", "139020", "139021", "137022", "137023", "139021", "139022" };
            using (var client = new WebClient())
            {
                using (var netStream = client.OpenRead("https://landsat-pds.s3.amazonaws.com/c1/L8/scene_list.gz"))
                {
                    using (var gzipStream = new System.IO.Compression.GZipStream(netStream, System.IO.Compression.CompressionMode.Decompress))
                    {
                        using (var reader = new System.IO.StreamReader(gzipStream))
                        {
                            using (var csvStream = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
                            {     
                                return csvStream.GetRecords<Landsat8CsvInfo>().ToArray()                           ;
                                //return csvStream.GetRecords<Landsat8CsvInfo>().Where(c => c.cloudCover < 50 && c.acquisitionDate > DateTime.Now.AddDays(-120) && codes.Contains(c.path.ToString("D3") + c.row.ToString("D3")));
                            }
                        }
                    }
                }
            }
        }
    }
}
