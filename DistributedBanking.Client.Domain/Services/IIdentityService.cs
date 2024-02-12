using Contracts.Models;
using DistributedBanking.Client.Domain.Models.Identity;

namespace DistributedBanking.Client.Domain.Services;

public interface IIdentityService
{
    Task<IdentityOperationResult> CreateRole(string roleName);
    Task<IdentityOperationResult> RegisterCustomer(EndUserRegistrationModel registrationModel);
    Task<IdentityOperationResult> RegisterWorker(WorkerRegistrationModel registrationModel, string role);
    Task<IdentityOperationResult> UpdateCustomerPersonalInformation(string customerId, CustomerPassportModel customerPassport);
    Task<IdentityOperationResult> DeleteUser(string email);
    Task<(IdentityOperationResult LoginResult, string? Token)> Login(LoginModel loginModel);
    Task Logout();
}