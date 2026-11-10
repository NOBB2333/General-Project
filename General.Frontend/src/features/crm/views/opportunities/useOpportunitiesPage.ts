import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import {
  opportunitiesReferenceFixture,
  opportunityCustomers,
  opportunityOwners,
  opportunitySources,
  opportunityStages,
} from '@/features/crm/model/opportunities.data';
import type {
  OpportunityDraft,
  OpportunityRow,
  OpportunityScope,
  OpportunitySource,
  OpportunityStage,
  OpportunityView,
} from '@/features/crm/model/opportunities.types';

export function useOpportunitiesPage() {
  const rows = ref<OpportunityRow[]>(cloneReferenceRows<OpportunityRow>(opportunitiesReferenceFixture.rows));
  const query = ref('');
  const scope = ref<OpportunityScope>('all');
  const stage = ref<OpportunityStage | 'all'>('all');
  const source = ref<OpportunitySource | 'all'>('all');
  const view = ref<OpportunityView>('list');
  const editorOpen = ref(false);
  let nextCode = 8;

  const scopedRows = computed(() =>
    rows.value.filter((row) => {
      if (scope.value === 'all') return true;
      if (scope.value === 'following') return row.followed;
      return row.ownerScope === scope.value;
    }),
  );
  const filteredRows = computed(() => {
    const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
    return scopedRows.value.filter((row) => {
      const matchesStage = stage.value === 'all' || row.stage === stage.value;
      const matchesSource = source.value === 'all' || row.source === source.value;
      const matchesQuery =
        !keyword ||
        [row.name, row.code, row.customer, row.contact, row.owner].some((value) =>
          value.toLocaleLowerCase('zh-CN').includes(keyword),
        );
      return matchesStage && matchesSource && matchesQuery;
    });
  });
  const scopeCounts = computed(() => ({
    all: rows.value.length,
    mine: rows.value.filter((row) => row.ownerScope === 'mine').length,
    subordinate: rows.value.filter((row) => row.ownerScope === 'subordinate').length,
    following: rows.value.filter((row) => row.followed).length,
  }));
  const activeRows = computed(() => rows.value.filter((row) => !['赢单', '输单'].includes(row.stage)));
  const pipelineAmount = computed(() => activeRows.value.reduce((sum, row) => sum + row.amount, 0));
  const weightedAmount = computed(() =>
    activeRows.value.reduce((sum, row) => sum + row.amount * (row.probability / 100), 0),
  );
  const wonAmount = computed(() =>
    rows.value.filter((row) => row.stage === '赢单').reduce((sum, row) => sum + row.amount, 0),
  );
  const stageCounts = computed(
    () =>
      Object.fromEntries(
        opportunityStages.map((item) => [item, rows.value.filter((row) => row.stage === item).length]),
      ) as Record<OpportunityStage, number>,
  );

  function saveOpportunity(draft: OpportunityDraft) {
    const amount = Number(draft.amount);
    const probability = Number(draft.probability);
    if (!draft.name.trim() || !draft.customer || !Number.isFinite(amount) || amount <= 0) {
      toast.warning('请填写商机名称、客户和有效金额');
      return;
    }
    if (!Number.isFinite(probability) || probability < 0 || probability > 100) {
      toast.warning('成交概率必须在 0% 至 100% 之间');
      return;
    }
    const owner = draft.owner || opportunityOwners[0] || '张明';
    const code = `OPP-${String(nextCode).padStart(3, '0')}`;
    rows.value = [
      {
        id: `LOCAL-${code}`,
        code,
        name: draft.name.trim(),
        customer: draft.customer,
        contact: draft.contact.trim() || '-',
        stage: draft.stage,
        traffic: draft.traffic,
        priority: draft.priority,
        amount,
        probability,
        expectedDate: draft.expectedDate || '2026-12-31',
        owner,
        ownerScope: owner === '张明' ? 'mine' : 'subordinate',
        source: draft.source,
      },
      ...rows.value,
    ];
    nextCode += 1;
    editorOpen.value = false;
    toast.success('商机已创建');
  }

  return {
    rows,
    query,
    scope,
    stage,
    source,
    view,
    editorOpen,
    scopedRows,
    filteredRows,
    scopeCounts,
    activeRows,
    pipelineAmount,
    weightedAmount,
    wonAmount,
    stageCounts,
    stages: opportunityStages,
    sources: opportunitySources,
    customers: opportunityCustomers,
    owners: opportunityOwners,
    saveOpportunity,
  };
}
