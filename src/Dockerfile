# help: https://cloud.google.com/appengine/docs/flexible/dotnet/customizing-the-dotnet-runtime
FROM gcr.io/google-appengine/aspnetcore:2.0.0
COPY . /app
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://*:5000

# pre-intall: FFMPEG latest
RUN mkdir -p /root && cd /root \
  && (apt-get install wget less xz-utils p7zip-full -y || true) \
  && wget --no-check-certificate -O ffmpeg-release-64bit-static.tar.xz https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-64bit-static.tar.xz \
  && tar -xJf ffmpeg-release-64bit-static.tar.xz \
  && cd ffmpeg* \
  && d=$(pwd) \
  && cp ff* /usr/bin \
  && cd .. \
  && rm -rf "$d" && ffprobe -version

# pre-install MySQL Server 5.5 with root's password=root
RUN (echo mysql-server-5.5 mysql-server/root_password password root | debconf-set-selections) \
  && (echo mysql-server-5.5 mysql-server/root_password_again password root | debconf-set-selections) \
  && (sudo apt-get -y install mysql-server-5.5 && service mysql start)

# pre-install REDIS server 2.8
RUN sudo apt-get -y install redis-server \
  && (echo maxmemory 100M >> /etc/redis/redis.conf) \
  && service redis-server restart

ENTRYPOINT ["dotnet", "Gallery.MVC.dll"]
