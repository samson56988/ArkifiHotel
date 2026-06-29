#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "${ROOT_DIR}"

echo "Deploying ArkifiStay (config: appsettings.json + appsettings.Production.json)"

docker compose -f deploy/docker-compose.yml up -d --build --remove-orphans

docker compose -f deploy/docker-compose.yml ps

echo ""
echo "Deployed:"
echo "  API:            http://31.97.116.169:9001"
echo "  HotelBusiness:  http://31.97.116.169:9002"
echo "  HotelCustomer:  http://31.97.116.169:9003"
echo "  HotelAdmin:     http://31.97.116.169:9004"
