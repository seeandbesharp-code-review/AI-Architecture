using AutoMapper;
using DTOs;
using Entities;
using Moq;
using Repositories;
using Services;
using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace TestProject
{
    public class ProductsServicesCacheTests
    {
        private readonly Mock<IProductsRepository> _repositoryMock;
        private readonly Mock<IConnectionMultiplexer> _redisMock;
        private readonly Mock<IDatabase> _dbMock;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly List<Product> _products;

        public ProductsServicesCacheTests()
        {
            _repositoryMock = new Mock<IProductsRepository>();
            _redisMock = new Mock<IConnectionMultiplexer>();
            _dbMock = new Mock<IDatabase>();

            _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_dbMock.Object);

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Redis:TtlMinutes", "1" }
                })
                .Build();

            var mapperConfig = new MapperConfiguration(cfg =>
                cfg.CreateMap<Product, ProductDTO>());
            _mapper = mapperConfig.CreateMapper();

            _products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Phone", Price = 500, Description = "desc", ImageUrl = "img.jpg" },
                new Product { ProductId = 2, ProductName = "Tablet", Price = 800, Description = "desc2", ImageUrl = "img2.jpg" }
            };
        }

        private ProductsServices CreateService() =>
            new ProductsServices(_repositoryMock.Object, _mapper, _configuration, _redisMock.Object);

        [Fact]
        public async Task GetProducts_WhenCacheIsEmpty_ShouldFetchFromRepositoryAndStoreInCache()
        {
            _dbMock.Setup(d => d.StringGetAsync("products:all", It.IsAny<CommandFlags>()))
                   .ReturnsAsync(RedisValue.Null);
            _repositoryMock.Setup(r => r.GetProducts()).ReturnsAsync(_products);
            var service = CreateService();
            var result = await service.GetProducts();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _repositoryMock.Verify(r => r.GetProducts(), Times.Once);
            Assert.True(_dbMock.Invocations.Any(i => i.Method.Name == "StringSetAsync"));
        }

        [Fact]
        public async Task GetProducts_WhenCacheHasValue_ShouldReturnFromCacheWithoutCallingRepository()
        {
            var cached = JsonSerializer.Serialize(_mapper.Map<List<ProductDTO>>(_products));
            _dbMock.Setup(d => d.StringGetAsync("products:all", It.IsAny<CommandFlags>()))
                   .ReturnsAsync(new RedisValue(cached));

            var service = CreateService();
            var result = await service.GetProducts();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _repositoryMock.Verify(r => r.GetProducts(), Times.Never);
        }

        [Fact]
        public async Task GetProducts_WhenCacheExpires_ShouldFetchFromRepositoryAgain()
        {
            // First call: cache is empty → fetches from repo and caches
            _dbMock.SetupSequence(d => d.StringGetAsync("products:all", It.IsAny<CommandFlags>()))
                   .ReturnsAsync(RedisValue.Null)   // first call: cache miss
                   .ReturnsAsync(RedisValue.Null);  // second call: cache expired (miss again)

            _repositoryMock.Setup(r => r.GetProducts()).ReturnsAsync(_products);
            _dbMock.Setup(d => d.StringSetAsync("products:all", It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                   .ReturnsAsync(true);

            var service = CreateService();

            // First call: goes to repo
            var first = await service.GetProducts();
            // Second call: TTL expired (simulated by cache returning null again) → goes to repo again
            var second = await service.GetProducts();

            Assert.Equal(2, first.Count());
            Assert.Equal(2, second.Count());
            // Repository should be called twice (once per cache miss)
            _repositoryMock.Verify(r => r.GetProducts(), Times.Exactly(2));
        }
    }
}
