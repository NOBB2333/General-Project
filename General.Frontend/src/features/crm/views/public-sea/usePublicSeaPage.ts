import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { publicSeaIndustries, publicSeaReferenceFixture, publicSeaSources } from '@/features/crm/model/public-sea.data';
import type {
  PublicSeaDraft,
  PublicSeaRow,
  PublicSeaSource,
  PublicSeaTab,
} from '@/features/crm/model/public-sea.types';

export function usePublicSeaPage() {
  const rows = ref<PublicSeaRow[]>(cloneReferenceRows<PublicSeaRow>(publicSeaReferenceFixture.rows));
  const tab = ref<PublicSeaTab>('all');
  const industry = ref('all');
  const source = ref<PublicSeaSource | 'all'>('all');
  const query = ref('');
  const editorOpen = ref(false);
  let nextId = 9;

  const tabRows = computed(() =>
    rows.value.filter((row) => {
      if (tab.value === 'recent') return row.releaseDays <= 10;
      if (tab.value === 'dormant') return row.releaseDays >= 20;
      if (tab.value === 'valuable') return row.estimatedValue >= 500000;
      return true;
    }),
  );
  const filteredRows = computed(() => {
    const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
    return tabRows.value.filter((row) => {
      const matchesIndustry = industry.value === 'all' || row.industry === industry.value;
      const matchesSource = source.value === 'all' || row.source === source.value;
      const matchesQuery =
        !keyword ||
        [row.customer, row.contact, row.city, row.industry].some((value) =>
          value.toLocaleLowerCase('zh-CN').includes(keyword),
        );
      return matchesIndustry && matchesSource && matchesQuery;
    });
  });
  const tabCounts = computed(() => ({
    all: rows.value.length,
    recent: rows.value.filter((row) => row.releaseDays <= 10).length,
    dormant: rows.value.filter((row) => row.releaseDays >= 20).length,
    valuable: rows.value.filter((row) => row.estimatedValue >= 500000).length,
  }));
  const totalValue = computed(() => rows.value.reduce((sum, row) => sum + row.estimatedValue, 0));
  const valuableCount = computed(() => rows.value.filter((row) => row.estimatedValue >= 500000).length);

  function saveCustomer(draft: PublicSeaDraft) {
    const estimatedValue = Number(draft.estimatedValue);
    if (!draft.customer.trim() || !Number.isFinite(estimatedValue) || estimatedValue <= 0) {
      toast.warning('请填写客户名称和有效预估价值');
      return;
    }
    rows.value = [
      {
        id: `LOCAL-SEA-${nextId}`,
        customer: draft.customer.trim(),
        industry: draft.industry.trim() || '其他',
        level: draft.level,
        contact: draft.contact.trim() || '-',
        city: draft.city.trim() || '-',
        source: draft.source,
        estimatedValue,
        releaseDays: 0,
      },
      ...rows.value,
    ];
    nextId += 1;
    tab.value = 'all';
    industry.value = 'all';
    source.value = 'all';
    query.value = '';
    editorOpen.value = false;
    toast.success('公海客户已创建');
  }

  function exportCustomers() {
    const header = ['客户名称', '行业', '级别', '联系人', '所在城市', '来源', '预估价值', '释放天数'];
    const lines = filteredRows.value.map((row) =>
      [row.customer, row.industry, row.level, row.contact, row.city, row.source, row.estimatedValue, row.releaseDays]
        .map((cell) => `"${String(cell).replaceAll('"', '""')}"`)
        .join(','),
    );
    const url = URL.createObjectURL(
      new Blob([`\uFEFF${[header.join(','), ...lines].join('\n')}`], { type: 'text/csv;charset=utf-8' }),
    );
    const link = document.createElement('a');
    link.href = url;
    link.download = '公海客户.csv';
    link.click();
    URL.revokeObjectURL(url);
    toast.success(`已导出 ${filteredRows.value.length} 条公海客户`);
  }

  function toggleFollow(id: string) {
    rows.value = rows.value.map((row) => (row.id === id ? { ...row, followed: !row.followed } : row));
  }

  function claimCustomer(id: string) {
    const customer = rows.value.find((row) => row.id === id);
    if (!customer) return;
    rows.value = rows.value.filter((row) => row.id !== id);
    toast.success(`已领取客户“${customer.customer}”`);
  }

  return {
    rows,
    tab,
    industry,
    source,
    query,
    editorOpen,
    filteredRows,
    tabCounts,
    totalValue,
    valuableCount,
    industries: publicSeaIndustries,
    sources: publicSeaSources,
    saveCustomer,
    exportCustomers,
    toggleFollow,
    claimCustomer,
  };
}
