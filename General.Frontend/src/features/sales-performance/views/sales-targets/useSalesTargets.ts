import { computed, reactive, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import type { TargetPageSurfaceModel, TargetSummaryMetric } from '@/components/page';
import { toast } from '@/components/ui';
import {
  salesTargetSummaryDefinitions,
  salesTargetTabs,
  salesTargetUnits,
  salesTargetsReferenceFixture,
} from '../../model/sales-targets.data';
import type {
  SalesTargetDraft,
  SalesTargetPersonRow,
  SalesTargetReferenceRow,
  SalesTargetTab,
  SalesTargetTeamRow,
  SalesTargetUnit,
} from '../../model/sales-targets.types';

export function formatSalesTargetAmount(valueWan: number, unit: SalesTargetUnit, precision = 1) {
  if (unit === 'yuan') {
    return `¥${Math.round(valueWan * 10_000).toLocaleString('zh-CN')}`;
  }
  if (unit === 'thousand') {
    return `¥${(valueWan * 10).toLocaleString('zh-CN', {
      minimumFractionDigits: precision,
      maximumFractionDigits: precision,
    })}千`;
  }
  if (unit === 'million') {
    return `¥${(valueWan / 100).toLocaleString('zh-CN', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    })}百万`;
  }
  return `¥${valueWan.toLocaleString('zh-CN', {
    minimumFractionDigits: precision,
    maximumFractionDigits: precision,
  })}万`;
}

export function useSalesTargets(fixture: typeof salesTargetsReferenceFixture) {
  const defaults = fixture.defaults!;
  const rows = ref<SalesTargetReferenceRow[]>(cloneReferenceRows(fixture.rows));
  const unit = ref<SalesTargetUnit>(defaults.unit as SalesTargetUnit);
  const activeTab = ref<SalesTargetTab>(defaults.tab as SalesTargetTab);
  const createOpen = ref(false);
  const detailOpen = ref(false);
  const activeRow = ref<SalesTargetReferenceRow | null>(null);
  const submitting = ref(false);
  const draft = reactive<SalesTargetDraft>({
    name: '',
    kind: 'team',
    targetWan: '',
    owner: '华东销售一部',
  });

  const teams = computed(() => rows.value.filter((row): row is SalesTargetTeamRow => row.role === 'team'));
  const people = computed(() =>
    rows.value
      .filter((row): row is SalesTargetPersonRow => row.role === 'person')
      .sort((left, right) => left.rank - right.rank),
  );

  const summaries = computed<readonly TargetSummaryMetric[]>(() =>
    salesTargetSummaryDefinitions.map((summary) => ({
      id: summary.id,
      label: summary.label,
      description: summary.description,
      rate: Number(((summary.actualWan / summary.targetWan) * 100).toFixed(1)),
      target: formatSalesTargetAmount(summary.targetWan, unit.value, 1),
      actualLabel: summary.actualLabel,
      actual: formatSalesTargetAmount(summary.actualWan, unit.value, 1),
      tone: summary.tone,
      icon: summary.icon,
    })),
  );

  const surfaceModel = computed<TargetPageSurfaceModel>(() => ({
    module: '销售绩效',
    label: '销售目标',
    path: '/sales-perf/sales-targets',
    title: '销售目标管理',
    description: '设置和追踪销售目标，实时计算完成进度与动态调整建议',
    primaryActionLabel: '新建目标',
    periodLabel: String(defaults.period),
    participantLabel: `${people.value.length} 人参与考核`,
    units: salesTargetUnits,
    tabs: salesTargetTabs,
    summaries: summaries.value,
  }));

  const detailTitle = computed(() => activeRow.value?.name ?? '目标详情');
  const detailFacts = computed(() => {
    const row = activeRow.value;
    if (!row) return [];
    const common = [
      { label: '目标金额', value: formatSalesTargetAmount(row.targetWan, unit.value, 1) },
      { label: '已完成', value: formatSalesTargetAmount(row.actualWan, unit.value, row.actualPrecision) },
      { label: '完成率', value: `${row.rate.toFixed(1)}%` },
    ];
    return row.role === 'team'
      ? [...common, { label: '参与人数', value: `${row.memberCount} 人` }]
      : [{ label: '当前排名', value: `第 ${row.rank} 名` }, ...common, { label: '所属团队', value: row.teamName }];
  });

  const ownerOptions = computed(() => teams.value.map((team) => ({ value: team.name, label: team.name })));

  function setUnit(value: string) {
    if (salesTargetUnits.some((item) => item.value === value)) unit.value = value as SalesTargetUnit;
  }

  function setActiveTab(value: string) {
    if (salesTargetTabs.some((item) => item.value === value)) activeTab.value = value as SalesTargetTab;
  }

  function openDetail(row: SalesTargetReferenceRow) {
    activeRow.value = row;
    detailOpen.value = true;
  }

  function resetDraft() {
    draft.name = '';
    draft.kind = 'team';
    draft.targetWan = '';
    draft.owner = teams.value[0]?.name ?? '华东销售一部';
  }

  function openCreate() {
    resetDraft();
    createOpen.value = true;
  }

  function createTarget() {
    const targetWan = Number(draft.targetWan);
    if (!draft.name.trim() || !Number.isFinite(targetWan) || targetWan <= 0) {
      toast.error('请填写有效的目标名称和目标金额');
      return;
    }
    submitting.value = true;
    const id = `local-target-${Date.now()}`;
    if (draft.kind === 'team') {
      rows.value = [
        ...teams.value,
        {
          id,
          role: 'team',
          code: `TEAM-LOCAL-${teams.value.length + 1}`,
          name: draft.name.trim(),
          targetWan,
          actualWan: 0,
          actualPrecision: 1,
          rate: 0,
          memberCount: 0,
        },
        ...people.value,
      ];
      activeTab.value = 'team';
    } else {
      rows.value = [
        ...teams.value,
        ...people.value,
        {
          id,
          role: 'person',
          code: `PERSON-LOCAL-${people.value.length + 1}`,
          rank: people.value.length + 1,
          name: draft.name.trim(),
          teamName: draft.owner,
          targetWan,
          actualWan: 0,
          actualPrecision: 1,
          rate: 0,
        },
      ];
      activeTab.value = 'personal';
    }
    submitting.value = false;
    createOpen.value = false;
    toast.success(`已在本地创建目标：${draft.name.trim()}`);
  }

  return {
    unit,
    activeTab,
    createOpen,
    detailOpen,
    activeRow,
    submitting,
    draft,
    teams,
    people,
    summaries,
    surfaceModel,
    detailTitle,
    detailFacts,
    ownerOptions,
    setUnit,
    setActiveTab,
    openDetail,
    openCreate,
    createTarget,
  };
}
