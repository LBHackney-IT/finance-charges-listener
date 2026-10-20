# Finance Charges Listener

The Finance Charges Listener is the London Borough of Hackney (LBH) **MTFH finance** component that keeps
dwelling/block/estate **charges** in sync with the assets and tenancies held in the platform's other services.

It is an **AWS Lambda** function triggered by a **FIFO SQS queue**. It receives SNS-style domain events
published by the charges APIs, applies the corresponding charge changes in **DynamoDB**, and pushes the
resulting summaries back to the **Financial Summary API**. It also ingests the finance team's Estimate/Actual
spreadsheets from **S3** and produces the print-room CSV output.

## Stack

- **.NET 10** (`net10.0`) targeting the AWS Lambda managed runtime `dotnet10`.
- **Amazon.Lambda.SQSEvents** entry point (`Microsoft.NET.Sdk` class library published as a Lambda).
- **DynamoDB** as the datastore (via `Hackney.Core.DynamoDb` / `AWSSDK.DynamoDBv2`).
- **Amazon S3** for spreadsheet input/output, **SNS** for step chaining, **SQS** for the trigger.
- **ExcelDataReader** for `.xls`/`.xlsx` parsing, **Newtonsoft.Json** for domain (de)serialisation.
- **X-Ray** for tracing, **Serilog**/**Microsoft.Extensions.Logging** for logging.
- **xUnit** + **FluentAssertions** + **Moq** + **AutoFixture** for tests.

## How it works

The Lambda handler is `FinanceChargesListener::FinanceChargesListener.ChargesListener::FunctionHandler`.
It receives an `SQSEvent`, deserialises each record body into an `EntityEventSns` envelope and dispatches on
`EventType`. Messages are processed sequentially.

| `EventType` | Use case | Effect |
| --- | --- | --- |
| `DwellingChargeUpdatedEvent` | `UpdateChargesUseCase` | Applies `DetailedChargeChange.DifferenceAmount` to the parent-asset charges, then updates the Financial Summary totals. |
| `HeadOfChargeApplyEvent` | `ApplyHeadOfChargeUseCase` | Filters dwellings by tenure and applies head-of-charge rules (charge-name switch, tenant/leaseholder apportionment). |
| `FileUploadEvent` | `EstimateActualFileProcessUseCase` | Runs the 8-step Estimate/Actual spreadsheet import pipeline. |
| `GeneratePropertyCharges` | `GeneratePropertyChargesFileUseCase` | Selects the newest tagged Estimate/Actual spreadsheet, rebuilds per-property amounts and writes the print-room CSV. |
| *(anything else)* | — | Throws `ArgumentException`. |

Any handler exception is re-thrown so AWS retries the message and, after `maxReceiveCount` (3), moves it to
the dead-letter queue.

### Estimate/Actual spreadsheet pipeline

Each step publishes the next step back onto the same queue via SNS, so the pipeline is resumable across
Lambda invocations:

1. Scan and count existing leaseholder charges for the year/group/sub-group.
2. Delete the existing charges in paginated batches.
3. Map UH numeric property references to GUID asset ids using the Housing Search API.
4. Write dwelling charges.
5. Write block charges.
6. Write estate charges and the Hackney total.
7. Write block asset summaries to the Financial Summary API.
8. Write estate and Hackney summary totals, then tag the S3 object `status=Processed` (or `status=Failed` on error).

Spreadsheets are read from header row 0; column index 19 encodes the charge year (`20` + two digits) and the
group (third character `E`/`A` → Estimate/Actual). Data columns are hard-coded indices 0–39.

**Known limitation:** the pipeline keeps cross-invocation `static List<>` state, so it assumes a warm,
single-concurrency Lambda.

## Data sources and external dependencies

| Dependency | Detail |
| --- | --- |
| DynamoDB table `Charges` | `target_id` (HASH) + `id` (RANGE); `target_type`, `charge_group`, `charge_sub_group`, `charge_year`, `detailed_charges`, audit fields. |
| DynamoDB table `Assets` | Read-only lookups of asset data. |
| SQS `chargesqueue.fifo` | Trigger queue (FIFO, content-based dedup, 900 s visibility, KMS-encrypted). ARN published to SSM as `/sqs-queue/{stage}/charges/arn`. |
| SQS `chargesdeadletterqueue.fifo` | Dead-letter queue, `maxReceiveCount` 3. |
| SNS topic | `/sns-topic/{stage}/charges/arn` — both the event source and the target this Lambda publishes step-completion messages to. |
| S3 `lbh-charges-api-bucket-{development\|staging\|production}` | `uploads/` for incoming spreadsheets, `printoutput/` for generated CSV. Object tags `status`, `year`, `valuesType`, `fileType`. |
| S3 `mtfh-finance-printroom-{stage}` | `PRINT_ROOM_BUCKET_NAME` is passed to the function and granted in IAM, but is **not read in code** — print-room output currently goes to the charges bucket. |
| Housing Search API | `GET api/v1/search/assets/all` — resolves property references to asset ids. |
| Asset Information API | `GET api/v1/assets/assetId/{id}` and `GET api/v1/assets/{id}`. |
| Financial Summary API | `GET`/`PATCH api/v1/asset-summary/estimates/{assetId}`, `POST api/v1/asset-summary`, `POST api/v1/asset-summary/process-batch`. 5-minute client timeout. |
| AWS X-Ray | Distributed tracing. |
| Elasticsearch | **Not used directly.** NEST is only present transitively; Elasticsearch is reached over HTTP through the Housing Search API. |

## Configuration

All values are supplied per stage from SSM Parameter Store in `FinanceChargesListener/serverless.yml`;
local values live in `appsettings.LocalDevelopment.json`.

| Variable | Purpose |
| --- | --- |
| `ENVIRONMENT` | Environment name (`LocalDevelopment`, stage name). |
| `CHARGES_BUCKET_NAME` | S3 bucket for spreadsheet input/output. |
| `CHARGES_SNS_ARN` | SNS topic for events and step chaining. |
| `HOUSING_SEARCH_API_URL` / `HOUSING_SEARCH_API_TOKEN` | Housing Search API client. |
| `ASSET_INFORMATION_API_URL` / `ASSET_INFORMATION_API_TOKEN` | Asset Information API client. |
| `FINANCIAL_SUMMARY_API_URL` / `FINANCIAL_SUMMARY_API_TOKEN` | Financial Summary API client. |
| `PRINT_RENT_STATEMENTS_HEADER` | Header row for the generated print-room CSV. |
| `DynamoDb_LocalMode`, `DynamoDb_LocalServiceUrl` | Local DynamoDB (`true`, `http://localhost:8000`). |

AWS region is `eu-west-2` throughout.

## Setup

### Prerequisites

1. [Docker][docker-download] with the **`docker compose`** v2 plugin (the v1 `docker-compose` binary is not used).
2. The .NET 10 SDK (only needed for IDE work — build and test run in Docker).
3. A **`LBHPACKAGESTOKEN`** with `read:packages` access to the `LBHackney-IT` GitHub Packages NuGet feed.
   The `Hackney.*` packages this service depends on are not on nuget.org. Export it before running any
   Docker target, for example:
   ```sh
   export LBHPACKAGESTOKEN=<token>
   ```
4. Optionally copy [`.env.sample`](.env.sample) to `.env` and set `LBHPACKAGESTOKEN` there instead of
   exporting it — `docker compose` reads `.env` for the values referenced in `docker-compose.yml`. The file
   also lists every environment variable the function reads at runtime; those are **not** loaded from `.env`,
   so export them (or add them to the compose service) if you need to override the defaults.

### Clone and build

```sh
git clone git@github.com:LBHackney-IT/finance-charges-listener.git
cd finance-charges-listener
make build          # docker compose build finance-charges-listener
```

### Run locally

```sh
make serve          # builds and starts the Lambda container on port 3000
make shell          # bash inside the container
```

A local DynamoDB instance is provided by the `dynamodb-database` service in `docker-compose.yml`
(`amazon/dynamodb-local` on port 8000).

## Testing

```sh
make test
```

`make test` starts the local DynamoDB container, builds the test image
(`FinanceChargesListener.Tests/Dockerfile`) and runs the xUnit suite inside it. The suite currently covers
configuration/serialisation and exception handling; the E2E "story" classes are fixtures and step
definitions rather than executed tests.

### Agreed testing approach

- xUnit, FluentAssertions and Moq.
- Follow a TDD approach; keep tests independent of each other.
- Gateway tests interact with a real local DynamoDB instance.
- Test coverage should not go down.
- Unit and E2E tests run in CI.

## Deployment

CircleCI (`.circleci/config.yml`) gates deployment on a code-formatting check (`dotnet-format --check`), the
Docker test suite and SonarCloud analysis, then deploys infrastructure with **Terraform** and the Lambda with
the **Serverless Framework**. Changes are reviewed and approved on a branch before merging.

**Stages and triggers:**

- **Feature branches** (anything except `development` and `master`) — formatting, tests and Terraform **plan
  previews** against development, staging and production. Nothing is applied or deployed.
- **`development`** — formatting, tests, Terraform apply to development, then deployment to **development**.
- **`master`** — formatting, tests, Terraform apply to staging and deployment to **staging**. Production
  follows only after the `permit-production-terraform-release` and `permit-production-release` manual approvals.

Note that the development deployment is driven by the `development` branch, and that this repository has **no
pre-production stage**.

**Packaging and runtime:**

- `build.sh` installs `Amazon.Lambda.Tools` and runs
  `dotnet lambda package --configuration release --framework net10.0 --output-package ./bin/release/net10.0/finance-charges-listener.zip`.
  (`build.cmd` is the Windows equivalent for local use.)
- The deploy job runs `npx --yes serverless deploy --stage <stage> --conceal`.
- `serverless.yml` declares `runtime: dotnet10` and consumes that artifact. All scheduled invocations are
  disabled by default — they are driven by the step functions and the `chargesqueue.fifo` trigger.
- `aws-lambda-tools-defaults.json` holds the Visual Studio / `dotnet lambda` defaults (framework, runtime,
  handler, memory, timeout).

## Contacts

**Housing product team** — [mmh-project-team@hackney.gov.uk](mailto:mmh-project-team@hackney.gov.uk)

[docker-download]: https://www.docker.com/products/docker-desktop
[AWS-CLI]: https://aws.amazon.com/cli/
