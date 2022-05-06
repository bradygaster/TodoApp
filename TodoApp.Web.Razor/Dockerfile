#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TodoApp.Web.Razor/TodoApp.Web.Razor.csproj", "TodoApp.Web.Razor/"]
COPY ["TodoApp/TodoApp.csproj", "TodoApp/"]
RUN dotnet restore "TodoApp.Web.Razor/TodoApp.Web.Razor.csproj"
COPY . .
WORKDIR "/src/TodoApp.Web.Razor"
RUN dotnet build "TodoApp.Web.Razor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TodoApp.Web.Razor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TodoApp.Web.Razor.dll"]