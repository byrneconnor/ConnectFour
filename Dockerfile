# ---- Stage 1: build the React frontend ----
FROM node:22-alpine AS frontend
WORKDIR /web
COPY src/ConnectFour.Web/package.json src/ConnectFour.Web/package-lock.json ./
RUN npm ci
COPY src/ConnectFour.Web/ ./
RUN npm run build

# ---- Stage 2: build & publish the .NET API ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend
WORKDIR /src
COPY . .
RUN dotnet publish src/ConnectFour.Api/ConnectFour.Api.csproj -c Release -o /app/publish
COPY --from=frontend /web/dist /app/publish/wwwroot

# ---- Stage 3: runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=backend /app/publish ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "ConnectFour.Api.dll"]
