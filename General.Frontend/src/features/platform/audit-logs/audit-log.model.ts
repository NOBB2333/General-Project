import type { AuditLogApi } from '@/api/core/audit-log';
import type { ListPageRow } from '@/components/page/families/list/list-page.types';

export const auditCategoryOptions = [
  { value: '', label: '全部接口日志' },
  { value: 'access', label: '访问日志' },
  { value: 'operation', label: '操作日志' },
  { value: 'audit', label: '审计日志' },
  { value: 'exception', label: '异常日志' },
  { value: 'pagevisit', label: '页面访问' },
] as const;

export interface AuditFilters {
  category: string;
  endDate: string;
  startDate: string;
}

export interface AuditLogRow extends ListPageRow {
  id: string;
  time: string;
  actor: string;
  tenant: string;
  kind: string;
  method: string;
  status: string;
  duration: string;
  summary: string;
  path: string;
  source: string;
  exception: string;
}

function dayBoundary(value: string, end: boolean): string | undefined {
  if (!value) return undefined;
  const date = new Date(`${value}T00:00:00`);
  const normalized = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
  if (!/^\d{4}-\d{2}-\d{2}$/u.test(value) || Number.isNaN(date.getTime()) || normalized !== value) {
    throw new Error('请选择有效日期。');
  }
  if (end) date.setHours(23, 59, 59, 999);
  return date.toISOString();
}

export function buildAuditQuery(
  filters: AuditFilters,
  keyword: string,
  page: number,
  pageSize: number,
): AuditLogApi.Query {
  const startTime = dayBoundary(filters.startDate, false);
  const endTime = dayBoundary(filters.endDate, true);
  if (startTime && endTime && startTime > endTime) throw new Error('开始日期不能晚于结束日期。');
  if (!auditCategoryOptions.some((option) => option.value === filters.category))
    throw new Error('请选择有效日志类别。');
  if (!Number.isInteger(pageSize) || pageSize < 1 || pageSize > 500) throw new Error('每页条数必须在1至500之间。');
  return {
    category: filters.category || undefined,
    endTime,
    keyword: keyword.trim() || undefined,
    maxResultCount: pageSize,
    skipCount: (Math.max(1, page) - 1) * pageSize,
    startTime,
  };
}

export function auditPath(value: string | null): string {
  if (!value || (!value.startsWith('/') && !/^https?:\/\//iu.test(value)) || value.startsWith('//')) return '未记录';
  try {
    const url = new URL(value, 'https://audit.invalid');
    return url.pathname || '/';
  } catch {
    return '未记录';
  }
}

export function parseAuditTime(value: string): Date {
  // 请求/页面访问日志由后端按UTC写入；SQLite回读可能丢失DateTime.Kind。
  const utcValue = /^\d{4}-\d{2}-\d{2}T/u.test(value) && !/(Z|[+-]\d{2}:\d{2})$/iu.test(value) ? `${value}Z` : value;
  return new Date(utcValue);
}

export function toAuditLogRow(item: AuditLogApi.Item): AuditLogRow {
  const date = parseAuditTime(item.executionTime);
  return {
    id: item.id,
    actor: item.userName || '匿名',
    duration: `${item.executionDuration} ms`,
    exception: item.hasException ? '有异常' : '无异常标记',
    kind: item.category === 'pagevisit' ? '页面访问' : '接口日志',
    method: item.httpMethod || '未记录',
    path: auditPath(item.url),
    source: item.isHostTenantOperation ? item.hostOperatorUserName || '宿主运维' : '普通会话',
    status: item.httpStatusCode?.toString() ?? '未记录',
    summary: item.menuTitle || item.actionSummary || '未记录',
    tenant: item.tenantName || '宿主 / 未记录',
    time: Number.isNaN(date.getTime()) ? '未记录' : date.toLocaleString('zh-CN'),
  };
}
