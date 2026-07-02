namespace RiceCookerPos.Models
{
    public class Staff
    {
        public string StaffCode { get; set; } = string.Empty; // 員工編號，例如: A01
        public string StaffName { get; set; } = string.Empty; // 姓名
        public string Role { get; set; } = string.Empty;      // 職稱權限：店長 / 店員
        public string Password { get; set; } = string.Empty;  // 純數字密碼
    }

    public class LoginRequest
    {
        public string StaffCode { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}