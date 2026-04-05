using FreelanceMarket.Domain.Enums;

namespace FreelanceMarket.Application.Dtos;

// ─── Auth ───
public record RegisterRequest(string Email, string Password, string Name, UserRole Role);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, UserDto User);

// ─── User ───
public record UserDto(Guid Id, string Email, string Name, UserRole Role, DateTime CreatedAt);
public record FreelancerPortfolioDto(
    Guid UserId,
    string DisplayName,
    string Bio,
    string AvatarUrl,
    IReadOnlyList<string> Skills,
    int CompletedProjects,
    double ExternalRating,
    string Source);

// ─── Project ───
public record CreateProjectRequest(
    string Title,
    string Description,
    decimal Budget,
    ProjectType Type,
    DateTime? Deadline,
    List<string> RequiredSkills);

public record UpdateProjectRequest(
    string Title,
    string Description,
    decimal Budget,
    DateTime? Deadline,
    List<string> RequiredSkills);

public record ProjectDto(
    Guid Id,
    string Title,
    string Description,
    decimal Budget,
    ProjectStatus Status,
    ProjectType Type,
    Guid CustomerId,
    string CustomerName,
    Guid? FreelancerId,
    string? FreelancerName,
    DateTime CreatedAt,
    DateTime? Deadline,
    List<string> RequiredSkills,
    int ProposalCount);

// ─── Proposal ───
public record CreateProposalRequest(string CoverLetter, decimal BidAmount);

public record ProposalDto(
    Guid Id,
    Guid ProjectId,
    Guid FreelancerId,
    string FreelancerName,
    string CoverLetter,
    decimal BidAmount,
    ProposalStatus Status,
    DateTime CreatedAt);

// ─── Review ───
public record CreateReviewRequest(Guid ToUserId, int Rating, string Comment);

public record ReviewDto(
    Guid Id,
    Guid FromUserId,
    string FromUserName,
    Guid ToUserId,
    int Rating,
    string Comment,
    DateTime CreatedAt);

// ─── Template (Prototype) ───
public record CreateFromTemplateRequest(string TemplateId);

// ─── Composite: Categories ───
public record ProjectCategoryNodeDto(
    string Name,
    bool IsLeaf,
    IReadOnlyList<ProjectCategoryNodeDto> Children);

// ─── Error ───
public record ApiError(string Code, string Message);
