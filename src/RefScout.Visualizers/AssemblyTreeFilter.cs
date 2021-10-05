using System;
using System.Collections.Generic;
using System.Linq;
using RefScout.Analyzer;
using RefScout.Analyzer.Helpers;
using RefScout.Analyzer.Notes;

namespace RefScout.Visualizers;

internal static class AssemblyTreeFilter
{
    public static IReadOnlyList<Assembly> OnlyApplicationAssemblies(IEnumerable<Assembly> assemblies)
    {
        return AssemblyHelper.Clone(assemblies,
            assembly => !assembly.IsSystem && !assembly.IsNetApi || assembly.IsLocalSystem ||
                        assembly.IsEntryPoint);
    }

    public static IReadOnlyList<Assembly> OnlyConflictAssemblies(
        IEnumerable<Assembly> assemblies,
        NoteLevel minLevel)
    {
        var resultAssemblies = new HashSet<Assembly>();
        var conflictAssemblies = new List<Assembly>();

        var assemblyList = AssemblyHelper.Clone(assemblies);
        foreach (var assembly in assemblyList.Where(assembly => assembly.Level >= minLevel))
        {
            if (assembly.IsArchitectureMismatch)
            {
                conflictAssemblies.Add(assembly);
            }

            if (assembly.IsArchitectureMismatch && (!assembly.IsArchitectureMismatch || assembly.Notes.Count < 2))
            {
                continue;
            }

            conflictAssemblies.AddRange(assembly.ReferencedBy.Select(r => r.From));
            resultAssemblies.Add(assembly);
        }

        var entryPoint = assemblyList.SingleOrDefault(a => a.IsEntryPoint);
        if (entryPoint == null)
        {
            return assemblyList;
        }

        resultAssemblies.Add(entryPoint);

        // Add assemblies in path of conflicting assemblies
        var shortestPath = new BfsShortestPath(entryPoint);
        shortestPath.Prepare();
        foreach (var result in conflictAssemblies
            .Where(assembly => !resultAssemblies.Contains(assembly))
            .SelectMany(assembly => shortestPath.GetPath(assembly)))
        {
            resultAssemblies.Add(result);
        }

        // Remove now unused references
        foreach (var assembly in resultAssemblies)
        {
            assembly.References.RemoveAll(r => resultAssemblies.All(a => a.FullName != r.To.FullName));
            assembly.ReferencedBy.RemoveAll(r => resultAssemblies.All(a => a.FullName != r.From.FullName));
        }

        return resultAssemblies.ToList();
    }

    private class BfsShortestPath
    {
        private readonly Assembly _start;
        private readonly Dictionary<Assembly, Assembly> _previous;

        public BfsShortestPath(Assembly start)
        {
            _start = start ?? throw new ArgumentNullException(nameof(start));
            _previous = new Dictionary<Assembly, Assembly>();
        }

        public void Prepare()
        {
            var queue = new Queue<Assembly>();
            queue.Enqueue(_start);

            while (queue.Count > 0)
            {
                var assembly = queue.Dequeue();
                foreach (var reference in assembly.References)
                {
                    var to = reference.To;
                    if (_previous.ContainsKey(to) || to.Name == "netstandard")
                    {
                        continue;
                    }

                    _previous[to] = assembly;
                    queue.Enqueue(to);
                }
            }
        }

        public IReadOnlyList<Assembly> GetPath(Assembly assembly)
        {
            var path = new List<Assembly>();
            var current = assembly;
            while (!current.Equals(_start))
            {
                path.Add(current);
                if (!_previous.ContainsKey(current))
                {
                    break;
                }

                current = _previous[current];
            }

            path.Add(_start);
            path.Reverse();
            return path;
        }
    }

    //private static Func<Assembly, IReadOnlyList<Assembly>> ShortestPathFunction(Assembly start)
    //{
    //    // Simple BFS for shortest paths to conflicting assemblies
    //    var previous = new Dictionary<Assembly, Assembly>();
    //    var queue = new Queue<Assembly>();
    //    queue.Enqueue(start);
    //    while (queue.Count > 0)
    //    {
    //        var assembly = queue.Dequeue();
    //        foreach (var reference in assembly.References)
    //        {
    //            var to = reference.To;
    //            if (previous.ContainsKey(to) || to.Name == "netstandard")
    //            {
    //                continue;
    //            }

    //            previous[to] = assembly;
    //            queue.Enqueue(to);
    //        }
    //    }

    //    return assembly =>
    //    {
    //        var path = new List<Assembly>();
    //        var current = assembly;
    //        while (!current.Equals(start))
    //        {
    //            path.Add(current);
    //            if (!previous.ContainsKey(current))
    //            {
    //                break;
    //            }

    //            current = previous[current];
    //        }

    //        path.Add(start);
    //        path.Reverse();
    //        return path;
    //    };
    //}
}