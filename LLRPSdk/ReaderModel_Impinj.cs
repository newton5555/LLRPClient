
#nullable disable
namespace LLRPSdk
{
    /// <summary>
    /// All the possible reader models, mapped to their model numbers.
    /// </summary>
    public enum ReaderModel_Impinj
    {
        /// <summary>Unknown reader model</summary>
        Unknown = 0,
        /// <summary>Speedway R220</summary>
        SpeedwayR220 = 2001001, // 0x001E8869
        /// <summary>Speedway R420</summary>
        SpeedwayR420 = 2001002, // 0x001E886A
        /// <summary>xPortal</summary>
        XPortal = 2001003, // 0x001E886B
        /// <summary>xArrayWM</summary>
        XArrayWM = 2001004, // 0x001E886C
        /// <summary>xArrayEAP</summary>
        XArrayEAP = 2001006, // 0x001E886E
        /// <summary>xArray</summary>
        XArray = 2001007, // 0x001E886F
        /// <summary>xSpan</summary>
        XSpan = 2001008, // 0x001E8870
        /// <summary>Speedway R120</summary>
        SpeedwayR120 = 2001009, // 0x001E8871
        /// <summary>R700</summary>
        R700 = 2001052, // 0x001E889C
        /// <summary>R510</summary>
        R510 = 2001054, // 0x001E889E
        /// <summary>R720</summary>
        R720 = 2001056, // 0x001E88A0
        /// <summary>R515</summary>
        R515 = 2002001, // 0x001E8C51
    }

    public enum ReaderModel_Zebra
    {
        Unknown = 0,
        FX9600= 96008,
    }


    public enum ReaderModel_Seuic
    {
        Unknown = 0,
        UF40 = 40,
    }
}
