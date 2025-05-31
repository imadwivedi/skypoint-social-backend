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

### Development Mode
- **HTTP**: [http://localhost:5159](http://localhost:5159)  

### Production Mode
Set environment variables and production configurations accordingly.

---

## 📚 API Documentation

### Swagger / OpenAPI
- **Swagger UI**: [http://localhost:5159](http://localhost:5159/)  
- **OpenAPI Spec**: [http://localhost:5159/swagger/v1/swagger.json](http://localhost:5159/swagger/v1/swagger.json)

---

## 📘 API Endpoints

---

### 🔐 Authentication (`/auth`)

- **POST** `/signup` – Register a new user.  
- **POST** `/login` – Login with email and password.  
- **POST** `/logout` – Logout current user (requires authentication).  
- **POST** `/oauth/login` – Login or register with an OAuth provider (e.g., Google).
**GET** `/{userId:guid}` – Get public profile information for a specific user. (requires authentication using Google OAuth).

---

### 📝 Posts (`/post`)

- **POST** `/` – Create a new post (requires authentication).  
- **GET** `/user/{userId:guid}` – Get details of a specific post by an user. (requires authentication).

---

### 💬 Comments (`/comment`)

- **POST** `/{postId:guid}` – Add a comment to a specific post (requires authentication).  
- **GET** `/{postId:guid}}` – Get all comments for a specific post (requires authentication).

---

## 👍 Votes (`/vote`)

- **POST** `/` – Cast a vote (upvote/downvote) on a post (requires authentication).  

---

### 🧑‍🤝‍🧑 Follows (`/follow`)

- **POST** `/` – Follow or unfollow a user (requires authentication).  
- **GET** `/status/{userId:guid}` – Get follow status of the authenticated user (requires authentication). 

---

### 📰 Feed (`/feed`)

- **GET** `/` – Get the personalized news feed for the authenticated user (requires authentication).  
  Supports pagination (e.g., `?page=1&pageSize=10`).
  
---

## 🧾 Request/Response Models

All request and response schemas are available in Swagger UI. 

**Authentication Scheme**:  
- `Bearer Authentication` — JWT tokens must be included in the `Authorization` header.

---

## 🧪 Testing
- ✅ Run Integration Tests. All APIs are covered by integration tests using Postres docker image,  xUnit and FluentAssertions.

---

## 🏗 Architecture

Follows **Clean Architecture** principles:

### Key Design Patterns
- **Repository Pattern** – Abstracts data access logic  
- **Service Layer** – Encapsulates business logic  
- **Dependency Injection** – Promotes loose coupling  
- **JWT Authentication** – Stateless token-based authentication

### Potential Enhancements
- **Continuation Token Pagination**: Implement continuation token-based pagination for feeds for more efficient and scalable data fetching, especially with large datasets.
- **Custom Middleware**: Introduce dedicated middleware for:
    - **Request/Response Logging**: For detailed diagnostics and monitoring.
    - **Global Exception Handling**: To centralize error management and provide consistent error responses.
- **API Versioning**: Implement controller/API versioning (e.g., via URL path, query string, or headers) to manage changes and ensure backward compatibility as the API evolves.
- **Optimistic/Pessimistic Concurrency Control**: For operations like voting or updating shared resources, implement robust concurrency handling (e.g., using row-versioning for optimistic concurrency, or `SELECT FOR UPDATE` for pessimistic locking where appropriate, though the latter should be used judiciously due to potential performance impacts).


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
- Verify database exists in docker 

#### ❌ Migration Errors
- Delete and recreate migrations  
- Ensure DB user has `CREATE` permissions  

#### ❌ JWT Token Issues
- Secret key should be at least **32 characters**  
- Validate token expiration settings  


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
