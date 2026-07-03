# 🍚 飯匙雲端 POS 系統 (RiceSpoon POS)

本專案是一款專為小型攤販與門市設計的輕量級雲端收銀與人事管理系統。後端採用 **.NET 8 Web API** 驅動，搭配 **Entity Framework Core** 與 **Linux MySQL** 資料庫；前端採用原生輕量化技術，整合無懈可擊的**網頁安全政策（HTTPS）**與**第 0 道前置防禦權限攔截機制**。

全系統透過 **Cloudflare Tunnel 安全隧道**穿透，具備高安全性、無須對外開放實體 Port 即可多服務並存的商業級生產環境架構。

---

## 🛠️ 技術棧與底層架構

- **後端核心**：ASP.NET Core 8.0 Web API
- **資料庫系統**：Linux 本地端 MySQL (Pomelo EF Core 提供驅動)
- **外網對接**：Cloudflare Tunnel (多服務獨立常駐 Systemd)
- **前端架構**：原生 HTML5 / CSS3 / Vanilla JavaScript (安全前置攔截、CORS 放行對齊)

---

## 📁 專案目錄分工規範 (核心觀念)

本專案嚴格遵循**開發與運行分離**的黃金工作流，請務必遵循以下規範：

- **`~/RiceSpoon_POS/wwwroot/`**：**原始碼倉庫 (靈魂)**
  - 所有的 HTML/CSS/JS 修改、功能開發與 Git 版本控制，**一律在此目錄進行**。
- **`~/RiceSpoon_POS/dist/`**：**發行產物 (實體執行肉身)**
  - 純粹供 Linux 系統服務背景常駐執行使用，此目錄由 `.gitignore` 自動忽略，**嚴禁手動修改內部檔案**。

---

## 🔄 獨立開發者日常維護：黃金工作流

當有任何網頁版面或後端邏輯需要修改更新時，請嚴格執行以下三部曲：

### 1. Mac 本地端修改與提交
在 Mac 上使用 VS Code 編輯外面的原始碼檔案（如修改 `auth.js` 或 `Program.cs`），確認無誤後推上 GitHub：
```bash
git add .
git commit -m "feat: 描述你的新功能或修正"
git push origin main
