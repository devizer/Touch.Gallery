FROM https://cloud.google.com/appengine/docs/flexible/dotnet/customizing-the-dotnet-runtime
COPY . /app
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://*:5000
RUN mkdir -p /root && cd /root \
  && (apt-get install wget less xz-utils p7zip-full p7zip-rar -y || true) \
  && wget --no-check-certificate -O ffmpeg-release-64bit-static.tar.xz https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-64bit-static.tar.xz \
  && tar -xJf ffmpeg-release-64bit-static.tar.xz \
  && cd ffmpeg* \
  && d=$(pwd) \
  && cp ff* /usr/bin \
  && cd .. \
  && rm -rf "$d" && ffprobe -version
ENTRYPOINT ["dotnet", "Gallery.MVC.dll"]
