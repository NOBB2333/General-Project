<script setup lang="ts">
import { FileScan } from 'lucide-vue-next';
import { ref, watch } from 'vue';
import { UiButton, UiDialog, toast } from '@/components/ui';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{
  'update:open': [value: boolean];
  recognize: [fileName: string];
}>();
const selectedFileName = ref('');

watch(
  () => props.open,
  (open) => {
    if (open) selectedFileName.value = '';
  },
);

function selectFile(event: Event) {
  selectedFileName.value = (event.target as HTMLInputElement).files?.[0]?.name ?? '';
}

function submit() {
  if (!selectedFileName.value) {
    toast.warning('请先选择本地票据文件');
    return;
  }
  emit('recognize', selectedFileName.value);
}
</script>

<template>
  <UiDialog
    :open="open"
    title="批量识别票据"
    description="选择本地票据图片或 PDF，识别结果不会上传到外部站点"
    size="md"
    @update:open="emit('update:open', $event)"
  >
    <div :class="$style.body">
      <label :class="$style.filePicker">
        <FileScan
          :size="28"
          :stroke-width="1.8"
        />
        <strong>{{ selectedFileName || '选择票据文件' }}</strong>
        <small>支持 JPG、PNG、PDF</small>
        <input
          type="file"
          accept="image/jpeg,image/png,application/pdf"
          @change="selectFile"
        />
      </label>
    </div>
    <template #footer>
      <UiButton
        size="sm"
        variant="secondary"
        @click="emit('update:open', false)"
        >取消</UiButton
      >
      <UiButton
        size="sm"
        variant="primary"
        @click="submit"
        >开始识别</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.body {
  @apply p-5;
}

.filePicker {
  @apply flex min-h-36 cursor-pointer flex-col items-center justify-center rounded border border-dashed text-center transition-colors;
  color: var(--app-primary);
  border-color: var(--app-primary-light);
  background: var(--app-primary-soft);
}

.filePicker:hover {
  border-color: var(--app-primary);
}

.filePicker strong {
  @apply mt-3 text-[12px];
}

.filePicker small {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}

.filePicker input {
  @apply sr-only;
}
</style>
