# Модуль 06. Практическая работа: Защита доступа к API (API Key и JWT-аутентификация)

Практическая работа по дисциплине «Разработка веб-сервисов» (CSE5032).  
Кафедра Программной инженерии, Институт Автоматики и информационных технологий.

---

## 🎯 Цель работы
Научиться защищать методы ASP.NET Core Web API, используя:
1. **API Key** через пользовательские HTTP-заголовки (`X-Api-Key`).
2. **JWT-аутентификацию** (JSON Web Token) с помощью `Microsoft.AspNetCore.Authentication.JwtBearer`.
3. Передачу учетных данных и токенов в HTTP-заголовках (`Authorization: Bearer <token>`).
4. Ограничение доступа к конечным точкам с помощью атрибута `[Authorize]`.

---

## 🛠 Технологический стек
- **Платформа:** .NET 9.0 (C# 13)
- **Фреймворк:** ASP.NET Core Web API
- **Аутентификация:** `Microsoft.AspNetCore.Authentication.JwtBearer`
- **Документация и тестирование:** Swagger / OpenAPI (Swashbuckle 6.6.2) с поддержкой ввода API-Key и Bearer-токена

---

## 📂 Структура проекта

```text
Web_service_development_modul_06-pra_06/
├── .gitignore
├── ProductSecurityApi.sln
├── README.md
├── DESIGN_AND_ANSWERS.md
└── src/
    └── ProductSecurityApi/
        ├── Attributes/
        │   └── ApiKeyAttribute.cs       # Фильтр проверки API-ключа (X-Api-Key: SecretKey123)
        ├── Controllers/
        │   ├── AuthController.cs        # Контроллер входа (POST /api/auth/login) и генерации JWT
        │   └── ProductController.cs     # Защищенный контроллер товаров
        ├── Models/
        │   ├── LoginRequest.cs          # DTO запроса авторизации (username, password)
        │   └── Product.cs               # Модель товара (Id, Name, Price)
        ├── appsettings.json             # Настройки JWT (Issuer, Audience, Key, Expiry)
        ├── Program.cs                   # Конфигурация DI, JWT Middleware и Swagger
        └── ProductSecurityApi.csproj
```

---

## 🚀 Быстрый запуск

### 1. Сборка проекта
```powershell
cd E:\Web_service_development_RK1\week_6\Web_service_development_modul_06-pra_06
dotnet build
```

### 2. Запуск приложения
```powershell
dotnet run --project src\ProductSecurityApi
```
После запуска откройте в браузере:
- **Swagger UI:** `http://localhost:5000/swagger` (или порт, указанный в терминале)

---

## 📋 Эндпоинты API

| Метод | Маршрут | Требования безопасности | Описание |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/login` | Публичный доступ | Получение JWT токена (`student` / `12345`) |
| `GET` | `/api/product` | Публичный доступ | Получение списка всех товаров |
| `GET` | `/api/product/{id}` | Публичный доступ | Получение товара по ID |
| `GET` | `/api/product/api-key-protected` | Заголовок `X-Api-Key: SecretKey123` | Проверка доступа по API Key |
| `GET` | `/api/product/secure` | Заголовок `Authorization: Bearer <token>` | Проверка доступа по JWT |
| `POST` | `/api/product` | Заголовок `Authorization: Bearer <token>` | Добавление товара (защищено JWT) |

---

## 🔒 Тестирование сценариев безопасности

### 1. Проверка API Key (`X-Api-Key: SecretKey123`)
* **Запрос без ключа:**
  ```http
  GET /api/product/api-key-protected HTTP/1.1
  Host: localhost:5000
  ```
  **Ответ:** `401 Unauthorized` (`API-ключ отсутствует`).

* **Запрос с валидным ключом:**
  ```http
  GET /api/product/api-key-protected HTTP/1.1
  Host: localhost:5000
  X-Api-Key: SecretKey123
  ```
  **Ответ:** `200 OK`.

### 2. Проверка JWT-аутентификации
1. **Получение токена:**
   ```http
   POST /api/auth/login HTTP/1.1
   Content-Type: application/json

   {
     "username": "student",
     "password": "12345"
   }
   ```
   **Ответ:** `200 OK` с полем `token`.

2. **Запрос без токена к защищенному методу:**
   ```http
   GET /api/product/secure HTTP/1.1
   ```
   **Ответ:** `401 Unauthorized`.

3. **Запрос с валидным токеном:**
   ```http
   GET /api/product/secure HTTP/1.1
   Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```
   **Ответ:** `200 OK` с данными пользователя и каталога.
