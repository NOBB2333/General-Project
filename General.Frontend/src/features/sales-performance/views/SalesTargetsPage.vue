<script setup lang="ts">
import { computed } from 'vue';
import { TargetPageSurface } from '@/components/page';
import { UiButton, UiDialog, UiInput, UiSelect, UiTabPanel } from '@/components/ui';
import { salesTargetsReferenceFixture } from '../model/sales-targets.data';
import type { SalesTargetReferenceRow } from '../model/sales-targets.types';
import SalesTargetComparison from './sales-targets/SalesTargetComparison.vue';
import SalesTargetRanking from './sales-targets/SalesTargetRanking.vue';
import { formatSalesTargetAmount, useSalesTargets } from './sales-targets/useSalesTargets';

const kindOptions = [
  { value: 'team', label: '团队目标' },
  { value: 'person', label: '个人目标' },
] as const;

const {
  unit,
  activeTab,
  createOpen,
  detailOpen,
  submitting,
  draft,
  teams,
  people,
  surfaceModel,
  detailTitle,
  detailFacts,
  ownerOptions,
  setUnit,
  setActiveTab,
  openDetail,
  openCreate,
  createTarget,
} = useSalesTargets(salesTargetsReferenceFixture);

const detailDescription = computed(() => (detailFacts.value.length ? '2026 年度销售目标完成情况' : '查看销售目标详情'));

function selectTarget(row: SalesTargetReferenceRow) {
  openDetail(row);
}

function updateDraftKind(value: string) {
  if (value === 'team' || value === 'person') draft.kind = value;
}
</script>

<template>
  <TargetPageSurface
    :model="surfaceModel"
    :unit="unit"
    :active-tab="activeTab"
    :data-target-local-row-count="teams.length + people.length"
    @update:unit="setUnit"
    @update:active-tab="setActiveTab"
    @primary-action="openCreate"
  >
    <UiTabPanel value="overview">
      <section :class="$style.overviewGrid">
        <SalesTargetComparison
          :teams="teams"
          :unit="unit"
          :format-amount="formatSalesTargetAmount"
          @select="selectTarget"
        />
        <SalesTargetRanking
          :people="people"
          :unit="unit"
          :format-amount="formatSalesTargetAmount"
          @select="selectTarget"
        />
      </section>
    </UiTabPanel>

    <UiTabPanel value="breakdown">
      <section :class="$style.singlePanel">
        <SalesTargetComparison
          :teams="teams"
          :unit="unit"
          :format-amount="formatSalesTargetAmount"
          @select="selectTarget"
        />
      </section>
    </UiTabPanel>

    <UiTabPanel value="personal">
      <section :class="$style.narrowPanel">
        <SalesTargetRanking
          :people="people"
          :unit="unit"
          :format-amount="formatSalesTargetAmount"
          @select="selectTarget"
        />
      </section>
    </UiTabPanel>

    <UiTabPanel value="team">
      <section :class="$style.singlePanel">
        <SalesTargetComparison
          :teams="teams"
          :unit="unit"
          :format-amount="formatSalesTargetAmount"
          @select="selectTarget"
        />
      </section>
    </UiTabPanel>
  </TargetPageSurface>

  <UiDialog
    v-model:open="detailOpen"
    :title="detailTitle"
    :description="detailDescription"
    size="sm"
  >
    <dl
      :class="$style.detailGrid"
      data-target-detail
    >
      <div
        v-for="fact in detailFacts"
        :key="fact.label"
      >
        <dt>{{ fact.label }}</dt>
        <dd>{{ fact.value }}</dd>
      </div>
    </dl>
    <template #footer>
      <UiButton
        variant="primary"
        size="sm"
        @click="detailOpen = false"
      >
        知道了
      </UiButton>
    </template>
  </UiDialog>

  <UiDialog
    v-model:open="createOpen"
    title="新建销售目标"
    description="新增目标只写入本地运行态数据"
    size="md"
  >
    <form
      :class="$style.form"
      data-target-create-form
      @submit.prevent="createTarget"
    >
      <label :class="$style.field">
        <span>目标类型</span>
        <UiSelect
          :model-value="draft.kind"
          :options="kindOptions"
          label="目标类型"
          @update:model-value="updateDraftKind"
        />
      </label>
      <label :class="$style.field">
        <span>目标名称</span>
        <UiInput
          v-model="draft.name"
          placeholder="请输入团队或员工名称"
          autocomplete="off"
        />
      </label>
      <label
        v-if="draft.kind === 'person'"
        :class="$style.field"
      >
        <span>所属团队</span>
        <UiSelect
          v-model="draft.owner"
          :options="ownerOptions"
          label="所属团队"
        />
      </label>
      <label :class="$style.field">
        <span>年度目标（万元）</span>
        <UiInput
          v-model="draft.targetWan"
          type="number"
          min="0.01"
          step="0.01"
          placeholder="0.00"
        />
      </label>
    </form>
    <template #footer>
      <UiButton
        variant="secondary"
        size="sm"
        @click="createOpen = false"
      >
        取消
      </UiButton>
      <UiButton
        variant="primary"
        size="sm"
        :loading="submitting"
        @click="createTarget"
      >
        创建目标
      </UiButton>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.overviewGrid {
  @apply grid min-w-0 gap-4 pb-6 pt-3;
  grid-template-columns: minmax(0, 3fr) minmax(330px, 2fr);
}

.singlePanel,
.narrowPanel {
  @apply min-w-0 pb-6 pt-3;
}

.narrowPanel {
  max-width: 640px;
}

.detailGrid {
  @apply grid grid-cols-2 gap-3 p-5;
}

.detailGrid div {
  @apply rounded border px-3 py-2.5;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.detailGrid dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.detailGrid dd {
  @apply mt-1 text-[12px] font-semibold;
}

.form {
  @apply grid grid-cols-2 gap-4 p-5;
}

.field {
  @apply min-w-0;
}

.field > span {
  @apply mb-1.5 block text-[11px] font-semibold;
  color: var(--ui-text-subtle);
}

@media (max-width: 1120px) {
  .overviewGrid {
    grid-template-columns: minmax(0, 1fr);
  }
}
</style>
