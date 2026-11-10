<script setup lang="ts">
import { Plus } from 'lucide-vue-next';
import { UiButton, UiCheckbox } from '@/components/ui';
import type { ServiceConfigItem, ServiceConfigPanelDefinition } from '../../model/service-config.types';

defineProps<{
  panel: ServiceConfigPanelDefinition;
  items: readonly ServiceConfigItem[];
}>();

const emit = defineEmits<{
  add: [];
  toggle: [id: string, enabled: boolean];
}>();
</script>

<template>
  <section :class="$style.panel">
    <header :class="$style.panelHeader">
      <h2>{{ panel.heading }}</h2>
      <UiButton
        size="sm"
        variant="primary"
        @click="emit('add')"
      >
        <template #leading
          ><Plus
            :size="14"
            :stroke-width="1.8"
        /></template>
        {{ panel.addLabel }}
      </UiButton>
    </header>

    <div :class="$style.grid">
      <article
        v-for="item in items"
        :key="item.id"
        :class="$style.card"
      >
        <span :class="[$style.mark, $style[`tone_${item.tone}`]]">{{ item.mark }}</span>
        <div :class="$style.copy">
          <div>
            <h3>{{ item.name }}</h3>
            <span v-if="item.builtIn">内置</span>
          </div>
          <p>{{ item.description }}</p>
        </div>
        <label :class="$style.enabled">
          <UiCheckbox
            :model-value="item.enabled"
            :label="`${item.name}启用状态`"
            @update:model-value="emit('toggle', item.id, $event === true)"
          />
          <span>启用</span>
        </label>
      </article>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply rounded border bg-white px-5 pb-5 pt-4;
  border-color: var(--ui-border);
}

.panelHeader {
  @apply mb-3 flex min-h-10 items-center justify-between;
}

.panelHeader h2 {
  @apply text-[13px] font-semibold;
  color: var(--ui-text);
}

.grid {
  @apply grid grid-cols-2 gap-3;
}

.card {
  @apply flex min-h-[74px] items-center rounded border bg-white px-3 py-2.5;
  border-color: var(--ui-border);
}

.mark {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded text-[13px] font-semibold;
}

.tone_red {
  color: #de350b;
  background: #ffebe6;
}

.tone_green {
  color: #00875a;
  background: #e3fcef;
}

.tone_blue {
  color: #0065ff;
  background: #deebff;
}

.tone_purple {
  color: #6554c0;
  background: #eae6ff;
}

.tone_orange {
  color: #ff8b00;
  background: #fff0d0;
}

.tone_cyan {
  color: #00a3bf;
  background: #e6fcff;
}

.tone_gray {
  color: #6b778c;
  background: #ebecf0;
}

.copy {
  @apply min-w-0 flex-1 px-3;
}

.copy > div {
  @apply flex items-center gap-2;
}

.copy h3 {
  @apply text-[13px] font-semibold;
  color: var(--ui-text);
}

.copy span {
  @apply rounded px-1.5 py-0.5 text-[9px];
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}

.copy p {
  @apply mt-1 truncate text-[11px];
  color: var(--ui-text-subtle);
}

.enabled {
  @apply flex shrink-0 items-center gap-1.5 text-[11px] font-medium;
  color: var(--ui-text-muted);
}

@media (max-width: 1100px) {
  .grid {
    @apply grid-cols-1;
  }
}
</style>
