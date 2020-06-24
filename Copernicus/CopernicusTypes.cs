namespace EarthExplorer.Copernicus
{
    public enum PlatformName
    {
        Sentinel_1, Sentinel_2, Sentinel_4, Sentinel_5
    }

    namespace Sentinel_1
    {
        public enum ProductType
        {
            SLC, GRD, OCN
        }
        public enum PolarisationMode
        {
            HH, VV, HV, VH
        }
        public enum SwathIdentifier
        {
            S1, S2, S3, S4, S5, S6, IW, IW1, IW2, IW3, EW, EW1, EW2, EW3, EW4, EW5
        }

    }

    namespace Sentinel_2
    {
        public enum ProductType
        {
            S2MSI2A, S2MSI1C, S2MS2Ap
        }

        public class ImgData {
            public enum R10m {
                AOT, B02, B03, B04, B08, TCI, WVP
            }

            public enum R20m {
                AOT, B02, B03, B04, B05, B06, B07, B8A, B11, B12, SCL, TCI, WVP
            }

            public enum R60m {
                AOT, B01, B02, B03, B04, B05, B06, B07, B8A, B09, B11, B12, SCL, TCI, WVP
            }
        }
    }

    namespace Sentinel_4
    {
        public enum ProductType
        {
            SR_1_SRA___, SR_1_SRA_A, SR_1_SRA_BS, SR_2_LAN___, OL_1_EFR___, OL_1_ERR___, OL_2_LFR___, OL_2_LRR___, SL_1_RBT___, SL_2_LST___, SY_2_SYN___, SY_2_V10___, SY_2_VG1___, SY_2_VGP___
        }
    }

    namespace Sentinel_5
    {
        public enum ProductType
        {
            L1B_IR_SIR, L1B_IR_UVN, L1B_RA_BD1, L1B_RA_BD2, L1B_RA_BD3, L1B_RA_BD4, L1B_RA_BD5, L1B_RA_BD6, L1B_RA_BD7, L1B_RA_BD8, L2__AER_AI, L2__AER_LH, L2__CH4, L2__CLOUD_, L2__CO____, L2__HCHO__, L2__NO2___, L2__NP_BD3, L2__NP_BD6, L2__NP_BD7, L2__O3_TCL, L2__O3____, L2__SO2___
        }
    }

    public class Tile
    {
        public string Code { get; private set; }

        public Tile(string code_) => Code = code_;
    }

}