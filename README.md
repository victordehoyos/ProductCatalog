
# 🚀 ProductCatalog - Monorepo

Este repositorio contiene la solución completa **ProductCatalog**, estructurada como un **monorepo** con diferentes capas y herramientas:

- Backend (API .NET 9 / EF Core / Clean Architecture)
- Frontend (React)
- CI/CD (GitHub Actions)
- Infraestructura (Docker)

---

## 🧩 Estructura General

```bash
productcatalog/
├─ backend/                    # API en .NET 9 (Clean Architecture)
│
├─ frontend/                   # Aplicación Next.js (Material UI)
│
├─ .github/                    # Configuración de CI/CD (GitHub Actions)
│
├─ docker-compose.yml           # Orquestación de servicios
├─ .gitignore                   # Exclusiones de Git
└─ README.md                    # Documentación principal
```

---

## ⚙️ Backend (.NET 9 – Clean Architecture)

La API se encuentra en `/backend` y sigue la arquitectura **Clean Code / Hexagonal**:

### Capas
| Capa | Descripción |
|------|--------------|
| **Domain** | Entidades y puertos de negocio |
| **Application** | Casos de uso, UnitOfWork, Excepciones |
| **Infrastructure** | EF Core, Repositorios, Persistencia |
| **WebAPI** | Controladores, Configuración, Middlewares |

---

## 💻 Frontend (React)

La interfaz de usuario se encuentra en `/frontend`, desarrollada con desarrollada con **Next.js 15** y **Material UI**. Conectada a la API del backend (.NET 9). 
### Scripts principales

```bash
# Instalar dependencias
npm install

# Ejecutar entorno local
npm run dev

# Compilar para producción
npm run build

```

### Configuración API
Archivo `.env` (en `/frontend`):

```bash
NEXT_PUBLIC_API_URL=http://localhost:5000
```

---

## ⚙️ CI/CD (GitHub Actions)

Pipeline principal en `.github/workflows/ci.yml`:

### Flujo
1. Checkout del código  
2. Setup .NET + Node.js  
3. Build backend (`dotnet build`)  
4. Test backend (`dotnet test`)  
5. Build frontend (`npm run build`)  
6. Test Docker Compose (levantamiento completo)  

```yaml
on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]
```

### Resultados esperados
- Compilación y tests automáticos en cada PR
- Validación de integridad antes de merge a main

---

## 🐳 Docker Compose

El archivo raíz `docker-compose.yml` orquesta los servicios principales:

### Levantar todo el stack
```bash
docker compose up -d --build
```

### Detener
```bash
docker compose down -v
```

---

## 📜 Autor

Desarrollado por **Víctor Alfonso De Hoyos Paternina**  
🧭 Arquitectura: Clean Code / Hexagonal  
⚙️ Tecnologías: .NET 9 · EF Core · MySQL · React · Docker · GitHub Actions  
📚 Patrones: UoW · Repository · Optimistic Concurrency · Idempotencia

---