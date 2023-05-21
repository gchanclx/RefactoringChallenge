using Microsoft.AspNetCore.Mvc;
using RefactoringChallenge.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RefactoringChallenge.Repositories
{
    public interface IOrdersRepository
    {
        public Task<string> GetAsync(int? skip = null, int? take = null);
        public Task<string> GetByIdAsync([FromRoute] int orderId);
        public Task<string> CreateAsync(
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
            IEnumerable<OrderDetailRequest> orderDetails);
        public Task<string> AddProductsToOrderAsync([FromRoute] int orderId, IEnumerable<OrderDetailRequest> orderDetails);
        public Task<string> DeleteAsync([FromRoute] int orderId);
    }
}
