using Contracts;
using DistributedBanking.Client.Domain.Models.Identity;

namespace DistributedBanking.Client.Domain.Services;

public interface IIdentityService
{
    Task<IdentityOperationResult> CreateRole(string roleName);

    Task<IdentityOperationResult> RegisterUser(
        EndUserRegistrationModel registrationModel, string role);
    
    Task DeleteUser(string email);

    Task<(IdentityOperationResult LoginResult, string? Token)> Login(LoginModel loginModel);

    Task Logout();

    Task<OperationStatusModel> UpdateCustomerPersonalInformation(string customerId, CustomerPassportModel customerPassport);
}