namespace HoneyRaesAPI.Models;

public class ServiceTicket
{
  public int Id { get; set; }
  public int CustomerId { get; set; }
  public int? EmployeeId { get; set; }
  public string Description { get; set; }
  public bool Emergency { get; set; }
  public string DateCompleted { get; set; }
  public Employee Employee { get; set; }
  public Customer Customer { get; set; }
}
