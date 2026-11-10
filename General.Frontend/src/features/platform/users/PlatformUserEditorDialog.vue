<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import { AlertTriangle, RefreshCw } from 'lucide-vue-next';
import type { OrganizationApi } from '@/api/core/organization';
import { getOrganizationUnitTreeApi } from '@/api/core/organization';
import type { RoleApi } from '@/api/core/role';
import { getRoleOptionsApi } from '@/api/core/role';
import type { UserApi } from '@/api/core/user';
import { getUserListApi } from '@/api/core/user';
import { UiButton, UiCheckbox, UiDialog, UiInput, UiSelect, UiSwitch } from '@/components/ui';

const props = defineProps<{
  activeOverride: boolean | null;
  hostScope: boolean;
  open: boolean;
  saveError: string;
  saving: boolean;
  user: UserApi.ListItem | null;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  submit: [input: UserApi.SaveInput];
}>();

interface UserEditorForm {
  displayName: string;
  email: string;
  employeeNo: string;
  isActive: boolean;
  organizationUnitId: string;
  password: string;
  phoneNumber: string;
  roleNames: string[];
  username: string;
}

const form = reactive<UserEditorForm>({
  displayName: '',
  email: '',
  employeeNo: '',
  isActive: true,
  organizationUnitId: '',
  password: '',
  phoneNumber: '',
  roleNames: [],
  username: '',
});

const roleOptions = ref<RoleApi.Option[]>([]);
const organizationOptions = ref<OrganizationApi.TreeItem[]>([]);
const optionsLoading = ref(false);
const optionsError = ref('');
const optionsLoaded = ref(false);
const validationError = ref('');
const confirmOrganizationChange = ref(false);
const checkingUsername = ref(false);
let optionsRequestId = 0;

const isCreate = computed(() => props.user === null);
const title = computed(() => (isCreate.value ? '新增用户' : '编辑用户'));
const flatOrganizations = computed(() => flattenOrganizations(organizationOptions.value));
const organizationSelectOptions = computed(() => [
  { label: '无所属组织', value: '' },
  ...flatOrganizations.value.map((item) => ({
    disabled: item.disabled,
    label: item.label,
    value: item.id,
  })),
]);
const unavailableRoles = computed(() => {
  if (!props.user || !optionsLoaded.value) return [];
  const availableNames = new Set(roleOptions.value.map((option) => option.name.toLocaleLowerCase()));
  return props.user.roles.filter((role) => !availableNames.has(role.toLocaleLowerCase()));
});
const existingOrganizationNames = computed(() => props.user?.organizationUnitNames ?? []);
const organizationChangeNeedsConfirmation = computed(() => {
  return Boolean(props.user && existingOrganizationNames.value.length > 0 && optionsLoaded.value);
});
const canSubmit = computed(
  () =>
    !props.saving &&
    !checkingUsername.value &&
    !optionsLoading.value &&
    optionsLoaded.value &&
    !optionsError.value &&
    unavailableRoles.value.length === 0 &&
    (!organizationChangeNeedsConfirmation.value || confirmOrganizationChange.value),
);

function flattenOrganizations(
  items: OrganizationApi.TreeItem[],
  parent = '',
): Array<{ disabled: boolean; id: string; label: string }> {
  return items.flatMap((item) => {
    const label = parent ? `${parent} / ${item.displayName}` : item.displayName;
    return [{ disabled: item.disabled, id: item.id, label }, ...flattenOrganizations(item.children ?? [], label)];
  });
}

function setSelectedRole(name: string, checked: boolean | 'indeterminate') {
  const normalizedName = name.toLocaleLowerCase();
  if (checked === true && !form.roleNames.some((role) => role.toLocaleLowerCase() === normalizedName)) {
    form.roleNames = [...form.roleNames, name];
  }
  if (checked !== true) form.roleNames = form.roleNames.filter((role) => role.toLocaleLowerCase() !== normalizedName);
}

function initializeForm() {
  const user = props.user;
  form.username = user?.username ?? '';
  form.displayName = user?.displayName ?? '';
  form.email = user?.email ?? '';
  form.employeeNo = user?.employeeNo ?? '';
  form.isActive = props.activeOverride ?? user?.isActive ?? true;
  form.organizationUnitId = '';
  form.password = '';
  form.phoneNumber = user?.phoneNumber ?? '';
  form.roleNames = [...(user?.roles ?? [])];
  validationError.value = '';
  confirmOrganizationChange.value = false;
}

async function loadOptions() {
  const requestId = ++optionsRequestId;
  optionsLoading.value = true;
  optionsError.value = '';
  optionsLoaded.value = false;
  try {
    const [roles, organizations] = await Promise.all([getRoleOptionsApi(), getOrganizationUnitTreeApi()]);
    if (requestId !== optionsRequestId) return;
    roleOptions.value = roles;
    organizationOptions.value = organizations;
    optionsLoaded.value = true;

    const organizationNames = props.user?.organizationUnitNames ?? [];
    if (organizationNames.length === 1) {
      const matches = flatOrganizations.value.filter(
        (item) => item.label === organizationNames[0] || item.label.endsWith(` / ${organizationNames[0]}`),
      );
      if (matches.length === 1 && !matches[0].disabled) {
        form.organizationUnitId = matches[0].id;
      }
    }
  } catch (error) {
    if (requestId !== optionsRequestId) return;
    optionsError.value = error instanceof Error ? error.message : '角色或组织选项加载失败，请重试。';
  } finally {
    if (requestId === optionsRequestId) optionsLoading.value = false;
  }
}

function validateForm(): boolean {
  validationError.value = '';
  const username = form.username.trim();
  const displayName = form.displayName.trim();
  const email = form.email.trim();
  const phoneNumber = String(form.phoneNumber ?? '').trim();
  const password = form.password ?? '';

  if (!/^[A-Za-z][A-Za-z0-9._-]{2,63}$/.test(username)) {
    validationError.value = '账号须以英文字母开头，只能包含字母、数字、点、下划线或中划线，长度为 3–64 位。';
    return false;
  }
  if (!displayName || displayName.length > 64) {
    validationError.value = '姓名不能为空，且不能超过 64 个字符。';
    return false;
  }
  if (email.length > 256 || (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email))) {
    validationError.value = '邮箱格式无效或超过 256 个字符。';
    return false;
  }
  if (phoneNumber && !/^1[3-9]\d{9}$/.test(phoneNumber)) {
    validationError.value = '手机号须为 11 位中国大陆手机号。';
    return false;
  }
  if (String(form.employeeNo ?? '').length > 64) {
    validationError.value = '员工编号不能超过 64 个字符。';
    return false;
  }
  if (isCreate.value && (!password.trim() || password.length > 128)) {
    validationError.value = '新用户初始密码必填，且不能超过 128 个字符。';
    return false;
  }
  if (unavailableRoles.value.length > 0) {
    validationError.value = `当前选项中找不到用户已有角色：${unavailableRoles.value.join('、')}。为避免意外移除，请先切换到对应租户或修复角色选项。`;
    return false;
  }
  if (organizationChangeNeedsConfirmation.value && !confirmOrganizationChange.value) {
    validationError.value = '请先确认组织关系变更后再保存。';
    return false;
  }
  return true;
}

async function submitForm() {
  if (!validateForm() || !canSubmit.value) return;
  if (isCreate.value) {
    checkingUsername.value = true;
    try {
      const existingUsers = await getUserListApi({
        maxResultCount: 1,
        skipCount: 0,
        userName: form.username.trim(),
      });
      if (
        existingUsers.items.some(
          (item) => item.username.toLocaleLowerCase() === form.username.trim().toLocaleLowerCase(),
        )
      ) {
        validationError.value = '该登录账号已存在，请更换账号后重试。';
        return;
      }
    } catch {
      // When data scope hides the record, the write API remains the final authority.
    } finally {
      checkingUsername.value = false;
    }
  }
  const input: UserApi.SaveInput = {
    displayName: form.displayName.trim(),
    email: form.email.trim(),
    employeeNo: String(form.employeeNo ?? '').trim() || null,
    externalSource: props.user?.externalSource ?? null,
    externalUserId: props.user?.externalUserId ?? null,
    isActive: form.isActive,
    organizationUnitId: form.organizationUnitId || null,
    phoneNumber: String(form.phoneNumber ?? '').trim() || null,
    roleNames: [...form.roleNames],
    username: form.username.trim(),
  };
  if (isCreate.value) input.password = form.password ?? '';
  emit('submit', input);
}

function updateOpen(value: boolean) {
  if (!value && props.saving) return;
  emit('update:open', value);
}

watch(
  () => [props.open, props.user?.id, props.activeOverride] as const,
  ([isOpen]) => {
    if (!isOpen) {
      optionsRequestId += 1;
      return;
    }
    initializeForm();
    void loadOptions();
  },
  { immediate: true },
);

watch(
  () => form.organizationUnitId,
  () => {
    confirmOrganizationChange.value = false;
    validationError.value = '';
  },
);
</script>

<template>
  <UiDialog
    :open="open"
    :title="title"
    :description="
      isCreate ? '创建可登录的后台用户，并配置当前身份域可用的角色与组织。' : '修改用户资料、角色、组织关系和账号状态。'
    "
    size="xl"
    @update:open="updateOpen"
  >
    <form
      id="platform-user-editor-form"
      :class="$style.form"
      @submit.prevent="submitForm"
    >
      <p
        v-if="hostScope && isCreate"
        :class="$style.scopeNotice"
        role="status"
      >
        <AlertTriangle :size="16" /> 当前在宿主身份域，新用户会创建在宿主身份域。
      </p>

      <div
        v-if="optionsLoading"
        :class="$style.loadingState"
        role="status"
      >
        正在读取角色和组织选项…
      </div>
      <div
        v-else-if="optionsError"
        :class="$style.errorState"
        role="alert"
      >
        <span>{{ optionsError }}</span>
        <UiButton
          size="sm"
          @click="loadOptions"
        >
          <template #leading><RefreshCw :size="14" /></template>重试
        </UiButton>
      </div>

      <template v-else>
        <div :class="$style.fieldGrid">
          <label :class="$style.field">
            <span>登录账号 <i>*</i></span>
            <UiInput
              v-model="form.username"
              :disabled="!isCreate || saving"
              autocomplete="username"
              maxlength="64"
              aria-label="登录账号"
            />
          </label>
          <label :class="$style.field">
            <span>姓名 <i>*</i></span>
            <UiInput
              v-model="form.displayName"
              :disabled="saving"
              maxlength="64"
              aria-label="姓名"
            />
          </label>
          <label :class="$style.field">
            <span>邮箱</span>
            <UiInput
              v-model="form.email"
              :disabled="saving"
              type="email"
              maxlength="256"
              aria-label="邮箱"
            />
          </label>
          <label :class="$style.field">
            <span>手机号</span>
            <UiInput
              v-model="form.phoneNumber"
              :disabled="saving"
              type="tel"
              maxlength="32"
              aria-label="手机号"
            />
          </label>
          <label :class="$style.field">
            <span>员工编号</span>
            <UiInput
              v-model="form.employeeNo"
              :disabled="saving"
              maxlength="64"
              aria-label="员工编号"
              placeholder="留空由服务端生成"
            />
          </label>
          <label
            v-if="isCreate"
            :class="$style.field"
          >
            <span>初始密码 <i>*</i></span>
            <UiInput
              v-model="form.password"
              :disabled="saving"
              type="password"
              maxlength="128"
              autocomplete="new-password"
              aria-label="初始密码"
            />
          </label>
          <div :class="$style.field">
            <span>账号状态</span>
            <div :class="$style.switchField">
              <UiSwitch
                v-model="form.isActive"
                :disabled="saving"
                label="启用用户账号"
                tone="success"
              />
              <span>{{ form.isActive ? '启用' : '停用' }}</span>
            </div>
          </div>
          <label :class="$style.field">
            <span>所属组织</span>
            <UiSelect
              v-model="form.organizationUnitId"
              label="所属组织"
              :disabled="saving || optionsLoading"
              :options="organizationSelectOptions"
              size="md"
            />
          </label>
        </div>

        <section
          :class="$style.roleSection"
          aria-labelledby="platform-user-role-heading"
        >
          <div :class="$style.sectionHeading">
            <div>
              <h2 id="platform-user-role-heading">角色</h2>
              <p>保存时会按当前选择同步用户角色。</p>
            </div>
            <span>{{ form.roleNames.length }} 项已选</span>
          </div>
          <div
            v-if="roleOptions.length"
            :class="$style.roleGrid"
          >
            <label
              v-for="role in roleOptions"
              :key="role.id"
              :class="$style.roleOption"
            >
              <UiCheckbox
                :model-value="form.roleNames.some((name) => name.toLocaleLowerCase() === role.name.toLocaleLowerCase())"
                :disabled="saving"
                :label="`选择角色${role.name}`"
                @update:model-value="setSelectedRole(role.name, $event)"
              />
              <span
                ><strong>{{ role.name }}</strong
                ><small>{{ role.description || '未提供角色说明' }}</small></span
              >
            </label>
          </div>
          <p
            v-else
            :class="$style.emptyRoles"
          >
            当前身份域没有可分配角色。
          </p>
          <p
            v-if="unavailableRoles.length"
            :class="$style.inlineWarning"
            role="alert"
          >
            当前角色列表无法提供：{{ unavailableRoles.join('、') }}。为避免保存时丢失角色，本次编辑已锁定。
          </p>
        </section>

        <div
          v-if="organizationChangeNeedsConfirmation"
          :class="$style.organizationWarning"
          role="group"
          aria-label="组织关系变更确认"
        >
          <div>
            <strong>组织关系将被替换</strong>
            <p>
              当前列表返回 {{ existingOrganizationNames.length }} 个组织名称（{{
                existingOrganizationNames.join('、')
              }}）；数据范围可能隐藏其他组织。保存接口只接收一个组织，服务端会先移除全部原关联，再应用上方选择。
            </p>
          </div>
          <label :class="$style.confirmLabel">
            <UiCheckbox
              :model-value="confirmOrganizationChange"
              :disabled="saving"
              label="确认替换现有组织关系"
              @update:model-value="confirmOrganizationChange = $event === true"
            />
            <span>我确认保存会移除全部现有组织关系，并按所选的单个组织（或无所属组织）重新设置。</span>
          </label>
        </div>
      </template>

      <p
        v-if="validationError"
        :class="$style.errorState"
        role="alert"
      >
        {{ validationError }}
      </p>
      <p
        v-if="saveError"
        :class="$style.errorState"
        role="alert"
      >
        {{ saveError }}
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
        form="platform-user-editor-form"
        variant="primary"
        :disabled="!canSubmit"
        :loading="saving || checkingUsername"
      >
        {{ isCreate ? '创建用户' : '保存修改' }}
      </UiButton>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.form {
  @apply space-y-5 p-5;
}
.fieldGrid {
  @apply grid grid-cols-2 gap-4;
}
.field {
  @apply flex min-w-0 flex-col gap-2 text-[12px] font-medium;
  color: var(--ui-text-muted);
}
.field i {
  color: var(--ui-danger);
  font-style: normal;
}
.switchField {
  @apply flex h-9 items-center gap-2;
  color: var(--ui-text);
}
.roleSection {
  @apply border-t pt-4;
  border-color: var(--ui-border-subtle);
}
.sectionHeading {
  @apply mb-3 flex items-center justify-between gap-3;
}
.sectionHeading h2 {
  @apply m-0 text-[13px] font-semibold;
  color: var(--ui-text);
}
.sectionHeading p,
.sectionHeading > span {
  @apply m-0 text-[11px];
  color: var(--ui-text-subtle);
}
.roleGrid {
  @apply grid grid-cols-2 gap-2;
}
.roleOption {
  @apply flex min-h-14 items-center gap-3 border px-3 py-2;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}
.roleOption > span {
  @apply flex min-w-0 flex-col gap-1;
}
.roleOption strong {
  @apply text-[12px] font-semibold;
  color: var(--ui-text);
}
.roleOption small,
.emptyRoles {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.organizationWarning,
.scopeNotice {
  @apply flex flex-col gap-3 border px-3 py-3 text-[12px];
  color: #8a4b08;
  background: var(--ui-warning-soft);
  border-color: color-mix(in srgb, var(--ui-warning) 25%, transparent);
  border-radius: var(--app-control-radius);
}
.organizationWarning p {
  @apply mb-0 mt-1 leading-5;
}
.confirmLabel {
  @apply flex items-start gap-2;
}
.confirmLabel span {
  @apply leading-5;
}
.scopeNotice {
  @apply flex-row items-center;
}
.inlineWarning,
.errorState {
  @apply flex items-center gap-2 text-[12px];
  color: var(--ui-danger);
}
.errorState {
  @apply justify-between border px-3 py-2;
  background: var(--ui-danger-soft);
  border-color: color-mix(in srgb, var(--ui-danger) 25%, transparent);
  border-radius: var(--app-control-radius);
}
.loadingState {
  @apply py-8 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
