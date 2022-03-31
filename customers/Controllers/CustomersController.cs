using Microsoft.AspNetCore.Mvc;
using Marten;

namespace Customer.Controllers;

[ApiController]
[Route("[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ILogger<CustomersController> _logger;
    private readonly IDocumentStore _store;

    public CustomersController(ILogger<CustomersController> logger, IDocumentStore store)
    {
        _logger = logger;
        _store = store;
    }

    [HttpPost(Name = "CreateCustomer")]
    public async Task<Customer> Post(Customer customer)
    {
        var session = _store.LightweightSession();
        if (customer.Id == Guid.Empty) {
            customer.Id = Guid.NewGuid();
        }
        session.Insert<Customer>(customer);
        await session.SaveChangesAsync();
        return customer;
    }
}

public class Customer
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
}
