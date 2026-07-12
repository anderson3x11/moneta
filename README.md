# Moneta

Application de facturation qui génère des factures au format **Factur-X** : un PDF/A-3 lisible avec le XML structuré (CII, norme EN 16931) embarqué. C'est le format hybride retenu par la réforme française de la facturation électronique.

Le projet couvre une chaîne complète et réaliste : API REST sécurisée, front web, persistance, calcul de TVA multi-taux et génération du document conforme.

## Fonctionnalités

- Gestion des clients et des factures (brouillon, émission, paiement, annulation)
- Calcul de la TVA par taux (20 %, 10 %, 5,5 %, 2,1 %, exonéré) selon les règles de l'EN 16931
- Numérotation séquentielle des factures par année
- Génération d'une facture Factur-X : rendu PDF et XML CII embarqué
- Authentification par JWT, mots de passe hachés (BCrypt)
- Validation des entrées, y compris le contrôle du SIRET (clé de Luhn)

## Architecture

Le code suit une architecture en couches (Clean Architecture) pour isoler le métier des détails techniques.

```
src/
  Moneta.Domain          Entités, objets valeur, règles métier (aucune dépendance technique)
  Moneta.Application      Cas d'usage, contrats (DTO), interfaces, validation
  Moneta.Infrastructure   EF Core, sécurité, génération Factur-X (implémentations)
  Moneta.Api              API REST ASP.NET Core (contrôleurs, JWT, OpenAPI)
  Moneta.Web              Front Blazor (consomme l'API)
tests/
  Moneta.Domain.Tests         Tests unitaires du domaine (TVA, SIRET, cycle de vie)
  Moneta.Application.Tests    Tests d'intégration des services et du Factur-X
```

Les dépendances pointent toujours vers le domaine : l'API et l'infrastructure connaissent l'application, jamais l'inverse.

## Stack technique

- .NET 10, C#
- ASP.NET Core (Web API) et Blazor (Server)
- Entity Framework Core avec SQLite
- QuestPDF pour le rendu PDF/A-3
- FluentValidation, JWT, BCrypt
- xUnit pour les tests

SQLite est utilisé pour rester sans configuration. Le passage à SQL Server ou PostgreSQL se limite au fournisseur EF Core et à la chaîne de connexion.

## Démarrage

Prérequis : le SDK .NET 10.

```bash
# Restaurer les outils (EF Core) et compiler
dotnet tool restore
dotnet build
```

Lancer l'API puis le front dans deux terminaux :

```bash
dotnet run --project src/Moneta.Api    # http://localhost:5181
dotnet run --project src/Moneta.Web    # http://localhost:5056
```

La base est créée et alimentée au premier démarrage de l'API (un vendeur, un client et une facture d'exemple).

Compte de démonstration :

```
demo@moneta.fr / Password123!
```

La documentation de l'API (OpenAPI) est disponible sur `http://localhost:5181/scalar/v1`.

## Le format Factur-X

Une facture émise peut être exportée en Factur-X depuis le front ou via l'API :

```
GET /api/invoices/{id}/facturx
```

Le PDF produit est déclaré en PDF/A-3 et contient le fichier `factur-x.xml` en pièce jointe, avec la relation `Alternative` prévue par la spécification. Le XML suit le modèle CII et le profil EN 16931 (`urn:cen.eu:en16931:2017`), avec les parties vendeur et acheteur, les lignes, la ventilation de TVA et les totaux.

## Tests

```bash
dotnet test
```

Les tests couvrent le calcul de TVA (ventilation par taux, arrondis), la validation du SIRET, le cycle de vie d'une facture, la numérotation, l'authentification et la génération du Factur-X (présence du XML dans le PDF et cohérence des montants).

## Périmètre

Il s'agit d'un projet de démonstration. Quelques choix assumés :

- une seule fiche vendeur, gérée par l'application
- la clé de signature JWT fournie dans `appsettings.json` est un placeholder de développement, à remplacer par une variable d'environnement ou un secret en production
- le schéma d'extension XMP propre à Factur-X n'est pas encore émis ; un validateur strict pourra donc signaler ce point, même si la structure du PDF et du XML est en place
