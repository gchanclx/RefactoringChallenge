using Mapster;
using MapsterMapper;
using Moq;
using NUnit.Framework;
using RefactoringChallenge.Entities;
using RefactoringChallenge.Repositories;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Text.Json;
using System.Timers;

namespace RefactoringChallenge.Tests
{
    public class OrdersRepositoryTest
    {
        [TestFixture]
        public class OrdersRepositoryTests
        {
            private Mock<NorthwindDbContext> _dbContextMock;
            private Mock<IMapper> _mapperMock;
            private OrdersRepository _repository;

            [SetUp]
            public void Setup()
            {
                _dbContextMock = new Mock<NorthwindDbContext>();
                _mapperMock = new Mock<IMapper>();
                _repository = new OrdersRepository(_dbContextMock.Object, _mapperMock.Object);
            }

            [Test]
            public async Task AddProductsToOrderAsync_ExistingOrder_ReturnsSerializedOrderDetails()
            {
                // Arrange
                var orderId = 10248;
                var orderDetails = new List<OrderDetailRequest>
                {
                    new OrderDetailRequest
                    {
                        ProductId = 1,
                        UnitPrice = 10.99m,
                        Quantity = 5,
                        Discount = 0.1f
                    },
                    new OrderDetailRequest
                    {
                        ProductId = 2,
                        UnitPrice = 15.99m,
                        Quantity = 3,
                        Discount = 0.05f
                    }
                };

                var orders = new List<Order>
                {
                    new Order {
                        OrderId = orderId
                    }
                };

                var orderDbSetMock = new Mock<DbSet<Order>>();
                orderDbSetMock.As<IQueryable<Order>>().Setup(m => m.Provider).Returns(orders.AsQueryable().Provider);
                orderDbSetMock.As<IQueryable<Order>>().Setup(m => m.Expression).Returns(orders.AsQueryable().Expression);
                orderDbSetMock.As<IQueryable<Order>>().Setup(m => m.ElementType).Returns(orders.AsQueryable().ElementType);
                orderDbSetMock.As<IQueryable<Order>>().Setup(m => m.GetEnumerator()).Returns(orders.GetEnumerator());

                _dbContextMock.Setup(m => m.Orders).Returns(orderDbSetMock.Object);

                var newOrderDetails = new List<OrderDetail>();
                _dbContextMock.Setup(m => m.OrderDetails.AddRange(It.IsAny<IEnumerable<OrderDetail>>()))
                              .Callback<IEnumerable<OrderDetail>>(list => newOrderDetails.AddRange(list));

                _dbContextMock.Setup(m => m.SaveChanges())
                              .Returns(await Task.FromResult(0));

                _mapperMock.Setup(m => m.Map<OrderDetail, OrderDetailResponse>(It.IsAny<OrderDetail>()))
                           .Returns<OrderDetail>(od => od.Adapt<OrderDetailResponse>());

                // Act
                var result = await _repository.AddProductsToOrderAsync(orderId, orderDetails);

                // Assert
                Assert.NotNull(result);
                Assert.IsInstanceOf<string>(result);

            }

            [Test]
            public async Task CreateAsync_ValidInput_ReturnsSerializedOrderResponse()
            {
                // Arrange
                var customerId = "VINET";
                var employeeId = 4;
                var requiredDate = new DateTime(2023, 5, 21);
                var shipVia = 1;
                var freight = 10.5m;
                var shipName = "Ship Name";
                var shipAddress = "Ship Address";
                var shipCity = "Ship City";
                var shipRegion = "Ship Region";
                var shipPostalCode = "12345";
                var shipCountry = "Ship Country";
                var orderDetails = new List<OrderDetailRequest>
            {
                new OrderDetailRequest
                {
                    ProductId = 1,
                    Discount = (float)0.1m,
                    Quantity = 5,
                    UnitPrice = 10.0m
                }
            };

                var mockDbContext = new Mock<NorthwindDbContext>();

                var newOrderDetails = new List<OrderDetail>();

                mockDbContext.Setup(c => c.Orders.AddAsync(It.IsAny<Order>(), default))
                    .Callback<Order, System.Threading.CancellationToken>((order, _) =>
                    {
                        order.OrderDetails = newOrderDetails;
                    });

                mockDbContext.Setup(c => c.SaveChangesAsync(default))
                    .Returns(Task.FromResult(1));

                var ordersRepository = new OrdersRepository(mockDbContext.Object, null); // Pass IMapper mock as needed

                // Act
                var result = await ordersRepository.CreateAsync(
                    customerId,
                    employeeId,
                    requiredDate,
                    shipVia,
                    freight,
                    shipName,
                    shipAddress,
                    shipCity,
                    shipRegion,
                    shipPostalCode,
                    shipCountry,
                    orderDetails);

                // Assert
                Assert.IsNotNull(result);
                var deserializedOrderResponse = JsonSerializer.Deserialize<OrderResponse>(result);
                Assert.IsNotNull(deserializedOrderResponse);
            }

            [Test]
            public async Task AddProductsToOrderAsync_OrderNotFound_ReturnsErrorMessage()
            {
                // Arrange
                var orderId = 1;
                var orderDetails = new List<OrderDetailRequest>
            {
                new OrderDetailRequest
                {
                    ProductId = 1,
                    Discount = (float)0.1m,
                    Quantity = 5,
                    UnitPrice = 10.0m
                }
            };

                var mockDbContext = new Mock<NorthwindDbContext>();
                var mockOrderSet = new Mock<DbSet<Order>>();
                mockDbContext.Setup(c => c.Orders).Returns(mockOrderSet.Object);
                mockOrderSet.Setup(s => s.FirstOrDefault(o => o.OrderId == orderId)).Returns((Order)null);

                var ordersRepository = new OrdersRepository(mockDbContext.Object, null);

                // Act
                var result = await ordersRepository.AddProductsToOrderAsync(orderId, orderDetails);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual($"Order ID {orderId} is not found.", result);
            }

            [Test]
            public async Task DeleteAsync_OrderExists_DeletesOrderAndOrderDetails()
            {
                var orderId = 10248;
                var order = new Order { OrderId = orderId };
                var orderDetails = new List<OrderDetail>
            {
                new OrderDetail { OrderId = orderId},
                new OrderDetail { OrderId = orderId}
            };

                var mockDbContext = new Mock<NorthwindDbContext>();

                mockDbContext.Setup(c => c.Orders.FirstOrDefault(o => o.OrderId == orderId)).Returns(order);
                mockDbContext.Setup(c => c.OrderDetails.Where(od => od.OrderId == orderId)).Returns(orderDetails.AsQueryable());

                mockDbContext.Setup(c => c.OrderDetails.RemoveRange(orderDetails));
                mockDbContext.Setup(c => c.Orders.Remove(order));
                mockDbContext.Setup(c => c.SaveChanges()).Returns(await Task.FromResult(1));

                var ordersRepository = new OrdersRepository(mockDbContext.Object, null); // Pass IMapper mock as needed

                // Act
                var result = await ordersRepository.DeleteAsync(orderId);

                // Assert
                Assert.AreEqual($"Order ID {orderId} has been deleted.", result);
                mockDbContext.Verify(c => c.OrderDetails.RemoveRange(orderDetails), Times.Once);
                mockDbContext.Verify(c => c.Orders.Remove(order), Times.Once);
                mockDbContext.Verify(c => c.SaveChanges(), Times.Once);
            }

            [Test]
            public async Task DeleteAsync_OrderNotFound_ReturnsErrorMessage()
            {
                // Arrange
                int orderId = 10248;

                var mockDbContext = new Mock<NorthwindDbContext>();
                mockDbContext.Setup(c => c.Orders.FirstOrDefault(o => o.OrderId == orderId)).Returns((Order)null);

                var ordersRepository = new OrdersRepository(mockDbContext.Object, null);

                // Act
                var result = await ordersRepository.DeleteAsync(orderId);

                // Assert
                Assert.AreEqual($"Order ID {orderId} is not found.", result);
                mockDbContext.Verify(c => c.OrderDetails.RemoveRange(It.IsAny<IEnumerable<OrderDetail>>()), Times.Never);
                mockDbContext.Verify(c => c.Orders.Remove(It.IsAny<Order>()), Times.Never);
                mockDbContext.Verify(c => c.SaveChanges(), Times.Never);
            }
        }


    }
}
