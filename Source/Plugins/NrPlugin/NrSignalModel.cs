﻿using System.Xml.Linq;
using NationalInstruments.RFmx.InstrMX;
using NationalInstruments.RFmx.NRMX;
using Serilog;

namespace NationalInstruments.Utilities.WaveformParsing.Plugins
{
    using static RfwsParserUtilities;


    //[RfwsSection]
    [RfwsSection("CarrierSet", version = "3")]
    public class CarrierSet : RfwsSection
    {
        #region RFmx Properties
        public CarrierSet(XElement propertySection)
            : base(propertySection) { }

        [RfwsProperty("AutoIncrementCellIdEnabled", 3)]
        public NrRfmxPropertyMap<bool> AutoIncrementCellId = new NrRfmxPropertyMap<bool>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.AutoIncrementCellIDEnabled,
            SelectorStringType = RfmxNrSelectorStringType.None
        };
        #endregion

        #region Sub Sections
        [RfwsSectionList("CarrierManager", version = "3")]
        public RfwsSectionList<Subblock> Subblocks;
        #endregion

    }


    [RfwsSection("Carrier", version = "3")]
    public class Subblock : RfwsSection// : NrSignalModel
    {
        public const string KeySubblockNumber = "CarrierSubblockNumber";
        public const string KeyCarrierDefinition = "CarrierDefinition";
        public const string KeyCarrierCCIndex = "CarrierCCIndex";

        static double absoluteFrequency = 1e9;

        public int CarrierDefinitionIndex { get; }
        public int SubblockIndex { get; }
        public int ComponentCarrierIndex { get; }

        public override string SelectorString
        {
            get
            {
                string selectorString = RFmxNRMX.BuildSubblockString(base.SelectorString, SubblockIndex);
                selectorString = RFmxNRMX.BuildCarrierString(selectorString, ComponentCarrierIndex);
                return selectorString;
            }
        }

        public Subblock(XElement childSection, RfwsSection parentGroup)
            : base(childSection, parentGroup)
        {
            SubblockIndex = int.Parse(FetchValue(SectionRoot, KeySubblockNumber));
            CarrierDefinitionIndex = int.Parse(FetchValue(SectionRoot, KeyCarrierDefinition));
            ComponentCarrierIndex = int.Parse(FetchValue(SectionRoot, KeyCarrierCCIndex));
        }

        public override void CustomConfigure(ISignalConfiguration signal)
        {
            RFmxNRMX nr = (RFmxNRMX)signal;
            nr.SetNumberOfSubblocks("", SubblockIndex + 1);
            string subblockString = RFmxNRMX.BuildSubblockString("", SubblockIndex);
            nr.SetSubblockFrequencyDefinition(subblockString, RFmxNRMXSubblockFrequencyDefinition.Absolute);
            nr.ComponentCarrier.SetNumberOfComponentCarriers(subblockString, ComponentCarrierIndex + 1);
        }

        #region RFmx Properties
        [RfwsProperty("CarrierSubblockOffset", 3)]
        public NrRfmxPropertyMap<double> CarrierSubblockOffset = new NrRfmxPropertyMap<double>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.CenterFrequency,
            SelectorStringType = RfmxNrSelectorStringType.Subblock,
            CustomMap = (value) => SiNotationToStandard((string)value) + absoluteFrequency,
        };
        [RfwsProperty("CarrierFrequencyOffset", 3)]
        public NrRfmxPropertyMap<double> CarrierFrequencyOffset = new NrRfmxPropertyMap<double>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.ComponentCarrierFrequency,
        };
        #endregion

    }

    [RfwsSection("CarrierDefinition", version = "1")]
    public class Carrier : RfwsSection
    {
        public const string SectionCarrierDefinitionManager = "CarrierDefinitionManager";

        public Carrier(XElement childSection, RfwsSection parentSection)
            : base(childSection, parentSection)
        {
        }
        [RfwsProperty("Bandwidth Part Count", 1)]
        public NrRfmxPropertyMap<int> BandwidthPartCount = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.NumberOfBandwidthParts,
        };
        // Current version (20.0) is 5, but properties below also work with 19.1 (version 3)

        [RfwsSection("Cell Settings", version = "5")]
        public CellSettings Cell;
        [RfwsSection("Output Settings", version = "3")]
        public OutputSettings Output;
        [RfwsSection(@"Ssb Settings", version = "4")]
        public SsbSettings Ssb;
        [RfwsSectionList(version = "3")]
        public RfwsSectionList<BandwidthPartSettings> BandwidthParts; 

    }
    public class CellSettings : RfwsSection
    {
        public CellSettings(XElement childSection, RfwsSection parentSection)
            : base(childSection, parentSection) { }

        [RfwsProperty("Cell ID", 3)]
        public NrRfmxPropertyMap<int> CellId = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.CellID,
        };
        [RfwsProperty("Bandwidth (Hz)", 3)]
        public NrRfmxPropertyMap<double> Bandwidth = new NrRfmxPropertyMap<double>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.ComponentCarrierBandwidth,
        };
        [RfwsProperty("Frequency Range", 3)]
        public NrRfmxPropertyMap<int> FrequencyRange = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.FrequencyRange,
            SelectorStringType = RfmxNrSelectorStringType.Subblock,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXFrequencyRange>()
        };
        [RfwsProperty("Reference Grid Alignment Mode", 3)]
        public static NrRfmxPropertyMap<int> RefGridAlignmentMode = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.ReferenceGridAlignmentMode,
            SelectorStringType = RfmxNrSelectorStringType.None,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXReferenceGridAlignmentMode>()
        };
        [RfwsProperty("Reference Grid Subcarrier Spacing", 3)]
        public NrRfmxPropertyMap<double> RefGridSubcarrierSpacing = new NrRfmxPropertyMap<double>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.ReferenceGridSubcarrierSpacing,
        };
        [RfwsProperty("Reference Grid Start", 3)]
        public NrRfmxPropertyMap<int> ReferenceGridStart = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.ReferenceGridStart,
        };
        // This key is only located here in RFmx 19.1; it is moved to the CarrierSet section in 20.0
        [RfwsProperty("Auto Increment Cell ID Enabled", 3, RfswVersionMode.SpecificVersions)]
        public NrRfmxPropertyMap<bool> AutoIncrementCellId_19_1 = new NrRfmxPropertyMap<bool>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.AutoIncrementCellIDEnabled,
            SelectorStringType = RfmxNrSelectorStringType.None
        };
    }

    public class OutputSettings : RfwsSection
    {
        public OutputSettings(XElement childSection, RfwsSection parentSection)
            : base(childSection, parentSection) { }

        [RfwsProperty("Link Direction", 3)]
        public NrRfmxPropertyMap<int> LinkDirection = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.LinkDirection,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXLinkDirection>(),
            SelectorStringType = RfmxNrSelectorStringType.None
        };
        [RfwsProperty("DL Ch Configuration Mode", 3)]
        public NrRfmxPropertyMap<int> DlChannelConfigMode = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.DownlinkChannelConfigurationMode,
            CustomMap = (value) =>
                (int)value.ToEnum<RFmxNRMXDownlinkChannelConfigurationMode>(),
            SelectorStringType = RfmxNrSelectorStringType.None
        };
        [RfwsProperty("DL Test Model", 3)]
        public NrRfmxPropertyMap<int> DlTestModel = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.DownlinkTestModel,
            CustomMap = (value) =>
            {
                string textValue = (string)value;
                textValue = textValue.Replace(".", "_");
                return (int)textValue.ToEnum<RFmxNRMXDownlinkTestModel>();
            },
            SelectorStringType = RfmxNrSelectorStringType.None
        };
        [RfwsProperty("DL Test Model Duplex Scheme", 3)]
        public NrRfmxPropertyMap<int> DlTestModelDuplexScheme = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.DownlinkTestModelDuplexScheme,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXDownlinkTestModelDuplexScheme>(),
            SelectorStringType = RfmxNrSelectorStringType.None
        };
    }

    public class SsbSettings : RfwsSection
    {
        public SsbSettings(XElement childSection, RfwsSection parentSection)
            : base(childSection, parentSection) { }

        // Using this key as a proxy for SSB enabled. RFmx WC doesn't seem to have a specific property for
        // enabling SSB; instead, this, "SSS Channel Mode", "PBCH DMRS Channel Mode", and 
        // "PBCH Channel Mode" are all set to True when the SSB enabled checkbox is set in the WC UI.
        [RfwsProperty("PSS Channel Mode", 4)]
        public NrRfmxPropertyMap<bool> SsbEnabled = new NrRfmxPropertyMap<bool>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.SsbEnabled
        };
        [RfwsProperty("Configuration Set", 4)]
        public NrRfmxPropertyMap<int> SsbPattern = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.SsbPattern,
            CustomMap = (value) =>
            {
                string textValue = (string)value;
                textValue = textValue.Replace("-", string.Empty);
                textValue = textValue.Replace("3up", "3GHz");
                return (int)textValue.ToEnum<RFmxNRMXSsbPattern>();
            }
        };
        [RfwsProperty("SSS Scaling Factor", 4)]
        public NrRfmxPropertyMap<double> SssPower = new NrRfmxPropertyMap<double>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.SssPower,
            CustomMap = (value) => ValueTodB((string)value),
        };
        [RfwsProperty("PSS Scaling Factor", 4)]
        public NrRfmxPropertyMap<double> PssPower = new NrRfmxPropertyMap<double>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PssPower,
            CustomMap = (value) => ValueTodB((string)value),
        };
        [RfwsProperty("PBCH Scaling Factor", 4)]
        public NrRfmxPropertyMap<double> PbchPower = new NrRfmxPropertyMap<double>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PbchPower,
            CustomMap = (value) => ValueTodB((string)value),
        };
        [RfwsProperty("PBCH DMRS Scaling Factor", 4)]
        public NrRfmxPropertyMap<double> PbchDrmsPower = new NrRfmxPropertyMap<double>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PbchDmrsPower,
            CustomMap = (value) => ValueTodB((string)value),
        };
        [RfwsProperty("Subcarrier Spacing Common", 4)]
        public NrRfmxPropertyMap<double> SubcarrierSpacingCommon = new NrRfmxPropertyMap<double>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.SubcarrierSpacingCommon,
        };
        [RfwsProperty("Subcarrier Offset", 4)]
        public NrRfmxPropertyMap<int> SubcarrierOffset = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.SsbSubcarrierOffset,
        };
        [RfwsProperty("Periodicity", 4)]
        public NrRfmxPropertyMap<double> SsbPeriodicity = new NrRfmxPropertyMap<double>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.SsbPeriodicity,
        };
        [RfwsProperty("SSB Active Blocks", 4)]
        public NrRfmxPropertyMap<string> SsbActiveBlocks = new NrRfmxPropertyMap<string>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.SsbActiveBlocks,
        };
    }

    // Bandwidth Part Section has number in title
    [RfwsSection(@"Bandwidth Part Settings \d+", version = "3", regExMatch = true)]
    public class BandwidthPartSettings : RfwsSection
    {
        const string KeyBandwidthPartIndex = "Bandwidth Part Index";

        public int BandwidthPartIndex { get; }
        public override string SelectorString
            => RFmxNRMX.BuildBandwidthPartString(base.SelectorString, BandwidthPartIndex);

        public BandwidthPartSettings(XElement childSection, RfwsSection parentSection)
            : base(childSection, parentSection)
        {
            BandwidthPartIndex = int.Parse(FetchValue(SectionRoot, KeyBandwidthPartIndex));
        }

        #region RFmx Properties
        [RfwsProperty("Subcarrier Spacing (Hz)", 3)]
        public NrRfmxPropertyMap<double> SubcarrierSpacing = new NrRfmxPropertyMap<double>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.BandwidthPartSubcarrierSpacing,
        };
        [RfwsProperty("Cyclic Prefix Mode", 3)]
        public NrRfmxPropertyMap<int> CyclicPrefixMode = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.BandwidthPartCyclicPrefixMode,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXBandwidthPartCyclicPrefixMode>()
        };
        [RfwsProperty("Grid Start", 3)]
        public NrRfmxPropertyMap<int> GridStart = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.GridStart,
        };
        [RfwsProperty("RB Offset", 3)]
        public NrRfmxPropertyMap<int> RbOffset = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.BandwidthPartResourceBlockOffset,
        };
        [RfwsProperty("Number of RBs", 3)]
        public NrRfmxPropertyMap<int> NumberOfRbs = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.BandwidthPartNumberOfResourceBlocks,
        };
        [RfwsProperty("UE Count", 3)]
        public NrRfmxPropertyMap<int> NumberOfUe = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.NumberOfUsers,
        };
        [RfwsProperty("Coreset Count", 3)]
        public NrRfmxPropertyMap<int> NumberOfCoreset = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.NumberOfCoresets,
        };
        #endregion

        [RfwsSectionList(version = "1")]
        public RfwsSectionList<UeSettings> Users;

    }
    // Section has number in title
    [RfwsSection(@"UE Settings \d+", version = "1", regExMatch = true)]
    public class UeSettings : RfwsSection
    {
        const string KeyUeIndex = "UE Index";
        public int UeIndex { get; }

        public override string SelectorString
            => RFmxNRMX.BuildUserString(base.SelectorString, UeIndex);

        #region RFmx Properties
        public UeSettings(XElement childSection, RfwsSection parentSection)
        : base(childSection, parentSection)
        {
            UeIndex = int.Parse(FetchValue(SectionRoot, KeyUeIndex));
        }

        [RfwsProperty("nRNTI", 1)]
        public NrRfmxPropertyMap<int> Rnti = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.Rnti,
        };
        #endregion

        [RfwsSectionList("PDSCH Settings", version = "3")]
        public PdschSettings PdschSlots;
        [RfwsSectionList("PUSCH Settings", version = "1")]
        public PuschSettings PuschSlots;
    }
    
    public class PdschSettings : RfwsSectionList<PdschSlotSettings>
    {
        public PdschSettings(XElement childSection, RfwsSection parentSection)
            : base(childSection, parentSection) { }

        [RfwsProperty("Count", 3)]
        public NrRfmxPropertyMap<int> NumPdsch = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.NumberOfPdschConfigurations,
        };
    }
    // Section has number in title
    [RfwsSection(@"PDSCH Slot Settings \d+", version = "4", regExMatch = true)]
    public class PdschSlotSettings : RfwsSection
    {
        public const string KeyPdschSlotIndex = "Array Index";
        public int PdschSlotIndex { get; }

        public override string SelectorString
            => RFmxNRMX.BuildPdschString(base.SelectorString, PdschSlotIndex);

        public PdschSlotSettings(XElement childSection, RfwsSection parentSection)
            : base(childSection, parentSection)
        {
            PdschSlotIndex = int.Parse(FetchValue(SectionRoot, KeyPdschSlotIndex));
        }

        [RfwsProperty("PDSCH Present in SSB RB", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<bool> PdschPressentInSsbRb = new NrRfmxPropertyMap<bool>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschPresentInSsbResourceBlock,
        };
        [RfwsProperty("Slot Allocation", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<string> RbAllocation = new NrRfmxPropertyMap<string>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschSlotAllocation,
        };
        [RfwsProperty("Symbol Allocation", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<string> SymbolAllocation = new NrRfmxPropertyMap<string>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschSymbolAllocation,
        };
        [RfwsProperty("Modulation Type", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<int> ModulationType = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschModulationType,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXPdschModulationType>()
        };
        [RfwsProperty("PDSCH Mapping Type", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<int> MappingType = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschMappingType,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXPdschMappingType>()
        };
        [RfwsProperty("DMRS Duration", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<int> DmrsDuration = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschDmrsDuration,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXPdschDmrsDuration>()
        };
        [RfwsProperty("DMRS Configuration", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<int> DmrsConfiguration = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschDmrsConfigurationType,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXPdschDmrsConfigurationType>()
        };
        [RfwsProperty("DMRS Power Mode", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<int> DmrsPowerMode = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschDmrsPowerMode,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXPdschDmrsPowerMode>()
        };
        [RfwsProperty("DMRS Scaling Factor", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<double> DmrsPower = new NrRfmxPropertyMap<double>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschDmrsPower,
            CustomMap = (value) => ValueTodB((string)value)
        };
        [RfwsProperty("DMRS Additional Positions", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<int> AdditionalPositions = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschDmrsAdditionalPositions,
        };
        [RfwsProperty("DMRS Type A Position", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<int> TypeAPosition = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschDmrsTypeAPosition,
        };
        [RfwsProperty("DMRS Scrambling ID", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<int> ScramblingId = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschDmrsScramblingID,
        };
        [RfwsProperty("DMRS Scrambling ID Mode", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<int> ScramblingMode = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschDmrsScramblingIDMode,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXPdschDmrsScramblingIDMode>()
        };
        [RfwsProperty("Number of CDM Groups", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<int> CdmGroups = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschDmrsNumberOfCdmGroups,
        };
        [RfwsProperty("nSCID", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<int> Nscid = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschDmrsnScid,
        };
        [RfwsProperty("Dmrs Release Version", 4)]
        public NrRfmxPropertyMap<int> ReleaseVersion = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschDmrsReleaseVersion,
            CustomMap = (value) =>
            {
                string textValue = (string)value;
                textValue = textValue.Replace("3GPP", string.Empty);
                return (int)textValue.ToEnum<RFmxNRMXPdschDmrsReleaseVersion>();
            }
        };
        [RfwsProperty("PTRS Ports", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<string> PtrsPorts = new NrRfmxPropertyMap<string>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschPtrsAntennaPorts,
        };
        [RfwsProperty("PTRS Time Density", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<int> PtrsTimeDensity = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschPtrsTimeDensity,
        };
        [RfwsProperty("PTRS Frequency Density", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<int> PtrsFrequencyDensity = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschPtrsFrequencyDensity,
        };
        [RfwsProperty("DL PTRS RE Offset", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<int> PtrsReOffset = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschPtrsREOffset,
        };
        [RfwsProperty("PTRS Enabled", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<bool> PtrsEnabled = new NrRfmxPropertyMap<bool>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschPtrsEnabled,
        };
        [RfwsProperty("PTRS Power Mode", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<int> PtrsPowerMode = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschPtrsPowerMode,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXPdschPtrsPowerMode>()
        };
        [RfwsProperty("PTRS Scaling Factor", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<double> PtrsPower = new NrRfmxPropertyMap<double>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschPtrsPower,
            CustomMap = (value) => ValueTodB((string)value)
        };
        [RfwsProperty("DMRS Ports", RfswVersionMode.SupportedVersionsAndLater, 3, 4)]
        public NrRfmxPropertyMap<string> DmrsPorts = new NrRfmxPropertyMap<string>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PdschDmrsAntennaPorts,
        };
    }

    public class PuschSettings : RfwsSectionList<PuschSlotSettings>
    {
        public PuschSettings(XElement childSection, RfwsSection parentSection)
            : base(childSection, parentSection) { }

        [RfwsProperty("Count", 1)]
        public NrRfmxPropertyMap<int> NumPusch = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.NumberOfPuschConfigurations,
        };

    }
    // Section has number in title
    [RfwsSection(@"PUSCH Slot Settings \d+", version = "6", regExMatch = true)]
    public class PuschSlotSettings : RfwsSection
    {
        const string KeyPuschSlotIndex = "Array Index";
        public int PuschSlotIndex { get; }

        public override string SelectorString
            => RFmxNRMX.BuildPuschString(base.SelectorString, PuschSlotIndex);

        public PuschSlotSettings(XElement childSection, RfwsSection parentSection)
            : base(childSection, parentSection)
        {
            PuschSlotIndex = int.Parse(FetchValue(SectionRoot, KeyPuschSlotIndex));
        }
        [RfwsProperty("Slot Allocation", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<string> RbAllocation = new NrRfmxPropertyMap<string>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschSlotAllocation,
        };
        [RfwsProperty("Symbol Allocation", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<string> SymbolAllocation = new NrRfmxPropertyMap<string>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschSymbolAllocation,
        };
        [RfwsProperty("Modulation Type", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<int> ModulationType = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschModulationType,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXPuschModulationType>()
        };
        [RfwsProperty("PUSCH Mapping Type", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<int> MappingType = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschMappingType,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXPuschMappingType>()
        };
        [RfwsProperty("DMRS Duration", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<int> DmrsDuration = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschDmrsDuration,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXPuschDmrsDuration>()
        };
        [RfwsProperty("DMRS Configuration Type", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<int> DmrsConfiguration = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschDmrsConfigurationType,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXPuschDmrsConfigurationType>()
        };
        [RfwsProperty("DMRS Power Mode", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<int> DmrsPowerMode = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschDmrsPowerMode,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXPuschDmrsPowerMode>()
        };
        [RfwsProperty("DMRS Scaling Factor", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<double> DmrsPower = new NrRfmxPropertyMap<double>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschDmrsPower,
            CustomMap = (value) => ValueTodB((string)value)
        };
        [RfwsProperty("DMRS Additional Positions", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<int> AdditionalPositions = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschDmrsAdditionalPositions,
        };
        [RfwsProperty("DMRS Type A Position", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<int> TypeAPosition = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschDmrsTypeAPosition,
        };
        [RfwsProperty("Transform Precoding Enabled", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<bool> TransformPreCodingEnabled = new NrRfmxPropertyMap<bool>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschTransformPrecodingEnabled,
        };
        [RfwsProperty("PTRS Time Density", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<int> PtrsTimeDensity = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschPtrsTimeDensity,
        };
        [RfwsProperty("PTRS Frequency Density", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<int> PtrsFrequencyDensity = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschPtrsFrequencyDensity,
        };
        [RfwsProperty("UL PTRS RE Offset", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<int> PtrsReOffset = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschPtrsREOffset,
        };
        [RfwsProperty("PTRS Enabled", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<bool> PtrsEnabled = new NrRfmxPropertyMap<bool>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschPtrsEnabled,
        };

        [RfwsProperty("DMRS Scrambling ID", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<int> ScramblingId = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschDmrsScramblingID,
        };
        [RfwsProperty("DMRS Scrambling ID Mode", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<int> ScramblingMode = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschDmrsScramblingIDMode,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXPuschDmrsScramblingIDMode>()
        };
        [RfwsProperty("Dmrs Release Version", 6)]
        public NrRfmxPropertyMap<int> ReleaseVersion = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschDmrsReleaseVersion,
            CustomMap = (value) =>
            {
                string textValue = (string)value;
                textValue = textValue.Replace("3GPP", string.Empty);
                return (int)textValue.ToEnum<RFmxNRMXPuschDmrsReleaseVersion>();
            }
        };
        [RfwsProperty("PTRS Power Mode", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<int> PtrsPowerMode = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschPtrsPowerMode,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXPuschPtrsPowerMode>()
        };
        [RfwsProperty("PTRS Scaling Factor", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<double> PtrsPower = new NrRfmxPropertyMap<double>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschPtrsPower,
            CustomMap = (value) => ValueTodB((string)value)
        };
        [RfwsProperty("PUSCH ID", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<int> DrmsPuschId = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschDmrsPuschID,
        };
        [RfwsProperty("PUSCH ID Mode", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<int> DrmsPuschIdMode = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschDmrsPuschIDMode,
            CustomMap = (value) => (int)value.ToEnum<RFmxNRMXPuschDmrsPuschIDMode>()
        };
        [RfwsProperty("Number of CDM Groups", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<int> CdmGroups = new NrRfmxPropertyMap<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschDmrsNumberOfCdmGroups,
        };
        [RfwsProperty("DMRS Ports", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<string> DmrsPorts = new NrRfmxPropertyMap<string>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschDmrsAntennaPorts,
        };
        [RfwsProperty("PTRS Ports", RfswVersionMode.SupportedVersionsAndLater, 3, 6)]
        public NrRfmxPropertyMap<string> PtrsPorts = new NrRfmxPropertyMap<string>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.PuschPtrsAntennaPorts,
        };
    }

    // TO BE COMPLETED AT A LATER DATE
    [RfwsSection(@"CORESET Settings \d+", version = "1", regExMatch = true)]
    public class CoresetSettings : RfwsSection
    {
        public const string KeyCoresetIndex = "Coreset Index";

        public CoresetSettings(XElement childSection, RfwsSection parentSection)
            : base(childSection, parentSection)
        {
            Log.Warning("Configuration includes one or more Coresets. Coresets are not supported in the " +
                "current version of this plugin and will be ignored.");
            //int coreSetIndex = int.Parse(FetchValue(SectionRoot, KeyCoresetIndex));
            //Signal.ComponentCarrier.SetNumberOfCoresets(SelectorString, coreSetIndex + 1);
            //SelectorString = RFmxNRMX.BuildCoresetString(SelectorString, coreSetIndex);
        }
        /*[RfwsProperty("Coreset Num Symbols", 1)]
        public NrRfwsKey<int> NumberOfCoreset = new NrRfwsKey<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.CoresetNumberOfSymbols,
        };
        [RfwsProperty("Coreset Num Symbols", 1)]
        public NrRfwsKey<int> NumberOfCoreset = new NrRfwsKey<int>
        {
            RfmxPropertyId = (int)RFmxNRMXPropertyId.Coreset,
        };*/
    }
}
