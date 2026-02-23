using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.Models;

/// <summary>Conversation between a freelancer and client. Created when application is accepted.</summary>
public class Conversation
{
    public int Id { get; set; }

    public int FreelancerId { get; set; }

    public FreelancerProfile Freelancer { get; set; } = null!;

    public int ClientId { get; set; }

    public ClientProfile Client { get; set; } = null!;

    public int? JobListingId { get; set; }

    public JobListing? JobListing { get; set; }

    public int? ApplicationId { get; set; }

    public Application? Application { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
