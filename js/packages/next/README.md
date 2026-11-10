# `@clavi/next`

Auth.js / NextAuth provider for Clavi OIDC.

```ts
import NextAuth from 'next-auth';
import { ClaviProvider, mapClaviSession, createClaviAdmin } from '@clavi/next';

export const { handlers, auth } = NextAuth({
  providers: [ClaviProvider((c) => c.readFrom.env().clientId('my-app'))],
  callbacks: {
    async jwt({ token, account, profile }) {
      return mapClaviSession({ token, account, profile }).token;
    },
    async session({ session, token }) {
      return mapClaviSession({ session, token }).session;
    },
  },
});

export const admin = createClaviAdmin((c) => c.readFrom.env());
```
