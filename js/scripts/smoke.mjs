import ClaviAdminClient from '@clavi/admin-client';

if (process.env.CLAVI_SDK_SMOKE !== '1') {
  console.log('smoke skipped (set CLAVI_SDK_SMOKE=1)');
  process.exit(0);
}

if (process.env.CLAVI_SDK_INSECURE_TLS !== '0') {
  process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
}

const password = process.env.CLAVI_E2E_ADMIN_PASSWORD;
if (!password) {
  console.error('CLAVI_E2E_ADMIN_PASSWORD required');
  process.exit(1);
}

const username = process.env.CLAVI_E2E_ADMIN_USERNAME ?? 'admin';
const client = new ClaviAdminClient();
await client.auth({ username, password });
const users = await client.users.find({ page: 1, pageSize: 10 });
if (!users?.items) throw new Error('expected paged users');
console.log(`ok users total=${users.total}`);
