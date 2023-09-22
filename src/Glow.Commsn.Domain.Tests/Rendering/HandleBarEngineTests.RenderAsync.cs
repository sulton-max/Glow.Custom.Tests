using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;

namespace Glow.Commsn.Domain.Tests.Rendering;

public partial class HandleBarEngineTests
{
    [Theory]
    [MemberData(nameof(GetValidJsonTemplate))]
    public async ValueTask RenderAsync_ReturnsFromCache_IfExists((JsonDocument Variables, string Template, string RenderedTemplate) templateValues)
    {
        // Arrange
        var compiledTemplate = GetCompiledTemplateForJson(templateValues.Template);
        _memoryCacheMock
            .TryGetValue(_templateCacheKey, out Arg.Any<object?>())
            .Returns(callInfo =>
            {
                callInfo[0] = compiledTemplate;
                return true;
            });

        var templateGetter = () => new ValueTask<string>(templateValues.Template);

        // Act
        var actual = await _sut.RenderAsync(_templateKey, templateValues.Variables, templateGetter, true);

        // Assert
        actual.Should().BeEquivalentTo(templateValues.RenderedTemplate);

        _memoryCacheMock.Received(1).TryGetValue(_templateCacheKey, out Arg.Any<object>()!);
        _memoryCacheMock.DidNotReceiveWithAnyArgs().Set(GetDefault<string>(), GetDefault<object>());
    }

    [Theory]
    [MemberData(nameof(GetValidJsonTemplate))]
    public async ValueTask RenderAsync_RendersAndSetsToCache_IfDoesNotExist(
        (JsonDocument Variables, string Template, string RenderedTemplate) templateValues
    )
    {
        // Arrange
        var compiledTemplate = GetCompiledTemplateForJson(templateValues.Template);
        _memoryCacheMock.TryGetValue(_templateCacheKey, out var value).Returns(false);
        var templateGetter = () => new ValueTask<string>(templateValues.Template);

        // Act
        var actual = await _sut.RenderAsync(_templateKey, templateValues.Variables, templateGetter, true);

        // Assert
        actual.Should().BeEquivalentTo(templateValues.RenderedTemplate);

        _memoryCacheMock.Received(1).TryGetValue(_templateCacheKey, out value);
        _memoryCacheMock.Received(1).Set(_templateCacheKey, compiledTemplate, TimeSpan.FromHours(_cacheSettings.LifetimeInHours));
    }

    [Theory]
    [MemberData(nameof(GetValidJsonTemplate))]
    public async ValueTask RenderAsync_ReturnsTemplate_IfTemplateIsInvalid(
        (JsonDocument Variables, string Template, string RenderedTemplate) templateValues
    )
    {
        // Arrange
        _memoryCacheMock.TryGetValue(_templateCacheKey, out var value).Returns(false);
        var templateGetter = () => new ValueTask<string>(templateValues.Template);

        // Act
        var actual = await _sut.RenderAsync(_templateKey, templateValues.Variables, templateGetter, true);

        // Assert
        actual.Should().BeEquivalentTo(templateValues.RenderedTemplate);
        _memoryCacheMock.Received(1).TryGetValue(_templateCacheKey, out value);
        _memoryCacheMock.DidNotReceiveWithAnyArgs().Set(GetDefault<string>(), GetDefault<object>());
    }

    [Theory]
    [MemberData(nameof(GetValidDictionaryTemplate))]
    public async ValueTask RenderAsyncDictionary_ReturnsFromCache_IfExists(
        (Dictionary<string, object> Variables, string Template, string RenderedTemplate) templateValues
    )
    {
        // Arrange
        var compiledTemplate = GetCompiledTemplateForDictionary(templateValues.Template);
        _memoryCacheMock
            .TryGetValue(_templateCacheKey, out Arg.Any<object?>())
            .Returns(callInfo =>
            {
                callInfo[1] = compiledTemplate;
                return true;
            });
        var templateGetter = () => new ValueTask<string>(templateValues.Template);

        // Act
        var actual = await _sut.RenderAsync(_templateKey, templateValues.Variables, templateGetter, true);

        // Assert
        actual.Should().BeEquivalentTo(templateValues.RenderedTemplate);

        _memoryCacheMock.Received(1).TryGetValue(_templateCacheKey, out Arg.Any<object>()!);
        _memoryCacheMock.DidNotReceiveWithAnyArgs().Set(GetDefault<string>(), GetDefault<object>());
    }

    [Theory]
    [MemberData(nameof(GetValidDictionaryTemplate))]
    public async ValueTask RenderAsyncDictionary_RendersAndSetsToCache_IfDoesNotExist(
        (Dictionary<string, object> Variables, string Template, string RenderedTemplate) templateValues
    )
    {
        // Arrange
        var compiledTemplate = GetCompiledTemplateForDictionary(templateValues.Template);
        _memoryCacheMock.TryGetValue(_templateCacheKey, out Arg.Any<object?>()).Returns(false);
        var templateGetter = () => new ValueTask<string>(templateValues.Template);

        // Act
        var actual = await _sut.RenderAsync(_templateKey, templateValues.Variables, templateGetter, true);

        // Assert
        actual.Should().BeEquivalentTo(templateValues.RenderedTemplate);
        _memoryCacheMock.Received(1).TryGetValue(_templateCacheKey, out Arg.Any<object>()!);
        _memoryCacheMock.Received(1).Set(_templateCacheKey, compiledTemplate, TimeSpan.FromHours(_cacheSettings.LifetimeInHours));
    }

    [Theory]
    [MemberData(nameof(GetValidDictionaryTemplate))]
    public async ValueTask RenderAsyncDictionary_ReturnsTemplate_IfTemplateIsInvalid(
        (Dictionary<string, object> Variables, string Template, string RenderedTemplate) templateValues
    )
    {
        // Arrange
        _memoryCacheMock.TryGetValue(_templateCacheKey, out var value).Returns(false);
        var templateGetter = () => new ValueTask<string>(templateValues.Template);

        // Act
        var actual = await _sut.RenderAsync(_templateKey, templateValues.Variables, templateGetter, true);

        // Assert
        actual.Should().BeEquivalentTo(templateValues.RenderedTemplate);
        _memoryCacheMock.Received(1).TryGetValue(_templateCacheKey, out value);
        _memoryCacheMock.DidNotReceiveWithAnyArgs().Set(GetDefault<string>(), GetDefault<object>());
    }
}