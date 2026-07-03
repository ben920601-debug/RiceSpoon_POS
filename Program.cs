using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RiceCookerPos.Data; // 假設你的 DbContext 放在 Data 命名空間下
using System;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. 讀取連線字串 (請根據你的 Linux 伺服器改寫)
// ==========================================
string mySqlConnectionStr = "Server=192.168.0.176;Port=3306;Database=RiceSpoon;Uid=buddy;Pwd=buddy1220;AllowUserVariables=True";

// ==========================================
// 2. 註冊後端核心服務 (Configure Services)
// ==========================================

// 註冊 MySQL DbContext 服務
builder.Services.AddDbContext<PosDbContext>(options =>
    options.UseMySql(mySqlConnectionStr, ServerVersion.AutoDetect(mySqlConnectionStr)));

// 註冊 Controller，並維持 C# 屬性原本的大小寫 (PascalCase)，方便前端與 JS 物件屬性對接
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// 註冊 Swagger/OpenAPI 測試面板
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔐 CORS 跨域原則設定：允許前端 MPA 網頁進行 fetch 呼叫
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPosFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ==========================================
// 3. 自動初始化資料庫（免去手動進 Linux 建立 Table 的麻煩）
// ==========================================
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PosDbContext>();
        // EnsureCreated 會自動檢查 Linux MySQL 是否有該資料庫與資料表，若無則自動建立
        dbContext.Database.EnsureCreated();
        Console.WriteLine("🚀 Linux MySQL 資料庫連線成功，資料表已就緒！");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ 無法連線至 Linux MySQL，請檢查 IP、帳密或防火牆。錯誤訊息: {ex.Message}");
    }
}

// ==========================================
// 4. 設定 HTTP 請求管線 (Configure Pipeline)
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "飯匙雲端POS系統 API v1 (MySQL 測試版)");
    });
}

// 🚀 關鍵校正一：強制將 HTTPS 安全轉向移到最頂部（必須最先執行）
app.UseHttpsRedirection();

// 🚀 關鍵校正二：先設定預設首頁，再啟用靜態檔案
app.UseDefaultFiles(); // 這會強制把根目錄 "/" 對齊到 "/index.html"
app.UseStaticFiles();  

// 啟用 CORS 跨域（必須放在 UseAuthorization 之前）
app.UseCors("AllowPosFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();