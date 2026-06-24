using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CloudClinic.Data;
using CloudClinic.Account.Services;
using CloudClinic.Doctor.Services;
using CloudClinic.Patients.Services;
using CloudClinic.Shared.AuthConstants;
using CloudClinic.Shared.Services;
using CloudClinic.SignalR;
using CloudClinic.SignalR.Config;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Configuration
string conStr = builder.Configuration.GetConnectionString("CloudClinicDb2")!;
builder.Services.AddDbContext<CloudClinicDb>(options => options.UseSqlServer(conStr));

// 2. Identity Configuration
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<CloudClinicDb>();

builder.Services.Configure<IdentityOptions>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 4;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
});

// 3. JWT & Authentication
var key = Encoding.UTF8.GetBytes(builder.Configuration["ApplicationSettings:JWT_Secret"]!)
    ?? throw new InvalidOperationException("JWT_Secret is not configured.");
builder.Services.AddAuthentication(auth => {
    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt => {
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
    opt.Events = new JwtBearerEvents
    {
        OnMessageReceived = context => {
            var token = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(token) && path.StartsWithSegments("/hub"))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options => {
    // Custom model state response factory (re-implement your logic here)
});

// 4. Custom Services
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<ITokenManager, TokenManager>();
builder.Services.AddScoped<IDrDetailsProvider, DrDetailsProvider>();
builder.Services.AddScoped<IDrStaffDetailsProvider, DrStaffDetailsProvider>();
builder.Services.AddScoped<IDrAppointmentService, DrAppointmentService>();
builder.Services.AddScoped<IDrClinicalDataService, DrClinicalDataService>();
builder.Services.AddScoped<IPatAppointmentService, PatAppointmentService>();
builder.Services.AddScoped<IPatReportManageService, PatReportManageService>();
builder.Services.AddScoped<ISearchDrService, SearchDrService>();
builder.Services.AddScoped<IUserProfileProvider, UserProfileProvider>();

builder.Services.AddSignalR(opt => opt.ClientTimeoutInterval = TimeSpan.FromMinutes(60));
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<SwaggerFileOperationFilter>();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();

    // 1. Seed Roles
    string[] roleNames = {
        Roles.StaffRole, Roles.DrLabStaffRole, Roles.DrXrayStaffRole,
        Roles.DrAssistantRole, Roles.DrPharmacyStaffRole, Roles.DrAccountantRole,
        Roles.DrRole, Roles.NormalUserRole
    };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // 2. Seed Admin User
    string adminEmail = "asif9@web.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new AppUser
        {
            UserName = "asif9@web.com",
            Email = adminEmail,
            EmailConfirmed = true, // Bypass email verification for admin
            DOB = new DateTime(1990, 1, 1),
            GuardianName="Guardian Name",
            Gender = "Male",
            FullName = "Admin User",
        };

        var result = await userManager.CreateAsync(adminUser, "123456");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, Roles.AdminRole);
        }
    }
}

// 5. Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error/OtherError");
}

app.UseStatusCodePagesWithReExecute("/error/StatusCodeError/{0}");

app.UseCors(b => b.WithOrigins("http://localhost:4200", "https://localhost:5001")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials());

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<ClientsHub>("/hub");
app.MapRazorPages();

app.Run();