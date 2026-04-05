namespace FreelanceMarket.Domain.Patterns;

/// <summary>
/// Composite: единый контракт для узла дерева категорий.
/// </summary>
public interface IProjectCategoryNode
{
    string Name { get; }
    IReadOnlyCollection<IProjectCategoryNode> Children { get; }
    bool IsLeaf { get; }
    void AddChild(IProjectCategoryNode child);
}

/// <summary>
/// Composite: составной узел категории, содержащий подкатегории.
/// </summary>
public class ProjectCategoryComposite : IProjectCategoryNode
{
    private readonly List<IProjectCategoryNode> _children = new();

    public ProjectCategoryComposite(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public IReadOnlyCollection<IProjectCategoryNode> Children => _children;
    public bool IsLeaf => false;

    public void AddChild(IProjectCategoryNode child)
    {
        _children.Add(child);
    }
}

/// <summary>
/// Composite: лист дерева категорий (без дочерних узлов).
/// </summary>
public class ProjectCategoryLeaf : IProjectCategoryNode
{
    public ProjectCategoryLeaf(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public IReadOnlyCollection<IProjectCategoryNode> Children => Array.Empty<IProjectCategoryNode>();
    public bool IsLeaf => true;

    public void AddChild(IProjectCategoryNode child)
    {
        throw new InvalidOperationException("Leaf category cannot contain children");
    }
}
