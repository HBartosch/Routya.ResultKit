using System.ComponentModel.DataAnnotations;
using Shouldly;

namespace Routya.ResultKit.Test.ValidationTests;

public class ValidationExtensionsV2Tests
{
    [Fact]
    public void Validate_WithValidObject_ShouldReturnSuccessfulResult()
    {
        var model = new TestModel
        {
            Name = "John Doe",
            Email = "john@example.com",
            Age = 30
        };

        var result = model.Validate();

        result.Success.ShouldBeTrue();
        result.Data.ShouldBe(model);
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void Validate_WithInvalidObject_ShouldReturnFailedResult()
    {
        var model = new TestModel
        {
            Name = "",
            Email = "invalid-email",
            Age = 5
        };

        var result = model.Validate();

        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
    }

    [Fact]
    public void Validate_WithValidationErrors_ShouldUseValidationFailedFactory()
    {
        var model = new TestModel
        {
            Name = "",
            Email = "invalid"
        };

        var result = model.Validate();

        result.Success.ShouldBeFalse();
        result.Error!.Title.ShouldBe("Validation Failed");
        result.Error.Status.ShouldBe(400);
    }

    [Fact]
    public void Validate_WithValidationErrors_ShouldStoreErrorsAsExtension()
    {
        var model = new TestModel
        {
            Name = "",
            Email = "invalid-email",
            Age = 5
        };

        var result = model.Validate();

        result.Success.ShouldBeFalse();
        result.Error!.TryGetExtension<Dictionary<string, string[]>>("errors", out var errors).ShouldBeTrue();
        errors.ShouldNotBeNull();
        errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Validate_WithValidationErrors_ShouldGroupErrorsByField()
    {
        var model = new TestModel
        {
            Name = "",
            Email = "invalid-email"
        };

        var result = model.Validate();

        result.Error!.TryGetExtension<Dictionary<string, string[]>>("errors", out var errors).ShouldBeTrue();
        errors.Keys.ShouldContain("Name");
        errors.Keys.ShouldContain("Email");
    }

    [Fact]
    public void Validate_WithMultipleErrorsOnSameField_ShouldCollectAll()
    {
        var model = new MultiErrorModel
        {
            Value = ""  // Both Required and StringLength will fail
        };

        var result = model.Validate();

        result.Error!.TryGetExtension<Dictionary<string, string[]>>("errors", out var errors).ShouldBeTrue();
        errors["Value"].Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Validate_WithNestedObject_ShouldValidateNested()
    {
        var model = new ParentModel
        {
            Name = "Parent",
            Child = new ChildModel
            {
                ChildName = ""  // Invalid
            }
        };

        var result = model.Validate();

        result.Success.ShouldBeFalse();
        result.Error!.TryGetExtension<Dictionary<string, string[]>>("errors", out var errors).ShouldBeTrue();
        errors.Keys.ShouldContain(k => k.Contains("Child"));
    }

    [Fact]
    public void Validate_CanBeChainedWithTransform()
    {
        var model = new TestModel
        {
            Name = "John",
            Email = "john@example.com",
            Age = 30
        };

        var result = model.Validate()
            .Transform(m => new { m.Name, m.Email });

        result.Success.ShouldBeTrue();
        result.Data!.Name.ShouldBe("John");
        result.Data.Email.ShouldBe("john@example.com");
    }

    [Fact]
    public void Validate_FailedValidation_TransformShouldNotExecute()
    {
        var model = new TestModel
        {
            Name = "",  // Invalid
            Email = "test@example.com"
        };
        var transformCalled = false;

        var result = model.Validate()
            .Transform(m =>
            {
                transformCalled = true;
                return new { m.Name };
            });

        result.Success.ShouldBeFalse();
        transformCalled.ShouldBeFalse();
    }

    [Fact]
    public void Validate_FailedValidation_ShouldPreserveErrorsInTransform()
    {
        var model = new TestModel
        {
            Name = "",
            Email = "invalid"
        };

        var result = model.Validate()
            .Transform(m => new { m.Name, m.Email });

        result.Success.ShouldBeFalse();
        result.Error!.TryGetExtension<Dictionary<string, string[]>>("errors", out var errors).ShouldBeTrue();
        errors.Keys.ShouldContain("Name");
        errors.Keys.ShouldContain("Email");
    }

    private class TestModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Range(18, 120)]
        public int Age { get; set; }
    }

    private class MultiErrorModel
    {
        [Required]
        [StringLength(10, MinimumLength = 3)]
        public string Value { get; set; } = string.Empty;
    }

    private class ParentModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public ChildModel? Child { get; set; }
    }

    private class ChildModel
    {
        [Required]
        public string ChildName { get; set; } = string.Empty;
    }
}
