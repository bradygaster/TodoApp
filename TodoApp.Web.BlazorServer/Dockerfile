#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TodoApp.Web.BlazorServer/TodoApp.Web.BlazorServer.csproj", "TodoApp.Web.BlazorServer/"]
COPY ["TodoApp/TodoApp.csproj", "TodoApp/"]
RUN dotnet restore "TodoApp.Web.BlazorServer/TodoApp.Web.BlazorServer.csproj"
COPY . .
WORKDIR "/src/TodoApp.Web.BlazorServer"
RUN dotnet build "TodoApp.Web.BlazorServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TodoApp.Web.BlazorServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TodoApp.Web.BlazorServer.dll"]