using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MyWebApplication;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;



var builder = WebApplication.CreateBuilder();
//string connection = "Host=localhost;Port=5432;Database=usersdb;Username=postgres;Password=0456769";
string connection = "Server=ec2-54-194-211-183.eu-west-1.compute.amazonaws.com;Port=5432;Database=d7n0ghfl7od103;User Id==wvtykgelqyapig;Password=feecf70ad5ad427aa149ae825edba6e717f38364575413388349095ebebd4982;sslmode=Prefer;TrustServerCertificate=True;";
builder.Services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(connection));

//builder.Services.AddDbContext<ApplicationContext>(options =>
//{
//        var m = Regex.Match(Environment.GetEnvironmentVariable("DATABASE_URL")!, @"postgres://(.*):(.*)@(.*):(.*)/(.*)");
//        options.UseNpgsql($"Server={m.Groups[3]};Port={m.Groups[4]};User Id={m.Groups[1]};Password={m.Groups[2]};Database={m.Groups[5]};sslmode=Prefer;Trust Server Certificate=true");
//        //options.UseNpgsql($"Server={m.Groups[3]};Port={m.Groups[4]};User Id={m.Groups[1]};Password={m.Groups[2]};Database={m.Groups[5]}");
//});

//var users = new List<Person>
// {
//    new() { Name = "Tom", Age = 35, Email = "tom@gmail.com", Password = "12345" },
//    new() { Name = "Bob", Age = 41, Email = "bob@gmail.com", Password = "55555" },
//    new() { Name = "Sam", Age = 24, Email = "sam@gmail.com", Password = "22222" }
//};

//using (ApplicationContext db = new ApplicationContext())
//{
//    db.Users.AddRange(users);
//    db.SaveChanges();
//}


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = AuthOptions.ISSUER,
            ValidateAudience = true,
            ValidAudience = AuthOptions.AUDIENCE,
            ValidateLifetime = true,
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true
        };
    });
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/users", [Authorize]  async (ApplicationContext db) => await db.Users.ToListAsync());

app.MapPost("/login", async (HttpContext context, ApplicationContext db) =>
{
    var request = context.Request;
    if (request.HasJsonContentType())
    {
        var loginData = await request.ReadFromJsonAsync<LoginData>();
        if (loginData != null && loginData is LoginData)
        {
            Person? person = await db.Users.FirstOrDefaultAsync(p => p.Email == loginData.Email && p.Password == loginData.Password);
            if (person is null || person.IsBlocked == true) return Results.Unauthorized();
            person.IsOnline = true;
            person.LastLogin = DateTime.Now.ToString();
            await db.SaveChangesAsync();
            string encodedJwt = Token.CreateToken(person);
            context.Session.SetString("currentId", person.Id);
            context.Session.SetString("accessToken", encodedJwt);

            var response = new
            {
                access_token = encodedJwt,
                current_Id = person.Id,
                username = person.Email
            };

            return Results.Json(response);
        }
    }
    return Results.NotFound(new { message = "Non-JSON format" });
});


app.MapGet("/api/users/{id}", [Authorize] async (string id, ApplicationContext db) =>
{   
    Person? user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
    if (user == null) return Results.NotFound(new { message = "User not found" });
    Person user2 = user with { Password = "" };
    return Results.Json(user2);
});

app.MapDelete("/api/users/{id}", [Authorize] async (string id, HttpContext context, ApplicationContext db) =>
{
    if (await isBlocked(context, db).ConfigureAwait(false)) return Results.Unauthorized();
    Person? user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
    if (user == null) return Results.NotFound(new { message = "User not found" });

    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.Json(user);
});

app.MapPut("/api/users", [Authorize] async (HttpContext context, ApplicationContext db) => {

    if (await isBlocked(context, db).ConfigureAwait(false)) return Results.Unauthorized();

    var request = context.Request;
    if (request.HasJsonContentType())
    {
        var person = await request.ReadFromJsonAsync<Person>();
        if (person != null && person is Person)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == person.Id);
            if (user == null) return Results.NotFound(new { message = "User not found" });

            if (person.Name != String.Empty) user.Name = person.Name;
            if (person.Age > 0) user.Age = person.Age;
            if (person.Email != String.Empty) user.Email = person.Email;
            user.IsBlocked = person.IsBlocked;
            await db.SaveChangesAsync();
            Person user2 = user with { Password = "" };
            string encodedJwt = Token.CreateToken(user);
            var response = new
            {
                user = user2,
                edited_user_tokenkey = encodedJwt
            };

            return Results.Json(response);
        }

    }
    return Results.NotFound(new { message = "Non-JSON format" });
});

app.MapPost("/signup", async (HttpContext context, ApplicationContext db) => {

    var response = context.Response;
    var request = context.Request;

    if (request.HasJsonContentType())
    {
        var person = await request.ReadFromJsonAsync<Person>();
        if (person != null)
        {
            await db.Users.AddAsync(person);
            await db.SaveChangesAsync();
            Person user2 = person with { Password = "" };
            await response.WriteAsJsonAsync(user2);
        }
    }
});

app.Map("/data", [Authorize] (HttpContext context) => new { message = "table.html" });


app.Run();


async Task<bool> isBlocked(HttpContext context, ApplicationContext db)
{
    string? id = context.Session.GetString("currentId");
    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
    if (user != null && user.IsBlocked)
    {
        context.Session.Remove("accessToken");
        return true;
    }
    return false;
}
