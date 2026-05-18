namespace Shared;

public record TellerLoginRequest(
    string Name,
    string Password
);

public record TellerRegisterRequest(
    string Name,
    string Password
);

public record AddCustomerRequest(
    string Name,
    string RegistNumber
);

public record AddAccountRequest(
    int CustomerId,
    string AccountNumber,
    string AccountType
);

public record ChangeBalanceRequest(
    int CustomerId,
    string AccountNumber,
    int Amount
);

public record CustomerInfoResponse(
    int Id,
    string Name,
    string RegistNumber,
    List<Account> Accounts
);

public record Teller(
    int Id,
    string Name,
    string Password
);

public record Customer(
    int Id,
    string Name,
    string RegistNumber
);

public record Account(
    int Id,
    int CustomerId,
    string AccountNumber,
    string AccountType,
    int Balance,
    long CreatedDate
);

public record CurrencyRate
{
    public string Code { get; set; } = "";
    public double Rate { get; set; }
}
