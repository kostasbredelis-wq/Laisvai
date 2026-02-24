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
[Authorize(Roles = "Freelancer,Client")]
public class ConversationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ConversationsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConversationSummaryDto>>> GetConversations(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var role = User.IsInRole("Freelancer") ? "Freelancer" : "Client";

        var query = _context.Conversations
            .Include(c => c.Freelancer)
            .Include(c => c.Client)
            .Include(c => c.JobListing)
            .Include(c => c.Messages)
            .AsQueryable();

        query = role == "Freelancer"
            ? query.Where(c => c.Freelancer.UserId == userId)
            : query.Where(c => c.Client.UserId == userId);

        var conversations = await query
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(cancellationToken);

        return Ok(conversations.Select(c => MapToSummaryDto(c, userId)));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ConversationDetailDto>> GetConversation(
        int id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var role = User.IsInRole("Freelancer") ? "Freelancer" : "Client";

        var conversation = await _context.Conversations
            .Include(c => c.Freelancer)
            .Include(c => c.Client)
            .Include(c => c.JobListing)
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (conversation == null)
            return NotFound();

        if (!IsParticipant(conversation, userId, role))
            return StatusCode(403, new { message = "You are not a participant in this conversation." });

        return Ok(MapToDetailDto(conversation, userId));
    }

    [HttpPost("{id:int}/messages")]
    public async Task<ActionResult<MessageResponseDto>> SendMessage(
        int id,
        SendMessageDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var role = User.IsInRole("Freelancer") ? "Freelancer" : "Client";

        var conversation = await _context.Conversations
            .Include(c => c.Freelancer)
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (conversation == null)
            return NotFound();

        if (!IsParticipant(conversation, userId, role))
            return StatusCode(403, new { message = "You are not a participant in this conversation." });

        var message = new Message
        {
            ConversationId = id,
            SenderId = userId,
            Content = dto.Content
        };

        conversation.UpdatedAt = DateTime.UtcNow;
        _context.Messages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        return StatusCode(201, MapToMessageDto(message));
    }

    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var role = User.IsInRole("Freelancer") ? "Freelancer" : "Client";

        var conversation = await _context.Conversations
            .Include(c => c.Freelancer)
            .Include(c => c.Client)
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (conversation == null)
            return NotFound();

        if (!IsParticipant(conversation, userId, role))
            return StatusCode(403, new { message = "You are not a participant in this conversation." });

        var unread = conversation.Messages
            .Where(m => m.SenderId != userId && !m.IsRead)
            .ToList();

        if (unread.Count > 0)
        {
            foreach (var m in unread)
                m.IsRead = true;

            await _context.SaveChangesAsync(cancellationToken);
        }

        return NoContent();
    }

    private static bool IsParticipant(Conversation conversation, int userId, string role) =>
        role == "Freelancer"
            ? conversation.Freelancer.UserId == userId
            : conversation.Client.UserId == userId;

    private int GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new UnauthorizedAccessException("User ID not found in claims.");
        return int.Parse(value);
    }

    private static ConversationSummaryDto MapToSummaryDto(Conversation c, int userId) => new()
    {
        Id = c.Id,
        FreelancerId = c.FreelancerId,
        FreelancerName = c.Freelancer.FullName,
        ClientId = c.ClientId,
        ClientName = c.Client.DisplayName,
        JobListingId = c.JobListingId,
        JobTitle = c.JobListing?.Title,
        LastMessageAt = c.UpdatedAt,
        UnreadCount = c.Messages.Count(m => !m.IsRead && m.SenderId != userId)
    };

    private static ConversationDetailDto MapToDetailDto(Conversation c, int userId) => new()
    {
        Id = c.Id,
        FreelancerId = c.FreelancerId,
        FreelancerName = c.Freelancer.FullName,
        ClientId = c.ClientId,
        ClientName = c.Client.DisplayName,
        JobListingId = c.JobListingId,
        JobTitle = c.JobListing?.Title,
        LastMessageAt = c.UpdatedAt,
        UnreadCount = c.Messages.Count(m => !m.IsRead && m.SenderId != userId),
        Messages = c.Messages
            .OrderBy(m => m.SentAt)
            .Select(MapToMessageDto)
            .ToList()
    };

    private static MessageResponseDto MapToMessageDto(Message m) => new()
    {
        Id = m.Id,
        ConversationId = m.ConversationId,
        SenderId = m.SenderId,
        Content = m.Content,
        IsRead = m.IsRead,
        SentAt = m.SentAt
    };
}
