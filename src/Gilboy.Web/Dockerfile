FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Gilboy.Web/Gilboy.Web.csproj", "src/Gilboy.Web/"]
RUN dotnet restore "src/Gilboy.Web/Gilboy.Web.csproj"
COPY . .
WORKDIR "/src/src/Gilboy.Web"
RUN dotnet build "Gilboy.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gilboy.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gilboy.Web.dll"]
