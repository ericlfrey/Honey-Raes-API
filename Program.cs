using HoneyRaesAPI.Models;
List<Customer> customers = new List<Customer> {
    new Customer{
        Id = 1,
        Name = "Bob Ross",
        Address = "123 Main St"
    },
    new Customer{
        Id = 2,
        Name = "Sue Smith",
        Address = "123 Main St"
    },
    new Customer{
        Id = 3,
        Name = "Joe Hall",
        Address = "123 Main St"
    }
    };
List<Employee> employees = new List<Employee> {
    new Employee{
        Id = 1,
        Name = "Jim Moore",
        Specialty = "Plumbing"
    },
    new Employee{
        Id = 2,
        Name = "Susan Willis",
        Specialty = "Electrical"
    },
    new Employee{
        Id = 3,
        Name = "Benjamin Franklin",
        Specialty = "HVAC"
    }
};
List<ServiceTicket> serviceTickets = new List<ServiceTicket> {
    new ServiceTicket{
        Id = 1,
        CustomerId = 1,
        EmployeeId = 1,
        Description = "Water is leaking from the faucet",
        Emergency = false,
        DateCompleted = "2021-08-01"
    },
    new ServiceTicket{
        Id = 2,
        CustomerId = 1,
        EmployeeId = 2,
        Description = "Can't get the power to come on",
        Emergency = true,
        DateCompleted = "2021-08-02"
    },
    new ServiceTicket{
        Id = 3,
        CustomerId = 2,
        EmployeeId = 1,
        Description = "Can't get the power to come on",
        Emergency = true,
        DateCompleted = "2021-08-03"
    },
    new ServiceTicket{
        Id = 4,
        CustomerId = 3,
        EmployeeId = 2,
        Description = "Water is leaking from the faucet",
        Emergency = false,
        DateCompleted = "2022-08-01"
    },
    new ServiceTicket{
        Id = 5,
        CustomerId = 3,
        EmployeeId = 3,
        Description = "Can't get the power to come on",
        Emergency = true,
        // DateCompleted = "2023-08-01"
    },
    new ServiceTicket{
        Id = 6,
        CustomerId = 1,
        EmployeeId = 1,
        Description = "Problem 6",
        Emergency = true,
        // DateCompleted = "2024-01-01"
    },
    new ServiceTicket{
        Id = 7,
        CustomerId = 2,
        EmployeeId = 1,
        Description = "Problem 7",
        Emergency = true,
        // DateCompleted = "2024-01-01"
    },
    new ServiceTicket{
        Id = 8,
        CustomerId = 3,
        EmployeeId = 1,
        Description = "Problem 8",
        Emergency = true,
        // DateCompleted = "2024-01-01"
    },
    new ServiceTicket{
        Id = 9,
        CustomerId = 1,
        EmployeeId = 2,
        Description = "Problem 7",
        Emergency = true,
        // DateCompleted = "2024-01-01"
    },
    new ServiceTicket{
        Id = 10,
        CustomerId = 2,
        EmployeeId = 3,
        Description = "Problem 8",
        Emergency = true,
        // DateCompleted = "2024-01-01"
    }
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Service Ticket Requests
app.MapGet("/api/servicetickets", () =>
{
    return serviceTickets;
});
app.MapGet("/api/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(s => s.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    serviceTicket.Customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);
    return Results.Ok(serviceTicket);
});
app.MapPost("/api/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});
app.MapPut("/api/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    //the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok(serviceTicket);
});
app.MapDelete("/api/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(s => s.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTickets.Remove(serviceTicket);
    return Results.Ok();
});
app.MapPost("/api/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today.ToString();
});
// GET all Emergency Tickets
app.MapGet("/api/servicetickets/emergency", () =>
{
    return serviceTickets.Where(st => st.Emergency == true && st.DateCompleted == null);
});
// GET all Unassigned Tickets
app.MapGet("/api/servicetickets/unassigned", () =>
{
    return serviceTickets.Where(st => st.EmployeeId == null);
});
// Past Ticket review
// TODO: Create an endpoint to return completed tickets in order of the completion data, oldest first.
app.MapGet("/api/servicetickets/completed-oldest-newest", () =>
{
    var completedTickets = serviceTickets.Where(ticket => ticket.DateCompleted != null)
        .OrderByDescending(ticket => DateTime.Parse(ticket.DateCompleted))
        .Reverse()
        .ToList();
    return Results.Ok(completedTickets);
});

// Customer Requests
app.MapGet("/api/customers", () =>
{
    return customers;
});
app.MapGet("/api/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    // customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(customer);
});

// Inactive Customers
app.MapGet("/api/customers/inactive", () =>
{
    // Calculate the date one year ago from the current date
    DateTime oneYearAgo = DateTime.Today.AddYears(-1);

    // Filter customers based on the absence of closed service tickets within the past year
    var inactiveCustomers = customers.Where(customer =>
    {
        // Check if there are any service tickets closed for the customer in the past year
        return !serviceTickets.Any(ticket =>
            ticket.CustomerId == customer.Id &&
            ticket.DateCompleted != null &&
            DateTime.Parse(ticket.DateCompleted) >= oneYearAgo);
    });

    return Results.Ok(inactiveCustomers);
});


// Employee Requests
app.MapGet("/api/employees", () =>
{
    return employees;
});
app.MapGet("/api/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(employee);
});
// Available Employees
app.MapGet("/api/employees/available", () =>
{
    var availableEmployees = employees.Where(employee =>
    {
        return !serviceTickets.Any(ticket =>
        ticket.EmployeeId == employee.Id &&
        ticket.DateCompleted == null);
    });
    return Results.Ok(availableEmployees);
});
// Employee's customers
app.MapGet("/api/employees/{id}/customers", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    var employeeCustomers = customers.Where(customer =>
    {
        return serviceTickets.Any(ticket =>
        ticket.CustomerId == customer.Id &&
        ticket.EmployeeId == employee.Id);
    });
    return Results.Ok(employeeCustomers);
});
// 6. Employee of the month
// TODO: Create and endpoint to return the employee who has completed the most service tickets last month.
app.MapGet("/api/employees/eom", () =>
{
    // Calculate the date one month ago from the current date
    DateTime oneMonthAgo = DateTime.Today.AddMonths(-1);

    // Filter service tickets closed in the last month
    var closedLastMonth = serviceTickets
        .Where(ticket =>
            ticket.DateCompleted != null &&
            DateTime.Parse(ticket.DateCompleted) >= oneMonthAgo);

    // Group the service tickets by EmployeeId and count the closed tickets for each employee
    var employeeTicketCounts = closedLastMonth
        .GroupBy(ticket => ticket.EmployeeId)
        .Select(group => new
        {
            EmployeeId = group.Key,
            TicketCount = group.Count()
        })
        .ToList();

    // Find the employee with the most closed service tickets
    var employeeWithMostClosedTickets = employees
        .OrderByDescending(employee =>
            employeeTicketCounts.FirstOrDefault(x => x.EmployeeId == employee.Id)?.TicketCount ?? 0)
        .FirstOrDefault();

    return Results.Ok(employeeWithMostClosedTickets);
});

app.Run();
