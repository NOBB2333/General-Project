<script setup lang="ts">
import { CalendarDays, CircleDollarSign, Gavel, Shield, UserRound } from 'lucide-vue-next';
import { UiButton, UiDialog } from '@/components/ui';
import type { TenderRow } from '@/features/crm/model/tenders.types';

defineProps<{ open: boolean; row: TenderRow | null }>();
const emit = defineEmits<{ 'update:open': [value: boolean] }>();
</script>

<template>
  <UiDialog
    :open="open"
    :title="row?.name ?? '投标项目详情'"
    :description="row ? `${row.code} · ${row.customer}` : undefined"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <template #icon
      ><span :class="$style.dialogIcon"><Gavel :size="18" /></span
    ></template>
    <div
      v-if="row"
      :class="$style.detail"
    >
      <section>
        <article>
          <UserRound :size="15" /><span>负责人</span><strong>{{ row.owner }}</strong>
        </article>
        <article>
          <CalendarDays :size="15" /><span>投标截止</span><strong>{{ row.deadline }}</strong>
        </article>
        <article>
          <CalendarDays :size="15" /><span>开标时间</span><strong>{{ row.openingDate }}</strong>
        </article>
        <article>
          <CircleDollarSign :size="15" /><span>保证金</span
          ><strong>¥{{ row.depositAmount.toLocaleString('zh-CN') }}</strong>
        </article>
      </section>
      <dl>
        <div>
          <dt>当前阶段</dt>
          <dd>{{ row.stage }}</dd>
        </div>
        <div>
          <dt>投标结果</dt>
          <dd>{{ row.result }}</dd>
        </div>
        <div>
          <dt>保证金状态</dt>
          <dd>{{ row.depositStatus }}</dd>
        </div>
        <div>
          <dt>招标方式</dt>
          <dd>{{ row.method }}</dd>
        </div>
        <div>
          <dt>优先级</dt>
          <dd>{{ row.priority }}</dd>
        </div>
        <div>
          <dt>风险提醒</dt>
          <dd>{{ row.risks.join('、') || '暂无风险' }}</dd>
        </div>
      </dl>
      <p><Shield :size="15" />项目数据仅保存在当前本地会话；刷新后恢复参考 fixture。</p>
    </div>
    <template #footer="{ close }"
      ><UiButton
        variant="primary"
        @click="close"
        >关闭</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded;
  color: #6554c0;
  background: #eae6ff;
}
.detail {
  @apply p-5;
}
.detail > section {
  @apply grid grid-cols-4 gap-3;
}
.detail article {
  @apply rounded border p-3;
  border-color: var(--ui-border);
}
.detail article > svg {
  color: var(--app-primary);
}
.detail article span,
.detail article strong {
  @apply block;
}
.detail article span {
  @apply mt-2 text-[10px];
  color: var(--ui-text-subtle);
}
.detail article strong {
  @apply mt-1 text-[12px];
}
.detail dl {
  @apply mt-4 grid grid-cols-2 rounded border;
  border-color: var(--ui-border);
}
.detail dl > div {
  @apply flex min-h-11 items-center border-b px-4 text-[12px];
  border-color: var(--ui-border-subtle);
}
.detail dt {
  @apply w-28;
  color: var(--ui-text-subtle);
}
.detail dd {
  @apply font-medium;
}
.detail > p {
  @apply mt-4 flex items-center gap-2 rounded px-3 py-2 text-[11px];
  color: var(--ui-text-muted);
  background: var(--app-primary-soft);
}
</style>
