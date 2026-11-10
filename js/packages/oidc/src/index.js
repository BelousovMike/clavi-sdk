/**
 * Shared Clavi OIDC helpers (Serilog-style config + PKCE + roles).
 * @see docs/oidc-adapters.md
 */

/** @typedef {import('./options.js').ClaviOidcOptions} ClaviOidcOptions */

export {
  createOidcOptions,
  ClaviOidcConfiguration,
  resolveAdminBaseUrl,
} from './options.js';
export {
  createPkcePair,
  discovery,
  buildAuthorizeUrl,
  exchangeCode,
  fetchUserInfo,
  endSessionUrl,
  rolesFromClaims,
  decodeJwtPayload,
} from './protocol.js';
