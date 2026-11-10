<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { ArrowLeft, Plus, Settings2 } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, UiTabs, UiTabsList, UiTabsTrigger, toast } from '@/components/ui';
import { serviceConfigPanels, serviceConfigReferenceFixture } from '../model/service-config.data';
import type { ServiceConfigDraft, ServiceConfigItem, ServiceConfigTabKey } from '../model/service-config.types';
import ServiceConfigPanel from './service-config/ServiceConfigPanel.vue';

const items = ref<ServiceConfigItem[]>(cloneReferenceRows(serviceConfigReferenceFixture.rows));
const activeTab = ref<ServiceConfigTabKey>('types');
const dialogOpen = ref(false);
const draft = reactive<ServiceConfigDraft>({ name: '', description: '' });

const activePanel = computed(
  () => serviceConfigPanels.find((panel) => panel.key === activeTab.value) ?? serviceConfigPanels[0],
);
const activeItems = computed(() => items.value.filter((item) => item.tab === activeTab.value));
const enabledCount = computed(() => activeItems.value.filter((item) => item.enabled).length);

function openCreate() {
  Object.assign(draft, { name: '', description: '' });
  dialogOpen.value = true;
}

function toggleItem(id: string, enabled: boolean) {
  const item = items.value.find((entry) => entry.id === id);
  if (!item) return;
  item.enabled = enabled;
  toast.success(`${item.name}已${enabled ? '启用' : '停用'}`);
}

function createItem() {
  if (!draft.name.trim() || !draft.description.trim()) {
    toast.warning('请填写配置名称和说明');
    return;
  }
  items.value.push({
    id: `${activeTab.value}-local-${Date.now()}`,
    tab: activeTab.value,
    mark: draft.name.trim().slice(0, 1),
    name: draft.name.trim(),
    description: draft.description.trim(),
    builtIn: false,
    enabled: true,
    tone: 'blue',
  });
  dialogOpen.value = false;
  toast.success(`${draft.name.trim()}已添加到本地配置`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="cards"
    data-page-route="/service/service-config"
    data-page-label="服务配置"
    data-page-module="服务管理"
    data-service-config-page
    :data-active-tab="activeTab"
    :data-item-count="activeItems.length"
    :data-enabled-count="enabledCount"
  >
    <header :class="$style.hero">
      <div :class="$style.heroCopy">
        <nav aria-label="面包屑"><span>服务管理</span><b>›</b><strong>服务配置</strong></nav>
        <div :class="$style.identity">
          <span :class="$style.heroIcon"
            ><Settings2
              :size="23"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>服务配置</h1>
            <p>工单类型、状态、质保、SLA、费用目录、服务人员与编号规则统一配置</p>
          </div>
        </div>
      </div>
      <div
        :class="$style.heroArt"
        aria-hidden="true"
      >
        <i
          v-for="cell in 4"
          :key="cell"
        />
      </div>
      <UiButton
        :class="$style.backButton"
        size="sm"
        variant="ghost"
        @click="toast.info('已返回本地服务工单')"
      >
        <template #leading
          ><ArrowLeft
            :size="14"
            :stroke-width="1.8"
        /></template>
        服务工单
      </UiButton>
    </header>

    <UiTabs v-model="activeTab">
      <UiTabsList
        label="服务配置分类"
        :class="$style.tabs"
      >
        <UiTabsTrigger
          v-for="panel in serviceConfigPanels"
          :key="panel.key"
          :class="$style.tab"
          :value="panel.key"
        >
          {{ panel.label }}
        </UiTabsTrigger>
      </UiTabsList>

      <ServiceConfigPanel
        :panel="activePanel"
        :items="activeItems"
        @add="openCreate"
        @toggle="toggleItem"
      />
    </UiTabs>
  </main>

  <UiDialog
    v-model:open="dialogOpen"
    :title="activePanel.addLabel"
    :description="`添加到${activePanel.label}，仅影响当前本地复刻数据`"
    size="md"
  >
    <div :class="$style.form">
      <UiFormField
        label="配置名称"
        required
      >
        <UiInput
          v-model="draft.name"
          aria-label="配置名称"
          placeholder="请输入名称"
        />
      </UiFormField>
      <UiFormField
        label="配置说明"
        required
      >
        <UiInput
          v-model="draft.description"
          aria-label="配置说明"
          placeholder="请输入适用范围或规则说明"
        />
      </UiFormField>
    </div>
    <template #footer>
      <UiButton
        variant="secondary"
        @click="dialogOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        @click="createItem"
      >
        <template #leading
          ><Plus
            :size="14"
            :stroke-width="1.8"
        /></template>
        确认添加
      </UiButton>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full min-w-[1180px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}

.hero {
  @apply relative mb-3 min-h-[120px] overflow-hidden px-6 py-5 text-white;
  border-radius: 14px;
  background: linear-gradient(108deg, #102a5c 0%, #0747a6 56%, #00b8d9 100%);
}

.heroCopy {
  @apply relative z-[1];
}

.hero nav {
  @apply mb-3 flex items-center gap-2 text-[12px];
  color: rgb(255 255 255 / 80%);
}

.hero nav strong {
  color: white;
}

.identity {
  @apply flex items-center gap-3;
}

.heroIcon {
  @apply flex h-11 w-11 items-center justify-center rounded-xl border;
  border-color: rgb(255 255 255 / 25%);
  background: rgb(255 255 255 / 15%);
}

.identity h1 {
  @apply text-[22px] font-bold;
}

.identity p {
  @apply mt-1 text-[12px];
  color: rgb(255 255 255 / 82%);
}

.heroArt {
  @apply absolute right-[275px] top-[-32px] grid h-[150px] w-[150px] grid-cols-2 place-items-center rounded-[28px] border-2 p-6 opacity-50;
  border-color: rgb(255 255 255 / 55%);
  transform: rotate(18deg);
}

.heroArt i {
  @apply h-8 w-8 rounded;
  background: rgb(255 255 255 / 70%);
}

.backButton {
  @apply absolute right-6 top-[58px];
  color: white;
  background: rgb(255 255 255 / 13%);
  border: 1px solid rgb(255 255 255 / 30%);
}

.tabs {
  @apply mb-3 flex h-12 w-full border-b px-1;
  border-color: var(--ui-border);
}

.tab {
  @apply h-12 rounded-none border-b-2 border-transparent px-4 text-[12px];
  background: transparent;
}

.tab[data-state='active'] {
  @apply font-semibold;
  color: var(--app-primary);
  border-color: var(--app-primary);
  background: transparent;
}

.form {
  @apply grid gap-4 p-5;
}

@media (prefers-reduced-motion: reduce) {
  .tab {
    transition: none;
  }
}
</style>
