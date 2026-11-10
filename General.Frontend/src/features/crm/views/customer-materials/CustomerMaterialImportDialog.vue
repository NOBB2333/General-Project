<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { ClipboardPaste, FileSpreadsheet, Upload } from 'lucide-vue-next';
import { UiButton, UiDialog, UiFormField, UiTextarea } from '@/components/ui';
import type { CustomerMaterialDraft, CustomerMaterialImportMode } from '@/features/crm/model/customer-materials.types';
import { parseCustomerMaterialCsv } from './customerMaterialCsv';

const props = defineProps<{
  open: boolean;
  mode: CustomerMaterialImportMode;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  import: [drafts: CustomerMaterialDraft[]];
}>();

const source = ref('');
const fileName = ref('');
const drafts = computed(() => parseCustomerMaterialCsv(source.value));

watch(
  () => props.open,
  (open) => {
    if (!open) return;
    source.value = '';
    fileName.value = '';
  },
);

async function readFile(event: Event) {
  const file = (event.target as HTMLInputElement).files?.[0];
  if (!file) return;
  fileName.value = file.name;
  source.value = await file.text();
}

function submit() {
  if (drafts.value.length) emit('import', drafts.value);
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="mode === 'file' ? '导入物料对照清单' : '粘贴导入物料对照'"
    description="数据仅导入当前本地会话，刷新页面后恢复参考数据。"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <template #icon>
      <span :class="$style.dialogIcon">
        <FileSpreadsheet
          v-if="mode === 'file'"
          :size="18"
        />
        <ClipboardPaste
          v-else
          :size="18"
        />
      </span>
    </template>
    <section :class="$style.content">
      <label
        v-if="mode === 'file'"
        :class="$style.filePicker"
      >
        <Upload :size="16" />
        <span>{{ fileName || '选择 CSV 文件' }}</span>
        <input
          type="file"
          accept=".csv,text/csv"
          @change="readFile"
        />
      </label>
      <UiFormField
        label="CSV 内容"
        hint="必需列：客户名称、我方物料编码、我方物料名称、客户物料编码、客户物料名称"
      >
        <UiTextarea
          v-model="source"
          :class="$style.sourceInput"
          placeholder="客户名称,我方物料编码,我方物料名称,客户物料编码,客户物料名称"
        />
      </UiFormField>
      <header :class="$style.previewHeader">
        <strong>导入预览</strong>
        <span>{{ drafts.length }} 条有效记录</span>
      </header>
      <div :class="$style.previewList">
        <article
          v-for="draft in drafts.slice(0, 5)"
          :key="draft.customer + '-' + draft.customerCode"
        >
          <strong>{{ draft.customer }}</strong>
          <span>{{ draft.internalCode }} ↔ {{ draft.customerCode }} · {{ draft.customerName }}</span>
        </article>
        <p v-if="!drafts.length">输入清单后显示有效记录</p>
      </div>
    </section>
    <template #footer>
      <UiButton @click="emit('update:open', false)">取消</UiButton>
      <UiButton
        variant="primary"
        :disabled="!drafts.length"
        @click="submit"
      >
        导入 {{ drafts.length }} 条
      </UiButton>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.content {
  @apply grid gap-4 p-5;
}

.filePicker {
  @apply flex min-h-12 cursor-pointer items-center justify-center gap-2 rounded border border-dashed text-[12px] font-medium transition-colors;
  color: var(--app-primary);
  border-color: var(--app-primary-light);
  background: var(--app-primary-soft);
}

.filePicker:hover {
  border-color: var(--app-primary);
}

.filePicker input {
  @apply sr-only;
}

.sourceInput {
  min-height: 112px;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
}

.previewHeader {
  @apply flex items-center justify-between text-[12px];
}

.previewHeader span,
.previewList span,
.previewList p {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}

.previewList {
  @apply grid max-h-44 gap-1 overflow-y-auto rounded border p-2;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.previewList article {
  @apply rounded bg-white px-3 py-2;
}

.previewList strong,
.previewList span {
  @apply block;
}

.previewList p {
  @apply py-8 text-center;
}
</style>
