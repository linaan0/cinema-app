# Cinema Ticket Booking Platform

A cinema ticket booking system built as five independently deployable
ASP.NET Core microservices, each following the same layered structure
(**Domain / Repository / Service / Api**).

## Services

| Service | Responsibility | Database | Extra infra |
|---|---|---|---|
| `frontend` | React + Vite SPA: browse movies, live seat map, bookings | - | served via Nginx |
| `auth` | Register/login, issues JWTs | `auth_db` | - |
| `movies` | Movie catalog (CRUD) | `movies_db` | - |
| `bookings` | Screenings, seat map, **seat locking**, bookings | `bookings_db` | Redis, RabbitMQ, SignalR |
| `pricing` | Dynamic pricing rules + price calculation | `pricing_db` | - |
| `notifications` | Consumes booking events, "sends" confirmation emails | `notifications_db` | RabbitMQ |

All five backend services share **one MongoDB instance** (deployed as a
`StatefulSet`), each with its own database. This keeps the project
runnable on a laptop while still respecting "each service owns its data" -
splitting any of them onto a separate MongoDB instance later is a config
change, not a rewrite.

## Project layout

```
cinema-app/
├── docker-compose.yml          # local orchestration (Mongo, Redis, RabbitMQ + 5 services + frontend)
├── .env.example                 # copy to .env - shared JWT signing key
├── frontend/
│   ├── Dockerfile                # vite build -> nginx static serve
│   ├── nginx.conf
│   └── src/
│       ├── api.js                # client for all 5 backend services
│       ├── signalr.js            # minimal hand-rolled SignalR client (no extra dep)
│       ├── AuthContext.jsx
│       ├── App.jsx
│       └── pages/                 # Movies, MovieDetail, SeatMap, MyBookings, Login, Register
├── services/
│   ├── auth/
│   │   ├── Dockerfile
│   │   ├── CinemaApp.Auth.sln
│   │   └── src/
│   │       ├── CinemaApp.Auth.Domain/       # entities, DTOs, enums
│   │       ├── CinemaApp.Auth.Repository/   # Mongo repositories
│   │       ├── CinemaApp.Auth.Service/      # business logic
│   │       └── CinemaApp.Auth.Api/          # controllers, Program.cs
│   ├── movies/        (same 4-layer structure)
│   ├── bookings/      (same 4-layer structure + SignalR hub)
│   ├── pricing/       (same 4-layer structure)
│   └── notifications/ (same 4-layer structure + RabbitMQ consumer)
├── k8s/
│   ├── namespace.yaml
│   ├── infra/           # MongoDB StatefulSet, Redis, RabbitMQ, shared JWT secret
│   ├── frontend/         # ConfigMap-free static Deployment + Service
│   ├── auth/            # ConfigMap + Deployment + Service
│   ├── movies/
│   ├── bookings/        # + HorizontalPodAutoscaler
│   ├── pricing/
│   ├── notifications/
│   └── ingress.yaml      # Nginx Ingress routing /api/* to each service
└── .github/workflows/    # one CI pipeline per service (build + push to GHCR)
```


## Running locally with Docker Compose

```bash
# edit .env and set a real JWT_KEY (32+ random characters)

docker compose up --build
```

| Service | URL |
|---|---|
| Frontend | http://localhost:5173 |
| Auth | http://localhost:8081/swagger |
| Movies | http://localhost:8082/swagger |
| Bookings | http://localhost:8083/swagger |
| Pricing | http://localhost:8084/swagger |
| Notifications | http://localhost:8085/swagger |
| RabbitMQ management | http://localhost:15672 (guest/guest) |

## Deploying to Kubernetes (k3d)

```bash
# 1. Create a cluster with Nginx Ingress (Traefik disabled)
k3d cluster create cinema-app \
  --ports "80:80@loadbalancer" \
  --ports "443:443@loadbalancer" \
  --k3s-arg "--disable=traefik@server:0"

kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.10.0/deploy/static/provider/cloud/deploy.yaml

# 2. Namespace + shared infra
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/infra/


# 4. Each service
kubectl apply -f k8s/auth/
kubectl apply -f k8s/movies/
kubectl apply -f k8s/bookings/
kubectl apply -f k8s/pricing/
kubectl apply -f k8s/notifications/

# 5. Ingress
kubectl apply -f k8s/ingress.yaml

# 6. Verify
kubectl get all -n cinema-app
```

Once everything is `Running`/`Ready`, the API is reachable at
`http://localhost/api/auth/...`, `http://localhost/api/movies`, etc.

## CI/CD

Each service has its own GitHub Actions workflow
(`.github/workflows/ci-<service>.yml`) that triggers only when files under
`services/<service>/**` change, builds the Docker image, and pushes
`latest` + `<commit-sha>` tags to GHCR
(`ghcr.io/<owner>/cinema-app-<service>`). 

