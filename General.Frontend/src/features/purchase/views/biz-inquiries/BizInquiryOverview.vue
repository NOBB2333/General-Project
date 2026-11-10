<script setup lang="ts">
import {
  AlertCircle,
  ArrowRight,
  BadgeDollarSign,
  BriefcaseBusiness,
  CircleCheck,
  FileText,
  HandCoins,
  UserRoundCheck,
} from 'lucide-vue-next';
import { computed } from 'vue';
import type { BizInquiryRow } from '../../model/biz-inquiries.types';

const props = defineProps<{ rows: readonly BizInquiryRow[] }>();
const stages = [
  { label: '销售创建', desc: '录入销售线索物料', icon: BriefcaseBusiness },
  { label: '分配采购', desc: '指定采购负责人', icon: UserRoundCheck },
  { label: '采购报价', desc: '录入/发布链接收集', icon: BadgeDollarSign },
  { label: '报价汇总', desc: '多供应商比价', icon: HandCoins },
  { label: '完成反馈', desc: '中标/未中结果', icon: CircleCheck },
] as const;
const summary = computed(() => ({
  total: props.rows.length,
  amount: props.rows.reduce((sum, row) => sum + row.estimatedAmount, 0),
  pending: props.rows.filter((row) => row.status === '待分配').length,
  active: props.rows.filter((row) => ['已分配', '报价中'].includes(row.status)).length,
  won: props.rows.filter((row) => row.result === '中标').length,
}));
</script>

<template>
  <section
    :class="$style.workflow"
    aria-label="业务询价流程"
  >
    <div :class="$style.stageRow">
      <template
        v-for="(stage, index) in stages"
        :key="stage.label"
        ><article :class="$style.stage">
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
      /></template>
    </div>
    <p>销售人员发起 → 采购人员协作报价 → 汇总反馈结果</p>
  </section>
  <section
    :class="$style.stats"
    aria-label="业务询价统计"
  >
    <article>
      <div>
        <FileText
          :size="16"
          :stroke-width="1.8"
        />
        <p>总询价单</p>
      </div>
      <strong>{{ summary.total }}</strong
      ><small>预估总额 ¥{{ (summary.amount / 10000).toFixed(1) }}万</small>
    </article>
    <article>
      <div :class="$style.pending">
        <AlertCircle
          :size="16"
          :stroke-width="1.8"
        />
        <p>待分配</p>
      </div>
      <strong :class="$style.pending">{{ summary.pending }}</strong
      ><small>需尽快指定采购人员</small>
    </article>
    <article>
      <div :class="$style.active">
        <BadgeDollarSign
          :size="16"
          :stroke-width="1.8"
        />
        <p>进行中</p>
      </div>
      <strong :class="$style.active">{{ summary.active }}</strong
      ><small>已分配+报价中</small>
    </article>
    <article>
      <div :class="$style.won">
        <CircleCheck
          :size="16"
          :stroke-width="1.8"
        />
        <p>已中标</p>
      </div>
      <strong :class="$style.won">{{ summary.won }}</strong
      ><small>成功拿下订单</small>
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
.stage:nth-of-type(2) {
  color: #0052cc;
}
.stage:nth-of-type(3) {
  color: #ff8b00;
}
.stage:nth-of-type(4) {
  color: #00875a;
}
.stage:nth-of-type(5) {
  color: #6554c0;
}
.arrow {
  color: #c1c7d0;
}
.workflow > p {
  @apply mt-2 text-center text-[10px];
  color: #97a0af;
}
.stats {
  @apply mb-5 grid grid-cols-4 gap-3;
}
.stats article {
  @apply rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.stats article > div {
  @apply flex items-center gap-2;
  color: #0052cc;
}
.stats p {
  @apply text-[12px];
  color: #6b778c;
}
.stats strong {
  @apply mt-1 block text-[22px] font-bold;
  color: #172b4d;
}
.stats small {
  @apply mt-0.5 block text-[10px];
  color: #6b778c;
}
.stats .pending {
  color: #de350b;
}
.stats .active {
  color: #ff8b00;
}
.stats .won {
  color: #00875a;
}
</style>
