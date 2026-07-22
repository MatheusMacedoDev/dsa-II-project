namespace EncurtadorUfabc.Core.Models;

public record BenchmarkResponse(string Structure, int Operations, double PutMs, double GetMs, double DeleteMs, double TotalMs);
