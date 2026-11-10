<script setup lang="ts">
import { Settings2 } from 'lucide-vue-next';
import { UiSwitch } from '@/components/ui';
import type { CostRule } from '../../model/cost-centers.types';

defineProps<{
  rules: readonly CostRule[];
}>();

const emit = defineEmits<{
  toggle: [rule: CostRule, enabled: boolean];
}>();
</script>

<template>
  <section :class="$style.panel">
    <header>
      <span>
        <Settings2
          :size="18"
          :stroke-width="1.8"
        />
      </span>
      <div>
        <h2>成本规则</h2>
        <p>配置不同业务来源的成本自动归集方式</p>
      </div>
    </header>
    <div :class="$style.ruleList">
      <article
        v-for="rule in rules"
        :key="rule.id"
      >
        <div>
          <strong>{{ rule.name }}</strong>
          <p>{{ rule.description }}</p>
        </div>
        <UiSwitch
          :model-value="rule.enabled"
          :label="rule.name"
          @update:model-value="emit('toggle', rule, $event)"
        />
      </article>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply mt-4 rounded border bg-white p-5;
  border-color: var(--ui-border);
}

.panel header {
  @apply flex items-center gap-3;
}

.panel header > span {
  @apply flex h-10 w-10 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.panel h2,
.panel p {
  @apply m-0;
}

.panel h2 {
  @apply text-[15px] font-semibold;
}

.panel p {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}

.ruleList {
  @apply mt-5 divide-y rounded border px-4;
  border-color: var(--ui-border);
}

.ruleList article {
  @apply flex min-h-[72px] items-center justify-between gap-4;
  border-color: var(--ui-border-subtle);
}

.ruleList strong {
  @apply text-[12px] font-semibold;
}
</style>
