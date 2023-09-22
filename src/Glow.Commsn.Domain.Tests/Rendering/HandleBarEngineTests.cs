using System.Text.Json;
using Glow.Comms.Domain.Infrastructure.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Glow.Commsn.Domain.Tests.Rendering;

public partial class HandleBarEngineTests
{
    private readonly IMemoryCache _memoryCacheMock;
    private readonly HandlebarTemplateEngine _sut;
    private readonly string _templateCacheKey;
    private readonly string _templateKey;

    public HandleBarEngineTests()
    {
        // Create mocks
        _memoryCacheMock = Substitute.For<IMemoryCache>();

        // Create settings
        var cacheSettings = new CompiledTemplateCacheSettings
        {
            LifetimeInHours = 24,
            Name = "CompiledTemplate"
        };

        var cacheOptions = Options.Create(cacheSettings);
        _templateKey = "templateKey";
        _templateCacheKey = $"{_templateKey}_{cacheSettings.Name}";

        // Initialize sut
        _sut = new HandlebarTemplateEngine(_memoryCacheMock, cacheOptions);
    }

    public static TheoryData<(JsonDocument Variables, string Template, string RenderedTemplate)> GetValidJsonTemplate()
    {
        var variables = JsonDocument.Parse("""
                                           {
                                             "title": "My Blog",
                                             "greeting": "Hello John!"
                                           }
                                           """);
        var template = "{{title}}. {{greeting}}";
        var renderedTemplate = "My Blog. Hello John!";

        return new TheoryData<(JsonDocument Variables, string Template, string RenderedTemplate)>
        {
            (variables, template, renderedTemplate)
        };
    }

    public static TheoryData<(JsonDocument Variables, string Template, string RenderedTemplate)> GetInvalidJsonTemplate()
    {
        var emptyVariables = JsonDocument.Parse("{}");
        var variables = JsonDocument.Parse("""
                                           {
                                             "title": "My Blog",
                                             "greeting": "Hello John!"
                                           }
                                           """);
        var template = "My Blog. Hello John!";
        var renderedTemplate = "My Blog. Hello John!";

        return new TheoryData<(JsonDocument Variables, string Template, string RenderedTemplate)>
        {
            (variables, template, renderedTemplate),
            (emptyVariables, template, renderedTemplate)
        };
    }

    public static TheoryData<(Dictionary<string, string> Variables, string Template, string RenderedTemplate)> GetValidDictionaryTemplate()
    {
        var variables = new Dictionary<string, string>
        {
            {"title", "My Blog"},
            {"greeting", "Hello John!"}
        };
        var template = "{{title}}. {{greeting}}";
        var renderedTemplate = "My Blog. Hello John!";

        return new TheoryData<(Dictionary<string, string> Variables, string Template, string RenderedTemplate)>
        {
            (variables, template, renderedTemplate)
        };
    }

    public static TheoryData<(Dictionary<string, string> Variables, string Template, string RenderedTemplate)> GetInvalidDictionaryTemplate()
    {
        var emptyVariables = new Dictionary<string, string>();
        var variables = new Dictionary<string, string>
        {
            {"title", "My Blog"},
            {"greeting", "Hello John!"}
        };
        var template = "My Blog. Hello John!";
        var renderedTemplate = "My Blog. Hello John!";

        return new TheoryData<(Dictionary<string, string> Variables, string Template, string RenderedTemplate)>
        {
            (variables, template, renderedTemplate),
            (emptyVariables, template, renderedTemplate)
        };
    }
}