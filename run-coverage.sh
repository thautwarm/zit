# install dotnet-coverage if not installed
if ! command -v dotnet-coverage &> /dev/null
then
    dotnet tool install --global dotnet-coverage
fi

if ! command -v reportgenerator &> /dev/null
then
    dotnet tool install --global dotnet-reportgenerator-globaltool
fi

mkdir -p build/coverage

dotnet-coverage collect -f xml dotnet run --project test/zit.test.csproj -o build/coverage/cov.xml

# open -a "Google Chrome" build/coverage/report/index.html
# start build/coverage/report/index.html
