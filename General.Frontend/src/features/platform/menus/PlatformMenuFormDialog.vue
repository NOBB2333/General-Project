<script setup lang="ts">
import { computed } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiSwitch } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { MenuCompatibility, MenuDraft } from './menu.model';

const open = defineModel<boolean>('open', { required: true });
const draft = defineModel<MenuDraft>('draft', { required: true });

const props = defineProps<{
  appCodeOptions: readonly UiSelectOption[];
  componentCompatibility: MenuCompatibility;
  error: string;
  mode: 'create' | 'edit';
  parentOptions: readonly UiSelectOption[];
  saving: boolean;
  typeOptions: readonly UiSelectOption[];
}>();

const emit = defineEmits<{
  submit: [];
  'type-change': [value: string];
}>();

const typeValue = computed(() => String(draft.value.type));
</script>

<template>
  <UiDialog
    v-model:open="open"
    :title="mode === 'create' ? '新建菜单节点' : '编辑菜单节点'"
    :description="
      mode === 'create'
        ? '目录、菜单和按钮使用后端真实类型；组件只匹配本地白名单。'
        : '保存会保留完整路由元数据；未知组件不会被前端动态加载。'
    "
    size="lg"
  >
    <form
      :class="$style.createForm"
      @submit.prevent="emit('submit')"
    >
      <UiFormField
        label="节点类型"
        required
      >
        <UiSelect
          :model-value="typeValue"
          label="节点类型"
          :options="typeOptions"
          @update:model-value="emit('type-change', $event)"
        />
      </UiFormField>
      <UiFormField
        label="AppCode"
        required
      >
        <UiSelect
          v-model="draft.appCode"
          label="AppCode"
          :options="appCodeOptions"
        />
      </UiFormField>
      <UiFormField
        label="菜单名称（Name）"
        required
      >
        <UiInput
          v-model="draft.name"
          autofocus
          maxlength="64"
          placeholder="例如 PlatformMenus"
        />
      </UiFormField>
      <UiFormField
        label="显示标题"
        required
      >
        <UiInput
          v-model="draft.title"
          maxlength="128"
          placeholder="请输入中文菜单标题"
        />
      </UiFormField>
      <UiFormField
        :label="draft.type === 3 ? '权限动作路径' : '路由路径'"
        required
      >
        <UiInput
          v-model="draft.path"
          maxlength="256"
          :placeholder="draft.type === 3 ? '例如 Platform.Menu.Manage' : '/platform/menus'"
        />
      </UiFormField>
      <UiFormField label="父级菜单">
        <UiSelect
          v-model="draft.parentId"
          label="父级菜单"
          :options="parentOptions"
          placeholder="选择同一 AppCode 的目录或菜单"
        />
      </UiFormField>
      <UiFormField
        v-if="draft.type !== 3"
        label="组件标识"
      >
        <UiInput
          v-model="draft.component"
          :disabled="draft.type === 1"
          maxlength="256"
          :placeholder="draft.type === 1 ? '目录固定使用 BasicLayout' : '/platform/users/index'"
        />
      </UiFormField>
      <UiFormField label="权限码">
        <UiInput
          v-model="draft.permissionCode"
          maxlength="128"
          :placeholder="draft.type === 3 ? '留空时使用权限动作路径' : '可留空'"
        />
      </UiFormField>
      <UiFormField label="排序值">
        <UiInput
          v-model="draft.order"
          type="number"
          step="1"
          placeholder="10"
        />
      </UiFormField>
      <UiFormField label="图标标识">
        <UiInput
          v-model="draft.icon"
          maxlength="128"
          placeholder="例如 lucide:waypoints"
        />
      </UiFormField>
      <UiFormField
        v-if="draft.type !== 3"
        label="站内重定向"
      >
        <UiInput
          v-model="draft.redirect"
          maxlength="256"
          placeholder="可留空；只接受站内绝对路径"
        />
      </UiFormField>
      <UiFormField
        v-if="draft.type !== 3"
        label="外部链接"
      >
        <UiInput
          v-model="draft.link"
          maxlength="512"
          placeholder="可留空；仅允许 HTTP 或 HTTPS"
        />
      </UiFormField>
      <p
        v-if="draft.type !== 3"
        role="status"
      >
        {{ componentCompatibility.label }}：{{ componentCompatibility.detail }}
      </p>
      <UiFormField label="启用节点">
        <UiSwitch
          v-model="draft.isEnabled"
          label="启用菜单节点"
        />
      </UiFormField>
      <template v-if="draft.type !== 3">
        <UiFormField label="固定页签">
          <UiSwitch
            v-model="draft.affixTab"
            label="固定页签"
          />
        </UiFormField>
        <UiFormField label="缓存页面">
          <UiSwitch
            v-model="draft.keepAlive"
            label="缓存页面"
          />
        </UiFormField>
        <UiFormField label="隐藏面包屑">
          <UiSwitch
            v-model="draft.hideInBreadcrumb"
            label="隐藏面包屑"
          />
        </UiFormField>
        <UiFormField label="隐藏导航项">
          <UiSwitch
            v-model="draft.hideInMenu"
            label="隐藏导航项"
          />
        </UiFormField>
        <UiFormField label="隐藏页签">
          <UiSwitch
            v-model="draft.hideInTab"
            label="隐藏页签"
          />
        </UiFormField>
        <UiFormField label="无权时仍显示">
          <UiSwitch
            v-model="draft.menuVisibleWithForbidden"
            label="无权时仍显示菜单"
          />
        </UiFormField>
      </template>
      <p
        v-if="error"
        role="alert"
      >
        {{ error }}
      </p>
      <div :class="$style.dialogActions">
        <UiButton
          :disabled="saving"
          @click="open = false"
          >取消</UiButton
        >
        <UiButton
          type="submit"
          variant="primary"
          :loading="saving"
          >{{ saving ? '正在保存…' : mode === 'create' ? '创建菜单' : '保存修改' }}</UiButton
        >
      </div>
    </form>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.createForm {
  @apply grid grid-cols-2 gap-4 p-5;
}
.dialogActions {
  @apply col-span-2 mt-2 flex justify-end gap-2 border-t pt-4;
  border-color: var(--ui-border-subtle);
}
</style>
