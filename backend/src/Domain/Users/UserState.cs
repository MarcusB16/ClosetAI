namespace ClosetAPI.Domain.Users;

public class UserState
{
    public const int SingleUserId = 1;
    public const int InitialStyleVariation = 3;

    public int Id { get; set; } = SingleUserId;
    public bool Onboarded { get; set; }
    public string? PersonPhotoFile { get; set; }
    public DateOnly? TodayOutfitDate { get; set; }
    public string? TodayOutfitId { get; set; }
    public int StyleVariation { get; set; } = InitialStyleVariation;
}
