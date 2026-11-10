<script setup lang="ts">
import { Bot, Pin, Plus, Search, Volume2 } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import type { FeishuConversation } from '../../model/feishu-orders.types';

const props = withDefaults(
  defineProps<{ conversations: readonly FeishuConversation[]; activeId: string; mode?: 'ai' | 'feishu' }>(),
  {
    mode: 'feishu',
  },
);
const emit = defineEmits<{ select: [id: string]; create: [] }>();
const query = ref('');
const filteredConversations = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  if (!needle) return props.conversations;
  return props.conversations.filter((item) =>
    `${item.name} ${item.preview}`.toLocaleLowerCase('zh-CN').includes(needle),
  );
});
</script>

<template>
  <aside
    :class="$style.sidebar"
    :aria-label="props.mode === 'ai' ? 'AI会话列表' : '飞书会话列表'"
  >
    <header :class="$style.searchBar">
      <label :class="$style.search"
        ><Search
          :size="13"
          :stroke-width="1.8" /><input
          v-model="query"
          :placeholder="props.mode === 'ai' ? '搜索会话' : '搜索'"
          aria-label="搜索会话"
      /></label>
      <button
        type="button"
        :title="props.mode === 'ai' ? '新建AI对话' : '新建会话'"
        :aria-label="props.mode === 'ai' ? '新建AI对话' : '新建会话'"
        @click="emit('create')"
      >
        <Plus
          :size="15"
          :stroke-width="1.8"
        />
      </button>
    </header>
    <div :class="$style.list">
      <button
        v-for="item in filteredConversations"
        :key="item.id"
        type="button"
        :class="[$style.item, activeId === item.id && $style.active]"
        @click="emit('select', item.id)"
      >
        <span :class="[$style.avatar, $style[item.tone]]">
          <Bot
            v-if="item.tone === 'bot'"
            :size="16"
            :stroke-width="1.8"
          />
          <b v-else>{{ item.name.charAt(0) }}</b>
          <em v-if="item.unread">{{ item.unread }}</em>
        </span>
        <span :class="$style.info">
          <span :class="$style.nameLine"
            ><strong>{{ item.name }}</strong
            ><time>{{ item.time }}</time></span
          >
          <span :class="$style.preview"
            ><Pin
              v-if="item.pinned"
              :size="9"
              :stroke-width="1.8"
            />{{ item.status === 'Archived' ? '已归档 · ' : '' }}{{ item.preview }}</span
          >
        </span>
      </button>
      <p
        v-if="!filteredConversations.length"
        :class="$style.empty"
      >
        没有匹配的会话
      </p>
    </div>
    <footer>
      <Volume2
        :size="11"
        :stroke-width="1.8"
      />{{ props.mode === 'ai' ? 'AI对话 · 来自服务端记录' : '演示侧栏 · 飞书侧不会渲染该列表(直接走飞书原生)' }}
    </footer>
  </aside>
</template>

<style module>
@reference "../../../../styles/index.css";

.sidebar {
  @apply flex w-[260px] shrink-0 flex-col overflow-hidden rounded-lg border bg-white;
  border-color: var(--ui-border);
}
.searchBar {
  @apply flex h-[52px] shrink-0 items-center gap-2 border-b px-3;
  border-color: var(--ui-border);
}
.search {
  @apply relative min-w-0 flex-1;
}
.search svg {
  @apply pointer-events-none absolute left-2.5 top-1/2 -translate-y-1/2;
  color: #97a0af;
}
.search input {
  @apply h-8 w-full rounded border-0 py-1.5 pl-7 pr-2 text-[12px] outline-none;
  color: var(--ui-text);
  background: var(--ui-surface-hover);
}
.search input:focus {
  @apply bg-white;
  box-shadow: 0 0 0 2px rgb(51 112 255 / 15%);
}
.searchBar > button {
  @apply rounded p-1.5 transition-colors;
  color: var(--ui-text-muted);
}
.searchBar > button:hover {
  background: var(--ui-surface-hover);
}
.list {
  @apply min-h-0 flex-1 overflow-y-auto;
}
.item {
  @apply flex w-full items-start gap-2.5 border-l-2 border-transparent px-3 py-2.5 text-left transition-colors;
}
.item:hover {
  background: var(--ui-surface-hover);
}
.active {
  border-color: #3370ff;
  background: #e6f0ff;
}
.avatar {
  @apply relative flex h-9 w-9 shrink-0 items-center justify-center rounded-lg text-white;
}
.avatar em {
  @apply absolute -right-1 -top-1 flex h-4 min-w-4 items-center justify-center rounded-full px-1 text-[9px] font-bold not-italic text-white;
  background: var(--ui-danger);
}
.bot {
  background: #3370ff;
}
.coral {
  background: #ff7452;
}
.green {
  background: #36b37e;
}
.purple {
  background: #6554c0;
}
.orange {
  background: #ffab00;
}
.info {
  @apply min-w-0 flex-1;
}
.nameLine {
  @apply flex items-center gap-1.5;
}
.nameLine strong {
  @apply min-w-0 flex-1 truncate text-[12px] font-medium;
  color: var(--ui-text);
}
.active .nameLine strong {
  @apply font-semibold;
}
.nameLine time {
  @apply shrink-0 text-[10px];
  color: #97a0af;
}
.preview {
  @apply mt-0.5 flex items-center gap-1 truncate text-[11px];
  color: var(--ui-text-subtle);
}
.preview svg {
  @apply shrink-0;
  color: #ffab00;
}
.empty {
  @apply p-6 text-center text-[11px];
  color: #97a0af;
}
.sidebar footer {
  @apply flex items-center gap-1.5 border-t px-3 py-2 text-[10px] leading-4;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
</style>
