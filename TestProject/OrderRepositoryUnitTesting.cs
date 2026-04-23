using DTOs;
using Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Repositories;
using Services;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TestProject
{
    public class OrderRepositoryUnitTests
    {
        [Fact]

        public async Task GetOrderById_ShouldReturnOrder_WhenOrderExists()
        {
            var options = new DbContextOptions<ApiShopContext>();
            var mockContext = new Mock<ApiShopContext>(options);
            var orderId = 10;
            var order = new Order 
            { 
                OrderId = orderId, 
                OrderSum = 500, 
                UserId = 1,
                OrderItems = new List<OrderItem>()
            };

            mockContext.Setup(x => x.Orders).ReturnsDbSet(new List<Order> { order });

            var repository = new OrdersRepository(mockContext.Object);
            var result = await repository.GetOrderById(orderId);
            Assert.NotNull(result);
            Assert.Equal(500, result.OrderSum);
            Assert.Equal(orderId, result.OrderId);
        }

        [Fact]
        public async Task GetOrderById_ShouldReturnNull_WhenOrderDoesNotExist()
        {
            var options = new DbContextOptions<ApiShopContext>();
            var mockContext = new Mock<ApiShopContext>(options);
     
            mockContext.Setup(x => x.Orders).ReturnsDbSet(new List<Order>());
            var repository = new OrdersRepository(mockContext.Object);
            var result = await repository.GetOrderById(1);
            Assert.Null(result);
        }

        [Fact]
        public async Task AddOrder_ShouldAddOrderAndSaveChanges()
        {
            var options = new DbContextOptions<ApiShopContext>();
            var mockContext = new Mock<ApiShopContext>(options);

            var mockSet = new Mock<DbSet<Order>>();
            mockContext.Setup(m => m.Orders).Returns(mockSet.Object);
            var repository = new OrdersRepository(mockContext.Object);
            var order = new Order { OrderSum = 100, OrderDate = DateOnly.FromDateTime(DateTime.Now) };
            var result = await repository.AddOrder(order);
            mockSet.Verify(m => m.AddAsync(order, default), Times.Once());
            mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once());
            Assert.Equal(order, result);
        }
    }

    public class OrderSumValidationTests
    {
        private readonly Mock<IOrdersRepository> _mockOrdersRepository;
        private readonly Mock<IProductsRepository> _mockProductsRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<OrdersServices>> _mockLogger;
        private readonly OrdersServices _ordersServices;

        public OrderSumValidationTests()
        {
            _mockOrdersRepository = new Mock<IOrdersRepository>();
            _mockProductsRepository = new Mock<IProductsRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<OrdersServices>>();
            _ordersServices = new OrdersServices(
                _mockOrdersRepository.Object,
                _mockProductsRepository.Object,
                _mockMapper.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task ValidateOrderSum_ShouldReturnTrue_WhenOrderSumMatchesCalculatedSum()
        {
            // Arrange - Happy Path
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Laptop", Price = 1000m },
                new Product { ProductId = 2, ProductName = "Mouse", Price = 50m }
            };

            _mockProductsRepository.Setup(r => r.GetProducts())
                .ReturnsAsync(products);

            var orderItems = new List<OrderItemDTO>
            {
                new OrderItemDTO(OrderId: 1, ProductId: 1, Quantity: 2),
                new OrderItemDTO(OrderId: 1, ProductId: 2, Quantity: 3) 
            };

            var order = new OrderDTO(
                OrderId: 1,
                OrderDate: DateOnly.FromDateTime(DateTime.Now),
                OrderSum: 2150,  
                UserId: 1,
                OrderItems: orderItems
            );

            // Act
            var result = await _ordersServices.ValidateOrderSum(order);

            // Assert
            Assert.Equal(2150, result);
        }

        [Fact]
        public async Task ValidateOrderSum_ShouldReturnFalse_WhenOrderSumDoesNotMatchCalculatedSum()
        {
            // Arrange - Unhappy Path
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Laptop", Price = 1000m },
                new Product { ProductId = 2, ProductName = "Mouse", Price = 50m }
            };

            _mockProductsRepository.Setup(r => r.GetProducts())
                .ReturnsAsync(products);

            var orderItems = new List<OrderItemDTO>
            {
                new OrderItemDTO(OrderId: 1, ProductId: 1, Quantity: 2), 
                new OrderItemDTO(OrderId: 1, ProductId: 2, Quantity: 3)  
            };

            var order = new OrderDTO(
                OrderId: 1,
                OrderDate: DateOnly.FromDateTime(DateTime.Now),
                OrderSum: 999,  // Incorrect sum (should be 2150)
                UserId: 1,
                OrderItems: orderItems
            );

            // Act
            var result = await _ordersServices.ValidateOrderSum(order);

            // Assert
            Assert.Equal(2150, result);
        }
    }
}