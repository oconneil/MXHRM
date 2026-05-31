.PHONY: infra-up infra-down app-up app-down app-build ps logs logs-api logs-web logs-db restart-api restart-web clean prod-pull prod-up prod-down prod-ps prod-logs

infra-up:
	docker compose --profile infra up -d

infra-down:
	docker compose --profile infra down

app-up:
	docker compose --profile infra --profile app up -d --build

app-down:
	docker compose --profile infra --profile app down

app-build:
	docker compose --profile infra --profile app build

ps:
	docker compose --profile infra --profile app ps

logs:
	docker compose --profile infra --profile app logs -f

logs-api:
	docker compose --profile infra --profile app logs -f api

logs-web:
	docker compose --profile infra --profile app logs -f web

logs-db:
	docker compose --profile infra logs -f sqlserver

restart-api:
	docker compose --profile infra --profile app restart api

restart-web:
	docker compose --profile infra --profile app restart web

clean:
	docker compose --profile infra --profile app down

prod-pull:
	docker compose -f docker-compose.prod.yml pull

prod-up:
	docker compose -f docker-compose.prod.yml up -d

prod-down:
	docker compose -f docker-compose.prod.yml down

prod-ps:
	docker compose -f docker-compose.prod.yml ps

prod-logs:
	docker compose -f docker-compose.prod.yml logs -f