FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CartService/CartService.csproj", "CartService/"]
COPY ["Shared.Contracts/Shared.Contracts.csproj", "Shared.Contracts/"]

RUN dotnet restore "CartService/CartService.csproj"

COPY . .
WORKDIR /src/CartService

RUN dotnet build "CartService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CartService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 8080 

ENTRYPOINT ["dotnet", "CartService.dll"]