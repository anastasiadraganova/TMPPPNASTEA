using FreelanceMarket.Application.Dtos;
using FreelanceMarket.Domain.Patterns;

namespace FreelanceMarket.Application.Services;

/// <summary>
/// Composite в Application: сервис работает с единым интерфейсом узлов дерева,
/// не различая листья и составные категории.
/// </summary>
public class ProjectCategoryService : IProjectCategoryService
{
    public Task<IReadOnlyList<ProjectCategoryNodeDto>> GetTreeAsync()
    {
        var rootNodes = BuildCategoryTree();
        var result = rootNodes.Select(ToDto).ToList();
        return Task.FromResult<IReadOnlyList<ProjectCategoryNodeDto>>(result);
    }

    private static IReadOnlyList<IProjectCategoryNode> BuildCategoryTree()
    {
        var web = new ProjectCategoryComposite("Web Development");
        web.AddChild(new ProjectCategoryLeaf("Frontend"));
        web.AddChild(new ProjectCategoryLeaf("Backend"));
        web.AddChild(new ProjectCategoryLeaf("Fullstack"));

        var mobile = new ProjectCategoryComposite("Mobile Development");
        mobile.AddChild(new ProjectCategoryLeaf("iOS"));
        mobile.AddChild(new ProjectCategoryLeaf("Android"));
        mobile.AddChild(new ProjectCategoryLeaf("React Native"));

        var design = new ProjectCategoryComposite("Design");
        design.AddChild(new ProjectCategoryLeaf("UI/UX"));
        design.AddChild(new ProjectCategoryLeaf("Branding"));

        var devops = new ProjectCategoryComposite("DevOps & Cloud");
        devops.AddChild(new ProjectCategoryLeaf("CI/CD"));
        devops.AddChild(new ProjectCategoryLeaf("Kubernetes"));
        devops.AddChild(new ProjectCategoryLeaf("Cloud Infrastructure"));

        return new List<IProjectCategoryNode> { web, mobile, design, devops };
    }

    private static ProjectCategoryNodeDto ToDto(IProjectCategoryNode node)
    {
        return new ProjectCategoryNodeDto(
            node.Name,
            node.IsLeaf,
            node.Children.Select(ToDto).ToList());
    }
}
