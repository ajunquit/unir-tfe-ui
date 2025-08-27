# Makefile mínimo — solo lo esencial
SHELL := bash
.ONESHELL:
.SHELLFLAGS := -eu -o pipefail -c

# Target por defecto. Ejecuta: `make`
.PHONY: default
default:
	@echo "Makefile mínimo listo. Agrega tus targets aquí."
	
update-submodules:
	git submodule sync --recursive
	git submodule update --init --recursive --remote --force