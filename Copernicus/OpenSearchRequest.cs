using System.Collections.Generic;
using System;
using System.Linq;

namespace EarthExplorer.Copernicus {
    public class OpenSearchRequest
    {
        public IEnumerable<string> tiles;
        //TODO: from 00 00 to 23 59
        public DateTime from = DateTime.Now.AddDays(-10);
        public DateTime to = DateTime.Now;

        public int cloudCoverPercenrage;

        public int start = 0;
        public int rows = 100;
        public PlatformName platform;

        //TODO: All platform supporting
        public override string ToString()
        {
            string output = "/dhus/search?q=(producttype:S2MSI2A AND ";
            switch (platform)
            {
                case PlatformName.Sentinel_2: output += "platformname:Sentinel-2 AND "; break;
            }
            output += $"ingestiondate:[{from.ToString("yyyy-MM-ddTHH:mm:ssZ")} TO {to.ToString("yyyy-MM-ddTHH:mm:ssZ")}] AND ";
            //at least one
            if (tiles.Any())
            {
                output += $"((filename:*_{tiles.First()}_*) ";
                //at least two
                if (tiles.Skip(1).Any())
                {
                    //from second to all
                    foreach (var tile in tiles.Skip(1))
                    {
                        output += $"OR (filename:*_{tile}_*)";
                    }
                }
                output += ") AND ";
            }
            output += $"cloudcoverpercentage:[0 TO {cloudCoverPercenrage}])&";
            output += $"start={start}&";
            output += $"rows={rows}";
            return output;
        }
    }
}