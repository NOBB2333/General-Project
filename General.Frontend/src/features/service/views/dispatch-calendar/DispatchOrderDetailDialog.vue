<script setup lang="ts">
import {
  CalendarClock,
  ContactRound,
  ExternalLink,
  FileText,
  Package,
  ShieldCheck,
  TriangleAlert,
  UserRound,
  Wrench,
  X,
} from 'lucide-vue-next';
import { UiButton, UiDialog } from '@/components/ui';
import type { DispatchUnscheduledRow } from '../../model/dispatch-calendar.types';

defineProps<{
  open: boolean;
  order: DispatchUnscheduledRow | null;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  viewDetails: [];
}>();
</script>

<template>
  <UiDialog
    :open="open"
    :title="order?.code ?? '服务工单详情'"
    :description="order?.title"
    size="xl"
    bare
    :overlay-class="$style.detailOverlay"
    :content-class="$style.detailDialog"
    @update:open="emit('update:open', $event)"
  >
    <template #default="{ close }">
      <article
        v-if="order"
        :class="$style.detailSurface"
        :data-detail-order="order.code"
      >
        <header :class="$style.detailHero">
          <div
            :class="$style.heroArtwork"
            aria-hidden="true"
          >
            <Wrench
              :size="112"
              :stroke-width="1.1"
            />
          </div>

          <div :class="$style.badgeRow">
            <span :class="$style.codeBadge">{{ order.code }}</span>
            <span :class="$style.typeBadge">{{ order.serviceType }}</span>
            <span :class="$style.priorityBadge">{{ order.priority }}</span>
            <span :class="$style.statusBadge">{{ order.detailStatus }}</span>
          </div>

          <h2>{{ order.title }}</h2>

          <UiButton
            :class="$style.closeButton"
            variant="ghost"
            size="icon-sm"
            aria-label="关闭"
            title="关闭"
            @click="close"
          >
            <X
              :size="17"
              :stroke-width="1.8"
            />
          </UiButton>

          <div :class="$style.summaryGrid">
            <section :class="$style.summaryCard">
              <span
                ><CalendarClock
                  :size="13"
                  :stroke-width="1.8"
                />计划时间</span
              >
              <strong :class="$style.scheduleStatus">待预约</strong>
              <small>待与客户约定上门时间</small>
            </section>
            <section :class="$style.summaryCard">
              <span
                ><Wrench
                  :size="13"
                  :stroke-width="1.8"
                />服务方式</span
              >
              <strong>{{ order.serviceMethod }}</strong>
            </section>
            <section :class="$style.summaryCard">
              <span
                ><UserRound
                  :size="13"
                  :stroke-width="1.8"
                />指派工程师</span
              >
              <strong>{{ order.engineerName }}</strong>
              <small>{{ order.engineerTeam }}</small>
            </section>
          </div>
        </header>

        <div :class="$style.detailBody">
          <section :class="$style.infoCard">
            <h3>
              <ContactRound
                :size="13"
                :stroke-width="1.8"
              />客户信息
            </h3>
            <dl :class="$style.infoRows">
              <div>
                <dt>客户</dt>
                <dd>{{ order.customer }}</dd>
              </div>
              <div>
                <dt>联系人</dt>
                <dd>
                  {{ order.contactName }} <a :href="`tel:${order.contactPhone}`">{{ order.contactPhone }}</a>
                </dd>
              </div>
              <div v-if="order.address">
                <dt>地址</dt>
                <dd>{{ order.address }}</dd>
              </div>
            </dl>
          </section>

          <section :class="$style.infoCard">
            <h3>
              <Package
                :size="13"
                :stroke-width="1.8"
              />设备信息
            </h3>
            <dl :class="$style.infoRows">
              <div>
                <dt>产品</dt>
                <dd>{{ order.product }}</dd>
              </div>
              <div>
                <dt>型号</dt>
                <dd>{{ order.model }}</dd>
              </div>
              <div>
                <dt>SN</dt>
                <dd>{{ order.serialNumber }}</dd>
              </div>
              <div>
                <dt>质保</dt>
                <dd :class="$style.warrantyValue">
                  <ShieldCheck
                    :size="12"
                    :stroke-width="1.8"
                  />
                  <span>{{ order.warrantyStatus }}</span>
                  <small>至 {{ order.warrantyEnd }}</small>
                </dd>
              </div>
            </dl>
          </section>

          <section :class="$style.infoCard">
            <h3>
              <TriangleAlert
                :size="13"
                :stroke-width="1.8"
              />故障与影响
            </h3>
            <dl :class="$style.infoRows">
              <div>
                <dt>故障描述</dt>
                <dd>{{ order.faultDescription }}</dd>
              </div>
              <div>
                <dt>影响范围</dt>
                <dd>{{ order.impactScope }}</dd>
              </div>
            </dl>
          </section>

          <section :class="$style.infoCard">
            <h3>
              <FileText
                :size="13"
                :stroke-width="1.8"
              />单据来源
            </h3>
            <dl :class="$style.infoRows">
              <div>
                <dt>来源</dt>
                <dd>{{ order.source }}</dd>
              </div>
              <div>
                <dt>创建</dt>
                <dd>{{ order.createdBy }} · {{ order.createdAt }}</dd>
              </div>
            </dl>
          </section>
        </div>

        <footer :class="$style.detailFooter">
          <p>最后更新：{{ order.updatedAt }}</p>
          <div :class="$style.footerActions">
            <UiButton
              variant="secondary"
              size="sm"
              @click="close"
              >关闭</UiButton
            >
            <UiButton
              variant="primary"
              size="sm"
              @click="emit('viewDetails')"
            >
              <template #leading
                ><ExternalLink
                  :size="14"
                  :stroke-width="1.8"
              /></template>
              查看详情
            </UiButton>
          </div>
        </footer>
      </article>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.detailOverlay {
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(2px);
}

:global([data-ui-dialog]).detailDialog {
  @apply max-h-[92vh] border-0;
  width: min(784px, calc(100vw - 32px));
  border-radius: 11px;
}

.detailSurface {
  @apply flex max-h-[92vh] flex-col overflow-hidden bg-white;
}

.detailHero {
  @apply relative shrink-0 overflow-hidden px-5 pb-3.5 pt-4;
  color: white;
  background: linear-gradient(116deg, #1b2a6b 0%, #0747a6 48%, #00a3bf 100%);
}

.heroArtwork {
  @apply pointer-events-none absolute right-8 top-0 opacity-15;
  transform: rotate(-18deg);
}

.badgeRow {
  @apply relative z-[1] flex items-center gap-1.5;
}

.badgeRow span {
  @apply rounded px-2 py-1 text-[10px] font-semibold leading-none;
}

.codeBadge {
  color: white;
  background: rgba(255, 255, 255, 0.12);
  border: 1px solid rgba(255, 255, 255, 0.48);
}

.typeBadge {
  color: #00a3bf;
  background: #e6fcff;
}

.priorityBadge {
  color: var(--ui-text-subtle);
  background: white;
}

.statusBadge {
  color: #00a3bf;
  background: #e6fcff;
}

.detailHero h2 {
  @apply relative z-[1] mt-2 text-[18px] font-bold leading-7;
}

.closeButton {
  @apply absolute right-3 top-3 z-[2] rounded-full;
  color: white;
  background: rgba(255, 255, 255, 0.12);
  border: 1px solid rgba(255, 255, 255, 0.42);
}

.closeButton:hover {
  background: rgba(255, 255, 255, 0.22);
}

.summaryGrid {
  @apply relative z-[1] mt-2.5 grid grid-cols-3 gap-2.5;
}

.summaryCard {
  @apply flex min-h-[68px] flex-col rounded border px-3 py-2;
  background: rgba(255, 255, 255, 0.12);
  border-color: rgba(255, 255, 255, 0.28);
}

.summaryCard > span {
  @apply flex items-center gap-1.5 text-[10px] font-medium;
  color: rgba(255, 255, 255, 0.72);
}

.summaryCard strong {
  @apply mt-1 text-[12px] font-semibold;
}

.summaryCard small {
  @apply mt-0.5 text-[9px];
  color: rgba(255, 255, 255, 0.72);
}

.scheduleStatus {
  color: #ffcc5c;
}

.detailBody {
  @apply grid min-h-0 flex-1 grid-cols-2 gap-3 overflow-y-auto p-5;
  background: var(--ui-surface-subtle);
}

.infoCard {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}

.infoCard h3 {
  @apply flex h-8 items-center gap-1.5 border-b px-3 text-[10px] font-semibold;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.infoRows {
  @apply space-y-2.5 px-3 py-2.5;
}

.infoRows > div {
  @apply grid grid-cols-[70px_minmax(0,1fr)] gap-2 text-[11px] leading-4;
}

.infoRows dt {
  color: var(--ui-text-subtle);
}

.infoRows dd {
  @apply min-w-0 font-medium;
  color: var(--ui-text);
}

.infoRows a {
  @apply ml-1 font-semibold;
  color: var(--app-primary);
}

.warrantyValue {
  @apply flex items-center gap-1.5;
}

.warrantyValue span {
  @apply rounded px-1.5 py-0.5 text-[9px] font-semibold;
  color: var(--ui-success);
  background: var(--ui-success-soft);
}

.warrantyValue small {
  color: var(--ui-text-subtle);
}

.detailFooter {
  @apply flex min-h-[58px] shrink-0 items-center justify-between border-t bg-white px-5 py-3;
  border-color: var(--ui-border);
}

.detailFooter p {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.footerActions {
  @apply flex items-center gap-2;
}

@media (max-width: 680px) {
  .summaryGrid,
  .detailBody {
    @apply grid-cols-1;
  }

  .detailBody {
    @apply max-h-[50vh];
  }
}
</style>
