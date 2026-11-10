<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, type UiSelectOption } from '@/components/ui';
import type { StocktakingDraft, StocktakingType } from '../../model/stocktaking.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: StocktakingDraft] }>();
const typeOptions: UiSelectOption[] = ['全盘', '抽盘', '清盘'].map((value) => ({ value, label: value }));
const warehouseOptions: UiSelectOption[] = ['上海主仓', '备品备件仓', '华能DCS项目仓', '中石化仪控项目仓'].map(
  (value) => ({ value, label: value }),
);
const defaults: StocktakingDraft = {
  name: '',
  description: '',
  warehouse: '上海主仓',
  type: '全盘',
  plannedDate: '2026-03-31',
};
const draft = reactive<StocktakingDraft>({ ...defaults });
watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, { ...defaults, type: '全盘' as StocktakingType });
  },
);
function submit() {
  if (!draft.name.trim()) return;
  emit('save', { ...draft, name: draft.name.trim(), description: draft.description.trim() });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建盘点"
    description="创建本地盘点计划并锁定账面快照"
    size="lg"
    @update:open="emit('update:open', $event)"
    ><form
      :class="$style.form"
      @submit.prevent="submit"
    >
      <UiFormField
        label="盘点名称"
        required
        :class="$style.wide"
        ><UiInput
          v-model="draft.name"
          placeholder="输入盘点计划名称" /></UiFormField
      ><UiFormField label="仓库"
        ><UiSelect
          v-model="draft.warehouse"
          :options="warehouseOptions"
          label="盘点仓库" /></UiFormField
      ><UiFormField label="盘点类型"
        ><UiSelect
          v-model="draft.type"
          :options="typeOptions"
          label="盘点类型" /></UiFormField
      ><UiFormField label="计划日期"
        ><UiInput
          v-model="draft.plannedDate"
          type="date" /></UiFormField
      ><UiFormField label="说明"><UiInput v-model="draft.description" /></UiFormField>
    </form>
    <template #footer
      ><UiButton
        size="sm"
        variant="secondary"
        @click="emit('update:open', false)"
        >取消</UiButton
      ><UiButton
        size="sm"
        variant="primary"
        @click="submit"
        >保存盘点</UiButton
      ></template
    ></UiDialog
  >
</template>

<style module>
@reference "../../../../styles/index.css";
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.wide {
  @apply col-span-2;
}
</style>
