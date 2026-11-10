# `@clavi/nestjs`

```ts
import { NestFactory } from '@nestjs/core';
import { ClaviModule, ClaviNest } from '@clavi/nestjs';
import session from 'express-session';

@Module({
  imports: [
    ClaviModule.forRoot((c) => c.readFrom.env().clientId('my-app')),
  ],
})
class AppModule {}

const app = await NestFactory.create(AppModule);
app.use(session({ secret: 'dev', resave: false, saveUninitialized: false }));
ClaviNest.attach(app.getHttpAdapter().getInstance()); // OIDC routes on Express
```

Inject Admin: `@Inject(CLAVI_ADMIN) admin: ClaviAdminClient`.
