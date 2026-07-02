namespace RiceCookerPOS.API.DTOs
{
    // 結帳請求主檔
    public class CheckoutRequest
    {
        public string CashierName { get; set; } = string.Empty; // 經手收銀員
        public decimal TotalAmount { get; set; }               // 應付總計
        public List<CartItemDto> CartItems { get; set; } = new(); // 購物車品項
    }

    // 購物車單一品項明細
    public class CartItemDto
    {
        public string ProductName { get; set; } = string.Empty; // 商品名稱
        public int Quantity { get; set; }                      // 數量
        public decimal UnitPrice { get; set; }                  // 單價
    }
}