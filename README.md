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
  
![Texto alternativo da imagem](https://nicolas-public-assets.s3.sa-east-1.amazonaws.com/Screenshot+2025-06-23+at+21.00.25.png)

---

## 3. Monitoramento

- **Tracing distribuído**
  - AWS X-Ray habilitado em API Gateway e Lambdas
  - Subsegments automáticos para chamadas nos demais serviços da AWS
  - Visão de performance de Cold Start de cada execução Lambda e de latência End-to-End entre recursos
- **Logs**
  - CloudWatch Logs para cada função Lambda, podendo ser concultado via Query nos Logs Insights

![Trace X-Ray](https://nicolas-public-assets.s3.sa-east-1.amazonaws.com/Screenshot+2025-06-23+at+21.04.09.png)

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