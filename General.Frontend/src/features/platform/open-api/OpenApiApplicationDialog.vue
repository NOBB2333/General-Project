<script setup lang="ts">
import { computed } from 'vue';
import type { OpenApiApplicationDraft } from './open-api.model';
import { openApiAllScope, openApiScopeOptions, toggleOpenApiScope } from './open-api.model';
import { UiButton, UiCheckbox, UiDialog, UiFormField, UiInput, UiSwitch } from '@/components/ui';

const props = defineProps<{
  draft: OpenApiApplicationDraft;
  error: string;
  mode: 'create' | 'edit';
  open: boolean;
  saving: boolean;
}>();

const emit = defineEmits<{
  'update:open': [open: boolean];
  submit: [];
}>();

const legacyScopes = computed(() =>
  props.draft.scopes.filter(
    (scope) => scope !== openApiAllScope && !openApiScopeOptions.some((option) => option.code === scope),
  ),
);

function updateScope(scope: string, checked: boolean | 'indeterminate') {
  props.draft.scopes = toggleOpenApiScope(props.draft.scopes, scope, checked === true);
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="mode === 'create' ? '新建开放接口应用' : '编辑开放接口应用'"
    description="仅配置当前后端已登记的开放接口授权范围。"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      id="platform-open-api-application-form"
      :class="$style.form"
      @submit.prevent="emit('submit')"
    >
      <UiFormField
        label="应用名称"
        required
        hint="最多128个字符。"
      >
        <UiInput
          v-model="draft.name"
          maxlength="128"
          autocomplete="off"
          placeholder="例如：商城订单同步"
          required
        />
      </UiFormField>
      <UiFormField label="应用状态">
        <UiSwitch
          v-model="draft.isEnabled"
          label="启用开放接口应用"
        />
      </UiFormField>
      <UiFormField
        label="授权范围"
        required
        hint="默认只开放字典读取；全部开放接口会授权当前和后续登记的所有scope。"
        wide
      >
        <div :class="$style.scopeOptions">
          <div :class="$style.scopeOption">
            <UiCheckbox
              :model-value="draft.scopes.includes('dict:read')"
              label="授权平台字典读取"
              @update:model-value="updateScope('dict:read', $event)"
            />
            <div :class="$style.scopeText">
              <span>平台字典读取（dict:read）</span>
              <span :class="$style.scopeDescription">{{ openApiScopeOptions[0]?.description }}</span>
            </div>
          </div>
          <div :class="$style.scopeOption">
            <UiCheckbox
              :model-value="draft.scopes.includes(openApiAllScope)"
              label="授权全部开放接口"
              @update:model-value="updateScope(openApiAllScope, $event)"
            />
            <div :class="$style.scopeText">
              <span>全部开放接口（*）</span>
              <span :class="$style.scopeDescription">适用于确需访问所有公开scope的应用。</span>
            </div>
          </div>
          <div
            v-for="scope in legacyScopes"
            :key="scope"
            :class="$style.scopeOption"
          >
            <UiCheckbox
              :model-value="true"
              :label="`保留已有未识别授权范围 ${scope}`"
              @update:model-value="updateScope(scope, $event)"
            />
            <div :class="$style.scopeText">
              <span>已有未识别范围：{{ scope }}</span>
              <span :class="$style.scopeDescription">本版本没有对应公开端点；取消选择可收回。</span>
            </div>
          </div>
        </div>
      </UiFormField>
      <UiFormField
        label="备注"
        hint="最多256个字符。"
        wide
      >
        <UiInput
          v-model="draft.remark"
          maxlength="256"
          autocomplete="off"
          placeholder="说明用途、调用系统或负责人"
        />
      </UiFormField>
      <p
        v-if="error"
        :class="$style.formError"
        role="alert"
      >
        {{ error }}
      </p>
    </form>
    <template #footer>
      <UiButton
        :disabled="saving"
        @click="emit('update:open', false)"
        >取消</UiButton
      >
      <UiButton
        type="submit"
        form="platform-open-api-application-form"
        variant="primary"
        :loading="saving"
        :disabled="saving"
        >{{ saving ? '正在保存…' : mode === 'create' ? '创建应用' : '保存修改' }}</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.form {
  @apply grid grid-cols-2 gap-4 p-5;
}

.scopeOptions {
  @apply space-y-3;
}

.scopeOption {
  @apply flex items-start gap-2;
}

.scopeText {
  @apply min-w-0 text-[12px];
  color: var(--ui-text);
}

.scopeDescription {
  @apply mt-1 block text-[10px] leading-4;
  color: var(--ui-text-subtle);
}

.formError {
  @apply col-span-2 text-[12px];
  color: var(--ui-danger);
}
</style>
