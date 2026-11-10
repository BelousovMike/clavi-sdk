function env(name) {
  try {
    return typeof process !== 'undefined' ? process.env?.[name] : undefined;
  } catch {
    return undefined;
  }
}

/**
 * @typedef {object} ClaviOidcOptions
 * @property {string} authority
 * @property {string} clientId
 * @property {string} [clientSecret]
 * @property {string} [callbackPath]
 * @property {string} [signedOutCallbackPath]
 * @property {string[]} [scopes]
 * @property {boolean} [mapRoles]
 * @property {boolean} [acceptAnyServerCertificate]
 * @property {boolean} [disablePushedAuthorization]
 * @property {string} [baseUrl]
 * @property {string} [tenantHost]
 */

const DEFAULTS = {
  callbackPath: '/signin-oidc',
  signedOutCallbackPath: '/signout-callback-oidc',
  scopes: ['openid', 'profile', 'email'],
  mapRoles: true,
  acceptAnyServerCertificate: false,
  disablePushedAuthorization: true,
};

/**
 * Merge partial options + env (`CLAVI_*`).
 * @param {Partial<ClaviOidcOptions>} [partial]
 * @returns {ClaviOidcOptions}
 */
export function createOidcOptions(partial = {}) {
  const fromEnv = {
    authority: env('CLAVI_AUTHORITY') ?? env('Clavi__Authority'),
    clientId: env('CLAVI_CLIENT_ID') ?? env('Clavi__ClientId'),
    clientSecret: env('CLAVI_CLIENT_SECRET') ?? env('Clavi__ClientSecret'),
    callbackPath: env('CLAVI_CALLBACK_PATH') ?? env('Clavi__CallbackPath'),
    signedOutCallbackPath:
      env('CLAVI_SIGNED_OUT_CALLBACK_PATH') ?? env('Clavi__SignedOutCallbackPath'),
    baseUrl: env('CLAVI_BASE_URL') ?? env('Clavi__BaseUrl'),
    tenantHost: env('CLAVI_TENANT_HOST') ?? env('Clavi__TenantHost'),
  };

  const scopesRaw = env('CLAVI_SCOPES') ?? env('Clavi__Scopes');
  const scopes = partial.scopes
    ?? (scopesRaw ? scopesRaw.split(/[,\s]+/).filter(Boolean) : undefined)
    ?? DEFAULTS.scopes;

  /** @type {ClaviOidcOptions} */
  const opts = {
    authority: (partial.authority ?? fromEnv.authority ?? '').replace(/\/$/, ''),
    clientId: partial.clientId ?? fromEnv.clientId ?? '',
    clientSecret: partial.clientSecret ?? fromEnv.clientSecret,
    callbackPath: partial.callbackPath ?? fromEnv.callbackPath ?? DEFAULTS.callbackPath,
    signedOutCallbackPath:
      partial.signedOutCallbackPath
      ?? fromEnv.signedOutCallbackPath
      ?? DEFAULTS.signedOutCallbackPath,
    scopes,
    mapRoles: partial.mapRoles ?? DEFAULTS.mapRoles,
    acceptAnyServerCertificate:
      partial.acceptAnyServerCertificate ?? DEFAULTS.acceptAnyServerCertificate,
    disablePushedAuthorization:
      partial.disablePushedAuthorization ?? DEFAULTS.disablePushedAuthorization,
    baseUrl: partial.baseUrl ?? fromEnv.baseUrl,
    tenantHost: partial.tenantHost ?? fromEnv.tenantHost,
  };

  if (!opts.authority) throw new Error('Clavi:Authority / CLAVI_AUTHORITY is required');
  if (!opts.clientId) throw new Error('Clavi:ClientId / CLAVI_CLIENT_ID is required');
  return opts;
}

/** Admin BaseUrl falls back to Authority. */
export function resolveAdminBaseUrl(opts) {
  const raw = (opts.baseUrl || opts.authority || '').replace(/\/?$/, '/');
  if (!raw || raw === '/') throw new Error('Clavi:BaseUrl or Authority is required for Admin');
  return raw;
}

/**
 * Fluent builder (Serilog-style): `new ClaviOidcConfiguration().readFrom.env().clientId('x')`.
 */
export class ClaviOidcConfiguration {
  constructor() {
    /** @type {Partial<ClaviOidcOptions>} */
    this._partial = {};
    this.readFrom = {
      /** Bind process.env / CLAVI_* (already done in createOidcOptions). */
      env: () => {
        return this;
      },
      /**
       * @param {Record<string, unknown>} obj Nested `Clavi` section or flat options.
       */
      object: (obj) => {
        const section = obj?.Clavi && typeof obj.Clavi === 'object' ? obj.Clavi : obj;
        const map = {
          Authority: 'authority',
          ClientId: 'clientId',
          ClientSecret: 'clientSecret',
          CallbackPath: 'callbackPath',
          SignedOutCallbackPath: 'signedOutCallbackPath',
          Scopes: 'scopes',
          MapRoles: 'mapRoles',
          AcceptAnyServerCertificate: 'acceptAnyServerCertificate',
          DisablePushedAuthorization: 'disablePushedAuthorization',
          BaseUrl: 'baseUrl',
          TenantHost: 'tenantHost',
        };
        for (const [k, v] of Object.entries(section || {})) {
          const dest = map[k] ?? (k.charAt(0).toLowerCase() + k.slice(1));
          if (v !== undefined && v !== null && v !== '') this._partial[dest] = v;
        }
        return this;
      },
    };
  }

  authority(v) { this._partial.authority = v; return this; }
  clientId(v) { this._partial.clientId = v; return this; }
  clientSecret(v) { this._partial.clientSecret = v; return this; }
  callbackPath(v) { this._partial.callbackPath = v; return this; }
  signedOutCallbackPath(v) { this._partial.signedOutCallbackPath = v; return this; }
  scopes(...s) { this._partial.scopes = s.flat(); return this; }
  mapRoles(v = true) { this._partial.mapRoles = v; return this; }
  acceptAnyServerCertificate(v = true) { this._partial.acceptAnyServerCertificate = v; return this; }
  baseUrl(v) { this._partial.baseUrl = v; return this; }
  tenantHost(v) { this._partial.tenantHost = v; return this; }
  configure(fn) { fn(this._partial); return this; }

  /** @returns {ClaviOidcOptions} */
  build() {
    return createOidcOptions(this._partial);
  }
}
