dotnet-coverage collect -f cobertura dotnet run --project test/zit.test.csproj -o build/coverage/cov.xml
mkdir -p build/coverage/report
reportgenerator -reports:"build/coverage/cov.xml" -targetdir:"build/coverage/report"
reportgenerator -reports:"build/coverage/cov.xml" -targetdir:"build/coverage/report" -reporttypes:TextSummary
