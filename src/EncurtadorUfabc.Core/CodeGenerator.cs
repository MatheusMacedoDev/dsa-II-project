using System.Security.Cryptography;

namespace EncurtadorUfabc.Core;

public static class CodeGenerator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private const int Length = 7;

    public static string Generate()
    {
        var characters = new char[Length];
        for (int index = 0; index < Length; index++)
            characters[index] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        return new string(characters);
    }
}
