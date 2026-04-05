using FreelanceMarket.Application.Dtos;
using FreelanceMarket.Domain.Entities;
using FreelanceMarket.Domain.Enums;
using FreelanceMarket.Domain.Interfaces;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Application.Services;

public class ProposalService : IProposalService
{
    private readonly IProposalRepository _proposalRepo;
    private readonly IProjectRepository _projectRepo;
    private readonly IUserRepository _userRepo;

    public ProposalService(IProposalRepository proposalRepo, IProjectRepository projectRepo, IUserRepository userRepo)
    {
        _proposalRepo = proposalRepo;
        _projectRepo = projectRepo;
        _userRepo = userRepo;
    }

    public async Task<IReadOnlyList<ProposalDto>> GetByProjectIdAsync(Guid projectId)
    {
        var proposals = await _proposalRepo.GetByProjectIdAsync(projectId);
        var dtos = new List<ProposalDto>();
        foreach (var p in proposals) dtos.Add(await ToDtoAsync(p));
        return dtos;
    }

    public async Task<IReadOnlyList<ProposalDto>> GetByFreelancerIdAsync(Guid freelancerId)
    {
        var proposals = await _proposalRepo.GetByFreelancerIdAsync(freelancerId);
        var dtos = new List<ProposalDto>();
        foreach (var p in proposals) dtos.Add(await ToDtoAsync(p));
        return dtos;
    }

    public async Task<ProposalDto> CreateAsync(Guid projectId, CreateProposalRequest req, Guid freelancerId)
    {
        var project = await _projectRepo.GetByIdAsync(projectId)
            ?? throw new KeyNotFoundException("Проект не найден");

        if (project.Status != ProjectStatus.Open)
            throw new InvalidOperationException("Проект не открыт для откликов");

        var proposal = new Proposal
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            FreelancerId = freelancerId,
            CoverLetter = req.CoverLetter,
            BidAmount = req.BidAmount,
            Status = ProposalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _proposalRepo.AddAsync(proposal);

        // Паттерн Factory Method: создаём уведомление для заказчика
        var freelancer = await _userRepo.GetByIdAsync(freelancerId);
        INotificationFactory factory = new ProposalReceivedNotificationFactory(
            project.CustomerId, projectId, freelancer?.Name ?? "Фрилансер");
        var notification = factory.CreateNotification();
        // В production уведомление сохранялось бы в БД / отправлялось через SignalR
        _ = notification; // используется для демонстрации паттерна

        return await ToDtoAsync(proposal);
    }

    public async Task AcceptAsync(Guid proposalId, Guid customerId)
    {
        var proposal = await _proposalRepo.GetByIdAsync(proposalId)
            ?? throw new KeyNotFoundException("Отклик не найден");

        var project = await _projectRepo.GetByIdAsync(proposal.ProjectId)
            ?? throw new KeyNotFoundException("Проект не найден");

        if (project.CustomerId != customerId)
            throw new UnauthorizedAccessException("Вы не являетесь владельцем проекта");

        proposal.Status = ProposalStatus.Accepted;
        await _proposalRepo.UpdateAsync(proposal);

        // Назначаем фрилансера на проект
        project.FreelancerId = proposal.FreelancerId;
        project.Status = ProjectStatus.InProgress;
        project.UpdatedAt = DateTime.UtcNow;
        await _projectRepo.UpdateAsync(project);

        // Паттерн Factory Method: уведомляем фрилансера о принятии
        INotificationFactory factory = new ProposalAcceptedNotificationFactory(
            proposal.FreelancerId, proposalId, project.Title);
        var notification = factory.CreateNotification();
        _ = notification;
    }

    public async Task RejectAsync(Guid proposalId, Guid customerId)
    {
        var proposal = await _proposalRepo.GetByIdAsync(proposalId)
            ?? throw new KeyNotFoundException("Отклик не найден");

        var project = await _projectRepo.GetByIdAsync(proposal.ProjectId)
            ?? throw new KeyNotFoundException("Проект не найден");

        if (project.CustomerId != customerId)
            throw new UnauthorizedAccessException("Вы не являетесь владельцем проекта");

        proposal.Status = ProposalStatus.Rejected;
        await _proposalRepo.UpdateAsync(proposal);
    }

    private async Task<ProposalDto> ToDtoAsync(Proposal p)
    {
        var freelancer = await _userRepo.GetByIdAsync(p.FreelancerId);
        return new ProposalDto(
            p.Id, p.ProjectId, p.FreelancerId,
            freelancer?.Name ?? "Unknown",
            p.CoverLetter, p.BidAmount, p.Status, p.CreatedAt);
    }
}
