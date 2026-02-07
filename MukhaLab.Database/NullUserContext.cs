namespace MukhaLab.Database;

public class NullUserContext : IUserContext
{
    public Guid GetCurrentUserId()
    {
        return Guid.Empty; // Возвращаем пустой GUID для приложений без авторизации
    }
}