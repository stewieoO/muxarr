# Runtime-only image - binaries are pre-built by CI for each architecture
FROM mcr.microsoft.com/dotnet/aspnet:10.0

RUN apt-get update && apt-get install -y --no-install-recommends \
    gosu \
    curl \
    ffmpeg \
    mkvtoolnix \
    mediainfo \
    && rm -rf /var/lib/apt/lists/* \
    && groupadd -g 888 appgroup \
    && useradd -u 888 -g 888 -d /data -s /bin/false appuser \
    && usermod -G users appuser

WORKDIR /app

COPY publish/ ./
COPY entrypoint.sh ./entrypoint.sh
RUN chmod +x ./entrypoint.sh

VOLUME ["/data"]

ENV ASPNETCORE_HTTP_PORTS=8183
EXPOSE 8183

ENTRYPOINT ["./entrypoint.sh", "dotnet", "Muxarr.Web.dll"]
