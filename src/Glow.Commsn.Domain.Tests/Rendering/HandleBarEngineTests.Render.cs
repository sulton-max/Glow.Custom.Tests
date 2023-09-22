using System.Text.Json;
using FluentAssertions;
using NSubstitute;

namespace Glow.Commsn.Domain.Tests.Rendering;

public partial class HandleBarEngineTests
{
    [Theory]
    [MemberData(nameof(GetValidJsonTemplate))]
    public void Render_ReturnsFromCache_IfExists((JsonDocument Variables, string Template, string RenderedTemplate) templateValues)
    {
        // Arrange
        var (variables, template, renderedTemplate) = templateValues;

        _memoryCacheMock.TryGetValue(_templateCacheKey, out var value).Returns(true);

        // Act
        var actual = _sut.Render(_templateKey, variables, template);

        // Assert
        actual.Should().Be(renderedTemplate);

        _memoryCacheMock.Received(1).TryGetValue(_templateCacheKey, out value);
    }

    [Theory]
    [MemberData(nameof(GetValidJsonTemplate))]
    public void Render_RendersAndSetsToCache_IfDoesNotExists((JsonDocument Variables, string Template, string RenderedTemplate) templateValues)
    {
        // Arrange
        var (variables, template, renderedTemplate) = templateValues;

        _memoryCacheMock.TryGetValue(_templateCacheKey, out var value).Returns(false);

        // Act
        var actual = _sut.Render(_templateKey, variables, template);

        // Assert
        actual.Should().BeEquivalentTo(renderedTemplate);

        _memoryCacheMock.Received(1).TryGetValue(_templateCacheKey, out value);
    }
}