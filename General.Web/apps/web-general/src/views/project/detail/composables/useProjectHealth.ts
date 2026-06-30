import { computed, type Ref } from 'vue';

import type { ProjectApi } from '#/api/core';

export type ProjectHealthActionItem = {
  actionKey: string;
  actionLabel: string;
  dataSource: string;
  label: string;
  reason: string;
  ruleVersion: string;
  score: number;
};

type UseProjectHealthOptions = {
  detail: Ref<null | ProjectApi.ProjectDetail>;
  formatDate: (value?: null | string) => string;
  switchTab: (tabKey: string) => void;
};

const DEFAULT_HEALTH_DATA_SOURCE =
  '项目任务、RAID 风险、缺陷问题、任务完成率';
const DEFAULT_HEALTH_RULE_VERSION = 'RuleBasedV1';

export function useProjectHealth({
  detail,
  formatDate,
  switchTab,
}: UseProjectHealthOptions) {
  const currentHealth = computed(() => {
    const current = detail.value;
    if (!current) {
      return { color: 'default', label: '-', percent: 0 };
    }

    const riskScore =
      current.healthRiskScore ??
      current.highRiskCount * 20 +
        current.openIssueCount * 10 +
        Math.max(0, 100 - current.taskCompletionRate);
    if (riskScore >= 70) {
      return {
        color: 'error',
        label: current.healthStatus || '高风险',
        percent: Math.max(20, 100 - riskScore),
      };
    }

    if (riskScore >= 35) {
      return {
        color: 'warning',
        label: current.healthStatus || '关注中',
        percent: Math.max(35, 100 - riskScore),
      };
    }

    return {
      color: 'success',
      label: current.healthStatus || '健康',
      percent: Math.max(60, 100 - riskScore),
    };
  });

  const healthMetrics = computed(() => {
    const current = detail.value;
    const breakdowns = current?.healthBreakdowns || [];
    if (breakdowns.length > 0) {
      return breakdowns.map((item) => ({
        label: item.label,
        value: `${item.score} 分`,
      }));
    }

    return [
      { label: '任务完成率', value: `${current?.taskCompletionRate || 0}%` },
      { label: '高风险事项', value: `${current?.highRiskCount || 0} 项` },
      { label: '未闭环问题', value: `${current?.openIssueCount || 0} 项` },
    ];
  });

  const healthReasonItems = computed<ProjectHealthActionItem[]>(() => {
    const breakdowns = detail.value?.healthBreakdowns || [];
    if (breakdowns.length > 0) {
      return breakdowns
        .filter((item) => item.score > 0)
        .sort((left, right) => right.score - left.score)
        .map((item) => ({
          actionKey: item.actionKey,
          actionLabel: item.actionLabel || '去处理',
          dataSource: item.dataSource || detail.value?.healthDataSource || '-',
          label: item.label,
          reason: item.reason,
          ruleVersion: item.ruleVersion || DEFAULT_HEALTH_RULE_VERSION,
          score: item.score,
        }));
    }

    return [
      {
        actionKey: '',
        actionLabel: '',
        dataSource:
          detail.value?.healthDataSource || DEFAULT_HEALTH_DATA_SOURCE,
        label: '未触发扣分',
        reason:
          detail.value?.healthReasons?.[0] ||
          '当前健康度由风险、逾期任务、未闭环问题和任务完成率规则计算。',
        ruleVersion: DEFAULT_HEALTH_RULE_VERSION,
        score: 0,
      },
    ];
  });

  const healthRuleInfo = computed(() => {
    const current = detail.value;
    const firstBreakdown = current?.healthBreakdowns?.[0];
    return [
      {
        label: '规则版本',
        value: firstBreakdown?.ruleVersion || DEFAULT_HEALTH_RULE_VERSION,
      },
      {
        label: '数据来源',
        value:
          current?.healthDataSource ||
          firstBreakdown?.dataSource ||
          DEFAULT_HEALTH_DATA_SOURCE,
      },
      {
        label: '更新时间',
        value: formatDate(current?.healthCalculatedTime),
      },
    ];
  });

  function handleHealthAction(actionKey: string) {
    const tabMap: Record<string, string> = {
      control: 'control',
      issues: 'issues',
      tasks: 'tasks',
    };
    const tabKey = tabMap[actionKey];
    if (tabKey) {
      switchTab(tabKey);
    }
  }

  return {
    currentHealth,
    handleHealthAction,
    healthMetrics,
    healthReasonItems,
    healthRuleInfo,
  };
}
