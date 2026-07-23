namespace EncurtadorUfabc.Core.Models;

public record BenchmarkResponse(
    string Structure,
    int Operations,
    int ElementCount,
    double PutMs,
    double GetMs,
    double DeleteMs,
    double TotalMs,
    long AllocatedBytes,
    int? TreeHeight,
    int? BucketCount,
    double? LoadFactor,
    int? MaxChainLength
);
