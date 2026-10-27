using ClosetAPI.Domain.Media;

namespace ClosetAPI.Domain.Items;

public interface IItemTagger
{
    Task<ItemTags> TagPhotoAsync(PhotoContent photo, CancellationToken cancellationToken);
}
