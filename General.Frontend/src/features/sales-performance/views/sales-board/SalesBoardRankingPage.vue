<script setup lang="ts">
import { computed, ref } from 'vue';
import { Trophy } from 'lucide-vue-next';
import { salesBoardDetailedRankings } from '../../model/sales-board.data';
import type { SalesBoardDetailedRanking } from '../../model/sales-board.types';

type RankingDimension = 'performance' | 'contracts' | 'collections' | 'profit';
type RankingGroup = 'personal' | 'team';

const emit = defineEmits<{
  inspect: [title: string, description: string, facts: string[]];
}>();

const dimension = ref<RankingDimension>('performance');
const group = ref<RankingGroup>('personal');
const dimensions: readonly { value: RankingDimension; label: string }[] = [
  { value: 'performance', label: '业绩' },
  { value: 'contracts', label: '合同额' },
  { value: 'collections', label: '回款' },
  { value: 'profit', label: '毛利' },
];

const totalLabel = computed(() => (group.value === 'personal' ? '合计 ¥1,742,161.60' : '4 个销售团队'));

function inspectRow(row: SalesBoardDetailedRanking) {
  emit('inspect', `${row.name} · 排行详情`, row.team, [
    `确认业绩：${row.performance}`,
    `合同金额：${row.contracts}`,
    `实际回款：${row.collections}`,
    `销售毛利：${row.profit}`,
    `业绩占比：${row.share}`,
  ]);
}
</script>

<template>
  <section
    :class="$style.section"
    data-board-section="ranking"
    :data-ranking-dimension="dimension"
    :data-ranking-group="group"
  >
    <div :class="$style.controls">
      <div :class="$style.controlGroup">
        <span>排行维度</span>
        <button
          v-for="item in dimensions"
          :key="item.value"
          type="button"
          :aria-pressed="dimension === item.value"
          @click="dimension = item.value"
        >
          {{ item.label }}
        </button>
      </div>
      <div :class="$style.controlGroup">
        <span>分组</span>
        <button
          type="button"
          :aria-pressed="group === 'personal'"
          @click="group = 'personal'"
        >
          个人
        </button>
        <button
          type="button"
          :aria-pressed="group === 'team'"
          @click="group = 'team'"
        >
          团队
        </button>
      </div>
    </div>

    <article :class="$style.panel">
      <header :class="$style.panelHeader">
        <Trophy
          :size="19"
          :stroke-width="1.8"
        />
        <div>
          <h2>
            销售{{ group === 'personal' ? '个人' : '团队' }} ·
            {{ dimensions.find((item) => item.value === dimension)?.label }}排行
          </h2>
          <p>{{ totalLabel }}</p>
        </div>
      </header>

      <div :class="$style.tableViewport">
        <table>
          <thead>
            <tr>
              <th>排名</th>
              <th>销售员</th>
              <th>所属团队</th>
              <th>业绩</th>
              <th>合同额</th>
              <th>回款</th>
              <th>毛利</th>
              <th>单数</th>
              <th>占比</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in salesBoardDetailedRankings"
              :key="row.id"
            >
              <td>
                <span :data-rank="row.rank">{{ row.rank }}</span>
              </td>
              <td>
                <button
                  type="button"
                  @click="inspectRow(row)"
                >
                  {{ row.name }}
                </button>
              </td>
              <td>{{ row.team }}</td>
              <td :class="dimension === 'performance' ? $style.activeValue : undefined">{{ row.performance }}</td>
              <td :class="dimension === 'contracts' ? $style.activeValue : undefined">{{ row.contracts }}</td>
              <td :class="dimension === 'collections' ? $style.activeValue : undefined">{{ row.collections }}</td>
              <td :class="dimension === 'profit' ? $style.activeValue : undefined">{{ row.profit }}</td>
              <td>{{ row.orders }}</td>
              <td>
                <div :class="$style.shareCell">
                  <i :data-share="row.rank" />
                  <span>{{ row.share }}</span>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </article>

    <aside :class="$style.insight">
      <strong>一句话洞察</strong>
      <p>
        当前张经理在「{{ dimensions.find((item) => item.value === dimension)?.label }}」维度领跑，贡献 54.5%。前 3
        名合计占 91.1%，团队结构存在头部集中风险。
      </p>
    </aside>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.section {
  @apply min-w-0 pb-8 pt-4;
}

.controls {
  @apply flex items-center justify-between;
}

.controlGroup {
  @apply flex items-center;
}

.controlGroup > span {
  @apply mr-2 text-[11px] font-semibold;
  color: var(--ui-text-subtle);
}

.controlGroup button {
  @apply h-8 border-y border-r px-3 text-[10px] font-medium outline-none transition-colors;
  border-color: var(--ui-border);
  background: var(--ui-surface);
  color: var(--ui-text-subtle);
}

.controlGroup button:first-of-type {
  @apply rounded-l border-l;
}

.controlGroup button:last-child {
  @apply rounded-r;
}

.controlGroup button[aria-pressed='true'] {
  border-color: var(--app-primary);
  background: var(--app-primary);
  color: white;
}

.controlGroup button:focus-visible,
.tableViewport button:focus-visible {
  box-shadow: 0 0 0 2px var(--ui-focus);
}

.panel {
  @apply mt-4 overflow-hidden rounded border;
  border-color: var(--ui-border);
  background: var(--ui-surface);
}

.panelHeader {
  @apply flex items-center gap-2 px-5 py-4;
  color: #ff8b00;
}

.panelHeader h2 {
  @apply text-[14px] font-semibold;
  color: var(--ui-text);
}

.panelHeader p {
  @apply mt-0.5 text-[10px];
  color: var(--ui-text-subtle);
}

.tableViewport {
  @apply overflow-x-auto px-5 pb-4;
}

.tableViewport table {
  @apply w-full min-w-[1050px] border-collapse text-[10px];
}

.tableViewport th,
.tableViewport td {
  @apply border-b px-2 py-3 text-left;
  border-color: var(--ui-border-subtle);
}

.tableViewport th {
  @apply font-semibold;
  color: var(--ui-text-subtle);
}

.tableViewport td > span[data-rank] {
  @apply flex h-6 w-6 items-center justify-center rounded-full font-semibold;
  color: var(--ui-text-subtle);
}

.tableViewport td > span[data-rank='1'] {
  background: #ffab00;
  color: white;
}

.tableViewport td > span[data-rank='2'] {
  background: #c1c7d0;
  color: white;
}

.tableViewport td > span[data-rank='3'] {
  background: #ff8b00;
  color: white;
}

.tableViewport button {
  @apply rounded font-semibold outline-none;
  color: var(--ui-text);
}

.activeValue {
  @apply font-semibold;
  color: var(--app-primary);
}

.shareCell {
  @apply grid grid-cols-[1fr_38px] items-center gap-2;
}

.shareCell i {
  @apply h-1.5 rounded-full;
  width: 100%;
  background: var(--app-primary);
}

.shareCell i[data-share='2'] {
  width: 35%;
}

.shareCell i[data-share='3'] {
  width: 32%;
}

.shareCell i[data-share='4'] {
  width: 14%;
}

.shareCell i[data-share='5'] {
  width: 3%;
}

.shareCell span {
  color: var(--ui-text-subtle);
}

.insight {
  @apply mt-4 rounded border-l-2 px-4 py-3 text-[11px];
  border-color: #6554c0;
  background: #f7f5ff;
}

.insight strong {
  color: #6554c0;
}

.insight p {
  @apply mt-1 leading-5;
}

@media (prefers-reduced-motion: reduce) {
  .controlGroup button {
    transition: none;
  }
}
</style>
