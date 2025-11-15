# ----------------------------------------------------------------------------------
# שלב 1: שלב הבנייה (Build Stage) - משתמש ב-SDK לבניית הקוד
# ----------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# העתקת קובץ ה-csproj של הסולושן והפרויקט כדי לשחזר תלויות (NuGet)
# זה מנצל את ה-Caching של Docker
COPY ["CartService/CartService.csproj", "CartService/"]
COPY ["Shared.Contracts/Shared.Contracts.csproj", "Shared.Contracts/"]

# שחזור התלויות (Packages)
RUN dotnet restore "CartService/CartService.csproj"

# העתקת כל שאר קבצי הקוד של הפרויקט
COPY . .
WORKDIR /src/CartService

# בניית הפרויקט והוצאת קבצים מוגמרים לתיקיית 'app/build'
RUN dotnet build "CartService.csproj" -c Release -o /app/build

# ----------------------------------------------------------------------------------
# שלב 2: שלב הפרסום (Publish Stage) - מכין את קבצי הריצה הסופיים
# ----------------------------------------------------------------------------------
FROM build AS publish
RUN dotnet publish "CartService.csproj" -c Release -o /app/publish

# ----------------------------------------------------------------------------------
# שלב 3: שלב הריצה הסופי (Final Stage) - משתמש ב-Runtime בלבד
# ----------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# העתקת הקבצים המוגמרים שנוצרו בשלב הפרסום
COPY --from=publish /app/publish .

# פתיחת הפורט הנדרש (8080 כפי שקבענו ב-docker-compose)
EXPOSE 8080 

# פקודת ההפעלה של היישום
ENTRYPOINT ["dotnet", "CartService.dll"]