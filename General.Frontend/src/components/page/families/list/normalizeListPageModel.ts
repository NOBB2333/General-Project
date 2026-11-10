import type { PageAction } from '../../page.contracts';
import type { PageModel } from '../../page.types';
import type { ListPageModel, ListPageRow } from './list-page.types';

function inferActionKind(label: string): PageAction['kind'] {
  if (/新增|新建|发起|创建/.test(label)) return 'create';
  if (/导入|上传/.test(label)) return 'import';
  if (/导出|下载/.test(label)) return 'export';
  if (/打印/.test(label)) return 'print';
  if (/刷新|同步/.test(label)) return 'refresh';
  if (/设置|配置/.test(label)) return 'configure';
  if (/删除|清空/.test(label)) return 'delete';
  return 'custom';
}

/**
 * Adapts legacy catalog models to the typed list contract while a page is
 * being migrated. The adapter only supplies interaction metadata; feature
 * pages still own their fixture and local state.
 */
export function normalizeListPageModel<TRow extends ListPageRow>(
  model: PageModel,
  rows: readonly TRow[],
): ListPageModel<TRow> {
  const actions: PageAction[] = model.actions?.length
    ? [...model.actions]
    : model.tools.map((label, index) => ({
        id: `${model.pageSlug}-tool-${index}`,
        label,
        kind: inferActionKind(label),
        tone: /新增|新建|发起|创建/.test(label) ? ('primary' as const) : ('default' as const),
      }));

  return {
    ...model,
    surface: 'list',
    rows: [...rows],
    actions,
    replication: model.replication ?? {
      status: 'implemented',
      referenceUrl: `https://demo.risemap.cn${model.path}`,
    },
    listScopes: [
      { id: 'all', label: '所有', count: rows.length },
      { id: 'mine', label: '我负责的' },
      { id: 'followed', label: '我关注的' },
    ],
    listViews: [{ id: 'list', label: '列表' }],
    listTabs: [
      { id: 'all', label: '全部' },
      ...model.tabs.slice(1).map((label, index) => ({ id: `tab-${index + 1}`, label })),
    ],
    pageSize: model.pageSize ?? 10,
    emptyHint: model.emptyHint ?? `当前筛选下没有${model.label}`,
  };
}
