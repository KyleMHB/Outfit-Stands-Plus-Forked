using System;
using System.Collections.Generic;
using RimWorldDevTests;

namespace OutfitStandsPlusForked.DevTests;

public sealed class ModHarness : HarnessSettings
{
    public override string DisplayName => "Outfit Stands Plus Forked";
    public override string ArgPrefix => "osp";
    public override string PackageId => "kylemhb.outfitstandsplusforked";
    public override Type ProductionType => typeof(OutfitStandsPlusForkedMod);
    public override string EvidenceFolder => "OutfitStandsPlusForkedTests";
    public override string[] RuntimeFolders => new[] { "About", "Assemblies", "Defs", "Languages", "NoOdyssey", "Patches", "Textures" };
    public override string[] RuntimeFiles => new[] { "LoadFolders.xml" };
    public override IEnumerable<Batch> CreateBatches() => new Batch[] { new StandsBatch(), new TailorBatch() };
    public override IEnumerable<string> RunAll => new[] { "osp-stands", "osp-tailor" };
}
