# TractorTracker

Suivi GPS d'un tracteur via un tracker Ticatag. Affiche la position actuelle, l'historique des déplacements sur une carte et les heures travaillées par jour.

## Fonctionnalités

- **Position actuelle** — mise à jour en temps réel via webhook Ticatag
- **Historique** — tracé des déplacements sur carte OpenLayers, filtrable par journée (sliders 15 min) ou intervalle de dates
- **Heures travaillées** — graphique bar Chart.js des heures par jour
- **Notifications push** — alerte navigateur dès que le tracteur reprend du service après 4h d'inactivité

## Stack technique

| Couche | Technologie |
|---|---|
| Backend | C# / .NET 10, ASP.NET Core Web API |
| ORM | Entity Framework Core + PostGIS (NetTopologySuite) |
| Base de données | PostgreSQL + PostGIS (conteneur `cedpich-postgis-db`) |
| Frontend | Vue 3, TypeScript, PrimeVue, OpenLayers, Chart.js |
| Infra | Docker, Docker Compose, nginx-proxy (SSL automatique) |

Architecture Clean : **Domain → Application → Infrastructure → Api**

## Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Conteneur PostGIS `cedpich-postgis-db` en cours d'exécution sur le réseau Docker `cedpich-db`
- Tracker Ticatag configuré pour envoyer ses événements en webhook

## Développement local

### 1. Configuration

```bash
cp .env.example .env
```

Renseigner dans `.env` :

| Variable | Valeur |
|---|---|
| `DB_NAME` | `tractor_tracker` |
| `DB_USER` | valeur de `DB_USER_TRACTOR_TRACKER` dans `D:\Docker\PostGIS\.env` |
| `DB_PASSWORD` | valeur de `DB_PASS_TRACTOR_TRACKER` dans `D:\Docker\PostGIS\.env` |

Copier `src/TractorTracker.Api/appsettings.json` et créer `appsettings.Development.json` avec la chaîne de connexion locale :

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=tractor_tracker;Username=...;Password=..."
  }
}
```

### 2. Migrations EF

À effectuer une seule fois, ou après chaque modification du modèle :

```bash
dotnet ef migrations add Initial \
  -p src/TractorTracker.Infrastructure \
  -s src/TractorTracker.Api
```

Les migrations sont appliquées **automatiquement au démarrage** de l'API.

### 3. Lancer le backend

```bash
dotnet run --project src/TractorTracker.Api
# API disponible sur http://localhost:5204
# OpenAPI : http://localhost:5204/openapi/v1.json
```

> En dev, CORS est ouvert sur toutes les origines. En prod, seul `VIRTUAL_HOST` est autorisé.

### 4. Lancer le frontend

```bash
cd frontend
copy .env.example .env   # VITE_API_URL=http://localhost:5204
npm install
npm run dev
# Frontend sur http://localhost:5173
```

### 5. Simuler un webhook Ticatag en local

```bash
curl -X POST http://localhost:5204/api/webhook/ticatag \
  -H "Content-Type: application/json" \
  -d '{
    "hook_event": "location_changed",
    "device": { "name": "Mon tracteur", "serial_number": "861327082960092" },
    "event": {
      "name": "location",
      "timestamp": "2024-06-08T10:30:00Z",
      "latitude": 48.759,
      "longitude": -3.459
    }
  }'
```

## Déploiement (VPS)

### Première mise en production

```bash
# 1. Cloner le dépôt sur le VPS
git clone https://github.com/CedPich/CedPich.WebApps.TractorTracker.git /opt/tractortracker
cd /opt/tractortracker

# 2. Créer et renseigner le .env
cp .env.example .env
nano .env
```

Variables requises en production :

| Variable | Description |
|---|---|
| `VIRTUAL_HOST` | Domaine public, ex. `tracker.cedpich.fr` |
| `LETSENCRYPT_EMAIL` | Email pour le certificat SSL Let's Encrypt |
| `DB_NAME` | Nom de la base de données |
| `DB_USER` | Utilisateur PostgreSQL dédié |
| `DB_PASSWORD` | Mot de passe PostgreSQL |
| `VAPID_SUBJECT` | Email de contact VAPID, ex. `mailto:ton@email.fr` |
| `VAPID_PUBLIC_KEY` | Clé publique VAPID (générer ci-dessous) |
| `VAPID_PRIVATE_KEY` | Clé privée VAPID (générer ci-dessous) |

**Générer les clés VAPID (une seule fois, ne jamais les changer ensuite) :**

```bash
npx web-push generate-vapid-keys
```

Copier les deux clés dans le `.env`. Si elles sont régénérées, toutes les souscriptions push existantes deviennent invalides.

```bash
# 3. Build et démarrage
docker compose build
docker compose up -d
```

### Mises à jour

```bash
cd /opt/tractortracker
git pull
docker compose build
docker compose up -d
docker image prune -f
```

### Architecture Docker

```
nginx-proxy (SSL Let's Encrypt automatique)
  └── tractortracker-frontend [nginx-proxy + tractortracker-internal]
        ├── /          → Vue 3 (fichiers statiques)
        └── /api/      → tractortracker-api [tractortracker-internal + cedpich-db]
                              └── cedpich-postgis-db
```

## Configuration Ticatag

Dans l'interface Ticatag, configurer le webhook avec :

- **URL** : `https://tracker.ton-domaine.fr/api/webhook/ticatag`
- **Méthode** : `POST`
- **Événement** : `location_changed`

## Notifications push

Les notifications sont envoyées automatiquement au navigateur lorsque le tracteur reprend du service après plus de 4h d'inactivité.

Pour les recevoir, ouvrir le dashboard depuis le navigateur cible et accepter la demande de permission. La souscription est alors enregistrée en base.

Pour vérifier que la clé VAPID est bien exposée :

```bash
curl https://tracker.ton-domaine.fr/api/push/vapid-public-key
```

> Les notifications push nécessitent HTTPS. Elles ne fonctionnent pas en développement local sans configuration particulière.

## Ajouter la machine en base

Après le premier démarrage, insérer la machine avec le `serial_number` figurant dans l'interface Ticatag :

```sql
INSERT INTO "Machines" ("Id", "Name", "TrackerDeviceId")
VALUES (gen_random_uuid(), 'Mon tracteur', '861327082960092');
```

Récupérer l'UUID généré et le placer dans `MACHINE_ID` du `.env`.
