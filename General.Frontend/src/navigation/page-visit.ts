export function createAuthorizedPageVisit(input: {
  authenticated: boolean;
  failed: boolean;
  path: string;
  previousPath: string;
  page?: { localPath: string | null; label: string };
}): { menuPath: string; menuTitle: string } | null {
  if (
    !input.authenticated ||
    input.failed ||
    !input.page ||
    input.page.localPath !== input.path ||
    input.path === input.previousPath ||
    !input.path.startsWith('/') ||
    input.path.startsWith('//') ||
    /[?#\\]/u.test(input.path)
  ) {
    return null;
  }
  return { menuPath: input.path, menuTitle: input.page.label };
}
