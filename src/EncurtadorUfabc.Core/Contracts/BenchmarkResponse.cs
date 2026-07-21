namespace EncurtadorUfabc.Core.Contracts;

public record BenchmarkResponse(string Structure, int Operations, double PutMs, double GetMs, double DeleteMs, double TotalMs);
