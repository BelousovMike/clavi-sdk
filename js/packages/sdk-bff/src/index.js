import ClaviAdminClient from '@clavi/admin-client';

/** @deprecated Use `@clavi/admin-client`. */
export function createBffClient(opts = {}) {
  const client = new ClaviAdminClient(opts);
  return {
    options: client.options,
    login: (creds) => client.auth(creds),
    me: () => client.me.get(),
    logout: () => client.logout(),
  };
}
