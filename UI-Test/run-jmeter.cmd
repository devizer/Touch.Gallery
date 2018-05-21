set JMETER_HOME=C:\Apps\apache-jmeter-4.0
@rd /q /s log 1>nul 2>&1
@mkdir log 1>nul 2>&1
@pushd log
"%JMETER_HOME%\bin\jmeter" -n -t ..\Touch-Galleries.jmx -l jmeter-report.csv -e -o html\
@popd
