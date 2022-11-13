using System.Reflection;
using System.Text.RegularExpressions;

namespace Modaularity.TypeFinding;

public class TypeFinder
{
    private static Regex NameToRegex(string nameFilter)
    {
        var regex = $"^{Regex.Escape(nameFilter).Replace("\\?", ".").Replace("\\*", ".*")}$";

        return new Regex(regex, RegexOptions.Compiled);
    }

    public bool IsMatch(TypeFinderCriteria criteria, Type type, ITypeFindingContext typeFindingContext)
    {
        if (criteria.Query != null)
        {
            var isMatch = criteria.Query(typeFindingContext, type);

            return isMatch;
        }

        if (criteria.IsAbstract != null)
        {
            if (type.IsAbstract != criteria.IsAbstract.GetValueOrDefault())
                return false;
        }

        if (criteria.IsInterface != null)
        {
            if (type.IsInterface != criteria.IsInterface.GetValueOrDefault())
                return false;
        }

        if (!string.IsNullOrWhiteSpace(criteria.Name))
        {
            var regEx = NameToRegex(criteria.Name);

            if (!regEx.IsMatch(type.FullName))
            {
                var hasDirectNamingMatch = string.Equals(criteria.Name, type.Name, StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(criteria.Name, type.FullName, StringComparison.InvariantCultureIgnoreCase);

                if (!hasDirectNamingMatch)
                    return false;
            }
        }

        if (criteria.Inherits != null)
        {
            var inheritedType = typeFindingContext.FindType(criteria.Inherits);

            if (!inheritedType.IsAssignableFrom(type))
                return false;
        }

        if (criteria.Implements != null)
        {
            var interfaceType = typeFindingContext.FindType(criteria.Implements);

            if (!interfaceType.IsAssignableFrom(type))
                return false;
        }

        if (criteria.AssignableTo != null)
        {
            var assignableToType = typeFindingContext.FindType(criteria.AssignableTo);

            if (!assignableToType.IsAssignableFrom(type))
                return false;
        }

        if (criteria.HasAttribute != null)
        {
            var attributes = type.GetCustomAttributesData();
            var attributeFound = false;

            foreach ( var attribute in attributes)
            {
                if (!string.Equals(attribute.AttributeType.FullName, criteria.HasAttribute.FullName, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                attributeFound = true;

                break;
            }

            if (!attributeFound)
                return false;
        }

        return true;
    }

    public List<Type> Find(TypeFinderCriteria? criteria, Assembly assembly, ITypeFindingContext typeFindingContext)
    {
        if (criteria == null)
            throw new ArgumentNullException(nameof(criteria));

        var result = new List<Type>();

        var types = assembly.GetExportedTypes();

        foreach ( var type in types)
        {
            var isMatch = IsMatch(criteria, type, typeFindingContext);

            if (!isMatch) 
                continue;

            result.Add(type);
        }

        return result;
    }
}
