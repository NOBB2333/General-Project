<script setup lang="ts">
import { ContactRound, FileText, Package, ShieldCheck, TriangleAlert, UserRound } from 'lucide-vue-next';
import { UiButton, UiDialog } from '@/components/ui';
import type { ServiceTicketRow } from '../../model/service-tickets.types';

defineProps<{
  open: boolean;
  ticket: ServiceTicketRow | null;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
}>();
</script>

<template>
  <UiDialog
    :open="open"
    :title="ticket?.code ?? '服务工单详情'"
    :description="ticket?.title"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <div
      v-if="ticket"
      :class="$style.body"
      :data-service-ticket-detail="ticket.code"
    >
      <div :class="$style.badges">
        <span :class="$style.typeBadge">{{ ticket.type }}</span>
        <span :class="$style.urgencyBadge">{{ ticket.urgency }}</span>
        <span :class="$style.statusBadge">{{ ticket.status }}</span>
        <span
          v-if="ticket.overdue"
          :class="$style.overdueBadge"
          >超时</span
        >
      </div>

      <section :class="$style.summaryGrid">
        <article>
          <ContactRound
            :size="16"
            :stroke-width="1.8"
          />
          <span>客户 / 联系人</span>
          <strong>{{ ticket.customer }}</strong>
          <small>{{ ticket.contact }}</small>
        </article>
        <article>
          <Package
            :size="16"
            :stroke-width="1.8"
          />
          <span>产品 / SN</span>
          <strong>{{ ticket.product }}</strong>
          <small>{{ ticket.serialNumber }}</small>
        </article>
        <article>
          <UserRound
            :size="16"
            :stroke-width="1.8"
          />
          <span>负责人</span>
          <strong>{{ ticket.owner }}</strong>
          <small>创建于 {{ ticket.createdAt }}</small>
        </article>
      </section>

      <dl :class="$style.detailGrid">
        <div>
          <dt>
            <TriangleAlert
              :size="14"
              :stroke-width="1.8"
            />
            服务说明
          </dt>
          <dd>{{ ticket.description }}</dd>
        </div>
        <div>
          <dt>
            <ShieldCheck
              :size="14"
              :stroke-width="1.8"
            />
            质保与计费
          </dt>
          <dd>{{ ticket.warranty }} · {{ ticket.billable ? '有偿服务' : '免费 / 质保服务' }}</dd>
        </div>
        <div>
          <dt>
            <FileText
              :size="14"
              :stroke-width="1.8"
            />
            SLA 响应
          </dt>
          <dd>{{ ticket.response }}</dd>
        </div>
      </dl>
    </div>

    <template #footer>
      <UiButton
        variant="secondary"
        @click="emit('update:open', false)"
        >关闭</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.body {
  @apply space-y-4 p-5;
}

.badges {
  @apply flex flex-wrap items-center gap-1.5;
}

.badges span {
  @apply rounded px-2 py-1 text-[10px] font-semibold;
}

.typeBadge {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.urgencyBadge,
.overdueBadge {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}

.statusBadge {
  color: #6554c0;
  background: #eae6ff;
}

.summaryGrid {
  @apply grid grid-cols-3 gap-3;
}

.summaryGrid article {
  @apply relative min-w-0 border p-3 pl-10;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
  border-radius: var(--app-control-radius);
}

.summaryGrid article > svg {
  @apply absolute left-3 top-3;
  color: var(--app-primary);
}

.summaryGrid span,
.summaryGrid strong,
.summaryGrid small {
  @apply block truncate;
}

.summaryGrid span {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.summaryGrid strong {
  @apply mt-1 text-[12px] font-semibold;
}

.summaryGrid small {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}

.detailGrid {
  @apply grid grid-cols-2 gap-3;
}

.detailGrid > div {
  @apply border p-3;
  border-color: var(--ui-border-subtle);
  border-radius: var(--app-control-radius);
}

.detailGrid > div:first-child {
  @apply col-span-2;
}

.detailGrid dt {
  @apply flex items-center gap-1.5 text-[11px] font-semibold;
  color: var(--ui-text-muted);
}

.detailGrid dd {
  @apply mt-2 text-[12px] leading-5;
  color: var(--ui-text);
}

@media (max-width: 660px) {
  .summaryGrid,
  .detailGrid {
    @apply grid-cols-1;
  }

  .detailGrid > div:first-child {
    @apply col-span-1;
  }
}
</style>
