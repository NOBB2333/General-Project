<script setup lang="ts">
import {
  ArrowLeft,
  Boxes,
  Check,
  CirclePlus,
  ClipboardCheck,
  ClipboardList,
  Clock3,
  Download,
  Factory,
  Hammer,
  RotateCcw,
  Upload,
  WalletCards,
} from 'lucide-vue-next';
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { toast, UiButton } from '@/components/ui';
import { productionOrderDetail } from '../model/production-order-detail.data';

const route = useRoute();
const router = useRouter();
const order = computed(() => ({
  ...productionOrderDetail,
  code: String(route.params.orderCode || productionOrderDetail.code),
}));
const pageRoute = computed(() => `/mfg/orders/${order.value.code}`);
const orderActions = [
  { label: '领料', icon: Upload },
  { label: '补料', icon: CirclePlus },
  { label: '退料', icon: RotateCcw },
  { label: '报工', icon: ClipboardList },
] as const;

function runLocalAction(label: string) {
  toast.info(`${label}操作已打开（本地复刻）`);
}

function returnToOrders() {
  void router.push('/mfg/work-orders');
}
</script>

<template>
  <main
    :class="$style.page"
    :data-page-route="pageRoute"
    data-page-family="detail"
    data-production-order-detail
  >
    <UiButton
      :class="$style.backButton"
      variant="link"
      size="sm"
      @click="returnToOrders"
    >
      <template #leading
        ><ArrowLeft
          :size="14"
          :stroke-width="1.8"
      /></template>
      返回生产工单
    </UiButton>

    <section :class="$style.hero">
      <p :class="$style.breadcrumb">生产制造 <span>›</span> 生产工单 / {{ order.code }}</p>
      <div :class="$style.heroHeading">
        <span :class="$style.heroIcon"
          ><Factory
            :size="23"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>工单 {{ order.code }}</h1>
          <p>{{ order.product }} × {{ order.quantity }} · BOM {{ order.bom }}</p>
        </div>
      </div>
      <strong :class="$style.heroStatus">{{ order.status }}</strong>
    </section>

    <section :class="$style.actionBar">
      <div :class="$style.orderIdentity">
        <strong>{{ order.code }}</strong>
        <span :class="$style.productionBadge">{{ order.status }}</span>
        <span :class="$style.priorityBadge">优先级 {{ order.priority }}</span>
        <span :class="$style.sourceBadge">{{ order.source }}</span>
        <span>项目：{{ order.project }}</span>
      </div>
      <div :class="$style.actions">
        <UiButton
          v-for="action in orderActions"
          :key="action.label"
          size="sm"
          @click="runLocalAction(action.label)"
        >
          <template #leading>
            <component
              :is="action.icon"
              :size="14"
              :stroke-width="1.8"
            />
          </template>
          {{ action.label }}
        </UiButton>
        <UiButton
          variant="primary"
          size="sm"
          @click="runLocalAction('完工入库')"
        >
          <template #leading
            ><Download
              :size="14"
              :stroke-width="1.8"
          /></template>
          完工入库
        </UiButton>
      </div>
    </section>

    <section :class="$style.sectionCard">
      <header :class="$style.sectionHeader">
        <ClipboardCheck
          :size="16"
          :stroke-width="1.8"
        />
        <h2>状态进度</h2>
      </header>
      <ol :class="$style.stageList">
        <li
          v-for="stage in order.stages"
          :key="stage.id"
          :data-state="stage.state"
        >
          <span :class="$style.stageNode"
            ><Check
              v-if="stage.state === 'complete'"
              :size="13"
              :stroke-width="2"
            /><template v-else>{{ stage.step }}</template></span
          >
          <strong>{{ stage.label }}</strong>
        </li>
      </ol>
    </section>

    <section
      :class="$style.metricGrid"
      aria-label="生产工单指标"
    >
      <article
        v-for="(metric, index) in order.metrics"
        :key="metric.id"
        :data-tone="metric.tone"
      >
        <header>
          <component
            :is="[Boxes, Clock3, Hammer, WalletCards][index]"
            :size="16"
            :stroke-width="1.8"
          />
          <span>{{ metric.label }}</span>
        </header>
        <strong>{{ metric.value }}</strong>
        <p>{{ metric.hint }}</p>
      </article>
    </section>

    <section :class="$style.sectionCard">
      <header :class="$style.sectionHeader">
        <Hammer
          :size="16"
          :stroke-width="1.8"
        />
        <h2>工序进度</h2>
        <p>点击工序节点展开报工记录</p>
      </header>
      <div :class="$style.operationTrack">
        <article
          v-for="operation in order.operations"
          :key="operation.id"
          :data-state="operation.state"
        >
          <div :class="$style.operationHeading">
            <span>工序 {{ operation.operationNo }}</span
            ><strong>{{
              operation.state === 'completed' ? '已完工' : operation.state === 'active' ? '进行中' : '待派工'
            }}</strong>
          </div>
          <h3>{{ operation.name }}</h3>
          <p>{{ operation.center }}</p>
          <b>{{ operation.progress }}</b>
          <progress
            :value="operation.state === 'completed' ? 1 : 0"
            max="1"
          />
          <small>{{ operation.owner }}</small>
        </article>
      </div>
    </section>

    <section :class="$style.sectionCard">
      <header :class="$style.sectionHeader">
        <Boxes
          :size="16"
          :stroke-width="1.8"
        />
        <h2>用料明细</h2>
        <p>BOM 快照 V1.1 · 下达 2026-07-09 · 已领为累计净额</p>
      </header>
      <div :class="$style.tableFrame">
        <table :class="$style.table">
          <thead>
            <tr>
              <th>物料</th>
              <th>规格</th>
              <th>应领</th>
              <th>已领(净)</th>
              <th>欠领</th>
              <th>库存</th>
              <th>标记</th>
              <th>金额</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in order.materials"
              :key="row.id"
            >
              <td>
                <strong>{{ row.material }}</strong
                ><small>{{ row.code }}</small>
              </td>
              <td>{{ row.specification }}</td>
              <td>{{ row.required }}</td>
              <td>{{ row.issued }}</td>
              <td>{{ row.shortage }}</td>
              <td>{{ row.stock }}</td>
              <td>
                <span
                  v-for="tag in row.tags"
                  :key="tag"
                  >{{ tag }}</span
                ><template v-if="!row.tags.length">—</template>
              </td>
              <td>{{ row.amount }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <div :class="$style.twoColumn">
      <section :class="$style.sectionCard">
        <header :class="$style.sectionHeader">
          <ClipboardCheck
            :size="16"
            :stroke-width="1.8"
          />
          <h2>报工记录（2 条 · 含待审核）</h2>
        </header>
        <div :class="$style.tableFrame">
          <table :class="$style.table">
            <thead>
              <tr>
                <th>报工单号</th>
                <th>工序</th>
                <th>报工人</th>
                <th>合格</th>
                <th>不良</th>
                <th>工时</th>
                <th>时间</th>
                <th>状态</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="row in order.workReports"
                :key="row.id"
              >
                <td>{{ row.code }}</td>
                <td>{{ row.operation }}</td>
                <td>{{ row.reporter }}</td>
                <td>{{ row.qualified }}</td>
                <td>{{ row.defective }}</td>
                <td>{{ row.hours }}</td>
                <td>{{ row.time }}</td>
                <td>{{ row.status }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
      <section :class="$style.sectionCard">
        <header :class="$style.sectionHeader">
          <RotateCcw
            :size="16"
            :stroke-width="1.8"
          />
          <h2>领料单记录（1 张 · 领料/补料/退料）</h2>
        </header>
        <div :class="$style.tableFrame">
          <table :class="$style.table">
            <thead>
              <tr>
                <th>领料单号</th>
                <th>类型</th>
                <th>物料种数</th>
                <th>数量</th>
                <th>金额</th>
                <th>经手人</th>
                <th>日期</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="row in order.materialSlips"
                :key="row.id"
              >
                <td>{{ row.code }}</td>
                <td>{{ row.type }}</td>
                <td>{{ row.materialCount }}</td>
                <td>{{ row.quantity }}</td>
                <td>{{ row.amount }}</td>
                <td>{{ row.owner }}</td>
                <td>{{ row.date }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </div>

    <div :class="$style.threeColumn">
      <section :class="$style.sectionCard">
        <header :class="$style.sectionHeader">
          <WalletCards
            :size="16"
            :stroke-width="1.8"
          />
          <h2>成本归集</h2>
        </header>
        <dl :class="$style.definitionList">
          <div
            v-for="item in order.costs"
            :key="item.label"
            :data-emphasis="item.emphasis"
          >
            <dt>{{ item.label }}</dt>
            <dd>{{ item.value }}</dd>
          </div>
        </dl>
      </section>
      <section :class="$style.sectionCard">
        <header :class="$style.sectionHeader">
          <Boxes
            :size="16"
            :stroke-width="1.8"
          />
          <h2>关联单据</h2>
        </header>
        <dl :class="$style.definitionList">
          <div
            v-for="item in order.relatedDocuments"
            :key="item.label"
          >
            <dt>{{ item.label }}</dt>
            <dd>{{ item.value }}</dd>
          </div>
        </dl>
      </section>
      <section :class="$style.sectionCard">
        <header :class="$style.sectionHeader">
          <Clock3
            :size="16"
            :stroke-width="1.8"
          />
          <h2>操作日志</h2>
        </header>
        <ol :class="$style.logList">
          <li
            v-for="log in order.logs"
            :key="log.id"
          >
            <i />
            <div>
              <strong>{{ log.action }}</strong
              ><span>{{ log.owner }} · {{ log.time }}</span>
              <p>{{ log.description }}</p>
            </div>
          </li>
        </ol>
      </section>
    </div>
  </main>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply -mt-[10px] min-w-[1080px] pb-8;
  color: var(--ui-text);
}
.backButton {
  @apply mb-3;
}
.hero {
  @apply relative h-[120px] overflow-hidden rounded-lg px-6 py-5 text-white;
  background: linear-gradient(108deg, #0c2d69 0%, #0c52ad 58%, #6554c0 100%);
}
.hero::before,
.hero::after {
  content: '';
  @apply absolute rounded-full border-[4px] border-white/10;
}
.hero::before {
  @apply -right-4 -top-20 h-48 w-48;
}
.hero::after {
  @apply -bottom-16 right-28 h-28 w-28;
}
.breadcrumb {
  @apply text-[11px] font-semibold text-white/75;
}
.breadcrumb span {
  @apply mx-1;
}
.heroHeading {
  @apply mt-4 flex items-center gap-3;
}
.heroIcon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border border-white/30 bg-white/10;
}
.hero h1 {
  @apply text-[24px] font-bold;
}
.heroHeading p {
  @apply mt-1 text-[12px] text-white/80;
}
.heroStatus {
  @apply absolute right-6 top-1/2 -translate-y-1/2 rounded bg-white px-4 py-2 text-[13px];
  color: var(--ui-warning);
}
.actionBar {
  @apply mt-[19px] flex min-h-16 items-center justify-between gap-4 rounded border bg-white px-4;
  border-color: var(--ui-border);
}
.orderIdentity {
  @apply flex flex-wrap items-center gap-2 text-[11px];
  color: var(--ui-text-muted);
}
.orderIdentity > strong {
  @apply mr-1 text-[15px] font-bold;
  color: var(--ui-text);
}
.orderIdentity > span {
  @apply rounded px-2 py-1;
}
.productionBadge {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.priorityBadge {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.sourceBadge {
  background: var(--ui-surface-hover);
}
.actions {
  @apply flex items-center gap-2;
}
.sectionCard {
  @apply mt-[19px] overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.sectionHeader {
  @apply flex min-h-[52px] items-center gap-2 border-b px-5;
  border-color: var(--ui-border-subtle);
}
.sectionHeader svg {
  color: var(--app-primary);
}
.sectionHeader h2 {
  @apply text-[13px] font-semibold;
}
.sectionHeader p {
  @apply ml-auto text-[10px];
  color: var(--ui-text-subtle);
}
.stageList {
  @apply flex items-center px-5 py-[14.5px];
}
.stageList li {
  @apply relative flex min-w-[105px] items-center gap-2 text-[11px];
  color: var(--ui-text-subtle);
}
.stageList li:not(:last-child)::after {
  content: '';
  @apply mx-2 h-0.5 w-12;
  background: var(--ui-border);
}
.stageNode {
  @apply flex h-6 w-6 items-center justify-center rounded-full text-[10px] font-semibold;
  background: var(--ui-surface-hover);
}
.stageList li[data-state='complete'],
.stageList li[data-state='active'] {
  color: var(--ui-text);
}
.stageList li[data-state='complete'] .stageNode,
.stageList li[data-state='active'] .stageNode {
  @apply text-white;
  background: var(--app-primary);
}
.stageList li[data-state='complete']::after {
  background: var(--app-primary);
}
.metricGrid {
  @apply mt-[19px] grid grid-cols-4 gap-3;
}
.metricGrid article {
  @apply min-h-[101.5px] rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.metricGrid header {
  @apply flex items-center gap-2 text-[11px];
  color: var(--ui-text-subtle);
}
.metricGrid article > strong {
  @apply mt-3 block text-[22px] font-bold;
}
.metricGrid article > p {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}
.metricGrid article[data-tone='danger'] > strong {
  color: var(--ui-danger);
}
.metricGrid article[data-tone='warning'] > strong {
  color: var(--ui-warning);
}
.metricGrid article[data-tone='success'] > strong {
  color: var(--ui-success);
}
.operationTrack {
  @apply relative flex gap-7 px-5 pb-[21.5px] pt-[12.5px];
}
.operationTrack::before {
  content: '';
  @apply absolute left-16 right-16 top-1/2 h-0.5;
  background: var(--ui-border-subtle);
}
.operationTrack article {
  @apply relative z-[1] h-[143px] w-[191px] rounded border bg-white p-2.5;
  border-color: var(--ui-border);
}
.operationTrack article[data-state='active'] {
  background: var(--ui-warning-soft);
}
.operationHeading {
  @apply flex items-center justify-between text-[10px];
  color: var(--ui-text-subtle);
}
.operationHeading strong {
  @apply rounded px-2 py-0.5;
  background: var(--ui-surface-hover);
}
.operationTrack article[data-state='completed'] .operationHeading strong {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.operationTrack article[data-state='active'] .operationHeading strong {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.operationTrack h3 {
  @apply mt-2 text-[12px] font-semibold;
}
.operationTrack p,
.operationTrack b,
.operationTrack small {
  @apply mt-1.5 block text-[10px] leading-4;
  color: var(--ui-text-subtle);
}
.operationTrack progress {
  @apply mt-1.5 h-1.5 w-full;
  accent-color: var(--ui-success);
}
.tableFrame {
  @apply overflow-auto;
}
.table {
  @apply w-full min-w-[920px] border-collapse text-left text-[10px];
}
.table thead {
  background: var(--ui-surface-subtle);
  color: var(--ui-text-subtle);
}
.table th,
.table td {
  @apply border-b px-3 py-2.5;
  border-color: var(--ui-border-subtle);
}
.table td strong,
.table td small {
  @apply block;
}
.table td small {
  @apply mt-1;
  color: var(--ui-text-subtle);
}
.table td span {
  @apply mr-1 rounded px-1.5 py-1 text-[9px];
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.twoColumn {
  @apply grid grid-cols-2 gap-4;
}
.threeColumn {
  @apply grid grid-cols-3 gap-4;
}
.definitionList {
  @apply p-4;
}
.definitionList div {
  @apply flex items-center justify-between border-b py-2 text-[11px];
  border-color: var(--ui-border-subtle);
}
.definitionList dt {
  color: var(--ui-text-subtle);
}
.definitionList dd {
  @apply font-semibold;
}
.definitionList div[data-emphasis='true'] dd {
  color: var(--ui-success);
}
.logList {
  @apply p-4;
}
.logList li {
  @apply flex gap-3 pb-4 text-[10px];
}
.logList i {
  @apply mt-1 h-2 w-2 shrink-0 rounded-full;
  background: var(--app-primary);
}
.logList strong {
  @apply mr-2 text-[11px];
}
.logList span {
  color: var(--ui-text-subtle);
}
.logList p {
  @apply mt-1;
  color: var(--ui-text-muted);
}
</style>
