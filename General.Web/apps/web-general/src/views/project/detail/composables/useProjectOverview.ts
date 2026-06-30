import { computed, type ComputedRef, type Ref } from 'vue';

import type { ProjectApi } from '#/api/core';

type HealthSummary = {
  label: string;
};

type UseProjectOverviewOptions = {
  currentHealth: ComputedRef<HealthSummary>;
  currentUserAliases: ComputedRef<string[]>;
  currentUserId: Ref<string>;
  detail: Ref<null | ProjectApi.ProjectDetail>;
  formatDate: (value?: null | string) => string;
  formatMoney: (value?: null | number) => string;
  formatRatio: (value?: null | number) => string;
  relatedTaskCount: ComputedRef<number>;
};

export function useProjectOverview(options: UseProjectOverviewOptions) {
  const {
    currentHealth,
    currentUserAliases,
    currentUserId,
    detail,
    formatDate,
    formatMoney,
    formatRatio,
    relatedTaskCount,
  } = options;

  const memberSelectOptions = computed(() =>
    (detail.value?.members || []).map((item) => ({
      label: item.userName,
      value: item.userId,
    })),
  );

  const currentProjectUserId = computed(() => {
    if (currentUserId.value) {
      const matchedById = (detail.value?.members || []).find(
        (item) => item.userId === currentUserId.value,
      );
      if (matchedById) {
        return matchedById.userId;
      }
    }

    const aliases = currentUserAliases.value;
    const matchedByName = (detail.value?.members || []).find((item) =>
      aliases.includes(item.userName),
    );
    return matchedByName?.userId || detail.value?.members[0]?.userId || '';
  });

  const compactMetrics = computed(() => {
    const current = detail.value;
    const snapshot = current?.performanceSnapshot;
    return [
      { label: '健康度', value: currentHealth.value.label },
      { label: '任务完成率', value: `${current?.taskCompletionRate || 0}%` },
      { label: 'SPI / CPI', value: `${formatRatio(snapshot?.schedulePerformanceIndex)} / ${formatRatio(snapshot?.costPerformanceIndex)}` },
      { label: '待处理问题', value: `${current?.openIssueCount || 0}` },
      {
        label: '成员 / 工时',
        value: `${current?.memberCount || 0} / ${current?.totalWorklogHours || 0}小时`,
      },
    ];
  });

  const overviewDetailSections = computed(() => {
    const current = detail.value;

    return [
      {
        items: [
          { label: '项目编号', value: current?.projectCode || '-' },
          { label: '项目类型', value: current?.projectType || '-' },
          { label: '项目来源', value: current?.projectSource || '-' },
          { label: '所属部门', value: current?.organizationUnitName || '-' },
          { label: '项目经理', value: current?.managerName || '-' },
          { label: '发起人', value: current?.sponsorName || '-' },
        ],
        title: '基础信息',
      },
      {
        items: [
          { label: '计划开始', value: formatDate(current?.plannedStartDate) },
          { label: '计划结束', value: formatDate(current?.plannedEndDate) },
          {
            label: 'SPI',
            value: formatRatio(current?.performanceSnapshot?.schedulePerformanceIndex),
          },
          {
            label: 'CPI',
            value: formatRatio(current?.performanceSnapshot?.costPerformanceIndex),
          },
          {
            label: '合同金额',
            value: formatMoney(current?.contractTotalAmount),
          },
          { label: '预算金额', value: formatMoney(current?.budgetTotalAmount) },
          { label: '已回款', value: formatMoney(current?.receivedAmount) },
          { label: '关键项目', value: current?.isKeyProject ? '是' : '否' },
        ],
        title: '计划与经营',
      },
    ];
  });

  const relatedIssueCount = computed(
    () =>
      (detail.value?.issues || []).filter((item) =>
        [
          item.ownerName,
          item.productOwnerName,
          item.developerOwnerName,
          item.testerOwnerName,
        ]
          .filter((name): name is string => Boolean(name))
          .some((name) => matchesCurrentUser(name)),
      ).length,
  );

  const relatedWorklogCount = computed(
    () =>
      (detail.value?.worklogs || []).filter((item) =>
        matchesCurrentUser(item.userName),
      ).length,
  );

  const collaborationMetrics = computed(() => {
    const current = detail.value;
    return [
      { label: '我的项目关系', value: current?.myRelation || '-' },
      { label: '我负责事项', value: `${relatedTaskCount.value}` },
      { label: '我跟进问题', value: `${relatedIssueCount.value}` },
      { label: '我的工时记录', value: `${relatedWorklogCount.value}` },
      { label: '成员规模', value: `${current?.memberCount || 0} 人` },
      { label: '累计工时', value: `${current?.totalWorklogHours || 0} 小时` },
    ];
  });

  function matchesCurrentUser(name: string) {
    return currentUserAliases.value.some((alias) => alias === name);
  }

  return {
    collaborationMetrics,
    compactMetrics,
    currentProjectUserId,
    memberSelectOptions,
    overviewDetailSections,
  };
}
