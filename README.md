# ⚽ OffsideIQ

**OffsideIQ** is a full-stack football analytics platform to track matches, analyze performance, and generate intelligent insights.

> Track. Analyze. Predict.

---

## 🚀 Features

* 👤 Authentication (JWT-based)
* 🛡️ Team management
* ⚽ Match tracking & results
* 📊 Advanced stats (possession, shots, xG)
* ⭐ Player ratings & notes
* 🧠 Rule-based insight engine
* 📈 Dashboard with analytics

---

## 🧱 Architecture

```
OffsideIQ/
├── src/
│   ├── OffsideIQ.Core
│   ├── OffsideIQ.Infrastructure
│   ├── OffsideIQ.Application
│   └── OffsideIQ.API
├── frontend/
├── docs/
├── docker-compose.yml
└── OffsideIQ.sln
```

---

## ⚙️ Tech Stack

| Layer     | Tech                  |
| --------- | --------------------- |
| Backend   | ASP.NET Core (.NET 8) |
| ORM       | Entity Framework Core |
| Database  | PostgreSQL            |
| Frontend  | React + Vite          |
| Auth      | JWT                   |
| Container | Docker                |

---

## 🛠️ Getting Started

### Clone

```bash
git clone https://github.com/ismaxhere/OffsideIQ.git
cd OffsideIQ
```

---

### Run Backend

```bash
dotnet run --project src/OffsideIQ.API
```

API → http://localhost:5000
Swagger → http://localhost:5000/swagger

---

### Run Frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend → http://localhost:5173 (default Vite dev server; may vary if the port is already in use)

---

## 🔑 Environment Variables

### Backend

ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=offsideiq;Username=postgres;Password=yourpassword
Jwt__Secret=your_super_secure_secret_key_here
Jwt__Issuer=OffsideIQ
Jwt__Audience=OffsideIQ

---

### Frontend

VITE_API_URL=http://localhost:5000 (default; change if your backend runs on a different port)

---

## 📡 API Overview

### Auth

* POST `/api/auth/register`
* POST `/api/auth/login`

### Teams

* GET `/api/teams`
* POST `/api/teams`

### Matches

* GET `/api/matches`
* POST `/api/matches`

*(Refer to Swagger UI for the complete API documentation.)*

---

## 🧠 Insight Engine

Generates contextual match insights like:

* 🔥 Winning streak detection
* ⚽ Goal analysis
* 📊 Possession dominance
* 🎯 xG vs actual performance
* 🧮 Match prediction

---

## 🌍 Deployment

* ✅ Frontend → Vercel
* ✅ Backend → Render
* ✅ Database → Neon

---

## 📌 Future Improvements

* Comprehensive player profiles with stats, history, and performance tracking
* Real-time match updates with live data synchronization
* Advanced analytics including heatmaps, passing networks, and trend insights
* Social features for sharing matches, stats, and insights

---

## 🧑‍💻 Author

**Ankon Naskar**
GitHub: https://github.com/ismaxhere

---

## 📄 License

MIT License
