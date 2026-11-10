<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import type { TenantApi } from '@/api/core/tenant';
import { UiButton, UiDialog, UiFormField, UiInput, UiTextarea } from '@/components/ui';
import {
  createTenantDraft,
  toTenantSaveInput,
  validateTenantDraft,
  type TenantDraft,
  type TenantFormMode,
} from './tenant.model';

const props = defineProps<{
  mode: TenantFormMode;
  open: boolean;
  saveError: string;
  saving: boolean;
  tenant: TenantApi.ListItem | null;
}>();

const emit = defineEmits<{
  'update:open': [open: boolean];
  submit: [input: TenantApi.SaveInput];
}>();

const draft = reactive<TenantDraft>(createTenantDraft());
const validationError = ref('');

watch(
  () => [props.open, props.mode, props.tenant?.id] as const,
  ([open]) => {
    if (!open) return;
    Object.assign(draft, createTenantDraft(props.tenant ?? undefined));
    validationError.value = '';
  },
  { immediate: true },
);

function submit() {
  validationError.value = validateTenantDraft(draft, props.mode) ?? '';
  if (validationError.value) return;
  emit('submit', toTenantSaveInput(draft, props.mode));
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="mode === 'create' ? '新建租户' : '编辑租户'"
    :description="mode === 'create' ? '创建租户并初始化租户管理员。' : '修改租户名称或独立数据库连接配置。'"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="submit"
    >
      <div :class="$style.formGrid">
        <UiFormField
          label="租户名称"
          required
        >
          <UiInput
            v-model="draft.name"
            maxlength="64"
            placeholder="请输入租户名称"
          />
        </UiFormField>
        <UiFormField
          label="默认数据库连接串"
          :hint="mode === 'edit' ? '留空保留现有配置；列表只显示脱敏信息。' : '可选；留空时使用平台默认数据库。'"
        >
          <UiInput
            v-model="draft.defaultConnectionString"
            type="password"
            maxlength="512"
            autocomplete="off"
            placeholder="可选，按部署环境填写"
          />
        </UiFormField>

        <template v-if="mode === 'create'">
          <UiFormField
            label="管理员账号"
            required
            hint="3–64位，以英文字母开头，可用英文、数字、点、下划线和中划线。"
          >
            <UiInput
              v-model="draft.adminUserName"
              maxlength="64"
              autocomplete="off"
              placeholder="例如 tenant.admin"
            />
          </UiFormField>
          <UiFormField
            label="管理员初始密码"
            required
            hint="密码规则由服务端校验。"
          >
            <UiInput
              v-model="draft.adminPassword"
              type="password"
              maxlength="128"
              autocomplete="new-password"
              placeholder="设置初始密码"
            />
          </UiFormField>
          <UiFormField
            label="管理员邮箱"
            hint="可选；不填写时由服务端生成租户内邮箱。"
          >
            <UiInput
              v-model="draft.adminEmail"
              type="email"
              maxlength="256"
              autocomplete="off"
              placeholder="可选"
            />
          </UiFormField>
        </template>
        <template v-else>
          <UiFormField
            label="租户管理员"
            :hint="
              tenant?.adminUserName
                ? `当前管理员：${tenant.adminUserName}`
                : '当前未指定租户管理员。可在创建后从授权设置中选择。'
            "
          >
            <UiInput
              :model-value="tenant?.adminUserName ?? '未指定'"
              disabled
            />
          </UiFormField>
          <UiFormField label="管理员邮箱">
            <UiInput
              :model-value="tenant?.adminEmail ?? '未配置'"
              disabled
            />
          </UiFormField>
        </template>

        <UiFormField
          label="备注"
          wide
        >
          <UiTextarea
            v-model="draft.remark"
            maxlength="256"
            rows="3"
            placeholder="可选，补充租户用途或运维说明"
          />
        </UiFormField>
      </div>
      <p
        v-if="validationError"
        :class="$style.error"
        role="alert"
      >
        {{ validationError }}
      </p>
      <p
        v-if="saveError"
        :class="$style.error"
        role="alert"
        aria-live="polite"
      >
        {{ saveError }}
      </p>
      <p
        v-if="mode === 'edit' && tenant?.hasDefaultConnectionString"
        :class="$style.connectionNotice"
      >
        当前已配置独立连接：{{ tenant.defaultConnectionStringDisplay }}。现有接口不支持通过空值清除此配置。
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
        @click="submit"
        >{{ mode === 'create' ? '创建租户' : '保存修改' }}</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";
.form {
  @apply space-y-3;
}
.formGrid {
  @apply grid grid-cols-2 gap-x-4 gap-y-3;
}
.error {
  @apply rounded border px-3 py-2 text-[11px];
  color: var(--ui-danger);
  border-color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.connectionNotice {
  @apply rounded border px-3 py-2 text-[11px] leading-5;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
@media (max-width: 640px) {
  .formGrid {
    @apply grid-cols-1;
  }
}
</style>
