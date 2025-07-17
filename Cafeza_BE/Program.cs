using Cafeza_BE;
using Cafeza_BE.DB;
using Cafeza_BE.Hub;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);



// Cấu hình JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("[}61L3B>z?XvzH&#!jH?b_RJ=K£lh-J7TO~c+i")),
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
            ValidIssuer = "cafeza",
            ValidAudience = "api-cafeza"
        };
    });


// Cấu hình authorization
builder.Services.AddAuthorization();

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = "cafeza",
//        ValidAudience = "api-cafeza",
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("[}61L3B>z?XvzH&#!jH?b_RJ=K£lh-J7TO~c+i"))
//    };

//    // Cho phép đọc token từ Cookie
//    options.Events = new JwtBearerEvents
//    {
//        OnMessageReceived = context =>
//        {
//            var token = context.HttpContext.Request.Cookies["access_token"];
//            if (!string.IsNullOrEmpty(token))
//            {
//                context.Token = token;  
//            }
//            return Task.CompletedTask;
//        }
//    };
//});

//builder.Services.AddAuthorization();


//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });

//    // Thêm phần Auth Bearer Token
//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Description = "Please enter your Bearer token",
//        Name = "Authorization",
//        In = ParameterLocation.Header,
//        Type = SecuritySchemeType.ApiKey,
//        BearerFormat = "JWT",  // Thêm BearerFormat để chỉ định rằng đây là token JWT
//        Scheme = "Bearer"  // Thêm Scheme để yêu cầu Bearer token
//    });

//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            },
//            new string[] {}
//        }
//    });
//});


builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));
// Add services to the container.
builder.Services.AddSingleton<MongoDbContext>();


// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.WithOrigins("http://localhost:4200", "http://192.168.1.11:4200", "https://cafeza-fe-c132003.netlify.app")
               .AllowAnyMethod() // Cho phép tất cả các phương thức (GET, POST, PUT, DELETE, etc.)
               .AllowAnyHeader() // Cho phép tất cả các header
               .AllowCredentials(); // Cho phép thông tin xác thực,

    });
});
builder.Services.AddSignalR();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();



var app = builder.Build();
// Sử dụng CORS
app.UseCors("AllowAllOrigins");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty;  // Swagger UI sẽ xuất hiện tại root của ứng dụng
});
app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();   

app.MapControllers();
app.MapHub<SignalRHub>("/signalrHub");


app.Run();
