<script setup lang="ts">
import { Building2, Download, Plus, Users } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { LoanView } from '../../model/loans.types';

defineProps<{ view: LoanView; employeeCount: number; bankCount: number }>();
const emit = defineEmits<{
  'update:view': [value: LoanView];
  export: [];
  create: [];
}>();
</script>

<template>
  <section :class="$style.toolbar">
    <div
      :class="$style.viewSwitch"
      role="group"
      aria-label="借款类型"
    >
      <button
        type="button"
        :class="[$style.viewButton, view === 'employee' && $style.viewActive]"
        :aria-pressed="view === 'employee'"
        @click="emit('update:view', 'employee')"
      >
        <Users
          :size="15"
          :stroke-width="1.8"
        />员工借款 <span>{{ employeeCount }}</span>
      </button>
      <button
        type="button"
        :class="[$style.viewButton, view === 'bank' && $style.viewActive]"
        :aria-pressed="view === 'bank'"
        @click="emit('update:view', 'bank')"
      >
        <Building2
          :size="15"
          :stroke-width="1.8"
        />银行贷款 <span>{{ bankCount }}</span>
      </button>
    </div>
    <div :class="$style.actions">
      <UiButton
        variant="secondary"
        size="sm"
        @click="emit('export')"
      >
        <template #leading
          ><Download
            :size="14"
            :stroke-width="1.8" /></template
        >导出
      </UiButton>
      <UiButton
        v-if="view === 'employee'"
        variant="primary"
        size="sm"
        @click="emit('create')"
      >
        <template #leading
          ><Plus
            :size="14"
            :stroke-width="1.8" /></template
        >申请员工借款
      </UiButton>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.toolbar {
  @apply mt-4 flex items-center justify-between;
}
.viewSwitch {
  @apply flex items-center gap-1;
}
.viewButton {
  @apply flex min-h-9 items-center gap-2 rounded border px-5 text-[12px] font-semibold transition-colors;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface);
}
.viewButton:hover {
  background: var(--ui-surface-hover);
}
.viewButton span {
  @apply inline-flex h-[19px] min-w-[19px] items-center justify-center rounded-full px-1 text-[9px];
  background: var(--ui-surface-hover);
}
.viewActive {
  color: white;
  border-color: var(--app-primary);
  background: var(--app-primary);
}
.viewActive:hover {
  background: color-mix(in srgb, var(--app-primary) 84%, black);
}
.viewActive span {
  background: rgb(255 255 255 / 18%);
}
.actions {
  @apply flex items-center gap-2;
}
</style>
