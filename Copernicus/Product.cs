using System;

namespace EarthExplorer.Copernicus {
    public class Product
    {
        
        public System.Guid Uuid { get; set; }
        //todo: type
        //public dynamic Type { get; }

        public PlatformName Platform { get; set; }

        public DateTime IngestionDate {get; set;  }

        public Tile Tile { get; set;  }

        public String GmlFootprint {get; set; }
    }
}
