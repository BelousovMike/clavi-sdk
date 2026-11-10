/** @param {import('../index.js').ClaviAdminClient} client */
export class Roles {
  constructor(client) {
    this.client = client;
  }

  find() {
    return this.client.get('api/v1/roles');
  }
}
