using System;
using System.Collections.Generic;
using System.Linq; // 💡 確保 Select 擴充方法運作正常

namespace RiceCookerPos.Models
{
    // 1. 訂單明細模型
    public class OrderItem
    {
        public int Id { get; set; }                           // 👈 EF Core 需要的主鍵
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    // 2. 主訂單模型
    public class Order
    {
        public string OrderId { get; set; } = string.Empty;      // 交易單號
        public DateTime TransactionTime { get; set; }            // 交易時間
        public string CashierName { get; set; } = string.Empty;   // 經手收銀員
        
        // 💡 這裡會正確參考到上面的 OrderItem
        public List<OrderItem> Items { get; set; } = new();       // 購買明細
        public decimal TotalAmount { get; set; }                  // 交易總金額
        
        // 自動生成明細摘要，例如: "焦糖拿鐵咖啡x2, 經典美式咖啡x1"
        public string Summary => string.Join(", ", Items.Select(i => $"{i.ProductName}x{i.Quantity}")); 
    }
}