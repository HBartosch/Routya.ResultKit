using Routya.ResultKit.ProblemTypes;

namespace Routya.ResultKit.Test.ProblemTypesTests;

public class StandardProblemTypesTests
{
    [Fact]
    public void BadRequest_ShouldReturnRfc9110Uri()
    {
        var uri = StandardProblemTypes.BadRequest;

        Assert.StartsWith("https://datatracker.ietf.org/doc/html/rfc9110", uri);
        Assert.Contains("15.5.1", uri);
    }

    [Fact]
    public void Unauthorized_ShouldReturnRfc9110Uri()
    {
        var uri = StandardProblemTypes.Unauthorized;

        Assert.StartsWith("https://datatracker.ietf.org/doc/html/rfc9110", uri);
        Assert.Contains("15.5.2", uri);
    }

    [Fact]
    public void Forbidden_ShouldReturnRfc9110Uri()
    {
        var uri = StandardProblemTypes.Forbidden;

        Assert.StartsWith("https://datatracker.ietf.org/doc/html/rfc9110", uri);
        Assert.Contains("15.5.4", uri);
    }

    [Fact]
    public void NotFound_ShouldReturnRfc9110Uri()
    {
        var uri = StandardProblemTypes.NotFound;

        Assert.StartsWith("https://datatracker.ietf.org/doc/html/rfc9110", uri);
        Assert.Contains("15.5.5", uri);
    }

    [Fact]
    public void Conflict_ShouldReturnRfc9110Uri()
    {
        var uri = StandardProblemTypes.Conflict;

        Assert.StartsWith("https://datatracker.ietf.org/doc/html/rfc9110", uri);
        Assert.Contains("15.5.10", uri);
    }

    [Fact]
    public void InternalServerError_ShouldReturnRfc9110Uri()
    {
        var uri = StandardProblemTypes.InternalServerError;

        Assert.StartsWith("https://datatracker.ietf.org/doc/html/rfc9110", uri);
        Assert.Contains("15.6.1", uri);
    }

    [Fact]
    public void ValidationError_ShouldUseDefaultBaseUri()
    {
        var originalBaseUri = StandardProblemTypes.DefaultBaseUri;
        try
        {
            StandardProblemTypes.DefaultBaseUri = "urn:problem-type:";
            var uri = StandardProblemTypes.ValidationError;

            Assert.StartsWith("urn:problem-type:", uri);
            Assert.Contains("validation-error", uri);
        }
        finally
        {
            StandardProblemTypes.DefaultBaseUri = originalBaseUri;
        }
    }

    [Fact]
    public void BusinessRuleViolation_ShouldUseDefaultBaseUri()
    {
        var originalBaseUri = StandardProblemTypes.DefaultBaseUri;
        try
        {
            StandardProblemTypes.DefaultBaseUri = "urn:problem-type:";
            var uri = StandardProblemTypes.BusinessRuleViolation;

            Assert.StartsWith("urn:problem-type:", uri);
            Assert.Contains("business-rule-violation", uri);
        }
        finally
        {
            StandardProblemTypes.DefaultBaseUri = originalBaseUri;
        }
    }

    [Fact]
    public void DefaultBaseUri_CanBeChanged()
    {
        var originalBaseUri = StandardProblemTypes.DefaultBaseUri;
        try
        {
            StandardProblemTypes.DefaultBaseUri = "https://api.example.com/problems/";
            var uri = StandardProblemTypes.ValidationError;

            Assert.StartsWith("https://api.example.com/problems/", uri);
        }
        finally
        {
            StandardProblemTypes.DefaultBaseUri = originalBaseUri;
        }
    }

    [Fact]
    public void Custom_ShouldCombineBaseUriWithTypeName()
    {
        var originalBaseUri = StandardProblemTypes.DefaultBaseUri;
        try
        {
            StandardProblemTypes.DefaultBaseUri = "urn:problem-type:";
            var uri = StandardProblemTypes.Custom("user-quota-exceeded");

            Assert.Equal("urn:problem-type:user-quota-exceeded", uri);
        }
        finally
        {
            StandardProblemTypes.DefaultBaseUri = originalBaseUri;
        }
    }

    [Fact]
    public void ResourceAlreadyExists_ShouldReturnValidUri()
    {
        var uri = StandardProblemTypes.ResourceAlreadyExists;

        Assert.NotNull(uri);
        Assert.Contains("resource-already-exists", uri);
    }

    [Fact]
    public void OperationNotPermitted_ShouldReturnValidUri()
    {
        var uri = StandardProblemTypes.OperationNotPermitted;

        Assert.NotNull(uri);
        Assert.Contains("operation-not-permitted", uri);
    }

    [Fact]
    public void HttpErrorUris_ShouldBeDifferent()
    {
        var uris = new[]
        {
            StandardProblemTypes.BadRequest,
            StandardProblemTypes.Unauthorized,
            StandardProblemTypes.Forbidden,
            StandardProblemTypes.NotFound,
            StandardProblemTypes.Conflict,
            StandardProblemTypes.InternalServerError
        };

        var distinctUris = uris.Distinct().ToArray();
        Assert.Equal(uris.Length, distinctUris.Length);
    }

    [Fact]
    public void DomainSpecificUris_ShouldBeDifferent()
    {
        var originalBaseUri = StandardProblemTypes.DefaultBaseUri;
        try
        {
            StandardProblemTypes.DefaultBaseUri = "urn:problem-type:";
            var uris = new[]
            {
                StandardProblemTypes.ValidationError,
                StandardProblemTypes.BusinessRuleViolation,
                StandardProblemTypes.ResourceAlreadyExists,
                StandardProblemTypes.OperationNotPermitted
            };

            var distinctUris = uris.Distinct().ToArray();
            Assert.Equal(uris.Length, distinctUris.Length);
        }
        finally
        {
            StandardProblemTypes.DefaultBaseUri = originalBaseUri;
        }
    }
}
