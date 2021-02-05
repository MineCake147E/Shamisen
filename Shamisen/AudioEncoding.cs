using System;
using System.Collections.Generic;
using System.Text;

using Shamisen.Formats;

namespace Shamisen
{
    /// <summary>
    /// Defines known encoding types.
    /// </summary>
    public enum AudioEncoding : ushort
    {
        /// <summary>
        /// Unknown format
        /// </summary>
        Unknown = 0x0000,

        /// <summary>
        /// Microsoft PCM Format
        /// </summary>
        [FixedBitDepth]
        LinearPcm = 0x0001,

        /// <summary>
        /// Microsoft ADPCM Format
        /// </summary>
        MsAdpcm = 0x0002,

        /// <summary>
        /// IEEE 754 Single Precision Floating-Point Number
        /// </summary>
        IeeeFloat = 0x0003,

        /// <summary>
        /// Vector sum excited linear prediction Compaq Computer Corporation
        /// </summary>
        Vselp = 0x0004,

        /// <summary>
        /// IBM Corporation
        /// </summary>
        IbmCvsd = 0x0005,

        /// <summary>
        /// A-law format by Microsoft Corporation
        /// </summary>
        Alaw = 0x0006,

        /// <summary>
        /// μ-law format by Microsoft Corporation
        /// </summary>
        Mulaw = 0x0007,

        /// <summary>
        /// OKI ADPCM
        /// </summary>
        OkiAdpcm = 0x0010,

        /// <summary>
        /// Intel Corporation ADPCM
        /// </summary>
        ImaAdpcm = 0x0011,

        /// <summary>
        /// Videologic ADPCM
        /// </summary>
        MediaspaceAdpcm = 0x0012,

        /// <summary>
        /// Sierra Semiconductor Corp
        /// </summary>
        SierraAdpcm = 0x0013,

        /// <summary>
        /// Antex Electronics Corporation
        /// </summary>
        G723Adpcm = 0x0014,

        /// <summary>
        /// DSP Solutions, Inc.
        /// </summary>
        Digistd = 0x0015,

        /// <summary>
        /// DSP Solutions, Inc.
        /// </summary>
        Digifix = 0x0016,

        /// <summary>
        /// Dialogic Corporation
        /// </summary>
        DialogicOkiAdpcm = 0x0017,

        /// <summary>
        /// Media Vision, Inc.
        /// </summary>
        MediavisionAdpcm = 0x0018,

        /// <summary>
        /// Hewlett-Packard Company
        /// </summary>
        CuCodec = 0x0019,

        /// <summary>
        /// Yamaha Corporation of America
        /// </summary>
        YamahaAdpcm = 0x0020,

        /// <summary>
        /// Speech Compression
        /// </summary>
        Sonarc = 0x0021,

        /// <summary>
        /// DSP Group, Inc
        /// </summary>
        DspgroupTruespeech = 0x0022,

        /// <summary>
        /// Echo Speech Corporation
        /// </summary>
        Echosc1 = 0x0023,

        /// <summary>
        /// Audiofile, Inc.
        /// </summary>
        AudiofileAf36 = 0x0024,

        /// <summary>
        /// Audio Processing Technology
        /// </summary>
        Aptx = 0x0025,

        /// <summary>
        /// Audiofile, Inc.
        /// </summary>
        AudiofileAf10 = 0x0026,

        /// <summary>
        /// Aculab plc
        /// </summary>
        Prosody1612 = 0x0027,

        /// <summary>
        /// Merging Technologies S.A.
        /// </summary>
        Lrc = 0x0028,

        /// <summary>
        /// Dolby Laboratories
        /// </summary>
        DolbyAc2 = 0x0030,

        /// <summary>
        /// Microsoft Corporation
        /// </summary>
        Gsm610 = 0x0031,

        /// <summary>
        /// Microsoft Corporation
        /// </summary>
        Msnaudio = 0x0032,

        /// <summary>
        /// Antex Electronics Corporation
        /// </summary>
        AntexAdpcme = 0x0033,

        /// <summary>
        /// Control Resources Limited
        /// </summary>
        ControlResVqlpc = 0x0034,

        /// <summary>
        /// DSP Solutions, Inc.
        /// </summary>
        Digireal = 0x0035,

        /// <summary>
        /// DSP Solutions, Inc.
        /// </summary>
        Digiadpcm = 0x0036,

        /// <summary>
        /// Control Resources Limited
        /// </summary>
        ControlResCr10 = 0x0037,

        /// <summary>
        /// Natural MicroSystems
        /// </summary>
        NmsVbxadpcm = 0x0038,

        /// <summary>
        /// Roland
        /// </summary>
        RolandRdac = 0x0039,

        /// <summary>
        /// Echo Speech Corporation
        /// </summary>
        Echosc3 = 0x003A,

        /// <summary>
        /// Rockwell International
        /// </summary>
        RockwellAdpcm = 0x003B,

        /// <summary>
        /// Rockwell International
        /// </summary>
        RockwellDigitalk = 0x003C,

        /// <summary>
        /// Xebec Multimedia Solutions Limited
        /// </summary>
        Xebec = 0x003D,

        /// <summary>
        /// Antex Electronics Corporation
        /// </summary>
        G721Adpcm = 0x0040,

        /// <summary>
        /// Antex Electronics Corporation
        /// </summary>
        G728Celp = 0x0041,

        /// <summary>
        /// Microsoft Corporation
        /// </summary>
        Msg723 = 0x0042,

        /// <summary>
        /// Microsoft Corporation
        /// </summary>
        Mpeg = 0x0050,

        /// <summary>
        /// InSoft Inc.
        /// </summary>
        Rt24 = 0x0052,

        /// <summary>
        /// InSoft Inc.
        /// </summary>
        Pac = 0x0053,

        /// <summary>
        /// MPEG 3 Layer 1
        /// </summary>
        Mpeglayer3 = 0x0055,

        /// <summary>
        /// Lucent Technologies
        /// </summary>
        LucentG723 = 0x0059,

        /// <summary>
        /// Cirrus Logic
        /// </summary>
        Cirrus = 0x0060,

        /// <summary>
        /// ESS Technology
        /// </summary>
        Espcm = 0x0061,

        /// <summary>
        /// Voxware Inc
        /// </summary>
        Voxware = 0x0062,

        /// <summary>
        /// Canopus, Co., Ltd.
        /// </summary>
        CanopusAtrac = 0x0063,

        /// <summary>
        /// APICOM
        /// </summary>
        G726Adpcm = 0x0064,

        /// <summary>
        /// APICOM
        /// </summary>
        G722Adpcm = 0x0065,

        /// <summary>
        /// Microsoft Corporation
        /// </summary>
        Dsat = 0x0066,

        /// <summary>
        /// Microsoft Corporation
        /// </summary>
        DsatDisplay = 0x0067,

        /// <summary>
        /// Voxware Inc.
        /// </summary>
        VoxwareByteAligned = 0x0069,

        /// <summary>
        /// Voxware Inc.
        /// </summary>
        VoxwareAc8 = 0x0070,

        /// <summary>
        /// Voxware Inc.
        /// </summary>
        VoxwareAc10 = 0x0071,

        /// <summary>
        /// Voxware Inc.
        /// </summary>
        VoxwareAc16 = 0x0072,

        /// <summary>
        /// Voxware Inc.
        /// </summary>
        VoxwareAc20 = 0x0073,

        /// <summary>
        /// Voxware Inc.
        /// </summary>
        VoxwareRt24 = 0x0074,

        /// <summary>
        /// Voxware Inc.
        /// </summary>
        VoxwareRt29 = 0x0075,

        /// <summary>
        /// Voxware Inc.
        /// </summary>
        VoxwareRt29hw = 0x0076,

        /// <summary>
        /// Voxware Inc.
        /// </summary>
        VoxwareVr12 = 0x0077,

        /// <summary>
        /// Voxware Inc.
        /// </summary>
        VoxwareVr18 = 0x0078,

        /// <summary>
        /// Voxware Inc.
        /// </summary>
        VoxwareTq40 = 0x0079,

        /// <summary>
        /// Softsound, Ltd.
        /// </summary>
        Softsound = 0x0080,

        /// <summary>
        /// Voxware Inc.
        /// </summary>
        VoxareTq60 = 0x0081,

        /// <summary>
        /// Microsoft Corporation
        /// </summary>
        Msrt24 = 0x0082,

        /// <summary>
        /// AT&amp;T Laboratories
        /// </summary>
        G729a = 0x0083,

        /// <summary>
        /// Motion Pixels
        /// </summary>
        MviMv12 = 0x0084,

        /// <summary>
        /// DataFusion Systems (Pty) (Ltd)
        /// </summary>
        DfG726 = 0x0085,

        /// <summary>
        /// DataFusion Systems (Pty) (Ltd)
        /// </summary>
        DfGsm610 = 0x0086,

        /// <summary>
        /// OnLive! Technologies, Inc.
        /// </summary>
        Onlive = 0x0089,

        /// <summary>
        /// Siemens Business Communications Systems
        /// </summary>
        Sbc24 = 0x0091,

        /// <summary>
        /// Sonic Foundry
        /// </summary>
        DolbyAc3Spdif = 0x0092,

        /// <summary>
        /// ZyXEL Communications, Inc.
        /// </summary>
        ZyxelAdpcm = 0x0097,

        /// <summary>
        /// Philips Speech Processing
        /// </summary>
        PhilipsLpcbb = 0x0098,

        /// <summary>
        /// Studer Professional Audio AG
        /// </summary>
        Packed = 0x0099,

        /// <summary>
        /// Rhetorex, Inc.
        /// </summary>
        RhetorexAdpcm = 0x0100,

        /// <summary>
        /// IBM mu-law format
        /// </summary>
        IbmMulaw = 0x0101,

        /// <summary>
        /// IBM a-law format
        /// </summary>
        IbmAlaw = 0x0102,

        /// <summary>
        /// IBM AVC Adaptive Differential PCM format
        /// </summary>
        Adpcm = 0x0103,

        /// <summary>
        /// Vivo Software
        /// </summary>
        VivoG723 = 0x0111,

        /// <summary>
        /// Vivo Software
        /// </summary>
        VivoSiren = 0x0112,

        /// <summary>
        /// Digital Equipment Corporation
        /// </summary>
        DigitalG723 = 0x0123,

        /// <summary>
        /// Creative Labs, Inc
        /// </summary>
        CreativeAdpcm = 0x0200,

        /// <summary>
        /// Creative Labs, Inc
        /// </summary>
        CreativeFastspeech8 = 0x0202,

        /// <summary>
        /// Creative Labs, Inc
        /// </summary>
        CreativeFastspeech10 = 0x0203,

        /// <summary>
        /// Quarterdeck Corporation
        /// </summary>
        Quarterdeck = 0x0220,

        /// <summary>
        /// Fujitsu Corporation
        /// </summary>
        FmTownsSnd = 0x0300,

        /// <summary>
        /// Brooktree Corporation
        /// </summary>
        BzvDigital = 0x0400,

        /// <summary>
        /// AT&amp;T Labs, Inc.
        /// </summary>
        VmeVmpcm = 0x0680,

        /// <summary>
        /// Ing C. Olivetti &amp; C., S.p.A.
        /// </summary>
        Oligsm = 0x1000,

        /// <summary>
        /// Ing C. Olivetti &amp; C., S.p.A.
        /// </summary>
        Oliadpcm = 0x1001,

        /// <summary>
        /// Ing C. Olivetti &amp; C., S.p.A.
        /// </summary>
        Olicelp = 0x1002,

        /// <summary>
        /// Ing C. Olivetti &amp; C., S.p.A.
        /// </summary>
        Olisbc = 0x1003,

        /// <summary>
        /// Ing C. Olivetti &amp; C., S.p.A.
        /// </summary>
        Oliopr = 0x1004,

        /// <summary>
        /// Lernout &amp; Hauspie
        /// </summary>
        LhCodec = 0x1100,

        /// <summary>
        /// Norris Communications, Inc.
        /// </summary>
        Norris = 0x1400,

        /// <summary>
        /// AT&amp;T Labs, Inc.
        /// </summary>
        SoundspaceMusicompress = 0x1500,

        /// <summary>
        /// FAST Multimedia AG
        /// </summary>
        Dvm = 0x2000,

        /// <summary>
        /// ?????
        /// </summary>
        InterwavVsc112 = 0x7150,

        /// <summary>
        ///
        /// </summary>
        Extensible = unchecked((ushort)0xFFFEu),
    }
}
