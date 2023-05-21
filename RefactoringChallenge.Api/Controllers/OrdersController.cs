using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using RefactoringChallenge.Entities;
using RefactoringChallenge.Repositories;

namespace RefactoringChallenge.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : Controller
    {
        private readonly IOrdersRepository _ordersRepository;
         public OrdersController(IOrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository;
        }

        [HttpGet]
        public async Task<string> Get(int? skip = null, int? take = null)
        {
             try
            {
                var result = await _ordersRepository.GetAsync(skip, take);
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        [HttpGet("{orderId}")]
        public async Task<string> GetById([FromRoute] int orderId)
        {
            try
            {
                var result = await _ordersRepository.GetByIdAsync(orderId);
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpPost("[action]")]
        public async Task<string> Create(
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
            IEnumerable<OrderDetailRequest> orderDetails
            )
        {
            try
            {
                var result = _ordersRepository.CreateAsync(customerId,
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
                return await result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        [HttpPost("{orderId}/[action]")]
        public async Task<string> AddProductsToOrder([FromRoute] int orderId, IEnumerable<OrderDetailRequest> orderDetails)
        {
            try
            {
                var result = await _ordersRepository.AddProductsToOrderAsync(orderId, orderDetails);
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpPost("{orderId}/[action]")]
        //public IActionResult Delete([FromRoute] int orderId)
        public async Task<string> Delete([FromRoute] int orderId)
        {
            try
            {
                string result = await _ordersRepository.DeleteAsync(orderId);
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
