# Closet

AI wardrobe app: Expo (SDK 57) + Expo Router + TypeScript.

## Run it

```bash
npm install
npx expo start      # scan the QR code with Expo Go on your phone
```

## Choosing the backend

| `EXPO_PUBLIC_API_BASE_URL` | Client |
|---|---|
| unset or empty | `MockClosetApi`: in-app mock, works offline |
| set (for example `http://localhost:5099`) | `HttpClosetApi` against `<base>/api/...` |

Give the origin only, with no `/api` suffix. Expo inlines `EXPO_PUBLIC_*` variables when the bundle is built, so restart the dev server after changing it (`npx expo start -c` if the old value sticks).

```bash
EXPO_PUBLIC_API_BASE_URL=http://localhost:5099 npx expo start
```

Or put it in `mobile/.env.local` (git-ignored): `EXPO_PUBLIC_API_BASE_URL=http://192.168.1.20:5099`.

### Which address to use

`localhost` means "this device", so it only works when the app and the API run on the same machine.

| App runs on | Base URL |
|---|---|
| Web (`expo start --web`) | `http://localhost:<port>` |
| iOS simulator | `http://localhost:<port>` |
| Android emulator | `http://10.0.2.2:<port>` (the emulator's alias for the host machine) |
| Physical phone with Expo Go | `http://<your computer's LAN IP>:<port>`, on the same Wi-Fi |

Other things that trip people up:

- Bind the API to all interfaces (`http://0.0.0.0:<port>`), not only `localhost`, or phones and emulators cannot reach it.
- Allow the port through the computer's firewall for phone testing.
- Web runs in a browser, so the API needs CORS enabled for the Expo web origin (`http://localhost:8081`). The stub server does this.
- Plain `http://` works in Expo Go and development builds. Release builds block cleartext HTTP, so use HTTPS there.
- Image URLs that the API returns as `/media/...` are resolved against the same base URL.
- AI calls (analyze, style, refine, try-on) can be slow. They time out after 2 minutes, other calls after 20 seconds.

### Stub server

Until the real backend is running, `scripts/stub-server.js` serves the contract's endpoints with sample data (no dependencies):

```bash
npm run stub-server                          # http://localhost:5099
STUB_LATENCY_MS=2000 npm run stub-server     # slow responses
STUB_FAIL=style,tryOn npm run stub-server    # 502 problem+json for those calls (analyze, style, refine, tryOn)
```

## Tests

```bash
npm test               # Jest (jest-expo) + React Native Testing Library
npm run test:coverage
```

`HttpClosetApi` is tested against a mocked `fetch`.

## How it's put together

- `src/app/` — screens (Expo Router). Tabs: Today, Closet, Style, Outfits. Stack screens: `outfit/[id]`, `item/[id]`, `add` (modal), `try-on/[id]` (modal).
- `src/api/ClosetApi.ts` — **the backend contract.** Every screen goes through this interface.
- `src/api/mock/` — in-app mock that persists to device storage. `stylist.ts` is a rules-based stand-in for the outfit LLM call; `seed.ts` is the example wardrobe.
- `src/api/http/HttpClosetApi.ts` — the real client for the ASP.NET Core API (`docs/api-contract.md`).
- `src/api/index.ts` — picks the client from `EXPO_PUBLIC_API_BASE_URL` (see below).
- `src/components/garmentSvg.ts` — drawn placeholder garments for items without a photo. Items with `imageUri` show the photo.

## Calls the backend needs to provide

| Method | What it does |
|---|---|
| `analyzePhoto` | Background removal + AI tags for a new item (tagging call) |
| `styleOutfit` / `refineOutfit` | Build or change an outfit from the closet (outfit call) |
| `alternatives` / `replacePiece` | Options for one slot, and swapping it |
| `tryOn` | Render the outfit on the user's saved photo (image call). Cache per outfit. |
| `getProfile`, `setPersonPhoto`, `completeOnboarding` | First-launch setup: the full-body photo used for try-on |
| `logWear`, `listWorn`, `setSaved`, `listSaved`, `listItems`, `addItem`, `todaysOutfit`, `getContext` | Data |

To reset the example data on the mock, delete the app from the device or call `api.resetDemoData()`. With the HTTP client, `resetDemoData()` calls `POST /api/demo/reset`.
