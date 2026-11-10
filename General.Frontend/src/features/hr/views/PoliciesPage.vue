<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { Download, Plus, Settings2 } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast, type UiSelectOption } from '@/components/ui';
import { policiesFixture, policyCategories } from '../model/policies.data';
import type { PolicyCategory, PolicyDraft, PolicyRecord } from '../model/policies.types';
import PoliciesHero from './policies/PoliciesHero.vue';
import PoliciesSummary from './policies/PoliciesSummary.vue';
import PoliciesWorkspace from './policies/PoliciesWorkspace.vue';

const rows = ref<PolicyRecord[]>(cloneReferenceRows(policiesFixture.rows));
const query = ref('');
const category = ref('全部');
const status = ref('全部状态');
const creator = ref('全部创建人');
const sort = ref('生效日期');
const formOpen = ref(false);
const categoriesOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<PolicyRecord | null>(null);
const categoryOptions: readonly UiSelectOption[] = policyCategories.slice(1).map((value) => ({ value, label: value }));
const draft = reactive<PolicyDraft>({ title: '', code: '', category: '人事制度', version: 'V1.0', summary: '' });

const filteredRows = computed(() => {
  const keyword = query.value.trim().toLowerCase();
  const result = rows.value.filter((row) => {
    if (category.value !== '全部' && row.category !== category.value) return false;
    if (status.value !== '全部状态' && row.status !== status.value) return false;
    if (creator.value !== '全部创建人' && row.creator !== creator.value) return false;
    return !keyword || `${row.title}${row.summary}${row.code}`.toLowerCase().includes(keyword);
  });
  return [...result].sort((a, b) => {
    if (sort.value === '版本') return b.version.localeCompare(a.version);
    if (a.status !== b.status) return a.status === '生效中' ? -1 : 1;
    return b.effectiveAt.localeCompare(a.effectiveAt);
  });
});

function openDetail(row: PolicyRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}
function openForm() {
  Object.assign(draft, {
    title: '',
    code: `POL-${String(rows.value.length + 1).padStart(3, '0')}`,
    category: '人事制度',
    version: 'V1.0',
    summary: '',
  });
  formOpen.value = true;
}
function savePolicy() {
  if (!draft.title.trim() || !draft.code.trim()) {
    toast.warning('请填写制度名称和编号');
    return;
  }
  rows.value = [
    {
      id: `policy-${Date.now()}`,
      code: draft.code.trim(),
      title: draft.title.trim(),
      version: draft.version.trim() || 'V1.0',
      summary: draft.summary.trim() || '本地新增制度草稿',
      category: draft.category,
      status: '修订中',
      effectiveAt: '2026-09-17',
      creator: '孙丽萍',
      audience: '全体员工',
    },
    ...rows.value,
  ];
  formOpen.value = false;
  category.value = '全部';
  status.value = '全部状态';
  toast.success('制度草稿已保存到本地');
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="board"
    data-page-route="/hr/policies"
    data-page-label="规章制度"
    data-page-module="人事管理"
    data-policies-page
    :data-result-count="filteredRows.length"
  >
    <PoliciesHero />
    <div :class="$style.actions">
      <UiButton
        size="sm"
        @click="categoriesOpen = true"
        ><template #leading
          ><Settings2
            :size="14"
            :stroke-width="1.8" /></template
        >类别管理</UiButton
      >
      <UiButton
        size="sm"
        @click="toast.success('制度汇编已生成到本地下载队列')"
        ><template #leading
          ><Download
            :size="14"
            :stroke-width="1.8" /></template
        >导出汇编</UiButton
      >
      <UiButton
        variant="primary"
        size="sm"
        @click="openForm"
        ><template #leading
          ><Plus
            :size="14"
            :stroke-width="1.8" /></template
        >新增制度</UiButton
      >
    </div>
    <PoliciesSummary />
    <PoliciesWorkspace
      v-model:query="query"
      v-model:category="category"
      v-model:status="status"
      v-model:creator="creator"
      v-model:sort="sort"
      :rows="filteredRows"
      @open="openDetail"
    />
  </main>

  <UiDialog
    v-model:open="categoriesOpen"
    title="制度类别"
    description="当前共 6 个制度类别"
    size="md"
  >
    <div :class="$style.categoryGrid">
      <p
        v-for="item in policyCategories.slice(1)"
        :key="item"
      >
        <strong>{{ item }}</strong
        ><span>{{ rows.filter((row) => row.category === item).length }} 项制度</span>
      </p>
    </div>
    <template #footer
      ><UiButton
        variant="primary"
        size="sm"
        @click="categoriesOpen = false"
        >完成</UiButton
      ></template
    >
  </UiDialog>
  <UiDialog
    v-model:open="formOpen"
    title="新增制度"
    description="新制度仅保存到当前本地会话"
    size="lg"
  >
    <form
      :class="$style.form"
      @submit.prevent="savePolicy"
    >
      <UiFormField
        label="制度名称"
        required
        wide
        ><UiInput
          v-model="draft.title"
          aria-label="制度名称"
      /></UiFormField>
      <UiFormField
        label="制度编号"
        required
        ><UiInput
          v-model="draft.code"
          aria-label="制度编号"
      /></UiFormField>
      <UiFormField label="版本"
        ><UiInput
          v-model="draft.version"
          aria-label="制度版本"
      /></UiFormField>
      <UiFormField label="制度类别"
        ><UiSelect
          v-model="draft.category"
          :options="categoryOptions"
          label="制度类别"
      /></UiFormField>
      <UiFormField
        label="制度摘要"
        wide
        ><UiInput
          v-model="draft.summary"
          aria-label="制度摘要"
      /></UiFormField>
    </form>
    <template #footer
      ><UiButton
        size="sm"
        @click="formOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        size="sm"
        @click="savePolicy"
        >保存制度</UiButton
      ></template
    >
  </UiDialog>
  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.title || '制度详情'"
    :description="activeRow ? `${activeRow.code} · ${activeRow.version}` : undefined"
    size="lg"
  >
    <dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>制度类别</dt>
        <dd>{{ activeRow.category }}</dd>
      </div>
      <div>
        <dt>当前状态</dt>
        <dd>{{ activeRow.status }}</dd>
      </div>
      <div>
        <dt>生效日期</dt>
        <dd>{{ activeRow.effectiveAt }}</dd>
      </div>
      <div>
        <dt>创建人</dt>
        <dd>{{ activeRow.creator }}</dd>
      </div>
      <div :class="$style.wide">
        <dt>适用范围</dt>
        <dd>{{ activeRow.audience }}</dd>
      </div>
      <div :class="$style.wide">
        <dt>制度摘要</dt>
        <dd>{{ activeRow.summary }}</dd>
      </div>
    </dl>
    <template #footer
      ><UiButton
        variant="primary"
        size="sm"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1280px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.actions {
  @apply flex h-[64px] items-center justify-end gap-2;
}
.categoryGrid {
  @apply grid grid-cols-2 gap-3 p-5;
}
.categoryGrid p {
  @apply m-0 flex items-center justify-between rounded border px-4 py-3 text-[11px];
  border-color: var(--ui-border-subtle);
}
.categoryGrid span {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.details {
  @apply grid grid-cols-2 gap-4 p-5;
}
.details div {
  @apply rounded border p-4;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
.details dt {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.details dd {
  @apply m-0 mt-2 text-[11px] font-medium;
}
.wide {
  @apply col-span-2;
}
</style>
