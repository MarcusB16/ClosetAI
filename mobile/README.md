# Closet

AI wardrobe app: Expo (SDK 57) + Expo Router + TypeScript.

## Run it

```bash
npm install
npx expo start      # scan the QR code with Expo Go on your phone
```

## How it's put together

- `src/app/` — screens (Expo Router). Tabs: Today, Closet, Style, Outfits. Stack screens: `outfit/[id]`, `item/[id]`, `add` (modal), `try-on/[id]` (modal).
- `src/api/ClosetApi.ts` — **the backend contract.** Every screen goes through this interface.
- `src/api/mock/` — in-app mock that persists to device storage. `stylist.ts` is a rules-based stand-in for the outfit LLM call; `seed.ts` is the example wardrobe.
- `src/api/index.ts` — swap `MockClosetApi` for an HTTP client when the backend exists.
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

To reset the example data, delete the app from the device or call `api.resetDemoData()`.
