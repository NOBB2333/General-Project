<script setup lang="ts">
import { ArrowRight, CirclePlay, ClipboardList, Clock3, ShoppingCart, SquareCheckBig } from 'lucide-vue-next';
import { computed } from 'vue';
import type { InquiryRow, InquiryScope } from '../../model/inquiries.types';

const props = defineProps<{ rows: readonly InquiryRow[]; scope: InquiryScope }>();
const emit = defineEmits<{ 'update:scope': [value: InquiryScope] }>();
const scopes = [
  { key: 'all', label: '所有' },
  { key: 'mine', label: '我负责的' },
  { key: 'team', label: '下属负责的' },
  { key: 'watched', label: '我关注的' },
] as const;
const stages = [
  { label: '创建询价', desc: '待发布', icon: ClipboardList },
  { label: '发布询价', desc: '生成链接/设期限', icon: CirclePlay },
  { label: '收集报价', desc: '报价收集中', icon: Clock3 },
  { label: '确认比价', desc: '已完成比价', icon: SquareCheckBig },
  { label: '转采购单', desc: '已转采购单', icon: ShoppingCart },
] as const;
const scopedRows = computed(() => props.rows.filter((row) => matchesScope(row, props.scope)));
const summary = computed(() => ({
  draft: scopedRows.value.filter((row) => row.status === '待发布').length,
  quoting: scopedRows.value.filter((row) => row.status === '报价收集中').length,
  compared: scopedRows.value.filter((row) => row.status === '已完成比价').length,
  ordered: scopedRows.value.filter((row) => row.status === '已转采购单').length,
  amount: scopedRows.value.reduce((total, row) => total + row.totalBudget, 0),
}));

function matchesScope(row: InquiryRow, scope: InquiryScope) {
  if (scope === 'mine') return row.owner === '张经理';
  if (scope === 'team') return ['王工', '李工'].includes(row.owner);
  if (scope === 'watched') return row.watched;
  return true;
}
function scopeCount(scope: InquiryScope) {
  return props.rows.filter((row) => matchesScope(row, scope)).length;
}
</script>

<template>
  <section
    :class="$style.workflow"
    aria-label="询价流程"
  >
    <div :class="$style.stageRow">
      <template
        v-for="(stage, index) in stages"
        :key="stage.label"
      >
        <article :class="$style.stage">
          <component
            :is="stage.icon"
            :size="13"
            :stroke-width="1.8"
          />
          <div>
            <strong>{{ stage.label }}</strong
            ><span>{{ stage.desc }}</span>
          </div>
        </article>
        <ArrowRight
          v-if="index < stages.length - 1"
          :size="14"
          :stroke-width="1.8"
          :class="$style.arrow"
        />
      </template>
    </div>
    <p>任意阶段均可手动关闭 → 失效关闭</p>
  </section>

  <nav
    :class="$style.scopes"
    aria-label="管辖范围"
  >
    <button
      v-for="item in scopes"
      :key="item.key"
      type="button"
      :class="[$style.scope, scope === item.key && $style.scopeActive]"
      @click="emit('update:scope', item.key)"
    >
      {{ item.label }} <span>{{ scopeCount(item.key) }}</span>
    </button>
  </nav>

  <section
    :class="$style.stats"
    aria-label="询价统计"
  >
    <article>
      <p>待发布</p>
      <strong>{{ summary.draft }}</strong>
    </article>
    <article>
      <p>报价收集中</p>
      <strong :class="$style.quoting">{{ summary.quoting }}</strong>
    </article>
    <article>
      <p>已完成比价</p>
      <strong :class="$style.compared">{{ summary.compared }}</strong>
    </article>
    <article>
      <p>已转采购单</p>
      <strong :class="$style.ordered">{{ summary.ordered }}</strong>
    </article>
    <article>
      <p>总询价金额</p>
      <strong>{{ (summary.amount / 10_000).toFixed(1) }}万</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.workflow {
  @apply mb-5 rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.stageRow {
  @apply flex items-center justify-center gap-2;
}
.stage {
  @apply flex items-center gap-1.5 rounded-lg border px-3 py-2;
  color: #42526e;
  background: #f4f5f7;
  border-color: #ebecf0;
}
.stage:nth-of-type(2),
.stage:nth-of-type(3) {
  color: #0052cc;
}
.stage:nth-of-type(4) {
  color: #00875a;
}
.stage:nth-of-type(5) {
  color: #6554c0;
}
.stage div {
  @apply flex flex-col;
}
.stage strong {
  @apply text-[11px] font-semibold;
  color: #172b4d;
}
.stage span {
  @apply text-[9px];
  color: #6b778c;
}
.arrow {
  color: #c1c7d0;
}
.workflow > p {
  @apply mt-2 text-center text-[10px];
  color: #97a0af;
}
.scopes {
  @apply mb-4 flex items-center gap-1 overflow-x-auto rounded-lg border bg-white p-1;
  border-color: var(--ui-border);
}
.scope {
  @apply flex min-h-8 items-center gap-1.5 whitespace-nowrap rounded px-4 text-[12px] transition-colors;
  color: #42526e;
}
.scope:hover {
  background: #f4f5f7;
}
.scope span {
  @apply rounded-full px-1.5 py-0.5 text-[10px] font-semibold;
  color: #6b778c;
  background: #ebecf0;
}
.scopeActive {
  @apply font-semibold text-white shadow-sm;
  background: #0052cc;
}
.scopeActive span {
  color: white;
  background: rgb(255 255 255 / 20%);
}
.stats {
  @apply mb-5 grid grid-cols-5 gap-4;
}
.stats article {
  @apply rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.stats p {
  @apply text-[12px];
  color: #6b778c;
}
.stats strong {
  @apply mt-1 block text-[22px] font-bold;
  color: #172b4d;
}
.stats .quoting {
  color: #0052cc;
}
.stats .compared {
  color: #00875a;
}
.stats .ordered {
  color: #6554c0;
}
</style>
