.PHONY: start
start:
	$(info Make: Starting containers.)
	@docker compose up -d

.PHONY: stop
stop:
	$(info Make: Stopping containers.)
	@docker compose down

.PHONY: server
server:
	$(info Make: Starting labelbox assessment service)
	@docker compose up -d

.PHONY: test-e2e
test-e2e:
	$(info Make: Running all unit tests)
	@docker exec -it labelbox-tests dotnet test

.PHONY: test
test:
	$(info Make: Running tests matching test name $(TESTFILTER))
	@make -s start
	@docker exec -it labelbox-tests dotnet test --filter $(TESTFILTER)