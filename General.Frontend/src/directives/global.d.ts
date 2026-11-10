import type { vAccess } from './permission';

declare module 'vue' {
  interface GlobalDirectives {
    vAccess: typeof vAccess;
  }
}
