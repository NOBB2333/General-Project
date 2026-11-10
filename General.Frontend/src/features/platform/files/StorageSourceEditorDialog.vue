<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import type { PlatformFileStorageSourceApi } from '@/api/core/platform-file-storage-sources';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiSwitch, UiTextarea } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import {
  createStorageSourceDraft,
  toStorageSourceDraft,
  toStorageSourceSaveInput,
  validateStorageSourceDraft,
  type StorageSourceDraft,
} from './storage-sources.model';

const props = defineProps<{
  error: string;
  open: boolean;
  saving: boolean;
  source: null | PlatformFileStorageSourceApi.Item;
}>();

const emit = defineEmits<{
  'update:open': [open: boolean];
  submit: [input: PlatformFileStorageSourceApi.SaveInput];
}>();

const draft = reactive<StorageSourceDraft>(createStorageSourceDraft());
const localError = ref('');
const mode = computed(() => (props.source ? 'edit' : 'create'));
const shownError = computed(() => localError.value || props.error);
const providerOptions = computed(() => {
  const options: UiSelectOption[] = [
    { label: '本地文件存储（Local）', value: 'Local' },
    { label: '阿里云 OSS（AliyunOSS）', value: 'AliyunOSS' },
    { label: 'MinIO', value: 'MinIO' },
  ];
  if (draft.providerName && !options.some((item) => item.value === draft.providerName)) {
    options.unshift({ label: `${draft.providerName}（当前版本不支持）`, value: draft.providerName, disabled: true });
  }
  return options;
});
const isCloudProvider = computed(() => draft.providerName !== 'Local');

function resetDraft() {
  const source = props.source;
  Object.assign(draft, source ? toStorageSourceDraft(source) : createStorageSourceDraft());
}

watch(
  () => [props.open, props.source] as const,
  ([open]) => {
    if (open) {
      localError.value = '';
      resetDraft();
    } else {
      draft.secretKey = '';
    }
  },
  { immediate: true },
);

function submit() {
  const validationMessage = validateStorageSourceDraft(draft);
  if (validationMessage) {
    localError.value = validationMessage;
    return;
  }
  localError.value = '';
  emit('submit', toStorageSourceSaveInput(draft));
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="mode === 'create' ? '新增文件存储源' : '编辑文件存储源'"
    description="配置平台文件服务使用的存储位置；密钥不会回显。"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form @submit.prevent="submit">
      <UiFormField
        label="存储源名称"
        required
      >
        <UiInput
          v-model="draft.name"
          maxlength="128"
          autocomplete="off"
          placeholder="例如：平台本地文件"
          required
        />
      </UiFormField>
      <UiFormField
        label="存储类型"
        required
      >
        <UiSelect
          v-model="draft.providerName"
          label="存储类型"
          :options="providerOptions"
          required
        />
      </UiFormField>
      <template v-if="isCloudProvider">
        <UiFormField
          label="Endpoint"
          required
          hint="后端不提供单独的连通性检测接口；云端是否可用需通过实际文件操作验证。"
        >
          <UiInput
            v-model="draft.endpoint"
            maxlength="512"
            autocomplete="url"
            placeholder="https://oss-cn-hangzhou.aliyuncs.com"
            required
          />
        </UiFormField>
        <UiFormField
          label="AccessKeyId"
          required
          hint="该标识由后端返回，可按需重新填写。"
        >
          <UiInput
            v-model="draft.accessKeyId"
            maxlength="256"
            autocomplete="off"
            required
          />
        </UiFormField>
        <UiFormField
          label="SecretKey"
          required
          hint="密钥不会从后端读取。每次编辑云存储配置时都必须重新输入，否则后端会清除已保存密钥。"
        >
          <UiInput
            v-model="draft.secretKey"
            type="password"
            maxlength="1024"
            autocomplete="new-password"
            required
          />
        </UiFormField>
        <UiFormField
          label="Bucket名称"
          required
        >
          <UiInput
            v-model="draft.bucketName"
            maxlength="128"
            autocomplete="off"
            required
          />
        </UiFormField>
        <UiFormField label="区域">
          <UiInput
            v-model="draft.region"
            maxlength="128"
            autocomplete="off"
          />
        </UiFormField>
        <UiFormField label="自定义访问域名">
          <UiInput
            v-model="draft.customDomain"
            maxlength="512"
            autocomplete="url"
          />
        </UiFormField>
        <UiFormField label="对象路径模板">
          <UiInput
            v-model="draft.pathTemplate"
            maxlength="128"
            autocomplete="off"
            placeholder="可留空"
          />
        </UiFormField>
      </template>
      <template v-else>
        <UiFormField
          label="本地根目录"
          hint="建议使用部署环境可持久化且受控的目录。"
        >
          <UiInput
            v-model="draft.rootPath"
            maxlength="512"
            autocomplete="off"
            placeholder="留空时使用服务端默认目录"
          />
        </UiFormField>
      </template>
      <UiFormField label="备注">
        <UiTextarea
          v-model="draft.remark"
          maxlength="256"
          rows="3"
          placeholder="用途或负责人"
        />
      </UiFormField>
      <UiFormField label="启用状态">
        <UiSwitch
          v-model="draft.isEnabled"
          label="启用此存储源"
        />
      </UiFormField>
      <UiFormField label="默认存储源">
        <UiSwitch
          v-model="draft.isDefault"
          label="设为默认存储源"
          :disabled="!draft.isEnabled || Boolean(source?.isDefault)"
        />
      </UiFormField>
      <UiFormField
        label="文件公开属性"
        hint="新上传文件的公开属性；已有文件不随此选项改变。"
      >
        <UiSwitch
          v-model="draft.isPublic"
          label="默认将新文件标为公开"
        />
      </UiFormField>
      <UiFormField
        v-if="isCloudProvider"
        label="使用SSL"
      >
        <UiSwitch
          v-model="draft.useSsl"
          label="使用SSL连接云存储"
        />
      </UiFormField>
      <p
        v-if="shownError"
        role="alert"
      >
        {{ shownError }}
      </p>
    </form>
    <template #footer>
      <UiButton
        :disabled="saving"
        @click="emit('update:open', false)"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :loading="saving"
        :disabled="saving"
        @click="submit"
        >{{ saving ? '正在保存…' : mode === 'create' ? '创建存储源' : '保存配置' }}</UiButton
      >
    </template>
  </UiDialog>
</template>
