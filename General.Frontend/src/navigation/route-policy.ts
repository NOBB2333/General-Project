export interface RoutePolicyInput {
  authenticated: boolean;
  isErrorRoute: boolean;
  isLoginRoute: boolean;
  isRoot: boolean;
  registered: boolean;
  authorized: boolean;
}

export type RoutePolicyDecision = 'allow' | 'allow-login' | 'forbidden' | 'home' | 'login' | 'not-found';

export function decideRouteAccess(input: RoutePolicyInput): RoutePolicyDecision {
  if (!input.authenticated) return input.isLoginRoute ? 'allow-login' : 'login';
  if (input.isLoginRoute || input.isRoot) return 'home';
  if (input.isErrorRoute) return 'allow';
  if (!input.registered) return 'not-found';
  if (!input.authorized) return 'forbidden';
  return 'allow';
}
