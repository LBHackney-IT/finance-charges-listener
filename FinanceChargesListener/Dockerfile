FROM mcr.microsoft.com/dotnet/sdk:8.0

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN
WORKDIR /app

# Copy csproj and NUGet.Config and restore as distinct layers
COPY ./FinanceChargesListener/FinanceChargesListener.csproj ./
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config
RUN dotnet restore ./FinanceChargesListener.csproj


# Copy everything else and build
COPY ./FinanceChargesListener ./
RUN dotnet build -c Release -o out ./FinanceChargesListener.csproj
RUN dotnet publish -c Release -o out ./FinanceChargesListener.csproj

EXPOSE ${PORT:-3000}
CMD ASPNETCORE_URLS=http://+:${PORT:-3000} dotnet ./out/FinanceChargesListener.dll
