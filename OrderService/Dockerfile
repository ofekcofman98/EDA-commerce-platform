    # ----------------------------------------------------------------------------------
# שלב 1: שלב הבנייה (Build Stage) - משתמש ב-SDK לבניית הקוד
# ----------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# העתקת קובץ ה-csproj של הסולושן והפרויקט כדי לשחזר תלויות (NuGet)
# זה מנצל את ה-Caching של Docker
COPY ["OrderService/OrderService.csproj", "OrderService/"]
COPY ["Shared.Contracts/Shared.Contracts.csproj", "Shared.Contracts/"]

# שחזור התלויות (Packages)
RUN dotnet restore "OrderService/OrderService.csproj"

# העתקת כל שאר קבצי הקוד של הפרויקט
COPY . .
WORKDIR /src/OrderService

# בניית הפרויקט והוצאת קבצים מוגמרים לתיקיית 'app/build'
RUN dotnet build "OrderService.csproj" -c Release -o /app/build

# ----------------------------------------------------------------------------------
# שלב 2: שלב הפרסום (Publish Stage) - מכין את קבצי הריצה הסופיים
# ----------------------------------------------------------------------------------
FROM build AS publish
RUN dotnet publish "OrderService.csproj" -c Release -o /app/publish

# ----------------------------------------------------------------------------------
# שלב 3: שלב הריצה הסופי (Final Stage) - משתמש ב-Runtime בלבד
# ----------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# העתקת הקבצים המוגמרים שנוצרו בשלב הפרסום
COPY --from=publish /app/publish .

# פתיחת הפורט הנדרש (8080 כפי שקבענו ב-docker-compose)
EXPOSE 8081

# פקודת ההפעלה של היישום
ENTRYPOINT ["dotnet", "OrderService.dll"]