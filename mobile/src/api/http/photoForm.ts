import { Platform } from 'react-native';

export const PHOTO_FIELD = 'photo';

const MIME_BY_EXTENSION: Record<string, string> = { jpg: 'image/jpeg', jpeg: 'image/jpeg', png: 'image/png' };

function extensionOf(uri: string): string {
  const path = uri.split(/[?#]/)[0];
  return path.slice(path.lastIndexOf('.') + 1).toLowerCase();
}

function mimeFromDataUri(uri: string): string | undefined {
  return /^data:([^;,]+)/i.exec(uri)?.[1];
}

export function mimeTypeOf(uri: string): string {
  return mimeFromDataUri(uri) ?? MIME_BY_EXTENSION[extensionOf(uri)] ?? 'image/jpeg';
}

function fileNameFor(mimeType: string): string {
  return mimeType === 'image/png' ? 'photo.png' : 'photo.jpg';
}

/** React Native's FormData accepts a `{ uri, name, type }` descriptor in place of a Blob. */
function nativeFilePart(uri: string) {
  const type = mimeTypeOf(uri);
  return { uri, name: fileNameFor(type), type };
}

async function appendWebBlob(form: FormData, uri: string, fetchImpl: typeof fetch) {
  const blob = await (await fetchImpl(uri)).blob();
  const type = blob.type || mimeTypeOf(uri);
  form.append(PHOTO_FIELD, blob, fileNameFor(type));
}

export async function buildPhotoForm(uri: string, fetchImpl: typeof fetch): Promise<FormData> {
  const form = new FormData();
  if (Platform.OS === 'web') await appendWebBlob(form, uri, fetchImpl);
  else form.append(PHOTO_FIELD, nativeFilePart(uri) as unknown as Blob);
  return form;
}
