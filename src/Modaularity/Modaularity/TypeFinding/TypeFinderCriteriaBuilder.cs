namespace Modaularity.TypeFinding;

public class TypeFinderCriteriaBuilder
{
    private Type? _inherits;
    private Type? _implements;
    private Type? _assignableTo;
    private bool? _isAbstract = false;
    private bool? _isInterface = false;
    private string? _name;
    private Type? _hasAttribute;
    private List<string> _tags = new();

    public TypeFinderCriteria Build()
    {
        var res = new TypeFinderCriteria
        {
            IsInterface = _isInterface,
            IsAbstract = _isAbstract,
            Implements = _implements,
            Inherits = _inherits,
            AssignableTo = _assignableTo,
            Name = _name,
            HasAttribute = _hasAttribute,
            Tags = _tags
        };

        return res;
    }

    public static implicit operator TypeFinderCriteria(TypeFinderCriteriaBuilder criteriaBuilder)
        => criteriaBuilder.Build();

    public static TypeFinderCriteriaBuilder Create()
        => new TypeFinderCriteriaBuilder();

    public TypeFinderCriteriaBuilder HasName(string name)
    {
        _name = name;

        return this;
    }

    public TypeFinderCriteriaBuilder Implements(Type t)
    {
        _implements = t;

        return this;
    }

    public TypeFinderCriteriaBuilder Implements<T>()
        => Implements(typeof(T));

    public TypeFinderCriteriaBuilder Inherits(Type t)
    {
        _inherits = t;

        return this;
    }

    public TypeFinderCriteriaBuilder Inherits<T>()
        => Inherits(typeof(T));

    public TypeFinderCriteriaBuilder IsAbstract(bool? isAbstract)
    {
        _isAbstract = isAbstract;

        return this;
    }

    public TypeFinderCriteriaBuilder IsInterface(bool? isInterface)
    {
        _isInterface = isInterface;

        return this;
    }

    public TypeFinderCriteriaBuilder AssignableTo(Type assignableTo)
    {
        _assignableTo = assignableTo;

        return this;
    }

    public TypeFinderCriteriaBuilder HasAttribute(Type attribute)
    {
        _hasAttribute = attribute;

        return this;
    }

    public TypeFinderCriteriaBuilder Tag(string tag)
    {
        if (_tags == null)
            _tags = new();

        if (_tags.Contains(tag))
            return this;

        _tags.Add(tag);

        return this;
    }

    public TypeFinderCriteriaBuilder Tag(params string[] tags)
    {
        if (_tags == null)
            _tags = new();

        foreach (var tag in tags)
        {
            if (_tags.Contains(tag))
                continue;

            _tags.Add(tag);
        }

        return this;
    }
}
