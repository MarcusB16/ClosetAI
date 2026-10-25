# ClosetAI API contract (v1)

Source of truth for the backend and the mobile `HttpClosetApi`. If an endpoint or shape needs to change, change this file first.

The mobile `ClosetApi` interface (`mobile/src/api/ClosetApi.ts`) maps 1:1 onto these endpoints.

## Conventions

- REST for entity endpoints (plural nouns, HTTP verbs, 201 + `Location` on create, 204 for no content). RPC-style verb routes only for agentic actions: `outfits/style`, `outfits/{id}/refine`, `outfits/{id}/reject`, `items/analyze`, `outfits/{id}/try-on`, `demo/reset`.
- Base path `/api`. JSON bodies, `camelCase` property names.
- Single user for v1: no authentication. The "current user" is implicit. Auth is a later iteration and will not change these shapes.
- Enums serialize as `camelCase` strings (`"top"`, `"shirts"`, `"needsPhoto"`).
- Dates: `DateOnly` as `yyyy-MM-dd`; timestamps as ISO 8601 with offset.
- IDs are opaque strings.
- Errors use RFC 9457 `application/problem+json`. Unknown id → 404. Invalid body → 400 with field errors. AI provider failure → 502.
- Photos are uploaded as `multipart/form-data` (field name `photo`, JPEG or PNG, max 10 MB). Any `*Uri` field in a response is a URL the app can load directly, served by `GET /media/{fileName}`.
- Swagger UI at `/swagger` (Development). Every endpoint has a summary, documented response codes and example payloads; endpoints are grouped by tag below.

## Shared types

| Type | Shape |
|---|---|
| `Slot` | `outer` \| `top` \| `bottom` \| `shoes` \| `acc` |
| `Category` | `shirts` \| `pants` \| `jackets` \| `shoes` \| `accessories` \| `formal` |
| `Item` | `id, name, brand, category, slot, type, color, colorName, formality, fabric, size, timesWorn, addedAt` + optional `imageUri, variant{string:string}, price, lastWorn, keywords[], rainOk, noRain, seasons[]` |
| `NewItem` | `Item` without `id, timesWorn, lastWorn, addedAt` |
| `OutfitPieces` | `{ outer?: itemId, top: itemId, bottom: itemId, shoes: itemId, acc: itemId[] }` |
| `Outfit` | `{ id, title, prompt?, note?, pieces: OutfitPieces, saved, createdAt }` |
| `WearEntry` | `{ date, outfitId }` |
| `StyleContext` | `{ date, weekday, tempF, forecast, rain }` |
| `Profile` | `{ onboarded, personPhotoUri? }` |
| `ItemSuggestion` | `{ name, brand?, category, colorName, color, formality, seasons[], fabric?, cutoutUri, slot?, type? }` |
| `TryOnResult` | `{ status: ready \| needsPhoto \| failed, imageUri?, placeholder?, message? }` |

`formality` is an integer: 1 casual, 2 smart, 3 formal. (The backend DTOs and entity currently use `double`; change them to `int`.)
`type` is a garment type string (`tee`, `shirt`, `jeans`, `sneaker`, ...; full list in `mobile/src/domain/types.ts` `GarmentType`). The backend validates it against that list.

## Endpoints

### Context
| Method & route | Request | Response | Notes |
|---|---|---|---|
| `GET /api/context` | none | `StyleContext` | Today's date, weekday, weather. Real weather is out of scope for v1: return a seeded value. |

### Profile
| Method & route | Request | Response | Notes |
|---|---|---|---|
| `GET /api/profile` | none | `Profile` | |
| `PUT /api/profile/person-photo` | multipart `photo` | `Profile` | Full-body photo used for try-on. |
| `DELETE /api/profile/person-photo` | none | `Profile` | Mobile `setPersonPhoto(null)`. |
| `PATCH /api/profile` | `{ onboarded?: bool }` | `Profile` | Mobile `completeOnboarding()` sends `{ onboarded: true }`. |

### Items
| Method & route | Request | Response | Notes |
|---|---|---|---|
| `GET /api/items` | none | `Item[]` | |
| `POST /api/items` | `NewItem` | `201 Item` | `Location` header to the item. |
| `POST /api/items/analyze` | multipart `photo` | `ItemSuggestion` | Background removal plus AI tagging. Does not save anything. The user confirms the suggestion, then the app calls `POST /api/items`. |

### Outfits
| Method & route | Request | Response | Notes |
|---|---|---|---|
| `GET /api/outfits/today` | none | `Outfit` | Generated on first call each day, then stable for that day. |
| `GET /api/outfits/saved` | none | `Outfit[]` | |
| `GET /api/outfits/{id}` | none | `Outfit` | |
| `POST /api/outfits/style` | `{ prompt, lockItemId?, avoidRecent? }` | `201 Outfit` | AI outfit built only from the user's closet. |
| `POST /api/outfits/{id}/refine` | `{ instruction }` | `Outfit` | Free-text change, e.g. "different shoes". |
| `GET /api/outfits/{id}/alternatives?slot=shoes` | none | `Item[]` | Closet items that fit that slot. |
| `PUT /api/outfits/{id}/pieces/{slot}` | `{ itemId }` | `Outfit` | Swap one piece. 400 if the item does not belong to that slot. |
| `POST /api/outfits/{id}/reject` | none | `Outfit` | Returns the replacement outfit. |
| `PUT /api/outfits/{id}/saved` | `{ saved: bool }` | `Outfit` | |

### Wears
| Method & route | Request | Response | Notes |
|---|---|---|---|
| `POST /api/outfits/{id}/wears` | none | `201 WearEntry` | Logs today's date; increments `timesWorn` and `lastWorn` on each piece. |
| `GET /api/wears` | none | `WornOutfit[]` (`{ date, outfitId, outfit }`) | Newest first. |

### Try-on
| Method & route | Request | Response | Notes |
|---|---|---|---|
| `POST /api/outfits/{id}/try-on` | none | `TryOnResult` | Synchronous in v1. `needsPhoto` if the profile has no person photo. Results are cached per outfit pieces plus person photo. `placeholder: true` while the image model is stubbed. |

### Demo
| Method & route | Request | Response | Notes |
|---|---|---|---|
| `POST /api/demo/reset` | none | `204` | Reloads `Infrastructure/Seed/demo-closet.json`. Development only. |

## Backend requirements beyond the endpoints

- **Swagger**: Swashbuckle or `Microsoft.AspNetCore.OpenApi` plus Scalar/Swagger UI. XML summaries are not allowed by the style rules, so use `[EndpointSummary]`, `[ProducesResponseType]` and `Tags` attributes, with example payloads. Enums shown as strings.
- **Docker**: multi-stage `Dockerfile` for the API, and `docker-compose.yml` at the repo root running the API plus a database container. Configuration via environment variables; no secrets in the image.
- **Database**: EF Core with migrations applied on startup in Development. The provider is chosen by configuration; see the open decision below.
- **Config**: the Anthropic API key and image-model key come from user-secrets or environment variables, never from `appsettings.json`.
- **Tests**: one test project in `backend/tests/`; AI and image clients are behind interfaces so tests run with fakes.

## Out of scope for v1

Authentication and multiple users, real weather, push notifications, async try-on with polling, production hosting.

## Open decisions

1. Decided: SQL Server in a local container (`platform: linux/amd64` on Apple Silicon). Replace the SQLite/Npgsql package references with `Microsoft.EntityFrameworkCore.SqlServer`.
2. Decided: background removal is stubbed in v1; `/api/items/analyze` returns the original photo as `cutoutUri`. Category, colour and tags come from a real Claude vision call behind an interface.
3. Decided: try-on is stubbed in v1 (`placeholder: true`) behind an interface. Outfit styling and refinement use real Claude calls behind an interface, with a deterministic fake when no API key is configured.
