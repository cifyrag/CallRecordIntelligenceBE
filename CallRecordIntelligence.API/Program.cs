using CallRecordIntelligence.EF;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationDbContext(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "CDR API V1");
	c.RoutePrefix = string.Empty;
});

app.MapControllers();

app.Run();