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
        public Sentinel_2.ProductType productType = Sentinel_2.ProductType.S2MSI2A; 

        //TODO: All platform supporting
        public override string ToString()
        {
            string output = "";
            switch (productType) {                
                case Sentinel_2.ProductType.S2MS2Ap: output = "/dhus/search?q=(producttype:S2MS2Ap AND "; break;
                case Sentinel_2.ProductType.S2MSI1C: output = "/dhus/search?q=(producttype:S2MSI1C AND "; break;
                case Sentinel_2.ProductType.S2MSI2A: output = "/dhus/search?q=(producttype:S2MSI2A AND "; break;
            }
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