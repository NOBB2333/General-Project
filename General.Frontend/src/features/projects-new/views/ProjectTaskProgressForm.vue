<script setup lang="ts">
import { computed } from 'vue';
import { UiButton, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';

interface TaskProgressDraft {
  blockReason: string;
  hours: string;
  status: string;
  summary: string;
  workDate: string;
}

const props = withDefaults(defineProps<{ disabled?: boolean; saving?: boolean }>(), {
  disabled: false,
  saving: false,
});
const draft = defineModel<TaskProgressDraft>({ required: true });
const emit = defineEmits<{ submit: [] }>();

const taskStatuses = ['未开始', '进行中', '阻塞', '测试中', '验收中', '已完成', '已关闭'];
const isBlocked = computed(() => draft.value.status === '阻塞');
const statusOptions = computed<UiSelectOption[]>(() =>
  [...new Set([...taskStatuses, draft.value.status])].map((value) => ({ value, label: value })),
);
</script>

<template>
  <form
    :class="$style.form"
    @submit.prevent="emit('submit')"
  >
    <header :class="$style.sectionHeader">
      <div>
        <h3>登记进展</h3>
        <p>提交后追加当前登录用户的工时，并更新任务执行状态。</p>
      </div>
    </header>
    <div :class="$style.formGrid">
      <UiFormField
        label="进展状态"
        required
      >
        <UiSelect
          v-model="draft.status"
          label="进展状态"
          :options="statusOptions"
          :disabled="disabled || saving"
        />
      </UiFormField>
      <UiFormField
        label="本次工时（小时）"
        required
        hint="大于 0，最多 24 小时；服务端按 0.1 小时精度保存。"
      >
        <UiInput
          v-model="draft.hours"
          type="number"
          min="0.1"
          max="24"
          step="0.1"
          required
          :disabled="disabled || saving"
        />
      </UiFormField>
      <UiFormField
        label="工作日期"
        required
      >
        <UiInput
          v-model="draft.workDate"
          type="date"
          required
          :disabled="disabled || saving"
        />
      </UiFormField>
      <UiFormField
        v-if="isBlocked"
        label="阻塞原因"
        required
        wide
      >
        <UiInput
          v-model="draft.blockReason"
          required
          :disabled="disabled || saving"
        />
      </UiFormField>
      <UiFormField
        label="进展说明"
        required
        wide
      >
        <UiTextarea
          v-model="draft.summary"
          required
          rows="3"
          :disabled="disabled || saving"
        />
      </UiFormField>
    </div>
    <div :class="$style.actions">
      <UiButton
        type="submit"
        variant="primary"
        :loading="saving"
        :disabled="disabled"
      >
        提交进展与工时
      </UiButton>
    </div>
  </form>
</template>

<style module>
@reference "../../../styles/index.css";
.form {
  @apply rounded border border-[#DFE1E6] bg-white p-4;
}
.sectionHeader {
  @apply flex items-start justify-between gap-3;
}
.sectionHeader h3 {
  @apply text-[13px] font-semibold text-[#172B4D];
}
.sectionHeader p {
  @apply mt-1 text-[10px] text-[#6B778C];
}
.formGrid {
  @apply mt-3 grid grid-cols-2 gap-3;
}
.actions {
  @apply mt-4 flex justify-end;
}
</style>
