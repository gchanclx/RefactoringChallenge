using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using RefactoringChallenge.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace RefactoringChallenge.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly NorthwindDbContext _northwindDbContext;
        private readonly IMapper _mapper;

        public OrdersRepository(NorthwindDbContext northwindDbContext, IMapper mapper)
        {
            _northwindDbContext = northwindDbContext;
            _mapper = mapper;
        }

        public async Task<string> AddProductsToOrderAsync([FromRoute] int orderId, IEnumerable<OrderDetailRequest> orderDetails)
        {
            var order = _northwindDbContext.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                return string.Format("[AddProducts] Order ID {0} is not found.", orderId); ;

            var newOrderDetails = new List<OrderDetail>();
            foreach (var orderDetail in orderDetails)
            {
                newOrderDetails.Add(new OrderDetail
                {
                    OrderId = orderId,
                    ProductId = orderDetail.ProductId,
                    Discount = orderDetail.Discount,
                    Quantity = orderDetail.Quantity,
                    UnitPrice = orderDetail.UnitPrice,
                });
            }

            await Task.Run(() => _northwindDbContext.OrderDetails.AddRange(newOrderDetails));
            await Task.Run(() => _northwindDbContext.SaveChanges());

            return JsonSerializer.Serialize(newOrderDetails.Select(od => od.Adapt<OrderDetailResponse>()));
        }

        public async Task<string> CreateAsync(
            string customerId,
            int? employeeId,
            DateTime? requiredDate,
            int? shipVia,
            decimal? freight,
            string shipName,
            string shipAddress,
            string shipCity,
            string shipRegion,
            string shipPostalCode,
            string shipCountry,
            IEnumerable<OrderDetailRequest> orderDetails)
        {
            var newOrderDetails = new List<OrderDetail>();
            foreach (var orderDetail in orderDetails)
            {
                newOrderDetails.Add(new OrderDetail 
                {
                    ProductId = orderDetail.ProductId,
                    Discount = orderDetail.Discount,
                    Quantity = orderDetail.Quantity,
                    UnitPrice = orderDetail.UnitPrice,
                });
            }

            var newOrder = new Order
            {
                CustomerId = customerId,
                EmployeeId = employeeId,
                OrderDate = DateTime.Now,
                RequiredDate = requiredDate,
                ShipVia = shipVia,
                Freight = freight,
                ShipName = shipName,
                ShipAddress = shipAddress,
                ShipCity = shipCity,
                ShipRegion = shipRegion,
                ShipPostalCode = shipPostalCode,
                ShipCountry = shipCountry,
                OrderDetails = newOrderDetails,
            };

            await Task.Run(() => _northwindDbContext.Orders.AddAsync(newOrder));
            await Task.Run(() => _northwindDbContext.SaveChangesAsync());

            return JsonSerializer.Serialize(newOrder.Adapt<OrderResponse>());
        }

        public async Task<string> DeleteAsync([FromRoute] int orderId)
        {
            var order = _northwindDbContext.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                return string.Format("[Delete] Order ID {0} in is not found.", orderId);

            var orderDetails = _northwindDbContext.OrderDetails.Where(od => od.OrderId == orderId);

            await Task.Run(() => _northwindDbContext.OrderDetails.RemoveRange(orderDetails));
            await Task.Run(() => _northwindDbContext.Orders.Remove(order));
            await Task.Run(() => _northwindDbContext.SaveChangesAsync());

            return string.Format("Order ID {0} has been deleted.", orderId);
        }

        public async Task<string> GetAsync(int? skip = null, int? take = null)
        {
            var query = _northwindDbContext.Orders;
            if (skip != null)
            {
                query.Skip(skip.Value);
            }
            if (take != null)
            {
                query.Take(take.Value);
            }
            var result = await Task.Run(() => _mapper.From(query).ProjectToType<OrderResponse>().ToListAsync());
            return JsonSerializer.Serialize(result);
        }

        public async Task<string> GetByIdAsync([FromRoute] int orderId)
        {
            var result = await Task.Run(() => _mapper.From(_northwindDbContext.Orders).ProjectToType<OrderResponse>().FirstOrDefault(o => o.OrderId == orderId));

            if (result == null)
                return string.Format("order ID {0} is not found.", orderId);
            return JsonSerializer.Serialize(result);
        }
    }
}
