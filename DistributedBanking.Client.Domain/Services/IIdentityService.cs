using DistributedBanking.Client.Domain.Models.Identity;

namespace DistributedBanking.Client.Domain.Services;

public interface IIdentityService
{
    Task<IdentityOperationResult> CreateRole(string roleName);
    Task<bool> RegisterCustomer(EndUserRegistrationModel registrationModel);
    Task<bool> RegisterWorker(WorkerRegistrationModel registrationModel, string role);
    Task<bool> DeleteUser(string email);
    Task<(IdentityOperationResult LoginResult, string? Token)> Login(LoginModel loginModel);
    Task Logout();
    Task<bool> UpdateCustomerPersonalInformation(string customerId, CustomerPassportModel customerPassport);
}