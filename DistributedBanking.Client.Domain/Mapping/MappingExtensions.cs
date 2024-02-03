using DistributedBanking.Client.Domain.Models.Account;
using DistributedBanking.Client.Domain.Models.Identity;
using DistributedBanking.Client.Domain.Models.Transaction;
using Shared.Data.Entities.Constants;
using Shared.Kafka.Messages.Account;
using Shared.Kafka.Messages.Identity;
using Shared.Kafka.Messages.Identity.Registration;
using Shared.Kafka.Messages.Transaction;

namespace DistributedBanking.Client.Domain.Mapping;

public static class MappingExtensions
{

    public static UserRegistrationMessage ToKafkaMessage(this EndUserRegistrationModel registrationModel, 
        string passwordHash, string salt)
    {
        return new UserRegistrationMessage(
            FirstName: registrationModel.FirstName,
            LastName: registrationModel.LastName,
            BirtDate: registrationModel.BirthDate,
            PhoneNumber: registrationModel.PhoneNumber,
            Email: registrationModel.Email,
            PasswordHash: passwordHash,
            Salt: salt,
            Passport: new Passport(
                DocumentNumber: registrationModel.Passport.DocumentNumber,
                Issuer: registrationModel.Passport.Issuer,
                IssueDateTime: registrationModel.Passport.IssueDateTime,
                ExpirationDateTime: registrationModel.Passport.ExpirationDateTime));
    }

    public static WorkerRegistrationMessage ToKafkaMessage(this WorkerRegistrationModel registrationModel, 
        string role, string passwordHash, string salt)
    {
        return new WorkerRegistrationMessage(
            FirstName: registrationModel.FirstName,
            LastName: registrationModel.LastName,
            BirtDate: registrationModel.BirthDate,
            PhoneNumber: registrationModel.PhoneNumber,
            Email: registrationModel.Email,
            PasswordHash: passwordHash,
            Salt: salt,
            Role: role,
            Position: registrationModel.Position,
            Passport: new Passport(registrationModel.Passport.DocumentNumber, registrationModel.Passport.Issuer,
                registrationModel.Passport.IssueDateTime, registrationModel.Passport.ExpirationDateTime),
            Address: new Address(registrationModel.Address.Country, registrationModel.Address.Region,
                registrationModel.Address.City, registrationModel.Address.Street, registrationModel.Address.Building,
                registrationModel.Address.PostalCode));
    }

    public static CustomerInformationUpdateMessage ToKafkaMessage(this CustomerPassportModel customerPassportModel,
        string customerId)
    {
        return new CustomerInformationUpdateMessage(
            CustomerId: customerId,
            DocumentNumber: customerPassportModel.DocumentNumber,
            Issuer: customerPassportModel.Issuer,
            IssueDateTime: customerPassportModel.IssueDateTime,
            ExpirationDateTime: customerPassportModel.ExpirationDateTime);
    }
    
    public static AccountCreationMessage ToKafkaMessage(this AccountCreationModel accountCreationModel, string customerId)
    {
        return new AccountCreationMessage(customerId, accountCreationModel.Name, accountCreationModel.Type);
    }
    
    public static TransactionMessage ToKafkaMessage(this OneWayTransactionModel transactionModel)
    {
        return new TransactionMessage(
            SourceAccountId: transactionModel.SourceAccountId,
            SourceSecurityCode: default,
            DestinationAccountId: default,
            Type: TransactionType.Deposit,
            Amount: transactionModel.Amount,
            Description: transactionModel.Description);
    }
    
    public static TransactionMessage ToKafkaMessage(this OneWaySecuredTransactionModel transactionModel)
    {
        return new TransactionMessage(
            SourceAccountId: transactionModel.SourceAccountId,
            SourceSecurityCode: transactionModel.SecurityCode,
            DestinationAccountId: default,
            Type: TransactionType.Withdrawal,
            Amount: transactionModel.Amount,
            Description: transactionModel.Description);
    }
    
    public static TransactionMessage ToKafkaMessage(this TwoWayTransactionModel transactionModel)
    {
        return new TransactionMessage(
            SourceAccountId: transactionModel.SourceAccountId,
            SourceSecurityCode: transactionModel.SourceAccountSecurityCode,
            DestinationAccountId: transactionModel.DestinationAccountId,
            Type: TransactionType.Transfer,
            Amount: transactionModel.Amount,
            Description: transactionModel.Description);
    }
}