FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
ENV DOTNET_URLS=http://+:8080
WORKDIR /app
COPY /bin/Release/net9.0 .
RUN apk add --no-cache tzdata
RUN echo "Europe/Berlin" > /etc/timezone
RUN ln -sf /usr/share/zoneinfo/Europe/Berlin /etc/localtime
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs icu-data-full
ENV LC_ALL=de_DE.UTF-8
ENV LANG=de_DE.UTF-8

EXPOSE 8080

ENTRYPOINT ["dotnet", "DriverStatsWebApi.dll"]