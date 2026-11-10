/** @param {import('../index.js').ClaviAdminClient} client */
export class Me {
  constructor(client) {
    this.client = client;
  }

  get() {
    return this.client.get('api/v1/me');
  }

  /** @param {{ page?: number, pageSize?: number }} [q] */
  listSessions(q) {
    return this.client.get('api/v1/me/sessions', q ?? {});
  }

  revokeSession({ sessionId }) {
    return this.client.del(`api/v1/me/sessions/${sessionId}`);
  }
}
