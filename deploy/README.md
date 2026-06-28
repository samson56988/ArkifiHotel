# ArkifiStay VPS deployment (Docker + GitHub Actions)

Deploy all four apps to **31.97.116.169** on these ports:

| Service | Port | URL |
|---------|------|-----|
| **API** (`ArkifiHotel`) | 9001 | http://31.97.116.169:9001 |
| **HotelBusiness** | 9002 | http://31.97.116.169:9002 |
| **HotelCustomer** | 9003 | http://31.97.116.169:9003 |
| **HotelAdmin** | 9004 | http://31.97.116.169:9004 |

PostgreSQL stays on Aiven (or your own host) — only the app containers run on the VPS.

---

## What to commit to Git

These files are already in the repo:

```
deploy/
  docker-compose.yml      # orchestrates all 4 services
  Dockerfile.api          # .NET API image
  Dockerfile.angular      # shared Angular + nginx image
  nginx-spa.conf          # SPA routing for Angular apps
  .env.example            # template for server secrets
  deploy.sh               # one-command deploy script
.github/workflows/deploy.yml
ArkifiHotel/appsettings.Production.json.example
HotelBusiness|HotelCustomer|HotelAdmin/src/environments/
```

**Do NOT commit:**

- `deploy/.env` (real secrets)
- `ArkifiHotel/appsettings.Production.json` with passwords
- `ArkifiHotel/appsettings.json` if it contains live credentials — use env vars on the server instead

---

## One-time server setup (31.97.116.169)

SSH into the server as root or a deploy user:

```bash
# 1. Install Docker
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER
# log out and back in

# 2. Open firewall ports
sudo ufw allow 22
sudo ufw allow 9001/tcp
sudo ufw allow 9002/tcp
sudo ufw allow 9003/tcp
sudo ufw allow 9004/tcp
sudo ufw enable

# 3. Clone the repo
mkdir -p ~/arkifihotel
cd ~/arkifihotel
git clone https://github.com/YOUR_ORG/ArkifiHotel.git .

# 4. Create production env file
cp deploy/.env.example deploy/.env
nano deploy/.env   # fill ConnectionStrings, Jwt__Secret, SMTP, Paystack, etc.

# 5. First deploy
chmod +x deploy/deploy.sh
./deploy/deploy.sh
```

Verify:

```bash
curl http://31.97.116.169:9001/
docker compose -f deploy/docker-compose.yml ps
docker compose -f deploy/docker-compose.yml logs -f api
```

---

## GitHub Actions CI/CD

### Required GitHub repository secrets

| Secret | Example |
|--------|---------|
| `SSH_HOST` | `31.97.116.169` |
| `SSH_USER` | `ubuntu` or `root` |
| `SSH_PRIVATE_KEY` | contents of your deploy private key |
| `SSH_PORT` | `22` (optional) |
| `DEPLOY_PATH` | `/home/ubuntu/arkifihotel` (optional) |

### Flow

1. Push to `main` or `master`
2. GitHub Actions SSHs into the VPS
3. `git pull` + `./deploy/deploy.sh`
4. Docker rebuilds images and restarts containers
5. API auto-runs EF migrations on startup (Production only)

### Generate a deploy SSH key

On your laptop:

```bash
ssh-keygen -t ed25519 -C "github-deploy-arkifistay" -f ~/.ssh/arkifistay_deploy
```

On the server (`~/.ssh/authorized_keys`):

```
# paste contents of arkifistay_deploy.pub
```

In GitHub → Settings → Secrets → `SSH_PRIVATE_KEY`:

```
# paste contents of arkifistay_deploy (private key)
```

---

## Manual redeploy on server

```bash
cd ~/arkifihotel
git pull
./deploy/deploy.sh
```

---

## Changing URLs or ports

Edit `deploy/.env` on the server:

```env
API_PORT=9001
BUSINESS_PORT=9002
CUSTOMER_PORT=9003
ADMIN_PORT=9004
API_BASE_URL=http://31.97.116.169:9001
CustomerApp__BaseUrl=http://31.97.116.169:9003
Paystack__CallbackUrl=http://31.97.116.169:9002/subscription
```

Then redeploy. Angular apps bake `API_BASE_URL` at **build time**, so run `./deploy/deploy.sh` (rebuild) after changing it.

---

## Uploads persistence

Business logos and images are stored in Docker volume `api_uploads`. Back it up periodically:

```bash
docker run --rm -v arkifistay_api_uploads:/data -v $(pwd):/backup alpine \
  tar czf /backup/api-uploads-backup.tar.gz -C /data .
```

---

## Troubleshooting

| Issue | Fix |
|-------|-----|
| Angular can't reach API | Check `API_BASE_URL` in `deploy/.env`, rebuild frontends |
| 502 / connection refused | `docker compose -f deploy/docker-compose.yml logs api` |
| DB connection failed | Verify `ConnectionStrings__DefaultConnection` in `deploy/.env` |
| CORS errors | API allows all origins already |
| Paystack redirect wrong | Set `Paystack__CallbackUrl` to business app `/subscription` URL |

---

## Later: custom domains + HTTPS

Point DNS to the VPS and put **nginx** or **Caddy** in front on ports 80/443, proxying to 9001–9004. Until then, IP:port URLs work for testing.
