export function resolveAuthorizedNotificationPath(
  link: string,
  authorizedPaths: readonly string[],
  origin = window.location.origin,
): string | null {
  if (!link || link !== link.trim() || !link.startsWith('/') || link.startsWith('//') || link.includes('\\')) {
    return null;
  }

  let target: URL;
  try {
    target = new URL(link, origin);
  } catch {
    return null;
  }

  if (target.origin !== origin || target.username || target.password || !authorizedPaths.includes(target.pathname)) {
    return null;
  }

  return `${target.pathname}${target.search}${target.hash}`;
}

export function formatNotificationDate(creationTime: string, date: string): string {
  if (date.trim()) return date;
  if (!creationTime.trim()) return '时间未知';

  const parsed = new Date(creationTime);
  if (Number.isNaN(parsed.getTime())) return '时间未知';
  return new Intl.DateTimeFormat('zh-CN', { dateStyle: 'medium', timeStyle: 'short' }).format(parsed);
}
