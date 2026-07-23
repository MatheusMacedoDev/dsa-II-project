namespace EncurtadorUfabc.Core.Models;

public readonly record struct StructureSnapshot(
    int ElementCount,
    int? TreeHeight,
    int? BucketCount,
    double? LoadFactor,
    int? MaxChainLength
);
