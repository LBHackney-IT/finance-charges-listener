.PHONY: setup
setup:
	docker-compose build

.PHONY: build
build:
	docker-compose build finance-charges-listener

.PHONY: serve
serve:
	docker-compose build finance-charges-listener && docker-compose up finance-charges-listener

.PHONY: shell
shell:
	docker-compose run finance-charges-listener bash

.PHONY: test
test:
	docker-compose up dynamodb-database & docker-compose build finance-charges-listener-test && docker-compose up finance-charges-listener-test

.PHONY: lint
lint:
	-dotnet tool install -g dotnet-format
	dotnet tool update -g dotnet-format
	dotnet format

.PHONY: restart-db
restart-db:
	docker stop $$(docker ps -q --filter ancestor=dynamodb-database -a)
	-docker rm $$(docker ps -q --filter ancestor=dynamodb-database -a)
	docker rmi dynamodb-database
	docker-compose up -d dynamodb-database
