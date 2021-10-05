using System.Collections.Generic;
using OpenSoftware.DgmlTools.Model;

namespace RefScout.Visualizers.Dgml;

internal static class DgmlStyles
{
    public static IEnumerable<Style> ConflictStyles => new List<Style>
    {
        new()
        {
            TargetType = "Node", GroupLabel = "Assembly", ValueLabel = "Has category", Condition =
                new List<Condition>
                {
                    new() { Expression = "HasCategory('Assembly_Default')" }
                },
            Setter = new List<Setter>
            {
                new() { Property = "Background", Value = "#FF094167" },
                new() { Property = "Stroke", Value = "#FF094167" },
                new() { Property = "Icon", Value = "CodeSchema_Assembly" }
            }
        },
        new()
        {
            TargetType = "Node", GroupLabel = "Assembly from GAC", ValueLabel = "Has category", Condition =
                new List<Condition>
                {
                    new() { Expression = "HasCategory('Assembly_GAC')" }
                },
            Setter = new List<Setter>
            {
                new() { Property = "Background", Value = "#FF094167" },
                new() { Property = "Stroke", Value = "#c95dc4" },
                new() { Property = "StrokeThickness", Value = "3" },
                new() { Property = "Icon", Value = "CodeSchema_Assembly" }
            }
        },
        new()
        {
            TargetType = "Node", GroupLabel = "Fatal conflict", ValueLabel = "Has category", Condition =
                new List<Condition>
                {
                    new() { Expression = "HasCategory('Assembly_Error')" }
                },
            Setter = new List<Setter>
            {
                new() { Property = "Background", Value = "#a23326" },
                new() { Property = "Stroke", Value = "#a23326" },
                new() { Property = "Icon", Value = "CodeSchema_Assembly" }
            }
        },
        new()
        {
            TargetType = "Node", GroupLabel = "Warning conflict", ValueLabel = "Has category", Condition =
                new List<Condition>
                {
                    new() { Expression = "HasCategory('Assembly_Warning')" }
                },
            Setter = new List<Setter>
            {
                new() { Property = "Background", Value = "#ff9326" },
                new() { Property = "Stroke", Value = "#ff9326" },
                new() { Property = "Icon", Value = "CodeSchema_Assembly" }
            }
        },
        new()
        {
            TargetType = "Node", GroupLabel = "Info conflict", ValueLabel = "Has category", Condition =
                new List<Condition>
                {
                    new() { Expression = "HasCategory('Assembly_Info')" }
                },
            Setter = new List<Setter>
            {
                new() { Property = "Background", Value = "#ff9326" },
                new() { Property = "Stroke", Value = "#ff9326" },
                new() { Property = "Icon", Value = "CodeSchema_Assembly" }
            }
        },
        new()
        {
            TargetType = "Node", GroupLabel = "Success conflict", ValueLabel = "Has category",
            Condition =
                new List<Condition>
                {
                    new() { Expression = "HasCategory('Assembly_Success')" }
                },
            Setter = new List<Setter>
            {
                new() { Property = "Background", Value = "#78de83" },
                new() { Property = "Stroke", Value = "#78de83" },
                new() { Property = "Icon", Value = "CodeSchema_Assembly" }
            }
        },
        new()
        {
            TargetType = "Node", GroupLabel = "Target Framework", ValueLabel = "Has category", Condition =
                new List<Condition>
                {
                    new() { Expression = "HasCategory('TargetFramework')" }
                },
            Setter = new List<Setter>
            {
                new() { Property = "Background", Value = "#FF652A77" },
                new() { Property = "Stroke", Value = "#FF652A77" }
            }
        },
        new()
        {
            TargetType = "Node", GroupLabel = "Conflict notes", ValueLabel = "Has category", Condition =
                new List<Condition>
                {
                    new() { Expression = "HasCategory('Note')" }
                },
            Setter = new List<Setter>
            {
                new() { Property = "Background", Value = "White" },
                new() { Property = "MaxWidth", Value = "300" },
                new() { Property = "NodeRadius", Value = "15" },
                new() { Property = "Foreground", Value = "Gray" }
            }
        },

        //
        // Links
        //
        new()
        {
            TargetType = "Link", GroupLabel = "Fatal reference", ValueLabel = "Has category", Condition =
                new List<Condition>
                {
                    new() { Expression = "HasCategory('Assembly_Fatal')" }
                },
            Setter = new List<Setter>
            {
                new() { Property = "Stroke", Value = "#a23326" }
            }
        },
        new()
        {
            TargetType = "Link", GroupLabel = "Warning reference", ValueLabel = "Has category", Condition =
                new List<Condition>
                {
                    new() { Expression = "HasCategory('Assembly_Warning')" }
                },
            Setter = new List<Setter>
            {
                new() { Property = "Stroke", Value = "#ff9326" }
            }
        },
        new()
        {
            TargetType = "Link", GroupLabel = "Info reference", ValueLabel = "Has category", Condition =
                new List<Condition>
                {
                    new() { Expression = "HasCategory('Assembly_Info')" }
                },
            Setter = new List<Setter>
            {
                new() { Property = "Stroke", Value = "#ff9326" }
            }
        },
        new()
        {
            TargetType = "Link", GroupLabel = "Success reference", ValueLabel = "Has category",
            Condition =
                new List<Condition>
                {
                    new() { Expression = "HasCategory('Assembly_Success')" }
                },
            Setter = new List<Setter>
            {
                new() { Property = "Stroke", Value = "#78de83" }
            }
        },
        new()
        {
            TargetType = "Link", GroupLabel = "Link to note", ValueLabel = "Has category", Condition =
                new List<Condition>
                {
                    new() { Expression = "HasCategory('Note')" }
                },
            Setter = new List<Setter>
            {
                new() { Property = "StrokeDashArray", Value = "2 2" }
            }
        }
    };
}