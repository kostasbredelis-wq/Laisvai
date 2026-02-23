using Microsoft.EntityFrameworkCore;
using FreelanceMarketplace.Models;

namespace FreelanceMarketplace.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<FreelancerProfile> FreelancerProfiles => Set<FreelancerProfile>();
    public DbSet<ClientProfile> ClientProfiles => Set<ClientProfile>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<FreelancerSkill> FreelancerSkills => Set<FreelancerSkill>();
    public DbSet<JobListingSkill> JobListingSkills => Set<JobListingSkill>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<JobListing> JobListings => Set<JobListing>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Composite primary keys
        modelBuilder.Entity<FreelancerSkill>()
            .HasKey(fs => new { fs.FreelancerId, fs.SkillId });

        modelBuilder.Entity<JobListingSkill>()
            .HasKey(js => new { js.JobListingId, js.SkillId });

        // Unique constraints
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<FreelancerProfile>()
            .HasIndex(fp => fp.UserId)
            .IsUnique();

        modelBuilder.Entity<ClientProfile>()
            .HasIndex(cp => cp.UserId)
            .IsUnique();

        modelBuilder.Entity<Review>()
            .HasIndex(r => new { r.ClientId, r.FreelancerId })
            .IsUnique();

        modelBuilder.Entity<Application>()
            .HasIndex(a => new { a.JobListingId, a.FreelancerId })
            .IsUnique();

        modelBuilder.Entity<Conversation>()
            .HasIndex(c => new { c.FreelancerId, c.ClientId, c.JobListingId })
            .IsUnique()
            .HasFilter("\"JobListingId\" IS NOT NULL");

        modelBuilder.Entity<Conversation>()
            .HasIndex(c => new { c.FreelancerId, c.ClientId })
            .IsUnique()
            .HasFilter("\"JobListingId\" IS NULL");

        // User → FreelancerProfile (one-to-one)
        modelBuilder.Entity<User>()
            .HasOne(u => u.FreelancerProfile)
            .WithOne(fp => fp.User)
            .HasForeignKey<FreelancerProfile>(fp => fp.UserId);

        // User → ClientProfile (one-to-one)
        modelBuilder.Entity<User>()
            .HasOne(u => u.ClientProfile)
            .WithOne(cp => cp.User)
            .HasForeignKey<ClientProfile>(cp => cp.UserId);

        // User → SentMessages (one-to-many)
        modelBuilder.Entity<User>()
            .HasMany(u => u.SentMessages)
            .WithOne(m => m.Sender)
            .HasForeignKey(m => m.SenderId);

        // FreelancerProfile → Conversations (explicit FK configuration)
        modelBuilder.Entity<FreelancerProfile>()
            .HasMany(fp => fp.Conversations)
            .WithOne(c => c.Freelancer)
            .HasForeignKey(c => c.FreelancerId);

        // ClientProfile → Conversations (explicit FK configuration)
        modelBuilder.Entity<ClientProfile>()
            .HasMany(cp => cp.Conversations)
            .WithOne(c => c.Client)
            .HasForeignKey(c => c.ClientId);

        // Application → Conversation (one-to-one)
        modelBuilder.Entity<Application>()
            .HasOne(a => a.Conversation)
            .WithOne(c => c.Application)
            .HasForeignKey<Conversation>(c => c.ApplicationId);

        // Enum storage as strings
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        modelBuilder.Entity<FreelancerProfile>()
            .Property(fp => fp.Category)
            .HasConversion<string>();

        modelBuilder.Entity<FreelancerProfile>()
            .Property(fp => fp.ApplicationStatus)
            .HasConversion<string>();

        modelBuilder.Entity<Service>()
            .Property(s => s.Category)
            .HasConversion<string>();

        modelBuilder.Entity<Service>()
            .Property(s => s.PricingType)
            .HasConversion<string>();

        modelBuilder.Entity<JobListing>()
            .Property(j => j.Category)
            .HasConversion<string>();

        modelBuilder.Entity<JobListing>()
            .Property(j => j.BudgetType)
            .HasConversion<string>();

        modelBuilder.Entity<Application>()
            .Property(a => a.Status)
            .HasConversion<string>();
    }
}
