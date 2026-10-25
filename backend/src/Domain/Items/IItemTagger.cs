using ClosetAPI.Domain.Media;

namespace ClosetAPI.Domain.Items;

public interface IItemTagger
{
    Task<ItemTags> TagAsync(PhotoContent photo, CancellationToken cancellationToken);
}
