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

    [HttpPost]
    public async Task<ActionResult<Customer>> Post(CreateCustomerRequest customer)
    {
        using var session = _store.LightweightSession();
        var id = Guid.NewGuid();
        var customerCreatedEvent = new CustomerCreated(id, customer.Name);
        session.Events.StartStream<Customer>(id, customerCreatedEvent);
        await session.SaveChangesAsync();
        var createdCustomer = await session.Events.AggregateStreamAsync<Customer>(id);
        return CreatedAtAction(nameof(GetById), new { id = customerCreatedEvent.Id }, createdCustomer);
    }

    [HttpGet("{id}")]
    public async Task<Customer?> GetById(Guid id)
    {
        using var session = _store.LightweightSession();
        return await session.Events.AggregateStreamAsync<Customer>(id);
    }
}
public record CreateCustomerRequest(string Name);

public record CustomerCreated(Guid Id, string Name);


public class Customer {
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public void Apply(CustomerCreated @event) {
        Id = @event.Id;
        Name = @event.Name;
    }
}
