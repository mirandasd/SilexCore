# Imagen runtime-only: NO compila dentro del contenedor. Los paquetes
# NuGet privados (Karin.*) solo existen en el feed local E:\NuGets de esta
# máquina, así que el `dotnet publish` se hace en el host (ver
# SilexCore-Deploy/setup.ps1) y acá solo se empaca el resultado.
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY publish/SilexCore.Api/ .
ENTRYPOINT ["dotnet", "SilexCore.Api.dll"]
