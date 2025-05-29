# SkyPoint Social Backend

A social media platform backend built with ASP.NET Core that enables users to create posts, vote, comment, and follow other users with a personalized feed.


## ✨ Features

### Authentication & User Management
- Email-based signup/login with JWT authentication
- OAuth support (Google/Microsoft)
- Session duration tracking on logout
- Unique email and username validation

### Content Features
- **Posts**: Create short text-based posts
- **Voting**: Upvote/downvote posts with score calculation (upvotes - downvotes)
- **Comments**: Comment on posts with nested/threaded replies support
- **Following**: Follow/unfollow users with toggle functionality

### Personalized Feed
- Prioritized content from followed users
- Sorted by: followed users first, then by score, comment count, and recency
- Displays post content, author, vote count, comment count, and time ago
- Interactive controls for voting, commenting, and following

## 🛠 Tech Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **Authentication**: JWT Bearer Tokens
- **Testing**: xUnit, FluentAssertions
- **API Documentation**: Swagger/OpenAPI

## 📦 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

## 🚀 Setup Instructions
1. Clone the Repository
2. Install Dependencies
    ```bash
    dotnet restore
    ```
3. Environment Configuration
Create an `appsettings.Development.json` file in the `SkyPointSocial.API` project:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=SkyPointSocialDb;Username=your_username;Password=your_password"
  },
  "Jwt": {
    "Secret": "your-super-secret-key-at-least-32-characters-long",
    "Issuer": "SkyPointSocial",
    "Audience": "SkyPointSocialUsers",
    "ExpirationInMinutes": 60
  },
  "OAuth": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    },
    "Microsoft": {
      "ClientId": "your-microsoft-client-id",
      "ClientSecret": "your-microsoft-client-secret"
    }
  }
}
```
## 🗄 Local Setup

1. Create PostgreSQL docker container
```bash
docker-compose up -d
```
2. Run Application
```bash
dotnet run --project SkyPointSocial.API/SkyPointSocial.API.csproj
```
# SkyPointSocial API

## ▶️ Running the Application
````bash
dotnet run --project SkyPointSocial.API/SkyPointSocial.API.csproj
````

### Development Mode
- **HTTP**: [http://localhost:5159](http://localhost:5159)  
- **Swagger UI**: [http://localhost:5159/swagger](http://localhost:5159/swagger)

### Production Mode
Set environment variables and production configurations accordingly.

---

## 📚 API Documentation

### Swagger / OpenAPI
- **Swagger UI**: [http://localhost:5159/swagger](http://localhost:5159/swagger)  
- **OpenAPI Spec**: [http://localhost:5159/swagger/v1/swagger.json](http://localhost:5159/swagger/v1/swagger.json)

---

## 📌 API Endpoints

### 🔐 Authentication
- `POST /api/signup` – Register a new user  
- `POST /api/login` – Login with email and password  
- `POST /api/logout` – Logout current user  
- `POST /api/oauth/login` – Login with OAuth provider  

### 📝 Post Management
- `POST /api/post` – Create a new post  
- `POST /api/vote` – Vote on a post (upvote/downvote)  
- `POST /api/comment/{postId}` – Comment on a post  

### 👥 Follow Management
- `POST /api/follow` – Follow/unfollow a user  
- `GET /api/feed` – Get personalized feed with pagination  

### 🧪 Testing Endpoints
- `GET /api/Test/test-connection` – Test database connection  
- `POST /api/Test/create-user` – Create test user  
- `GET /api/Test/get-user/{userId}` – Get user details  

---

## 🧾 Request/Response Models

All request and response schemas are available in Swagger UI.  
**Authentication Scheme**:  
- `Bearer Authentication` — JWT tokens must be included in the `Authorization` header.

---

## 🧪 Testing

- ✅ Run Unit Tests  
- ✅ Run Integration Tests  
- ✅ Run All Tests  

---

## 🏗 Architecture

Follows **Clean Architecture** principles:

### Key Design Patterns
- **Repository Pattern** – Abstracts data access logic  
- **Service Layer** – Encapsulates business logic  
- **Dependency Injection** – Promotes loose coupling  
- **JWT Authentication** – Stateless token-based authentication  
- **Pessimistic Locking** – Uses PostgreSQL row-level locking for concurrency

---

## 🔒 Security Considerations

- Passwords hashed with **BCrypt**  
- JWT tokens expire after a configured duration  
- SQL injection prevention via parameterized queries  
- Input validation on all endpoints  
- CORS is configured for production use  

---

## 🚦 Troubleshooting

### Common Issues

#### ❌ Database Connection Failed
- Ensure PostgreSQL is running  
- Check connection string in `appsettings.json`  
- Verify database exists  

#### ❌ Migration Errors
- Delete and recreate migrations  
- Ensure DB user has `CREATE` permissions  

#### ❌ JWT Token Issues
- Secret key should be at least **32 characters**  
- Validate token expiration settings  

---

## 📝 License

This project is licensed under the **MIT License**.

---

## 👥 Contributing

1. Fork the repository  
2. Create a feature branch:  
   `git checkout -b feature/AmazingFeature`  
3. Commit your changes:  
   `git commit -m 'Add some AmazingFeature'`  
4. Push to the branch:  
   `git push origin feature/AmazingFeature`  
5. Open a **Pull Request**

---
