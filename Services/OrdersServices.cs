using DTOs;
using AutoMapper;
using Repositories;
using Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
namespace Services
{
    public class OrdersServices : IOrdersServices
    {
        private readonly IOrdersRepository _orders;
        private readonly IProductsRepository _products;
        private readonly IMapper _mapper;
        private readonly ILogger<OrdersServices> _logger;
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly string _ordersTopic;

        public OrdersServices(IOrdersRepository orders, IProductsRepository products, IMapper mapper, ILogger<OrdersServices> logger, IKafkaProducerService kafkaProducer, IConfiguration configuration)
        {
            _orders = orders;
            _products = products;
            _mapper = mapper;
            _logger = logger;
            _kafkaProducer = kafkaProducer;
            _ordersTopic = configuration["Kafka:Topics:Orders"] ?? "shop-orders";
        }

        public async Task<OrderDTO?> GetOrderById(int id)
        {
            var order = await _orders.GetOrderById(id);
            if(order == null)
            {
                return null;
            }
            return _mapper.Map<Order, OrderDTO>(order);
        }

        public async Task<OrderDTO?> AddOrder(OrderDTO order)
        {
           
            double calculatedSum = await ValidateOrderSum(order);

            if (calculatedSum <= 0)
            {
                _logger.LogWarning($"Order rejected - Validation failed for UserId: {order.UserId}"); //
                return null;
            }

            var orderWithCalculatedSum = new OrderDTO(
                order.OrderId,
                order.OrderDate,
                calculatedSum,
                order.UserId,
                order.OrderItems
            );

           
            Order placedOrder = await _orders.AddOrder(_mapper.Map<OrderDTO, Order>(orderWithCalculatedSum)); //
            _logger.LogInformation($"Order Id {placedOrder.OrderId} placed successfully with sum: {calculatedSum}");

            var orderResult = _mapper.Map<Order, OrderDTO>(placedOrder);

            var eventPayload = JsonSerializer.Serialize(new
            {
                orderId = placedOrder.OrderId,
                userId = placedOrder.UserId,
                orderDate = placedOrder.OrderDate,
                orderSum = placedOrder.OrderSum,
                items = orderResult.OrderItems
            });

            await _kafkaProducer.ProduceAsync(_ordersTopic, placedOrder.OrderId.ToString(), eventPayload);
            _logger.LogInformation($"Order event published to Kafka topic '{_ordersTopic}' for OrderId: {placedOrder.OrderId}");

            return orderResult;
        }

        public async Task<double> ValidateOrderSum(OrderDTO order)
        {
            if (order.OrderItems == null || !order.OrderItems.Any())
                return 0;

            var allProducts = await _products.GetProducts(); 
            double calculatedSum = 0;

            foreach (var item in order.OrderItems)
            {
                var product = allProducts.FirstOrDefault(p => p.ProductId == item.ProductId);
                if (product == null)
                {
                    _logger.LogWarning($"Validation failed - Product {item.ProductId} not found."); 
                    return -1; 
                }
                calculatedSum += (double)product.Price * item.Quantity;
            }

            return calculatedSum;
        }
    }
}
