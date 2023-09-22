using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;

namespace Glow.Commsn.Domain.Tests.Rendering;

public partial class HandleBarEngineTests
{
    [Theory]
    [MemberData(nameof(GetValidJsonTemplate))]
    public void Render_ReturnsFromCache_IfExists((JsonDocument Variables, string Template, string RenderedTemplate) templateValues)
    {
        // Arrange
        var compiledTemplate = GetCompiledTemplateForJson(templateValues.Template);
        _memoryCacheMock
            .TryGetValue(_templateCacheKey, out Arg.Any<object?>())
            .Returns(callInfo =>
            {
                callInfo[1] = compiledTemplate;
                return true;
            });

        // Act
        var actual = _sut.Render(_templateKey, templateValues.Variables, templateValues.Template, true);

        // Assert
        actual.Should().Be(templateValues.RenderedTemplate);

        _memoryCacheMock.Received(1).TryGetValue(_templateCacheKey, out Arg.Any<object>()!);
        _memoryCacheMock.DidNotReceiveWithAnyArgs().Set(GetDefault<string>(), GetDefault<object>());
    }

    [Theory]
    [MemberData(nameof(GetValidJsonTemplate))]
    public void Render_RendersAndSetsToCache_IfDoesNotExists((JsonDocument Variables, string Template, string RenderedTemplate) templateValues)
    {
        // Arrange
        var compiledTemplate = GetCompiledTemplateForJson(templateValues.Template);
        _memoryCacheMock.TryGetValue(_templateCacheKey, out var value).Returns(false);

        // Act
        var actual = _sut.Render(_templateKey, templateValues.Variables, templateValues.Template, true);

        // Assert
        actual.Should().Be(templateValues.RenderedTemplate);

        _memoryCacheMock.Received(1).TryGetValue(_templateCacheKey, out value);
        _memoryCacheMock.Received(1).Set(_templateCacheKey, compiledTemplate, TimeSpan.FromHours(_cacheSettings.LifetimeInHours));
    }

    [Theory]
    [MemberData(nameof(GetInvalidJsonTemplate))]
    public void Render_ReturnsTemplate_IfTemplateIsInvalid((JsonDocument Variables, string Template) templateValues)
    {
        // Arrange
        _memoryCacheMock.TryGetValue(_templateCacheKey, out var value).Returns(false);

        // Act
        var actual = _sut.Render(_templateKey, templateValues.Variables, templateValues.Template, true);

        // Assert
        actual.Should().Be(templateValues.Template);

        _memoryCacheMock.Received(1).TryGetValue(_templateCacheKey, out value);
        _memoryCacheMock.DidNotReceiveWithAnyArgs().Set(GetDefault<string>(), GetDefault<object>());
    }
}