const ABSOLUTE_URL = /^([a-z][a-z0-9+.-]*:|\/\/)/i;

export function stripTrailingSlashes(url: string): string {
  return url.trim().replace(/\/+$/, '');
}

export function resolveMediaUrl(baseUrl: string, uri: string | null | undefined): string | undefined {
  if (!uri) return undefined;
  if (ABSOLUTE_URL.test(uri)) return uri;
  return `${stripTrailingSlashes(baseUrl)}/${uri.replace(/^\/+/, '')}`;
}
