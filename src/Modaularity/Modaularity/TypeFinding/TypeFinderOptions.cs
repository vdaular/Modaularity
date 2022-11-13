namespace Modaularity.TypeFinding;

public class TypeFinderOptions
{
    public List<TypeFinderCriteria> TypeFinderCriterias { get; set; } = new(Defaults.GetDefaultTypeFinderCriterias());

    public static class Defaults
    {
        public static List<TypeFinderCriteria> TypeFinderCriterias { get; set; } = new();

        public static IReadOnlyCollection<TypeFinderCriteria> GetDefaultTypeFinderCriterias() => TypeFinderCriterias.AsReadOnly();
    }
}
