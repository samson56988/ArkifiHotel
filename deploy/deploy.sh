#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ENV_FILE="${ROOT_DIR}/deploy/.env"

if [[ ! -f "${ENV_FILE}" ]]; then
  echo "Missing ${ENV_FILE}. Copy deploy/.env.example to deploy/.env and fill secrets."
  exit 1
fi

cd "${ROOT_DIR}"

set -a
# shellcheck disable=SC1091
source "${ENV_FILE}"
set +a

docker compose -f deploy/docker-compose.yml --env-file deploy/.env up -d --build --remove-orphans

docker compose -f deploy/docker-compose.yml ps

echo ""
echo "Deployed:"
echo "  API:            http://${PUBLIC_HOST:-31.97.116.169}:${API_PORT:-9001}"
echo "  HotelBusiness:  http://${PUBLIC_HOST:-31.97.116.169}:${BUSINESS_PORT:-9002}"
echo "  HotelCustomer:  http://${PUBLIC_HOST:-31.97.116.169}:${CUSTOMER_PORT:-9003}"
echo "  HotelAdmin:     http://${PUBLIC_HOST:-31.97.116.169}:${ADMIN_PORT:-9004}"
