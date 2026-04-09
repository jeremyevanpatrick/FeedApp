# RSS Feed Reader Application

A full-stack RSS Feed Reader built with **ASP.NET Core MVC** and **Web API**.

---

## 🚀 Features

* 🔐 **Authentication & Authorization**

  * JWT-based authentication
  * Secure access token + refresh token workflow

* 📰 **RSS Feed Management**

  * Add, update, and delete feeds
  * Retrieve feed lists and individual feeds
  * Mark feeds and articles as read

* ⚙️ **Background Processing**

  * Asynchronous feed fetching and updates
  * Scheduled maintenance tasks

---

## 🛠️ Tech Stack

* **Backend:** ASP.NET Core MVC & Web API
* **Database:** SQL Server
* **ORM:** Entity Framework Core
* **Authentication:** JWT (JSON Web Tokens)
* **Testing:** xUnit
* **Logging:** Microsoft.Extensions.Logging

---

## 🔐 Authentication Flow

1. User logs in and receives:

   * Access Token (short-lived)
   * Refresh Token (long-lived)

2. Access token is used for API requests

3. When expired:

   * Refresh token is used to obtain a new access token

---

## ⚙️ Background Services

Background workers handle:

* Periodic RSS feed polling
* Article ingestion and updates
* Cleanup and maintenance tasks

Implemented using hosted services for efficient async processing.

---

## 📄 License

This project is licensed under the MIT License.
