using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace RefScout.Analyzer.Config.Framework;

public class FrameworkConfigErrorReport : IConfigErrorReport
{
    private readonly List<FrameworkConfigError> _errors = new();

    public bool HasErrors => _errors.Count > 0;
    public IReadOnlyList<ConfigError> Errors => _errors;

    internal void ReportIfAttributeMissing(XElement element, string attribute)
    {
        if (element.Attribute(attribute) == null)
        {
            Report(element, $"Required attribute '{attribute}' is missing.");
        }
    }

    internal void ReportIfAllAttributesMissing(XElement element, string[] attributes)
    {
        var allAttributesMissing = attributes.All(a => element.Attribute(a) == null);
        if (allAttributesMissing)
        {
            Report(element, $"Specify one of following attributes: '{string.Join(", ", attributes)}'.");
        }
    }

    internal void Report(XElement element, string message)
    {
        // High-tech namespace removal
        var elementString = element.ToString().Replace(" xmlns=\"urn:schemas-microsoft-com:asm.v1\"", "");
        var lineNumber = ((IXmlLineInfo)element).HasLineInfo() ? ((IXmlLineInfo)element).LineNumber : -1;
        _errors.Add(new FrameworkConfigError(elementString, message, lineNumber));
    }


    public override string ToString()
    {
        var groups = _errors.OrderBy(e => e.LineNumber).GroupBy(x => new { x.LineNumber, x.Element },
            (key, errors) =>
            {
                errors = errors.ToList();
                return (key.Element, Errors: errors, errors.First().LineNumber);
            });

        var sb = new StringBuilder();
        foreach (var (element, errors, lineNumber) in groups)
        {
            sb.AppendLine($"Line {lineNumber}: {element}");
            foreach (var error in errors)
            {
                sb.AppendLine($" - {error.Message}");
            }
        }

        return sb.ToString();
    }
}