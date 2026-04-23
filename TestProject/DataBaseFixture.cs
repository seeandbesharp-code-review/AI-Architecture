using Microsoft.EntityFrameworkCore;
using Repositories;
using System;

namespace TestProject
{
    public class DatabaseFixture : IDisposable
    {
        public ApiShopContext Context { get; private set; }
        private readonly string _databaseName;


        public DatabaseFixture()
        {
            _databaseName = $"ApiShopTest_{Guid.NewGuid():N}";
            var options = new DbContextOptionsBuilder<ApiShopContext>()

                .UseSqlServer($"Data Source=srv2\\pupils;Initial Catalog={_databaseName};Integrated Security=True;Pooling=False;TrustServerCertificate=True")
                .Options;

            Context = new ApiShopContext(options);
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }
    }
}