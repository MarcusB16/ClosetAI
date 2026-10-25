using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Media;

namespace Closet.Tests.Support;

public class ThrowingTagger : IItemTagger
{
    public Task<ItemTags> TagAsync(PhotoContent photo, CancellationToken cancellationToken) =>
        throw new AiProviderException("tagger down");
}
