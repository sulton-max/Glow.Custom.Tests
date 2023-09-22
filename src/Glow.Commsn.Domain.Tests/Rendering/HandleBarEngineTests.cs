using System.Text.Json;
using Glow.Comms.Domain.Infrastructure.Rendering;
using HandlebarsDotNet;
using HandlebarsDotNet.Extension.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Glow.Commsn.Domain.Tests.Rendering;

public partial class HandleBarEngineTests
{
    private readonly string _templateCacheKey;
    private readonly string _templateKey;

    private readonly CompiledTemplateCacheSettings _cacheSettings;

    private readonly IMemoryCache _memoryCacheMock;

    private readonly HandlebarTemplateEngine _sut;

    public HandleBarEngineTests()
    {
        // Create mocks
        _memoryCacheMock = Substitute.For<IMemoryCache>();

        // Create settings
        _cacheSettings = new CompiledTemplateCacheSettings
        {
            LifetimeInHours = 24,
            Name = "CompiledTemplate"
        };

        var cacheOptions = Options.Create(_cacheSettings);
        _templateKey = "templateKey";
        _templateCacheKey = $"{_templateKey}_{_cacheSettings.Name}";

        // Initialize sut
        _sut = new HandlebarTemplateEngine(_memoryCacheMock, cacheOptions);
    }

    private HandlebarsTemplate<object, object> GetCompiledTemplateForJson(string template)
    {
        var handlebars = Handlebars.Create();
        handlebars.Configuration.UseJson();
        var compiledTemplate = handlebars.Compile(template);
        return compiledTemplate;
    }

    private HandlebarsTemplate<object, object> GetCompiledTemplateForDictionary(string template)
    {
        var handlebars = Handlebars.Create();
        // handlebars.Configuration.UseJson();
        var compiledTemplate = handlebars.Compile(template);
        return compiledTemplate;
    }

    private TValue GetDefault<TValue>()
    {
        if (typeof(TValue) == typeof(string))
            return (TValue)(object)string.Empty;

        if (typeof(TValue) == typeof(object))
            return (TValue)new object();

        return default!;
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

    public static TheoryData<(JsonDocument Variables, string Template)> GetInvalidJsonTemplate()
    {
        var emptyVariables = JsonDocument.Parse("{}");
        var variables = JsonDocument.Parse("""
                                           {
                                             "title": "My Blog",
                                             "greeting": "Hello John!"
                                           }
                                           """);
        var template = "My Blog. Hello John!";
        var returnedTemplate = "My Blog. Hello John!";

        return new TheoryData<(JsonDocument Variables, string Template)>
        {
            (variables, template),
            (emptyVariables, template)
        };
    }

    public static TheoryData<(Dictionary<string, object> Variables, string Template, string RenderedTemplate)> GetValidDictionaryTemplate()
    {
        var variables = new Dictionary<string, object>
        {
            { "title", "My Blog" },
            { "greeting", "Hello John!" }
        };
        var template = "{{title}}. {{greeting}}";
        var renderedTemplate = "My Blog. Hello John!";

        return new TheoryData<(Dictionary<string, object> Variables, string Template, string RenderedTemplate)>
        {
            (variables, template, renderedTemplate)
        };
    }

    public static TheoryData<(Dictionary<string, object> Variables, string Template, string RenderedTemplate)> GetInvalidDictionaryTemplate()
    {
        var emptyVariables = new Dictionary<string, object>();
        var variables = new Dictionary<string, object>
        {
            { "title", "My Blog" },
            { "greeting", "Hello John!" }
        };
        var template = "My Blog. Hello John!";
        var renderedTemplate = "My Blog. Hello John!";

        return new TheoryData<(Dictionary<string, object> Variables, string Template, string RenderedTemplate)>
        {
            (variables, template, renderedTemplate),
            (emptyVariables, template, renderedTemplate)
        };
    }
}