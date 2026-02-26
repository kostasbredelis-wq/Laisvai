using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FreelanceMarketplace.Data;
using FreelanceMarketplace.DTOs;
using FreelanceMarketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelanceMarketplace.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReviewsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ReviewResponseDto>> CreateReview(
        CreateReviewDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == userId, cancellationToken);

        if (client == null)
            return NotFound();

        var freelancer = await _context.FreelancerProfiles
            .FirstOrDefaultAsync(fp => fp.Id == dto.FreelancerId
                && fp.ApplicationStatus == FreelancerApplicationStatus.Approved, cancellationToken);

        if (freelancer == null)
            return NotFound();

        var alreadyReviewed = await _context.Reviews
            .AnyAsync(r => r.ClientId == client.Id && r.FreelancerId == dto.FreelancerId, cancellationToken);

        if (alreadyReviewed)
            return Conflict(new { message = "You have already reviewed this freelancer." });

        var review = new Review
        {
            ClientId = client.Id,
            FreelancerId = dto.FreelancerId,
            Rating = dto.Rating,
            Comment = dto.Comment
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);

        review.Client = client;

        return StatusCode(201, MapToResponseDto(review));
    }

    [HttpGet("freelancer/{freelancerId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ReviewResponseDto>>> GetFreelancerReviews(
        int freelancerId,
        CancellationToken cancellationToken)
    {
        var freelancerExists = await _context.FreelancerProfiles
            .AnyAsync(fp => fp.Id == freelancerId
                && fp.ApplicationStatus == FreelancerApplicationStatus.Approved, cancellationToken);

        if (!freelancerExists)
            return NotFound();

        var reviews = await _context.Reviews
            .Include(r => r.Client)
            .Where(r => r.FreelancerId == freelancerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(reviews.Select(MapToResponseDto));
    }

    private int GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new UnauthorizedAccessException("User ID not found in claims.");
        return int.Parse(value);
    }

    private static ReviewResponseDto MapToResponseDto(Review r) => new()
    {
        Id = r.Id,
        ClientId = r.ClientId,
        ClientName = r.Client.DisplayName,
        FreelancerId = r.FreelancerId,
        Rating = r.Rating,
        Comment = r.Comment,
        CreatedAt = r.CreatedAt
    };
}
