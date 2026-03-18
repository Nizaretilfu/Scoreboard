.PHONY: restore build test check run-api ef-update

restore:
	./scripts/dev.sh restore

build:
	./scripts/dev.sh build

test:
	./scripts/dev.sh test

check:
	./scripts/dev.sh check

run-api:
	./scripts/dev.sh run-api

ef-update:
	./scripts/dev.sh ef-update
