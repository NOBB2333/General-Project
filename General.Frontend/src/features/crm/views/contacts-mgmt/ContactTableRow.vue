<script setup lang="ts">
import { Building2, Mail, MoreHorizontal, Phone, Star } from 'lucide-vue-next';
import type { ContactRow } from '@/features/crm/model/contacts-mgmt.types';

defineProps<{ row: ContactRow }>();
</script>

<template>
  <tr :class="$style.row">
    <td>
      <div :class="$style.nameCell">
        <Star
          v-if="row.isPrimary"
          :size="14"
          :fill="'#FFAB00'"
          :stroke-width="1.8"
          :class="$style.star"
        />
        <strong>{{ row.name }}</strong>
      </div>
    </td>
    <td>
      <strong :class="$style.position">{{ row.position }}</strong
      ><span :class="$style.subline">{{ row.department }}</span>
    </td>
    <td>
      <div :class="$style.company">
        <Building2 :size="14" /> <strong>{{ row.company }}</strong>
      </div>
      <div
        v-if="row.companyExtra"
        :class="$style.companyExtra"
      >
        ↳ {{ row.companyExtra }}
      </div>
    </td>
    <td>
      <span :class="[$style.status, $style[row.status]]">{{ row.status }}</span>
    </td>
    <td>
      <span :class="$style.decision">{{ row.decision }}</span>
    </td>
    <td>
      <div :class="$style.contactLine"><Phone :size="13" />{{ row.phone }}</div>
      <div :class="$style.contactLine"><Mail :size="13" />{{ row.email }}</div>
    </td>
    <td>
      <span :class="$style.owner"
        ><i>{{ row.ownerInitial }}</i
        >{{ row.owner }}</span
      >
    </td>
    <td>
      <button
        type="button"
        :class="$style.more"
        :aria-label="`更多操作：${row.name}`"
        title="更多操作"
      >
        <MoreHorizontal :size="16" />
      </button>
    </td>
  </tr>
</template>

<style module>
@reference "../../../../styles/index.css";
.row {
  @apply h-[74px] border-b transition-colors;
  border-color: var(--ui-border-subtle);
}
.row:hover {
  background: var(--ui-surface-hover);
}
.row td {
  @apply px-4 align-middle;
}
.nameCell {
  @apply flex items-center gap-1.5;
  color: var(--ui-text);
}
.nameCell strong {
  @apply text-[13px];
}
.star {
  color: #ffab00;
}
.position {
  @apply block text-[12px] font-medium;
  color: var(--ui-text);
}
.subline {
  @apply mt-0.5 block text-[11px];
  color: #97a0af;
}
.company {
  @apply flex items-center gap-1.5 whitespace-nowrap;
  color: var(--app-primary);
}
.company strong {
  @apply text-[12px] font-semibold;
}
.companyExtra {
  @apply mt-1 truncate text-[10px];
  color: var(--ui-text-subtle);
}
.status {
  @apply inline-flex rounded px-2 py-0.5 text-[11px] font-medium;
}
.在职 {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.已跳槽 {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.已离职,
.停用 {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.decision {
  color: var(--ui-text-muted);
}
.contactLine {
  @apply flex items-center gap-1.5 whitespace-nowrap text-[11px] leading-5;
  color: var(--ui-text-muted);
}
.contactLine svg {
  color: #97a0af;
}
.owner {
  @apply inline-flex items-center gap-1.5 whitespace-nowrap;
  color: var(--ui-text-muted);
}
.owner i {
  @apply flex h-5 w-5 items-center justify-center rounded-full text-[10px] not-italic text-white;
  background: var(--app-primary);
}
.more {
  @apply flex h-7 w-7 items-center justify-center rounded text-[#6B778C] opacity-0 transition-opacity hover:bg-[#F4F5F7];
}
.row:hover .more,
.more:focus-visible {
  opacity: 1;
}
</style>
