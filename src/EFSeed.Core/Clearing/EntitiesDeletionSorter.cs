using Microsoft.EntityFrameworkCore.Metadata;

namespace EFSeed.Core.Clearing;

public class EntitiesDeletionSorter
{
    private class EntityTypeNode
    {
        public IEntityType EntityType { get; init; }
        public List<IEntityType> Dependencies { get; init; }
    }

    // TODO: Test this
    public List<IEntityType> SortForDeletion(List<IEntityType> entities)
    {
        var graph = GetGraph(entities);
        var sorted = new List<IEntityType>();
        var visited = new HashSet<IEntityType>();
        foreach (var node in graph)
        {
            var removalStack = new Stack<EntityTypeNode>();
            var processingStack = new Stack<EntityTypeNode>();
            var visitedFromCurrentNode = new HashSet<IEntityType>();
            processingStack.Push(node);
            while (processingStack.Count > 0)
            {
                var current = processingStack.Pop();
                if (visited.Contains(current.EntityType))
                {
                    continue;
                }
                if (visitedFromCurrentNode.Contains(current.EntityType))
                {
                    var prevNode = removalStack.Peek();
                    // self loop
                    if(prevNode.EntityType == current.EntityType)
                    {
                        continue;
                    }
                    // TODO: Handle loops
                    throw new InvalidOperationException("Loop detected");
                }
                removalStack.Push(current);
                visited.Add(current.EntityType);
                visitedFromCurrentNode.Add(current.EntityType);
                foreach (var dependency in current.Dependencies)
                {
                    var dependencyNode = graph.Find(x => x.EntityType == dependency)!;
                    processingStack.Push(dependencyNode);
                }
            }
            sorted.AddRange(removalStack.Select(x => x.EntityType));
        }
        sorted.Reverse();
        return sorted;
    }

    private List<EntityTypeNode> GetGraph(List<IEntityType> entities)
    {
        var graph = new List<EntityTypeNode>();

        foreach (var entity in entities)
        {
            var dependencies = entity.GetForeignKeys()
                .Select(fk => fk.PrincipalEntityType)
                .ToList();

            var node = new EntityTypeNode
            {
                EntityType = entity,
                Dependencies = dependencies
            };
            graph.Add(node);
        }

        return graph;
    }

}
