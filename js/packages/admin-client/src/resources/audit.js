/** @param {import('../index.js').ClaviAdminClient} client */
export class Audit {
  constructor(client) {
    this.client = client;
  }

  /** @param {{ page?: number, pageSize?: number }} [q] */
  find(q) {
    return this.client.get('api/v1/audit/entries', q ?? {});
  }
}
