import type { PageAction, PageActionKind } from '../page.contracts';
import type { PageModel } from '../page.types';

function legacyActionKind(label: string, model: PageModel): PageActionKind {
  if (label.includes('列设置') || label.includes('配置') || label.includes('自定义')) return 'configure';
  if (label.includes('导入') || label.includes('扫描')) return 'import';
  if (label.includes('导出')) return 'export';
  if (label.includes('打印')) return 'print';
  if (label.includes('重置')) return 'reset';
  if (label.includes('刷新') || label.includes('同步')) return 'refresh';
  if (label.includes('删除') || label.includes('停用') || label.includes('清空')) return 'delete';
  if (
    label.includes('新建') ||
    label.includes('新增') ||
    label.includes('创建') ||
    label.includes('发起') ||
    label === model.primaryAction
  ) {
    return 'create';
  }
  if (label.includes('批量')) return 'batch';
  return 'custom';
}

/**
 * Verified pages should declare model.actions. Text inference is retained only
 * for legacy structural scaffolds and stays isolated in this adapter.
 */
export function resolveScaffoldAction(labelOrId: string, model: PageModel): PageAction {
  const explicit = model.actions?.find((action) => action.id === labelOrId || action.label === labelOrId);
  if (explicit) return explicit;

  const kind = legacyActionKind(labelOrId, model);
  return {
    id: `legacy:${kind}:${labelOrId}`,
    label: labelOrId,
    kind,
    tone: kind === 'delete' ? 'danger' : kind === 'create' ? 'primary' : 'default',
    requiresSelection: kind === 'batch',
  };
}
