<script setup lang="ts">
import { computed, ref } from 'vue';
import { Send } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, toast } from '@/components/ui';
import { benefitsPayoutFixture, codeInventory, physicalInventory } from '../model/benefits-payout.data';
import type { BenefitDistribution, BenefitDraft, BenefitView } from '../model/benefits-payout.types';
import BenefitDistributionDialog from './benefits-payout/BenefitDistributionDialog.vue';
import BenefitsPayoutHero from './benefits-payout/BenefitsPayoutHero.vue';
import BenefitsPayoutSummary from './benefits-payout/BenefitsPayoutSummary.vue';
import BenefitsPayoutWorkspace from './benefits-payout/BenefitsPayoutWorkspace.vue';
const rows = ref<BenefitDistribution[]>(cloneReferenceRows(benefitsPayoutFixture.rows));
const view = ref<BenefitView>('发放记录');
const formOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<BenefitDistribution | null>(null);
const inventory = computed(() => (view.value === '兑换码库' ? codeInventory : physicalInventory));
const money = (value: number) => `¥${value.toLocaleString('zh-CN', { minimumFractionDigits: 2 })}`;
function openRow(row: BenefitDistribution) {
  activeRow.value = row;
  detailOpen.value = true;
}
function saveDistribution(draft: BenefitDraft) {
  rows.value = [
    {
      id: `benefit-${Date.now()}`,
      title: draft.title.trim(),
      kind: draft.kind,
      date: '2026-09-17',
      owner: '孙丽萍',
      source: draft.source.trim(),
      audience: draft.audience.trim(),
      people: Number(draft.people),
      quantityPerPerson: Number(draft.quantityPerPerson),
      totalValue: Number(draft.totalValue),
      note: draft.note.trim() || '本地新增福利发放',
      status: '已完成',
    },
    ...rows.value,
  ];
  formOpen.value = false;
  view.value = '发放记录';
  toast.success('福利发放已加入本地记录');
}
</script>
<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="cards"
    data-page-route="/hr/benefits-payout"
    data-page-label="福利发放"
    data-page-module="人事管理"
    data-benefits-payout-page
    :data-view="view"
    :data-total-count="rows.length"
  >
    <BenefitsPayoutHero />
    <div :class="$style.actions">
      <UiButton
        variant="primary"
        size="sm"
        @click="formOpen = true"
        ><template #leading
          ><Send
            :size="14"
            :stroke-width="1.8" /></template
        >新建发放</UiButton
      >
    </div>
    <BenefitsPayoutSummary /><BenefitsPayoutWorkspace
      v-model:view="view"
      :rows="rows"
      :inventory="inventory"
      @open="openRow"
    />
  </main>
  <BenefitDistributionDialog
    v-model:open="formOpen"
    @save="saveDistribution"
  /><UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.title || '发放详情'"
    :description="activeRow ? `${activeRow.date} · ${activeRow.owner} · ${activeRow.kind}` : undefined"
    size="lg"
    ><dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>福利来源</dt>
        <dd>{{ activeRow.source }}</dd>
      </div>
      <div>
        <dt>发放对象</dt>
        <dd>{{ activeRow.audience }}</dd>
      </div>
      <div>
        <dt>人数 × 每人</dt>
        <dd>{{ activeRow.people }} × {{ activeRow.quantityPerPerson }}</dd>
      </div>
      <div>
        <dt>发放总值</dt>
        <dd>{{ money(activeRow.totalValue) }}</dd>
      </div>
      <div :class="$style.wide">
        <dt>备注</dt>
        <dd>{{ activeRow.note }}</dd>
      </div>
    </dl>
    <template #footer
      ><UiButton
        variant="primary"
        size="sm"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    ></UiDialog
  >
</template>
<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1280px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.actions {
  @apply flex h-[50px] items-end justify-end;
}
.details {
  @apply grid grid-cols-2 gap-4 p-5;
}
.details div {
  @apply rounded border p-4;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
.details dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.details dd {
  @apply m-0 mt-2 text-[12px] font-medium;
}
.wide {
  @apply col-span-2;
}
</style>
