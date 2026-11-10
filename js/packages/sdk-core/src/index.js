/** @typedef {{ baseUrl: string, tenantHost?: string, origin?: string, fetch?: typeof fetch, cookies?: Map<string, string> }} ClaviClientOptions */

function env(name) {
  try {
    return typeof process !== 'undefined' ? process.env?.[name] : undefined;
  } catch {
    return undefined;
  }
}

/**
 * @param {Partial<ClaviClientOptions>} [partial]
 * @returns {ClaviClientOptions}
 */
export function createOptions(partial = {}) {
  const fromEnv = {
    baseUrl: env('CLAVI_BASE_URL'),
    tenantHost: env('CLAVI_TENANT_HOST'),
    origin: env('CLAVI_ORIGIN'),
  };
  const baseUrl = (partial.baseUrl ?? fromEnv.baseUrl ?? '').replace(/\/?$/, '/');
  if (!baseUrl || baseUrl === '/') {
    throw new Error('baseUrl / CLAVI_BASE_URL is required');
  }
  const url = new URL(baseUrl);
  return {
    baseUrl,
    tenantHost: partial.tenantHost ?? fromEnv.tenantHost ?? url.host,
    origin: partial.origin ?? fromEnv.origin ?? url.origin,
    fetch: partial.fetch ?? globalThis.fetch.bind(globalThis),
    cookies: partial.cookies ?? new Map(),
  };
}

/**
 * @param {ClaviClientOptions} options
 * @param {string} path
 * @param {RequestInit} [init]
 */
export async function claviFetch(options, path, init = {}) {
  const headers = new Headers(init.headers);
  if (!headers.has('Origin')) headers.set('Origin', options.origin);
  // fetch forbids Host; point baseUrl at the tenant host (or inject via custom fetch).

  const cookie = [...options.cookies.entries()].map(([k, v]) => `${k}=${v}`).join('; ');
  if (cookie && !headers.has('Cookie')) headers.set('Cookie', cookie);

  const url = new URL(path.replace(/^\//, ''), options.baseUrl);
  const res = await options.fetch(url, {
    ...init,
    headers,
    credentials: init.credentials ?? 'include',
  });

  const setCookie = typeof res.headers.getSetCookie === 'function'
    ? res.headers.getSetCookie()
    : [];
  const single = res.headers.get('set-cookie');
  const raw = setCookie.length ? setCookie : single ? [single] : [];
  for (const line of raw) {
    const pair = line.split(';', 1)[0];
    const i = pair.indexOf('=');
    if (i > 0) options.cookies.set(pair.slice(0, i), pair.slice(i + 1));
  }

  if (!res.ok) {
    const body = await res.text();
    let code;
    try {
      const j = JSON.parse(body);
      code = j.code ?? j.extensions?.code;
    } catch {
      /* ignore */
    }
    const err = new Error(`Clavi HTTP ${res.status}${code ? ` [${code}]` : ''}: ${body.slice(0, 200)}`);
    err.status = res.status;
    err.code = code;
    err.body = body;
    throw err;
  }
  if (res.status === 204) return undefined;
  const text = await res.text();
  return text ? JSON.parse(text) : undefined;
}

/** @param {string} path @param {{ page?: number, pageSize?: number, search?: string }} q */
export function pagedPath(path, { page = 1, pageSize = 20, search } = {}) {
  const u = new URL(path.replace(/^\//, ''), 'https://dummy.local');
  u.searchParams.set('page', String(page));
  u.searchParams.set('page_size', String(pageSize));
  if (search?.trim()) u.searchParams.set('search', search.trim());
  return `${u.pathname.replace(/^\//, '')}${u.search}`;
}
