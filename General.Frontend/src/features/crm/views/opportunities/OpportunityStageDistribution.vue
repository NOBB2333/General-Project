<script setup lang="ts">
import type { OpportunityStage } from '@/features/crm/model/opportunities.types';

defineProps<{ counts: Record<OpportunityStage, number> }>();

const stages: readonly OpportunityStage[] = ['线索', '意向', '方案', '报价', '谈判', '赢单'];
const stageClass: Record<OpportunityStage, string> = {
  线索: 'lead',
  意向: 'intent',
  方案: 'proposal',
  报价: 'quote',
  谈判: 'negotiation',
  赢单: 'won',
  输单: 'lost',
};
</script>

<template>
  <section :class="$style.panel">
    <h2>商机阶段分布</h2>
    <div :class="$style.track">
      <span
        v-for="stage in stages"
        :key="stage"
        :class="[$style.segment, $style[stageClass[stage]], counts[stage] === 2 && $style.double]"
      >
        {{ stage }} {{ counts[stage] }}
      </span>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply rounded border bg-white;
  margin-top: 17.5px;
  padding: 12.5px 14px 14px;
  border-color: var(--ui-border);
}
.panel h2 {
  @apply mb-2 text-[12px] font-semibold;
  color: var(--ui-text-subtle);
}
.track {
  @apply flex gap-1 overflow-hidden;
  height: 21px;
}
.segment {
  @apply flex min-w-0 flex-1 items-center justify-center whitespace-nowrap text-[11px] font-semibold text-white;
}
.double {
  flex-grow: 2;
}
.lead {
  background: #6d778a;
}
.intent {
  background: #2151c5;
}
.proposal {
  background: #f09135;
}
.quote {
  background: #6255ba;
}
.negotiation,
.won {
  background: #3b855e;
}
.lost {
  background: #de350b;
}
</style>
