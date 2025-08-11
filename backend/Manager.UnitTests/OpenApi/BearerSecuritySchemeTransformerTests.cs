//using FluentAssertions;
//using Manager.UnitTests.TestHelpers;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.OpenApi;
//using Microsoft.OpenApi.Models;
//using Moq;

//public class BearerSecuritySchemeTransformerTests
//{
//    private IAuthenticationSchemeProvider p;

//    public BearerSecuritySchemeTransformerTests(IAuthenticationSchemeProvider p)
//    {
//        this.p = p;
//    }

//    private sealed class TestTransformer : IOpenApiDocumentTransformer
//    {
//        private readonly BearerSecuritySchemeTransformerTests _inner;
//        public TestTransformer(IAuthenticationSchemeProvider p) =>
//            _inner = new(p);

//        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
//            => _inner.TransformAsync(document, context, cancellationToken);
//    }

//    [Fact]
//    public async Task Adds_Bearer_Security_Scheme_When_Present()
//    {
//        var schemes = new[] { new AuthenticationScheme("Bearer", "Bearer", typeof(object)) };
//        var provider = new Mock<IAuthenticationSchemeProvider>();
//        provider.Setup(p => p.GetAllSchemesAsync()).ReturnsAsync(schemes);

//        var transformer = new TestTransformer(provider.Object);
//        var doc = new OpenApiDocument { Components = new OpenApiComponents() };
//        var ctx = new OpenApiDocumentTransformerContext();

//        await transformer.TransformAsync(doc, ctx, CancellationToken.None);

//        doc.Components.SecuritySchemes.Should().ContainKey("Bearer");
//        doc.SecurityRequirements.Should().NotBeEmpty();
//    }
//}
