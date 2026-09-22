# 📚 BlogVerse - ASP.NET MVC Blogging Platform

**BlogVerse** is a full-featured blogging web application built using **ASP.NET Core MVC**, designed to empower writers and readers to connect through meaningful content.

---

## 🚀 Features

### 👤 User Features
- 📝 **Register & Login** (with password hashing)
- 🌗 **Dark/Light Mode Toggle**
- 📖 **Explore Blogs** by categories
- ❤️ **Like, Comment, and Save** blogs
- 📂 **Reading History** and **Saved Blogs**
- 🔍 **Search and Filter** by topic or author
- 🔔 **Get Notified** when favorite authors post

### ✍️ Writer Features
- 🧠 **AI-based Blog Ideas** (optional future feature)
- ✨ **Rich Text Editor** (TinyMCE integrated)
- 📊 **Writer Dashboard**
  - My Posts
  - Saved Drafts
  - Analytics (e.g., views, likes, category-wise charts)

### 📢 Homepage Highlights
- 🦄 Hero section with CTA
- 🔥 Trending Blogs (interactive with detail view + popup)
- 🎯 "Why BlogVerse?" benefits grid
- 🛎️ Pop-up trigger after 30s on blog detail pages

---

## 🛠️ Tech Stack

| Layer        | Technology                         |
|--------------|-------------------------------------|
| Frontend     | HTML, CSS, Bootstrap, JavaScript    |
| Backend      | ASP.NET Core MVC (C#)               |
| Database     | SQL Server (via Entity Framework)   |
| Auth System  | ASP.NET Core Identity               |
| Rich Editor  | TinyMCE                             |

---

## 📸 Screenshots

Here are a few glimpses of BlogVerse in action:

### 🏠 Landing Page  
![Landing Page](https://github.com/sakshi01coder/BlogVerse/blob/master/screenshots/Screenshot%202025-06-22%20093632.png?raw=true)

![Landing Page2](https://github.com/sakshi01coder/BlogVerse/blob/master/screenshots/Screenshot%202025-06-22%20094739.png?raw=true)

### 🔐 Registration Page  
![Register Page](https://github.com/sakshi01coder/BlogVerse/blob/master/screenshots/Screenshot%202025-06-22%20094803.png?raw=true)

### 🔑 Login Page  
![Login Page](https://github.com/sakshi01coder/BlogVerse/blob/master/screenshots/Screenshot%202025-06-22%20094934.png?raw=true)

## 🧭 Main Sections
![main section1](https://github.com/sakshi01coder/BlogVerse/blob/master/screenshots/Screenshot%202025-06-22%20094949.png?raw=true)

![main section2](https://github.com/sakshi01coder/BlogVerse/blob/master/screenshots/Screenshot%202025-06-22%20095028.png?raw=true)

### 📊 Writer Dashboard  
![Dashboard](https://github.com/sakshi01coder/BlogVerse/blob/master/screenshots/Screenshot%202025-06-22%20095050.png?raw=true)

### 📝 Create Post (TinyMCE Editor)  
![Create Post](https://github.com/sakshi01coder/BlogVerse/blob/master/screenshots/Screenshot%202025-06-22%20095111.png?raw=true)

### 📈 Analytics View  
![Analytics View](https://github.com/sakshi01coder/BlogVerse/blob/master/screenshots/Screenshot%202025-04-16%20224337.png?raw=true)

### 🧠 Blog Detail View with Popup  
![Blog Detail + Popup](https://github.com/sakshi01coder/BlogVerse/blob/master/screenshots/Screenshot%202025-06-22%20094751.png?raw=true)

---

## 🏗️ Project Setup

### ✅ Prerequisites
- [.NET SDK 6.0+](https://dotnet.microsoft.com/download)
- Visual Studio 2022 or VS Code
- SQL Server

### 🔧 Setup Commands

```bash
# Restore packages
dotnet restore

# Apply migrations
dotnet ef database update

# Run the app
dotnet run!

[image](https://github.com/user-attachments/assets/3d3bd2a3-fa7e-45e0-8a9e-cd929d3d6cec)

