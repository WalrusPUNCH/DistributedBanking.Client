using Contracts.Models;
using DistributedBanking.Client.Domain.Models.Identity;

namespace DistributedBanking.Client.Domain.Services;

public interface IIdentityService
{
    Task<OperationResult> CreateRole(string roleName);
    Task<OperationResult> RegisterCustomer(EndUserRegistrationModel registrationModel);
    Task<OperationResult> RegisterWorker(WorkerRegistrationModel registrationModel, string role);
    Task<OperationResult> UpdateCustomerPersonalInformation(string customerId, CustomerPassportModel customerPassport);
    Task<OperationResult> DeleteUser(string email);
    Task<OperationResult<ShortUserModelResponse>> CustomerIdentityInformation(string customerId);
    Task<OperationResult<ShortWorkerModelResponse>> WorkerIdentityInformation(string workerId);
    Task<OperationResult<LoginResultModel>> Login(LoginModel loginModel);
    Task Logout();
}