FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish ExpenseTracker.Api/ExpenseTracker.Api.csproj -c Release -o /app

FROM node:22-slim AS cli
RUN npm install -g @anthropic-ai/claude-code@2.1.312

FROM mcr.microsoft.com/dotnet/aspnet:10.0
COPY --from=cli /usr/local/bin/node /usr/local/bin/node
COPY --from=cli /usr/local/lib/node_modules /usr/local/lib/node_modules
RUN ln -s ../lib/node_modules/@anthropic-ai/claude-code/bin/claude.exe /usr/local/bin/claude
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
# The sign-in lives here, on a volume, so it survives rebuilds.
ENV HOME=/home/app CLAUDE_CONFIG_DIR=/home/app/.claude
RUN mkdir -p /home/app/.claude && chown -R $APP_UID /home/app
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "ExpenseTracker.Api.dll"]
