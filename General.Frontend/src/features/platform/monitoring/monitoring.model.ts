import type { OnlineUserApi } from '@/api/core/online-user';
import type { SystemMonitorApi } from '@/api/core/system-monitor';
import type { ListPageRow } from '@/components/page/families/list/list-page.types';

const numberFormat = new Intl.NumberFormat('zh-CN', { maximumFractionDigits: 2 });

export function formatMonitoringDate(
  value: null | string,
  options: Intl.DateTimeFormatOptions = { dateStyle: 'short', timeStyle: 'medium' },
): string {
  if (!value) return '暂无记录';
  const utcValue = /(?:Z|[+-]\d{2}:\d{2})$/iu.test(value) ? value : `${value}Z`;
  const date = new Date(utcValue);
  return Number.isNaN(date.getTime()) ? '暂无记录' : new Intl.DateTimeFormat('zh-CN', options).format(date);
}

export function formatMonitorNumber(value: null | number, unit = ''): string {
  return value !== null && Number.isFinite(value) && value >= 0 ? `${numberFormat.format(value)}${unit}` : '未提供';
}

export function formatMonitorBytes(value: number): string {
  if (!Number.isFinite(value) || value < 0) return '未提供';
  const units = ['B', 'KiB', 'MiB', 'GiB', 'TiB'];
  const index = value === 0 ? 0 : Math.max(0, Math.min(units.length - 1, Math.floor(Math.log(value) / Math.log(1024))));
  return `${numberFormat.format(value / 1024 ** index)} ${units[index]}`;
}

export function previewMonitoringValue(value: string): string {
  const characters = Array.from(value);
  return characters.length > 18 ? `${characters.slice(0, 18).join('')}…` : value;
}

export function browserLabel(value: string): string {
  if (/Edg\//u.test(value)) return 'Edge';
  if (/Firefox\//u.test(value)) return 'Firefox';
  if (/Chrome\//u.test(value)) return 'Chrome';
  if (/Safari\//u.test(value)) return 'Safari';
  return '未识别';
}

export function isOwnOnlineAccount(item: OnlineUserApi.Item, username: string, tenantScoped: boolean): boolean {
  return item.userName.toLowerCase() === username.toLowerCase() && Boolean(item.currentTenantId) === tenantScoped;
}

export interface OnlineUserRow extends ListPageRow {
  id: string;
  name: string;
  tenant: string;
  device: string;
  browser: string;
  lastAccess: string;
}

export function deviceLabel(value: string): string {
  const devices: Readonly<Record<string, string>> = {
    Mobile: '移动端',
    Web: '网页',
    Mac: 'macOS',
    Windows: 'Windows',
    Linux: 'Linux',
  };
  return Object.hasOwn(devices, value) ? (devices[value] ?? '未识别') : '未识别';
}

export function toOnlineUserRow(item: OnlineUserApi.Item): OnlineUserRow {
  return {
    id: item.id,
    name: previewMonitoringValue(item.userName),
    account: item.userName,
    tenant: previewMonitoringValue(item.currentTenantId ? item.tenantName || '租户名称未提供' : '宿主'),
    device: deviceLabel(item.device),
    browser: browserLabel(item.browser),
    lastAccess: formatMonitoringDate(item.lastAccessedAt),
  };
}

export interface MonitorRow extends ListPageRow {
  id: string;
  name: string;
  group: string;
  value: string;
  fullValue: string;
  note: string;
}

export function toMonitorRows(item: SystemMonitorApi.Snapshot): MonitorRow[] {
  const rows: MonitorRow[] = [];
  const add = (id: string, name: string, group: string, fullValue: string, note = '') => {
    rows.push({ id, name, group, fullValue, value: previewMonitoringValue(fullValue), note });
  };
  add(
    'system-cpu',
    '系统处理器使用率',
    '处理器',
    formatMonitorNumber(item.systemCpuUsagePercent, '%'),
    item.cpuUsageNote || '以服务端采样口径为准。',
  );
  add('process-cpu', '进程处理器使用率', '处理器', formatMonitorNumber(item.processCpuUsagePercent, '%'));
  add('cpu-count', '逻辑处理器数', '处理器', formatMonitorNumber(item.processorCount));
  add('cpu-time', '进程累计处理器时间', '处理器', formatMonitorNumber(item.cpuTimeSeconds, ' 秒'));
  add(
    'system-memory-percent',
    '系统内存使用率',
    '内存',
    formatMonitorNumber(item.systemMemoryUsagePercent, '%'),
    item.memoryUsageNote || '以服务端采样口径为准。',
  );
  add(
    'total-memory',
    '系统总内存',
    '内存',
    item.totalMemoryBytes > 0 ? formatMonitorBytes(item.totalMemoryBytes) : '未提供',
  );
  add('used-memory', '系统已用内存', '内存', formatMonitorBytes(item.systemUsedMemoryBytes));
  add('available-memory', '系统可用内存', '内存', formatMonitorBytes(item.availableMemoryBytes));
  add(
    'process-memory-percent',
    '进程内存占比',
    '内存',
    formatMonitorNumber(item.processMemoryUsagePercent, '%'),
    '分母由服务端提供，详情可查看。',
  );
  add(
    'memory-denominator',
    '进程内存占比分母',
    '内存',
    item.processMemoryDisplayDenominatorBytes > 0
      ? formatMonitorBytes(item.processMemoryDisplayDenominatorBytes)
      : '未提供',
  );
  add('working-set', '进程工作集', '内存', formatMonitorBytes(item.workingSetBytes));
  add('managed-memory', '托管内存', '内存', formatMonitorBytes(item.managedMemoryBytes));
  add('uptime', '进程运行时长', '进程', formatMonitorNumber(item.uptimeMinutes, ' 分钟'));
  add('start-time', '进程启动时间', '进程', formatMonitoringDate(item.processStartTime));
  add('threads', '线程数', '进程', formatMonitorNumber(item.threadCount));
  add('handles', '句柄数', '进程', formatMonitorNumber(item.handleCount));
  const environments: Readonly<Record<string, string>> = { Development: '开发', Staging: '预发布', Production: '生产' };
  add('environment', '运行环境', '运行环境', environments[item.environmentName] ?? (item.environmentName || '未提供'));
  add('framework', '运行时版本', '运行环境', item.frameworkDescription || '未提供');
  add('machine', '节点名称', '运行环境', item.machineName || '未提供');
  add('os', '操作系统', '运行环境', item.osDescription || '未提供');
  add('os-arch', '系统架构', '运行环境', item.osArchitecture || '未提供');
  add('process-arch', '进程架构', '运行环境', item.processArchitecture || '未提供');
  item.disks.forEach((disk, index) => {
    const label = `磁盘 ${index + 1}`;
    add(
      `disk-${index}-capacity`,
      `${label} 容量`,
      '磁盘',
      formatMonitorBytes(disk.totalSizeBytes),
      '名称与挂载位置在详情中查看。',
    );
    add(`disk-${index}-free`, `${label} 可用空间`, '磁盘', formatMonitorBytes(disk.freeSpaceBytes));
    add(`disk-${index}-name`, `${label} 名称`, '磁盘', disk.name || '未提供');
    add(`disk-${index}-format`, `${label} 文件系统`, '磁盘', disk.driveFormat || '未提供');
    const types: Readonly<Record<string, string>> = {
      Fixed: '固定磁盘',
      Removable: '可移动磁盘',
      Network: '网络磁盘',
      Ram: '内存磁盘',
      CDRom: '光盘',
      Unknown: '未知',
      NoRootDirectory: '无根目录',
    };
    add(`disk-${index}-type`, `${label} 类型`, '磁盘', types[disk.type] ?? (disk.type || '未提供'));
  });
  return rows;
}
