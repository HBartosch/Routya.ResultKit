using Routya.ResultKit.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add exception mapping for automatic ProblemDetails conversion
builder.Services.AddExceptionMapping();

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Routya.ResultKit Demo API",
        Version = "v1",
        Description = "A comprehensive demo API showcasing all features of Routya.ResultKit and Routya.ResultKit.AspNetCore packages",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Routya.ResultKit",
            Url = new Uri("https://github.com/HBartosch/Routya.ResultKit")
        }
    });

    // Include XML comments for better Swagger documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Routya.ResultKit Demo API v1");
    options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});

// Use exception mapping middleware to automatically convert exceptions to ProblemDetails
app.UseExceptionMapping();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
