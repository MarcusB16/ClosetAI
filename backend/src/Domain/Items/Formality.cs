namespace ClosetAPI.Domain.Items;

public static class Formality
{
    public const int Casual = 1;
    public const int Smart = 2;
    public const int Formal = 3;

    public static bool IsValid(int value) => value is >= Casual and <= Formal;
}
