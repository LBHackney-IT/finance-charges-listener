FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ENV DynamoDb_LocalMode='true'
ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./FinanceChargesListener.sln ./
COPY ./FinanceChargesListener/FinanceChargesListener.csproj ./FinanceChargesListener/
COPY ./FinanceChargesListener.Tests/FinanceChargesListener.Tests.csproj ./FinanceChargesListener.Tests/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./FinanceChargesListener/FinanceChargesListener.csproj
RUN dotnet restore ./FinanceChargesListener.Tests/FinanceChargesListener.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out FinanceChargesListener.Tests/FinanceChargesListener.Tests.csproj

CMD dotnet test
