- Comando para subir a stack de Employees:
  cd HACKATON-FIAP/src/Employees ➜

  dotnet publish Employees.csproj -c Release -r linux-x64 --no-self-contained -o publish && \
  cd publish && \
  zip -r ../function.zip . && \
  cd .. && \
  sls deploy --stage prod
