import 'vue-router';
import type { AppView } from './route.types';
import type { vAccess } from '@/directives/permission';

declare module 'vue' {
  interface GlobalDirectives {
    vAccess: typeof vAccess;
  }
}

declare module 'vue-router' {
  interface RouteMeta {
    appView?: AppView;
    workspace?: string;
    title?: string;
    module?: string;
    pageLabel?: string;
    feature?: string;
    specialty?: string;
  }
}
