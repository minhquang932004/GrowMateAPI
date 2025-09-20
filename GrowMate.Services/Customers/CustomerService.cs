using GrowMate.Contracts.Requests;
using GrowMate.Repositories.Interfaces;
using GrowMate.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowMate.Services.Customers
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateByUserId(int userId, CustomerRequest? request, CancellationToken ct)
        {
                var newCustomer = new Customer
                {
                    CustomerId = userId,
                    ShippingAddress = request.ShippingAddress,
                    WalletBalance = request.WalletBalance,
                    CreatedAt = DateTime.Now,
                };
                await _unitOfWork.Customers.CreateAsync(newCustomer, ct);
        }

        public async Task<bool> GetCustomerByIdAsync(int id)
            => await _unitOfWork.Customers.AnyAsync(id);

        public async Task RemoveByUserIdAsync(int userId, CancellationToken ct)
        {
            var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId, ct);
            if (customer != null)
            {
                _unitOfWork.Customers.Remove(customer);
            }
        }

        public async Task UpdateCustomerAsync(int id, CustomerRequest request, CancellationToken ct)
        {
            var customerExist = await _unitOfWork.Customers.GetByUserIdAsync(id, ct);
            if(customerExist != null)
            {
                customerExist.ShippingAddress = request.ShippingAddress;
                customerExist.WalletBalance = request.WalletBalance;
                _unitOfWork.Customers.UpdateAsync(customerExist);
            }
        }
    }
}
