using Cafeza_BE;
using Cafeza_BE.DB;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));
// Add services to the container.
builder.Services.AddSingleton<MongoDbContext>();


// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.WithOrigins("http://localhost:4200", "http://192.168.1.6:4200")
               .AllowAnyMethod() // Cho phép tất cả các phương thức (GET, POST, PUT, DELETE, etc.)
               .AllowAnyHeader() // Cho phép tất cả các header
               .AllowCredentials(); // Cho phép thông tin xác thực

    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// Sử dụng CORS
app.UseCors("AllowAllOrigins");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
