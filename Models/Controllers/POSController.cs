using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RiceCookerPos.Data;
using RiceCookerPos.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RiceCookerPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class PosApiController : ControllerBase
    {
        private readonly PosDbContext _context;

        // 透過建構子注入剛剛設定的 MySQL DbContext
        public PosApiController(PosDbContext context)
        {
            _context = context;
        }

        #region 🔑 1. 員工人事與登入驗證模組 (index.html / staff.html)

        // 🔐 員工登入驗證
        [HttpPost("auth/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.StaffCode) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { Message = "請輸入員工編號與密碼" });
            }

            // 🔍 比對資料庫中的特徵：員工代碼與純數字密碼
            var staff = await _context.Staffs
                .FirstOrDefaultAsync(s => s.StaffCode == request.StaffCode && s.Password == request.Password);

            if (staff == null)
            {
                return Unauthorized(new { Message = "密碼錯誤，請重新輸入！" });
            }

            // 🚀 關鍵點：回傳大寫開頭的屬性，確保前端 localStorage 能精準對齊
            return Ok(new { 
                Message = "登入成功", 
                StaffName = staff.StaffName,  // 傳回員工姓名
                Role = staff.Role             // 傳回職稱權限（店長 / 店員）
            });
        }

        // 📋 取得所有員工名冊（依編號排序）
        [HttpGet("staff")]
        public async Task<IActionResult> GetStaff()
        {
            var staffList = await _context.Staffs.OrderBy(s => s.StaffCode).ToListAsync();
            return Ok(staffList);
        }

        // ➕ 新增門市員工權限
        [HttpPost("staff")]
        public async Task<IActionResult> CreateStaff([FromBody] Staff newStaff)
        {
            if (newStaff == null || string.IsNullOrEmpty(newStaff.StaffCode))
            {
                return BadRequest(new { Message = "資料不完整" });
            }

            // 檢查員工代碼是否重複
            var exists = await _context.Staffs.AnyAsync(s => s.StaffCode == newStaff.StaffCode);
            if (exists)
            {
                return BadRequest(new { Message = "員工編號/代碼已存在！" });
            }

            _context.Staffs.Add(newStaff);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "新增員工權限成功" });
        }

        // ✏️ 修改員工權限資料
        [HttpPut("staff/{code}")]
        public async Task<IActionResult> UpdateStaff(string code, [FromBody] Staff updatedStaff)
        {
            var staff = await _context.Staffs.FindAsync(code);
            if (staff == null)
            {
                return NotFound(new { Message = "找不到該員工" });
            }

            // 覆蓋修改欄位
            staff.StaffName = updatedStaff.StaffName;
            staff.Role = updatedStaff.Role;
            
            // 只有當前端有傳入新密碼時才更新
            if (!string.IsNullOrEmpty(updatedStaff.Password))
            {
                staff.Password = updatedStaff.Password;
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "修改員工資料成功" });
        }

        // ❌ 註銷/刪除員工帳號
        [HttpDelete("staff/{code}")]
        public async Task<IActionResult> DeleteStaff(string code)
        {
            var staff = await _context.Staffs.FindAsync(code);
            if (staff == null)
            {
                return NotFound(new { Message = "找不到該員工資料" });
            }

            _context.Staffs.Remove(staff);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "員工權限已成功註銷" });
        }

        #endregion

        #region 📦 2. 商品菜單管理模組 (home.html / products.html)

        // 📋 撈取實時同步的商品菜單
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.OrderBy(p => p.Id).ToListAsync();
            return Ok(products);
        }

        // ➕ 新增上架新品項
        [HttpPost("products")]
        public async Task<IActionResult> CreateProduct([FromBody] Product prod)
        {
            if (prod == null || string.IsNullOrEmpty(prod.Name))
            {
                return BadRequest(new { Message = "商品資料不完整" });
            }

            // 自動計算生成商品 ID，格式例如 P001, P002, P003...
            var count = await _context.Products.CountAsync();
            prod.Id = "P" + (count + 1).ToString("D3");

            _context.Products.Add(prod);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "商品上架成功", Product = prod });
        }

        // ❌ 下架刪除商品品項
        [HttpDelete("products/{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var prod = await _context.Products.FindAsync(id);
            if (prod == null)
            {
                return NotFound(new { Message = "找不到該商品品項" });
            }

            _context.Products.Remove(prod);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "商品已成功下架" });
        }

        #endregion

        #region 📊 3. 交易結帳與今日訂證明細看板 (home.html / orders.html)

        // 🚀 確認扣款結帳：正式將訂單與明細寫入 Linux MySQL
        [HttpPost("orders")]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            if (order == null || order.Items == null || order.Items.Count == 0)
            {
                return BadRequest(new { Message = "購物車明細為空，結帳失敗！" });
            }

            // 自動生成唯一交易單號，格式：TX + 年月日時分秒
            order.OrderId = "TX" + DateTime.Now.ToString("yyyyMMddHHmmss");
            order.TransactionTime = DateTime.Now;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "結帳扣款成功！", OrderId = order.OrderId });
        }

        // 📊 查詢今日所有收銀交易明細（最新訂單排在最上面）
        [HttpGet("orders/today")]
        public async Task<IActionResult> GetTodayOrders()
        {
            // 抓取今天的日期區間
            var today = DateTime.Today;

            // 使用 Include 連同 OrderItems 關聯明細一起撈出來
            var todayOrders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.TransactionTime >= today)
                .OrderByDescending(o => o.TransactionTime) // 新單在最上
                .ToListAsync();

            // 重新整理成前端需要解析的格式
            var result = todayOrders.Select(o => new {
                o.OrderId,
                o.TransactionTime,
                o.CashierName,
                o.TotalAmount,
                Summary = o.Summary // 調用 Model 裡的自動摘要字串，例如 "美式x2, 拿鐵x1"
            });

            return Ok(result);
        }

        #endregion
    }
}