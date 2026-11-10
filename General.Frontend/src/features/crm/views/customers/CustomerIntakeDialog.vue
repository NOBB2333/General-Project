<script setup lang="ts">
import { computed, onBeforeUnmount, reactive, ref, watch } from 'vue';
import { FileSpreadsheet, ScanLine, Upload } from 'lucide-vue-next';
import { UiButton, UiDialog, UiFormField, UiInput, UiTextarea } from '@/components/ui';
import type { CustomerDraft } from '@/features/crm/model/customers.types';
import { parseCustomerCsv } from './customerCsv';

type IntakeMode = 'import' | 'scan';

const props = defineProps<{
  open: boolean;
  mode: IntakeMode;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  import: [drafts: CustomerDraft[]];
  scan: [draft: CustomerDraft];
}>();

const csvText = ref('');
const csvFileName = ref('');
const cardFileName = ref('');
const cardPreview = ref('');
const cardDraft = reactive<CustomerDraft>(emptyDraft());

const importedDrafts = computed(() => parseCustomerCsv(csvText.value));
const scanReady = computed(
  () =>
    Boolean(cardFileName.value) &&
    Boolean(cardDraft.name.trim()) &&
    Boolean(cardDraft.contact.trim()) &&
    Boolean(cardDraft.phone.trim()),
);

watch(
  () => props.open,
  (open) => {
    if (!open) {
      releasePreview();
      return;
    }
    csvText.value = '';
    csvFileName.value = '';
    cardFileName.value = '';
    releasePreview();
    Object.assign(cardDraft, emptyDraft());
  },
);

onBeforeUnmount(releasePreview);

function emptyDraft(): CustomerDraft {
  return {
    name: '',
    type: '终端客户',
    level: 'B级（一般客户）',
    owner: '张伟',
    contact: '',
    phone: '',
    email: '',
    address: '',
    creditLimit: '300000',
    paymentDays: '30',
  };
}

async function readCsv(event: Event) {
  const file = (event.target as HTMLInputElement).files?.[0];
  if (!file) return;
  csvFileName.value = file.name;
  csvText.value = await file.text();
}

function readCard(event: Event) {
  const file = (event.target as HTMLInputElement).files?.[0];
  if (!file) return;
  cardFileName.value = file.name;
  releasePreview();
  cardPreview.value = URL.createObjectURL(file);
}

function releasePreview() {
  if (cardPreview.value) URL.revokeObjectURL(cardPreview.value);
  cardPreview.value = '';
}

function submitImport() {
  if (!importedDrafts.value.length) return;
  emit('import', importedDrafts.value);
}

function submitCard() {
  if (!scanReady.value) return;
  emit('scan', { ...cardDraft });
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="mode === 'import' ? '批量导入客户' : '扫描名片录入'"
    :description="mode === 'import' ? '从本地 CSV 文件预览并导入客户档案。' : '名片图片仅在当前浏览器会话中预览。'"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <template #icon>
      <span :class="$style.dialogIcon">
        <FileSpreadsheet
          v-if="mode === 'import'"
          :size="18"
          :stroke-width="1.8"
        />
        <ScanLine
          v-else
          :size="18"
          :stroke-width="1.8"
        />
      </span>
    </template>

    <section
      v-if="mode === 'import'"
      :class="$style.importContent"
    >
      <label :class="$style.filePicker">
        <Upload :size="16" />
        <span>{{ csvFileName || '选择 CSV 文件' }}</span>
        <input
          type="file"
          accept=".csv,text/csv"
          @change="readCsv"
        />
      </label>
      <UiFormField
        label="CSV 内容"
        hint="必需列：客户名称、联系人、联系电话"
      >
        <UiTextarea
          v-model="csvText"
          :class="$style.csvInput"
          placeholder="客户名称,客户类型,联系人,联系电话"
        />
      </UiFormField>
      <div :class="$style.previewHeader">
        <strong>导入预览</strong>
        <span>{{ importedDrafts.length }} 条有效记录</span>
      </div>
      <div :class="$style.previewList">
        <article
          v-for="draft in importedDrafts.slice(0, 5)"
          :key="`${draft.name}-${draft.phone}`"
        >
          <strong>{{ draft.name }}</strong>
          <span>{{ draft.type }} · {{ draft.contact }} · {{ draft.phone }}</span>
        </article>
        <p v-if="!importedDrafts.length">选择文件或粘贴 CSV 后显示有效记录</p>
      </div>
    </section>

    <section
      v-else
      :class="$style.scanContent"
    >
      <label :class="$style.cardPicker">
        <img
          v-if="cardPreview"
          :src="cardPreview"
          alt="名片本地预览"
        />
        <span v-else><ScanLine :size="28" />选择名片图片</span>
        <input
          type="file"
          accept="image/png,image/jpeg,image/webp"
          @change="readCard"
        />
      </label>
      <div :class="$style.cardFields">
        <UiFormField
          label="客户名称"
          required
          wide
        >
          <UiInput
            v-model="cardDraft.name"
            placeholder="请输入名片上的企业名称"
          />
        </UiFormField>
        <UiFormField
          label="联系人"
          required
        >
          <UiInput
            v-model="cardDraft.contact"
            placeholder="请输入名片联系人"
          />
        </UiFormField>
        <UiFormField
          label="联系电话"
          required
        >
          <UiInput
            v-model="cardDraft.phone"
            placeholder="请输入名片联系电话"
          />
        </UiFormField>
        <UiFormField label="邮箱">
          <UiInput
            v-model="cardDraft.email"
            type="email"
            placeholder="name@company.com"
          />
        </UiFormField>
        <UiFormField label="详细地址">
          <UiInput
            v-model="cardDraft.address"
            placeholder="请输入名片地址"
          />
        </UiFormField>
      </div>
    </section>

    <template #footer>
      <UiButton @click="emit('update:open', false)">取消</UiButton>
      <UiButton
        v-if="mode === 'import'"
        variant="primary"
        :disabled="!importedDrafts.length"
        @click="submitImport"
      >
        导入 {{ importedDrafts.length }} 条
      </UiButton>
      <UiButton
        v-else
        variant="primary"
        :disabled="!scanReady"
        @click="submitCard"
      >
        创建客户
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

.importContent,
.scanContent {
  @apply grid gap-4 p-5;
}

.filePicker {
  @apply flex min-h-12 cursor-pointer items-center justify-center gap-2 rounded border border-dashed text-[12px] font-medium transition-colors;
  color: var(--app-primary);
  border-color: var(--app-primary-light);
  background: var(--app-primary-soft);
}

.filePicker:hover,
.cardPicker:hover {
  border-color: var(--app-primary);
}

.filePicker input,
.cardPicker input {
  @apply sr-only;
}

.csvInput {
  min-height: 108px;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
}

.previewHeader {
  @apply flex items-center justify-between text-[12px];
  color: var(--ui-text);
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

.scanContent {
  @apply grid-cols-[180px_minmax(0,1fr)];
}

.cardPicker {
  @apply flex min-h-[230px] cursor-pointer items-center justify-center overflow-hidden rounded border border-dashed transition-colors;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.cardPicker img {
  @apply h-full w-full object-contain;
}

.cardPicker span {
  @apply flex flex-col items-center gap-2 text-[11px];
}

.cardFields {
  @apply grid grid-cols-2 gap-4;
}

@media (max-width: 720px) {
  .scanContent {
    @apply grid-cols-1;
  }
}
</style>
