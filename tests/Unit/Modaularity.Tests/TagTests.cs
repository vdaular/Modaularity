using Modaularity.Abstractions;
using Modaularity.Catalogs.Assemblies;
using Modaularity.Catalogs.Composites;
using Modaularity.Catalogs.Delegates;
using Modaularity.Catalogs.Folders;
using Modaularity.Catalogs.Types;
using Modaularity.Tests.Modules;
using Modaularity.TypeFinding;
using System.ComponentModel.DataAnnotations;

namespace Modaularity.Tests;

public class TagTests
{
    private char separator = Path.DirectorySeparatorChar;

    [Fact]
    public async Task CanTagTypeModule()
    {
        var catalog = new TypeModuleCatalog(typeof(TypeModule),
            new TypeModuleCatalogOptions()
            {
                TypeFinderOptions = new()
                {
                    TypeFinderCriterias = new()
                    {
                        TypeFinderCriteriaBuilder.Create().Tag("MyTag_1"),
                        TypeFinderCriteriaBuilder.Create().Tag("AnotherTag")
                    }
                }
            });

        await catalog.Initialize();

        var module = catalog.Single();

        Assert.Equal("MyTag_1", module.Tag);
    }

    [Fact]
    public async Task CanTagAssemblyModule()
    {
        var catalog = new AssemblyModuleCatalog($"..{separator}..{separator}..{separator}..{separator}..{separator}Assemblies{separator}output{separator}net7.0{separator}TestAssembly1.dll",
            null, taggedFilters: new Dictionary<string, Predicate<Type>>() { { "CustomTag", Type => true } });

        await catalog.Initialize();

        var modules = catalog.GetModules();

        foreach (var module in modules)
            Assert.Equal("CustomTag", module.Tag);
    }

    [Fact]
    public async Task CanTagAssemblyModuleUsingBuilder()
    {
        var catalog = new AssemblyModuleCatalog(typeof(TypeModule).Assembly, builder =>
        {
            builder.AssignableTo(typeof(TypeModule)).Tag("operator");
        });

        await catalog.Initialize();

        var allModules = catalog.GetModules();

        foreach (var module in allModules)
            Assert.Equal("operator", module.Tag);
    }

    [Fact]
    public async Task CanTagFolderModule()
    {
        var _moduleFolder =
            $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Assemblies{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}net7.0";
        var catalog = new FolderModuleCatalog(_moduleFolder, builder =>
        {
            builder.Tag("test_folder_tag");
        });

        await catalog.Initialize();

        var allModules = catalog.GetModules();

        foreach (var module in allModules)
            Assert.Equal("test_folder_tag", module.Tag);
    }

    [Fact]
    public async Task CanTagDelegate()
    {
        var catalog = new DelegateModuleCatalog(new Func<int, bool>(i => true), options: new()
        {
            Tags = new() { "CustomTagDelegate" }
        });

        await catalog.Initialize();

        var allModules = catalog.GetModules();

        foreach (var module in allModules)
            Assert.Equal("CustomTagDelegate", module.Tag);
    }

    [Fact]
    public async Task ModuleCanContainManyTags()
    {
        var catalog = new TypeModuleCatalog(typeof(TypeModule), new TypeModuleCatalogOptions()
        {
            TypeFinderOptions = new()
            {
                TypeFinderCriterias = new()
                {
                    TypeFinderCriteriaBuilder.Create().Tag("MyTag_1"),
                    TypeFinderCriteriaBuilder.Create().Tag("AnotherTag")
                }
            }
        });

        await catalog.Initialize();

        var module = catalog.Single();
        var coll = new List<string>() { "MyTag_1", "AnotherTag" };

        Assert.Equal(coll, module.Tags);
    }

    [Collection(nameof(NotThreadSafeResourceCollection))]
    public class DefaultOptions : IDisposable
    {
        private char separator = Path.DirectorySeparatorChar;

        public DefaultOptions()
        {
            TypeFinderOptions.Defaults.TypeFinderCriterias.Add(TypeFinderCriteriaBuilder.Create().Tag("CustomTag"));
            TypeFinderOptions.Defaults.TypeFinderCriterias.Add(TypeFinderCriteriaBuilder.Create().HasName(nameof(TypeModule)).Tag("MyTag_1"));
            TypeFinderOptions.Defaults.TypeFinderCriterias.Add(TypeFinderCriteriaBuilder.Create().HasName("*Json*").Tag("MyTag_1"));
        }

        [Fact]
        public async Task CanTagUsingDefaultOptions()
        {
            var assemblyModuleCatalog = new AssemblyModuleCatalog($"..{separator}..{separator}..{separator}..{separator}..{separator}Assemblies{separator}output{separator}net7.0{separator}TestAssembly1.dll");
            var typeModuleCatalog = new TypeModuleCatalog(typeof(TypeModule));
            var compositeCatalog = new CompositeModuleCatalog(assemblyModuleCatalog, typeModuleCatalog);

            await compositeCatalog.Initialize();

            var customTaggedModules = compositeCatalog.GetByTag("CustomTag");

            Assert.Equal(2, customTaggedModules.Count);

            var myTaggedModules = compositeCatalog.GetByTag("MyTag_1");

            Assert.Single(myTaggedModules);

            TypeFinderOptions.Defaults.TypeFinderCriterias.Clear();
        }

        [Fact]
        public async Task TypeCatalogCanTagUsingDefaultOptions()
        {
            var typeModuleCatalog = new TypeModuleCatalog(typeof(TypeModule));

            await typeModuleCatalog.Initialize();

            var myTaggedModules = typeModuleCatalog.GetByTag("MyTag_1");

            Assert.Single(myTaggedModules);

            TypeFinderOptions.Defaults.TypeFinderCriterias.Clear();
        }

        [Fact]
        public async Task DefaultTagsWithFolderCatalogTypeShouldNotDuplicateModules()
        {
            var catalog = new FolderModuleCatalog($"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Assemblies{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}JsonNew{Path.DirectorySeparatorChar}net7.0");

            await catalog.Initialize();

            Assert.Single(catalog.GetModules());

            var module = catalog.Get();

            Assert.Equal(2, module.Tags.Count);

            TypeFinderOptions.Defaults.TypeFinderCriterias.Clear();
        }

        [Fact]
        public async Task DefaultTagsWithAssemblyCatalogTypeShouldNotDuplicateModules()
        {
            var catalog = new AssemblyModuleCatalog($"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Assemblies{Path.DirectorySeparatorChar}output{Path.DirectorySeparatorChar}JsonNew{Path.DirectorySeparatorChar}net7.0{Path.DirectorySeparatorChar}JsonNet2.dll");

            await catalog.Initialize();

            Assert.Single(catalog.GetModules());

            var module = catalog.Get();

            Assert.Equal(2, module.Tags.Count);

            TypeFinderOptions.Defaults.TypeFinderCriterias.Clear();
        }

        public void Dispose()
        {
            TypeFinderOptions.Defaults.TypeFinderCriterias.Clear();
        }
    }
}
