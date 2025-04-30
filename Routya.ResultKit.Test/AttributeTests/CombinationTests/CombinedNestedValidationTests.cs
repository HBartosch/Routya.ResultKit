using Newtonsoft.Json;
using Routya.ResultKit.Test.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Routya.ResultKit.Test.AttributeTests.CombinationTests;
public class CombinedNestedValidationTests
{
    [Fact]
    public void CombinedNestedValidation_DeeplyNestedModel_ShouldPass()
    {
        var model = new DeepModel
        {
            Level1 = new Level1Model
            {
                Description = "Top-level description",
                Level2 = new Level2Model
                {
                    Title = "Nested title",
                    Level3 = new Level3Model
                    {
                        Code = "ABC123"
                    }
                }
            }
        };

        var result = model.Validate();

        Assert.True(result.Success);
        Assert.Empty(ValidationTestHelper.GetErrors(result));
    }


    [Fact]
    public void CombinedNestedValidation_DeeplyNestedModel_ShouldCatchAllNestedErrors()
    {
        var model = new DeepModel
        {
            Level1 = new Level1Model
            {
                Description = null,
                Level2 = new Level2Model
                {
                    Title = "",
                    Level3 = new Level3Model
                    {
                        Code = null
                    }
                }
            }
        };

        var result = model.Validate();
        Assert.False(result.Success);
        var errors = ValidationTestHelper.GetErrors(result);

        Assert.Contains("Level1.Description", errors.Keys);
        Assert.Contains("Level1.Level2.Title", errors.Keys);
        Assert.Contains("Level1.Level2.Level3.Code", errors.Keys);
    }

    private class DeepModel
    {
        [Required]
        public Level1Model Level1 { get; set; }
    }

    private class Level1Model
    {
        [Required]
        public string Description { get; set; }

        [Required]
        public Level2Model Level2 { get; set; }
    }

    private class Level2Model
    {
        [Required, MinLength(3)]
        public string Title { get; set; }

        [Required]
        public Level3Model Level3 { get; set; }
    }

    private class Level3Model
    {
        [Required]
        public string Code { get; set; }
    }

}