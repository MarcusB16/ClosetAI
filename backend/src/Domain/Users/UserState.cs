namespace ClosetAPI.Domain.Users;

public class UserState
{
    public bool Onboarded { get; set; }
    public string? PersonPhotoUri { get; set; }
    public DateOnly? TodayOutfitDate { get; set; }
    public string? TodayOutfitId { get; set; }
}
