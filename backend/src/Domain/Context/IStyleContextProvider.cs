namespace ClosetAPI.Domain.Context;

public interface IStyleContextProvider
{
    StyleContext GetCurrentStyleContext();
}
