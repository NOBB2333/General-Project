import { computed, ref, type Ref } from 'vue';

import { message } from 'ant-design-vue';

import {
  updateProjectRaidRiskApi,
  type ProjectApi,
} from '#/api/core';

type UseProjectRisksOptions = {
  detail: Ref<null | ProjectApi.ProjectDetail>;
  refreshSelectedProjectDetail: () => Promise<void>;
  showRequestError: (error: unknown, fallback: string) => void;
};

export function useProjectRisks(options: UseProjectRisksOptions) {
  const { detail, refreshSelectedProjectDetail, showRequestError } = options;

  const savingRaidRiskId = ref('');
  const raidRiskForms = ref<Record<string, ProjectApi.RaidRiskUpdateInput>>({});

  const riskResponseOptions = ['规避', '转移', '减轻', '接受', '升级'].map(
    (value) => ({ label: value, value }),
  );

  const activeRiskOptions = computed(() =>
    (detail.value?.raidItems || [])
      .filter((item) => item.type === '风险')
      .map((item) => ({
        label: `${item.level} · ${item.title}`,
        value: item.id,
      })),
  );

  const raidRiskRows = computed(() =>
    (detail.value?.raidItems || []).map((item) => {
      let form = raidRiskForms.value[item.id];
      if (!form) {
        form = buildRaidRiskForm(item);
        raidRiskForms.value[item.id] = form;
      }

      return { form, id: item.id, item };
    }),
  );

  const riskAlerts = computed(() =>
    (detail.value?.raidItems || [])
      .filter((item) => item.level === 'P0' || item.isOverdue)
      .slice(0, 4),
  );

  function resetRiskForms(current = detail.value) {
    raidRiskForms.value = Object.fromEntries(
      (current?.raidItems || []).map((item) => [
        item.id,
        buildRaidRiskForm(item),
      ]),
    );
  }

  async function handleUpdateRaidRisk(raidItemId: string) {
    const form = raidRiskForms.value[raidItemId];
    if (!form) {
      return;
    }

    savingRaidRiskId.value = raidItemId;
    try {
      await updateProjectRaidRiskApi(raidItemId, form);
      message.success('风险评分已更新');
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '风险评分更新失败');
    } finally {
      savingRaidRiskId.value = '';
    }
  }

  function buildRaidRiskForm(item: ProjectApi.RaidItem): ProjectApi.RaidRiskUpdateInput {
    return {
      contingencyPlan: item.contingencyPlan || '',
      impact: item.impact || 0,
      probability: item.probability || 0,
      responseStrategy: item.responseStrategy || '',
      triggerCondition: item.triggerCondition || '',
    };
  }

  return {
    activeRiskOptions,
    handleUpdateRaidRisk,
    raidRiskRows,
    resetRiskForms,
    riskAlerts,
    riskResponseOptions,
    savingRaidRiskId,
  };
}
