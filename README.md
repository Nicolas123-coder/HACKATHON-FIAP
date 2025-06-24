# POS Tech HACKATHON
## 1. Resumo do projeto

A FastTech Foods, rede de fast food em expansão, está construindo uma plataforma própria serverless em .NET para modernizar e escalar seu atendimento e operação de pedidos.

- **Diferencia funções** de clientes, pedidos, itens de cardápio e funcionários
- **Garante escalabilidade** com AWS Lambda, DynamoDB e SQS
- **Fornece observabilidade** via X-Ray, CloudWatch e tracing distribuído
- **Assegura segurança** com JWT e validações de perfil (manager/employee)
> Este MVP visa reduzir custos operacionais e melhorar a experiência de clientes e equipe, substituindo ferramentas de terceiros por uma solução própria, flexível e observável.

### Endpoints disponíveis

📁 **Orders**
- `POST /orders/reject`  — rejeita pedido (status = Rejected)
- `POST /orders/accept`  — aceita pedido (status = Accepted)
- `GET  /orders/list`    — lista todos os pedidos (status “Pending” por padrão)

📁 **Clients**
- `POST /clients/create-order`    — cria um novo pedido (envia para SQS)
- `GET  /clients/search-products` — busca itens de cardápio
- `POST /clients/login-email`     — autentica cliente por email
- `POST /clients/login-cpf`       — autentica cliente por CPF

📁 **Menu Items**
- `POST /menu-items/create` — cria itens de cardápio
- `PUT  /menu-items/update` — atualiza itens de cardápio

📁 **Employees**
- `POST /employees/create` — cria usuário funcionário
- `POST /employees/login`  — autentica funcionário
---

## 2. Infraestrutura

- **Compute**
  - AWS Lambda (funções empacotadas via `dotnet publish` + Serverless Framework)
- **API**
  - Amazon API Gateway (HTTP endpoints)
  - Rotas configuradas em `serverless.yml`
- **Persistência**
  - DynamoDB
- **Mensageria**
  - SQS FIFO `PosTech-Orders.fifo` (para desacoplar criação e processamento de pedidos)
- **Deployment**
  - Artefatos empacotados em `.zip`, deploy via `serverless deploy --stage {dev|prod}`
  
![Texto alternativo da imagem](https://nicolas-public-assets.s3.sa-east-1.amazonaws.com/Screenshot%202025-06-23%20at%2021.00.25.png?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Content-Sha256=UNSIGNED-PAYLOAD&X-Amz-Credential=ASIA6K5V7O5XA3D2P3GI%2F20250624%2Fsa-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250624T000131Z&X-Amz-Expires=300&X-Amz-Security-Token=IQoJb3JpZ2luX2VjECgaCXNhLWVhc3QtMSJGMEQCIB63Eq3CZslKqlv9%2FZmBgMy2Mvk%2BZAOKqqA7sbBxzIAcAiBrjq1ZfQu%2B%2BRtp6xGir9N9oWHizmPMFZrEG3EpgIATrSrbAgghEAAaDDk4NTUzOTc3MDIyMiIMEIxl4AaKH6HEq4hHKrgCz15L8c710gRWksbNKcSWqvR0A91fj5%2Fnja9M4wXBNPt9FvAFQ4Q3o3fRbwaNAo4OqFM7crH789m77BnKBTq9cE%2BMWwij4voamQQO%2FkTGXOaI9XYS0%2FIBefKjO97T6o98fd3ntDkwylVBQe%2Fs4JSh45QuT40F3mj6IMW9cPMeEgDsCmvFb1I3jDFf992iVAxWQinjp3lKbs4IECNdzhCEskfyGIx63FxudAUS%2BbA3MDHtyZvOnWshNQA63dfbvFJkxfNhE6iMsV38VDT2zgy7HP%2F9AVExuG7kHU7Ad75i4g2I8ouO7NXDJC48lDHOQgHCXYEErz5ULzL3R9tH7NWf56cwAbecLgiBgBrnCbkZAG5pLHvIW9JqLKxcfkQV%2BGhc8cIUwSd2q5HIOcPcnYYWW6uuqE3X6i5jMKKL5cIGOq4CDKsN%2B8HNwEZubF9xFRAnVURc1LxYpmuGv41ZsYB%2FPco70dGO3KuOoxJl3RA36cYAsv4UKxu91A1jPNRfOReuzheSclDF%2BZD9%2Bwu5FIItKY2cW09lBtjD7590mip34fpyzknrliQTHpioN5V0Fvby1ZTHBmrJXrt9aprYX4VmQVEoDOyRsg7XH5MZjGxTvmX6yR1Kj%2FayxjbGBpGCQeqq9ULlGALnXYaYANcd9OolqSkW%2F93%2Fv4P4eYMjaXb1IPsdJe7RjLiBB%2FivugrIj6is5K74BefvWBJY4MzynTL1tPokcQXLFJ0XIG7aF%2F%2B7yr%2F473rb9jB%2Fm3hj%2BozqEyZwg6aERUVMotBsgGQqLNHzq3Pcr8mSUlxo2y3EvMMD0iTGgwPCoZsr%2FKz6gzER83A%3D&X-Amz-Signature=c81de957635aa8cefa87549f8f9a53baa8e8e178fe459aa72ffab19b64d29942&X-Amz-SignedHeaders=host&response-content-disposition=inline)

---

## 3. Monitoramento

- **Tracing distribuído**
  - AWS X-Ray habilitado em API Gateway e Lambdas
  - Subsegments automáticos para chamadas nos demais serviços da AWS
  - Visão de performance de Cold Start de cada execução Lambda e de latência End-to-End entre recursos
- **Logs**
  - CloudWatch Logs para cada função Lambda, podendo ser concultado via Query nos Logs Insights

![Trace X-Ray](https://nicolas-public-assets.s3.sa-east-1.amazonaws.com/Screenshot%202025-06-23%20at%2021.04.09.png?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Content-Sha256=UNSIGNED-PAYLOAD&X-Amz-Credential=ASIA6K5V7O5XFTRH4KML%2F20250624%2Fsa-east-1%2Fs3%2Faws4_request&X-Amz-Date=20250624T000428Z&X-Amz-Expires=300&X-Amz-Security-Token=IQoJb3JpZ2luX2VjECgaCXNhLWVhc3QtMSJHMEUCIQCPs6RNkudgEvVPO2aMKCpfMZO9Csg6WWMUu7ErvbS6bAIgD3QY4p3oG3rnWUAHfuezgRK5Nq%2BU4v1S2qzygpuNar0q2wIIIRAAGgw5ODU1Mzk3NzAyMjIiDNLmCLpecVk%2BdLl1%2BSq4AnFtJX2Y7B7w88xM1uEtrR0iaP6WSZziVQQ%2Fv%2BOwcvRBKuGAR60CFtfpOIIpeGQyRF4sVBE%2FQSigQS83YsZgcR4fURkeOO7YBkHkuO68UGGrl8%2FXTSffZXaER3d1CUXZWlcCYEmIK80Q7qqJZAaF6Fq6RsP4M8nS7nTTW9016cmGPZDgFiHVDcWkgf6X3e8wpKuNz%2FHUiXKM9mDP5%2BtMegnjiH5nksUiLG2wwV4woocG16kA3T8y5bVld23%2B0LPjjHgSSJnL0RqRr9AsjIRpbRWqvgQBYVug1nrm5XXTpuK6RFJCdXY4R%2BBpdtMVCvLgskF479luiNrkQALCkbAqyrWu29tm467d5Vkid%2FqjwxUmPDmp00CpsKOk1aLlBaruOLWAkIkyKgSXxrCdmY6fcDYQtVSBcRHL2zCii%2BXCBjqtAjR9%2BJF3eI548NSi1Irn9P7DiiP4PWaZKunY0BpCoZANyWJgvy8YccuvuTR%2F0wh3ENcizbZctEO34qQfyO%2Bq9SduglhvPDEeAmxu0VmSVplAEC6tLuQ5IHKQ5ihhQVywYUt14Jcsxtia3s9ydrpCrINkJJ%2Bgj2k64vaUYg%2ByprXJ5YwFOChqeec65OGDp6yMiQImchlPFuyY5RWFowH616hvQB%2BAmUl2PWCiro5tePoqjVfUZyUGbdYweKFYFhr86qLJhV0S1%2F2MyhuJDgFsaoeHaI%2BzK5vLVFqkq2NkX6xDC822QOjJfXGTKJ2EIFqyR5iMWdtbIykTim1V5AjXyRwdcGven7eHkvJZfX%2Fu%2F8qVMs2qQc%2BntSzo%2BrHSx96EvPcA8QqvJAeL4vTkNTo%3D&X-Amz-Signature=021dc46f27145abe962e085422e7e7d6491594ebd72c96841f6a7533286d86bc&X-Amz-SignedHeaders=host&response-content-disposition=inline)

---

## 4. CI/CD

- **CI** (GitHub Actions)
  - Workflow `CI` (`.github/workflows/ci.yml`):
    1. checkout
    2. cache NuGet
    3. setup .NET 6
    4. `dotnet restore && dotnet build`
- **CD** (GitHub Actions)
  - Workflow `CD` (`.github/workflows/cd.yml`):
    1. disparado em `push: main` ou ao terminar o `CI` com sucesso
    2. configura AWS creds via `aws-actions/configure-aws-credentials`
    3. `dotnet publish` → `zip` → `serverless deploy --stage prod` para cada stack
- **Variáveis sensíveis**
  - `AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY` no GitHub Secrets
  - `JWT_SECRET` via SSM Parameter Store (referenciado em `serverless.yml`)

---

## 5. Comandos úteis

- Comando para subir uma stack localmente:

```shell
  cd HACKATON-FIAP/src/{StackName} ➜

  dotnet publish {StackName}.csproj -c Release -r linux-x64 --no-self-contained -o publish && \
  cd publish && \
  zip -r ../function.zip . && \
  cd .. && \
  sls deploy --stage prod
```