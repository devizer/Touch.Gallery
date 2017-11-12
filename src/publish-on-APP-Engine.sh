#!/bin/bash
cd $HOME && work=$HOME/Touch.Gallery && rm -rf $work && mkdir -p $work && cd $work && git clone https://github.com/devizer/Touch.Gallery && cd Touch.Gallery/src && dotnet publish -c Release && cd ./bin/Release/netcoreapp2.0/publish/
time (gcloud beta app deploy app.yaml --promote -q --stop-previous-version --verbosity=warning)
