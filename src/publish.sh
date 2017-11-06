#!/bin/bash
cd $HOME && work=$HOME/Touch.Gallery && rm -rf $work && mkdir -p $work && cd $work && git clone https://github.com/devizer/Touch.Gallery && cd Touch.Gallery/src && dotnet publish -c Release
time (gcloud beta app deploy ./bin/Release/netcoreapp2.0/publish/app.yaml --promote)
