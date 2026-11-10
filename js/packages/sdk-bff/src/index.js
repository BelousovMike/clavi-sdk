import ClaviAdminClient from '@clavi/admin-client';

/** @deprecated Use `@clavi/admin-client` for Admin API, or `@clavi/express` / `@clavi/nestjs` for OIDC app login. */
export function createBffClient(opts = {}) {
  const client = new ClaviAdminClient(opts);
  return {
    options: client.options,
    login: (creds) => client.auth(creds),
    me: () => client.me.get(),
    logout: () => client.logout(),
  };
}
