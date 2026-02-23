namespace FreelanceMarketplace.Models;

/// <summary>User role in the platform.</summary>
public enum UserRole
{
    Freelancer,
    Client,
    Admin
}

/// <summary>Freelancer and service category.</summary>
public enum FreelancerCategory
{
    WebDeveloper,
    MobileDeveloper,
    UIUXDesigner,
    GraphicDesigner
}

/// <summary>Status of a freelancer's application to a job listing.</summary>
public enum ApplicationStatus
{
    Pending,
    Accepted,
    Rejected
}

/// <summary>How pricing is specified for a service.</summary>
public enum PricingType
{
    Negotiable
}

/// <summary>How budget is specified for a job listing.</summary>
public enum BudgetType
{
    Negotiable
}

/// <summary>Status of a freelancer's platform application (admin approval).</summary>
public enum FreelancerApplicationStatus
{
    Pending,
    Approved,
    Rejected
}
