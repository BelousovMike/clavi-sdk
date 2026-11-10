/** @param {import('../index.js').ClaviAdminClient} client */
export class Security {
  constructor(client) {
    this.client = client;
  }

  getPasswordPolicy() {
    return this.client.get('api/v1/security/policies/password');
  }
}
