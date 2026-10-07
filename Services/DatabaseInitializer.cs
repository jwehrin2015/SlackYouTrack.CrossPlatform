using Microsoft.EntityFrameworkCore;
using SlackYouTrack.CrossPlatform.Data;

namespace SlackYouTrack.CrossPlatform.Services;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        LocalDatabase.EnsureDirectoryExists();
        await using var context = new AppDbContext();
        await context.Database.MigrateAsync(cancellationToken);
    }
}
