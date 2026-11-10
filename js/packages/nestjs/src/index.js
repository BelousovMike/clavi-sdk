import { ClaviAdminClient } from '@clavi/admin-client';
import {
  ClaviOidcConfiguration,
  createOidcOptions,
  resolveAdminBaseUrl,
} from '@clavi/oidc';
import { addClaviOpenIdConnect, requireClaviAuth } from '@clavi/express';

export { ClaviOidcConfiguration, createOidcOptions, requireClaviAuth };
export const CLAVI_OIDC_OPTIONS = 'CLAVI_OIDC_OPTIONS';
export const CLAVI_ADMIN = 'CLAVI_ADMIN';

/**
 * Nest-friendly registration without requiring Nest packages at publish time.
 *
 * Prefer calling from `main.ts` after creating the Nest app:
 *
 * ```ts
 * const app = await NestFactory.create(AppModule);
 * ClaviNest.attach(app.getHttpAdapter().getInstance(), (c) =>
 *   c.readFrom.env().clientId('my-app'));
 * ```
 *
 * Or use `ClaviModule.forRoot` when `@nestjs/common` is installed.
 */
export const ClaviNest = {
  /**
   * @param {import('express').Express} expressApp
   * @param {Parameters<typeof addClaviOpenIdConnect>[1]} configure
   * @param {Parameters<typeof addClaviOpenIdConnect>[2]} [opts]
   */
  attach(expressApp, configure, opts) {
    return addClaviOpenIdConnect(expressApp, configure, opts);
  },

  /**
   * Dynamic Nest module factory. Returns a plain object Nest can consume as a module.
   * @param {import('@clavi/oidc').ClaviOidcOptions | ((c: ClaviOidcConfiguration) => void) | Record<string, unknown>} configure
   */
  forRoot(configure) {
    const options = resolve(configure);
    const admin = new ClaviAdminClient({
      baseUrl: resolveAdminBaseUrl(options),
      tenantHost: options.tenantHost,
    });
    return {
      module: class ClaviModule {},
      providers: [
        { provide: CLAVI_OIDC_OPTIONS, useValue: options },
        { provide: CLAVI_ADMIN, useValue: admin },
        { provide: ClaviAdminClient, useValue: admin },
      ],
      exports: [CLAVI_OIDC_OPTIONS, CLAVI_ADMIN, ClaviAdminClient],
      global: true,
    };
  },

  /**
   * @param {{ useFactory: (...args: any[]) => any, inject?: any[] }} opts
   */
  forRootAsync(opts) {
    return {
      module: class ClaviModuleAsync {},
      providers: [
        {
          provide: CLAVI_OIDC_OPTIONS,
          useFactory: async (...args) => resolve(await opts.useFactory(...args)),
          inject: opts.inject ?? [],
        },
        {
          provide: CLAVI_ADMIN,
          useFactory: (options) => new ClaviAdminClient({
            baseUrl: resolveAdminBaseUrl(options),
            tenantHost: options.tenantHost,
          }),
          inject: [CLAVI_OIDC_OPTIONS],
        },
        {
          provide: ClaviAdminClient,
          useExisting: CLAVI_ADMIN,
        },
      ],
      exports: [CLAVI_OIDC_OPTIONS, CLAVI_ADMIN, ClaviAdminClient],
      global: true,
    };
  },
};

export const ClaviModule = ClaviNest;

function resolve(configure) {
  if (typeof configure === 'function') {
    const c = new ClaviOidcConfiguration();
    configure(c);
    return c.build();
  }
  if (configure?.authority || configure?.clientId) return createOidcOptions(configure);
  if (configure && typeof configure === 'object') {
    return new ClaviOidcConfiguration().readFrom.object(configure).build();
  }
  return createOidcOptions({});
}
