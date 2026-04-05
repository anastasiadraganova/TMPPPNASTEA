using FreelanceMarket.Application.Dtos;

namespace FreelanceMarket.Application.Services;

public interface IProjectCategoryService
{
    Task<IReadOnlyList<ProjectCategoryNodeDto>> GetTreeAsync();
}
