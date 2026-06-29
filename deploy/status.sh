#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
docker compose -f deploy/docker-compose.yml ps
echo ""
docker compose -f deploy/docker-compose.yml logs api --tail 40
