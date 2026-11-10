import ClaviAdminClient from '@clavi/admin-client';

/** @deprecated Use `@clavi/admin-client`. */
export function createAdminClient(opts = {}) {
  return new ClaviAdminClient(opts);
}

export { ClaviAdminClient };
export default ClaviAdminClient;
