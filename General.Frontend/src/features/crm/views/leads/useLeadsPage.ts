import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { leadsReferenceFixture, leadSources } from '@/features/crm/model/leads.data';
import type { LeadDraft, LeadRow, LeadScope, LeadSource, LeadStatus } from '@/features/crm/model/leads.types';

export function useLeadsPage() {
  const rows = ref<LeadRow[]>(cloneReferenceRows<LeadRow>(leadsReferenceFixture.rows));
  const scope = ref<LeadScope>('mine');
  const status = ref<LeadStatus | 'all'>('all');
  const source = ref<LeadSource | 'all'>('all');
  const query = ref('');
  const selectedIds = ref<string[]>([]);
  const editorOpen = ref(false);
  let nextCode = 13;

  const scopeRows = computed(() =>
    rows.value.filter((row) =>
      scope.value === 'mine' ? row.scope === 'mine' : row.scope === 'public' || row.alsoPublic,
    ),
  );
  const filteredRows = computed(() => {
    const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
    return scopeRows.value.filter((row) => {
      const matchesStatus = status.value === 'all' || row.status === status.value;
      const matchesSource = source.value === 'all' || row.source === source.value;
      const matchesQuery =
        !keyword ||
        [row.code, row.company, row.contact, row.owner].some((value) =>
          value.toLocaleLowerCase('zh-CN').includes(keyword),
        );
      return matchesStatus && matchesSource && matchesQuery;
    });
  });
  const scopeCounts = computed(() => ({
    mine: rows.value.filter((row) => row.scope === 'mine').length,
    public: rows.value.filter((row) => row.scope === 'public' || row.alsoPublic).length,
  }));
  const newCount = computed(() => rows.value.filter((row) => row.status === '新线索').length);
  const convertedCount = computed(() => rows.value.filter((row) => row.status === '已转化').length);
  const publicCount = computed(() => rows.value.filter((row) => row.scope === 'public' || row.alsoPublic).length);
  const allVisibleSelected = computed(
    () => filteredRows.value.length > 0 && filteredRows.value.every((row) => selectedIds.value.includes(row.id)),
  );

  function toggleSelected(id: string, selected: boolean) {
    selectedIds.value = selected
      ? [...new Set([...selectedIds.value, id])]
      : selectedIds.value.filter((item) => item !== id);
  }

  function toggleAll(selected: boolean) {
    const visibleIds = new Set(filteredRows.value.map((row) => row.id));
    selectedIds.value = selected
      ? [...new Set([...selectedIds.value, ...visibleIds])]
      : selectedIds.value.filter((id) => !visibleIds.has(id));
  }

  function convertLead(id: string) {
    rows.value = rows.value.map((row) => (row.id === id ? { ...row, status: '已转化' } : row));
    toast.success('线索已转化为商机');
  }

  function saveLead(draft: LeadDraft) {
    const amount = Number(draft.amount);
    if (!draft.company.trim() || !draft.contact.trim() || !Number.isFinite(amount) || amount <= 0) {
      toast.warning('请填写公司、联系人和有效预估金额');
      return;
    }
    const code = `LD-${String(nextCode).padStart(3, '0')}`;
    rows.value = [
      {
        id: `LOCAL-${code}`,
        code,
        company: draft.company.trim(),
        contact: draft.contact.trim(),
        role: draft.role.trim() || '-',
        status: '新线索',
        source: draft.source,
        score: 50,
        amount,
        region: draft.region.trim() || '-',
        industry: draft.industry.trim() || '-',
        updatedAt: '2026-03-16',
        owner: '张明',
        scope: 'mine',
      },
      ...rows.value,
    ];
    nextCode += 1;
    scope.value = 'mine';
    status.value = 'all';
    source.value = 'all';
    query.value = '';
    editorOpen.value = false;
    toast.success('线索已创建');
  }

  function exportLeads() {
    const header = ['线索号', '公司名称', '联系人', '状态', '来源', '评分', '预估金额', '地区', '行业', '更新日期'];
    const lines = filteredRows.value.map((row) =>
      [
        row.code,
        row.company,
        row.contact,
        row.status,
        row.source,
        row.score,
        row.amount,
        row.region,
        row.industry,
        row.updatedAt,
      ]
        .map((cell) => `"${String(cell).replaceAll('"', '""')}"`)
        .join(','),
    );
    const url = URL.createObjectURL(
      new Blob([`\uFEFF${[header.join(','), ...lines].join('\n')}`], { type: 'text/csv;charset=utf-8' }),
    );
    const link = document.createElement('a');
    link.href = url;
    link.download = '销售线索.csv';
    link.click();
    URL.revokeObjectURL(url);
    toast.success(`已导出 ${filteredRows.value.length} 条线索`);
  }

  return {
    rows,
    scope,
    status,
    source,
    query,
    selectedIds,
    editorOpen,
    filteredRows,
    scopeCounts,
    newCount,
    convertedCount,
    publicCount,
    allVisibleSelected,
    sources: leadSources,
    toggleSelected,
    toggleAll,
    convertLead,
    saveLead,
    exportLeads,
  };
}
