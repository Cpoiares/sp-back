using Microsoft.EntityFrameworkCore;
using sp_back_api.Database;

namespace sp_back.tests.Helpers;

public static class TestDatabaseHelper
{
    public static DbContextOptions<AuctionDbContext> GetInMemoryDbOptions(string dbName)
    {
        return new DbContextOptionsBuilder<AuctionDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }
}