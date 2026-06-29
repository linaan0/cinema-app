# ---- Build stage ----
FROM node:20-alpine AS build
ARG VITE_API_BASE
ENV VITE_API_BASE=$VITE_API_BASE
WORKDIR /app
COPY package.json package-lock.json* ./
RUN npm install
COPY . .
RUN npm run build

# ---- Runtime stage: static files served by Nginx ----
FROM nginx:1.27-alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
