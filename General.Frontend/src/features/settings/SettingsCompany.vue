<script setup lang="ts">
import { Check, Pencil, ShieldCheck, Upload, X } from 'lucide-vue-next';
import type { ComponentPublicInstance } from 'vue';
import { computed, nextTick, onBeforeUnmount, onMounted, reactive, ref } from 'vue';
import { createCompanyProfileApi, type CompanyProfileApi } from '@/api/core/company-profile';
import { apiClient } from '@/api/request';
import { UiButton, UiFormField, UiInput, UiTextarea } from '@/components/ui';
import { canAccess } from '@/directives/permission';
import SettingsPanel from './SettingsPanel.vue';

type CompanyInfo = CompanyProfileApi.SaveInput;

type NotificationTone = 'success' | 'warning' | 'error' | 'info';

const DEFAULT_LOGO_URL = '/logo.png';
const MAX_LOGO_SIZE = 2 * 1024 * 1024;
const SUPPORTED_LOGO_TYPES = new Set(['image/png', 'image/jpeg']);

const emit = defineEmits<{ notify: [message: string, tone?: NotificationTone] }>();
const api = createCompanyProfileApi(apiClient);
const canManage = computed(() => canAccess('Platform.Config.Manage'));
const company = reactive<CompanyInfo>({
  fullName: '',
  shortName: '',
  platformName: '',
  slogan: '',
  legalPerson: '',
  creditCode: '',
  industry: '',
  scale: '',
  province: '',
  city: '',
  address: '',
  phone: '',
  email: '',
  website: '',
  introduction: '',
});
const draft = reactive<CompanyInfo>({ ...company });
const editing = ref(false);
const loading = ref(false);
const saving = ref(false);
const loadError = ref('');
const pendingLogoFile = ref<File | null>(null);
const editButton = ref<ComponentPublicInstance | null>(null);
const logoUrl = ref(DEFAULT_LOGO_URL);
const logoInput = ref<HTMLInputElement | null>(null);
let loadRequest: AbortController | undefined;
const fields: { key: keyof CompanyInfo; label: string; full?: boolean }[] = [
  { key: 'fullName', label: '企业全称' },
  { key: 'shortName', label: '企业简称' },
  { key: 'platformName', label: '平台名称' },
  { key: 'slogan', label: '品牌口号' },
  { key: 'legalPerson', label: '法定代表人' },
  { key: 'creditCode', label: '统一社会信用代码' },
  { key: 'industry', label: '所属行业' },
  { key: 'scale', label: '企业规模' },
  { key: 'province', label: '所在省份' },
  { key: 'city', label: '所在城市' },
  { key: 'address', label: '详细地址', full: true },
  { key: 'phone', label: '联系电话' },
  { key: 'email', label: '企业邮箱' },
  { key: 'website', label: '企业网站' },
  { key: 'introduction', label: '企业简介', full: true },
];

function startEdit() {
  Object.assign(draft, company);
  pendingLogoFile.value = null;
  editing.value = true;
}

function finishEdit() {
  editing.value = false;
  void nextTick(() => {
    const element = editButton.value?.$el;
    if (element instanceof HTMLElement) element.focus();
  });
}

function cancelEdit() {
  Object.assign(draft, company);
  pendingLogoFile.value = null;
  if (logoInput.value) logoInput.value.value = '';
  void loadLogo();
  finishEdit();
}

async function save() {
  if (!canManage.value || saving.value) return;
  saving.value = true;
  try {
    await api.update({ ...draft });
    Object.assign(company, draft);

    const logoFile = pendingLogoFile.value;
    if (logoFile) {
      try {
        await api.uploadLogo(logoFile);
        pendingLogoFile.value = null;
        await loadLogo();
      } catch (error) {
        emit(
          'notify',
          `企业资料已保存，但 Logo 上传失败：${error instanceof Error ? error.message : '请重试。'}`,
          'error',
        );
        return;
      }
    }

    finishEdit();
    emit('notify', '企业资料已保存', 'success');
  } catch (error) {
    emit('notify', error instanceof Error ? error.message : '企业资料保存失败，请重试。', 'error');
  } finally {
    saving.value = false;
  }
}

function changeLogo(event: Event) {
  const input = event.currentTarget as HTMLInputElement;
  const file = input.files?.[0];
  if (!file) return;

  if (!editing.value || !canManage.value) {
    input.value = '';
    return;
  }

  if (!SUPPORTED_LOGO_TYPES.has(file.type)) {
    input.value = '';
    emit('notify', '仅支持 PNG 或 JPG 格式的企业 Logo', 'warning');
    return;
  }

  if (file.size > MAX_LOGO_SIZE) {
    input.value = '';
    emit('notify', '企业 Logo 不能超过 2MB', 'warning');
    return;
  }

  const reader = new FileReader();
  reader.addEventListener('load', () => {
    if (typeof reader.result !== 'string') return;

    try {
      releaseLogoObjectUrl();
      logoUrl.value = reader.result;
      pendingLogoFile.value = file;
      emit('notify', 'Logo 预览已更新，保存信息后上传', 'info');
    } catch {
      emit('notify', '企业 Logo 保存失败，请检查浏览器存储空间', 'error');
    }
  });
  reader.addEventListener('error', () => emit('notify', '企业 Logo 读取失败', 'error'));
  reader.readAsDataURL(file);
  input.value = '';
}

function toCompanyInfo(profile: CompanyProfileApi.Item): CompanyInfo {
  return {
    address: profile.address,
    city: profile.city,
    creditCode: profile.creditCode,
    email: profile.email,
    fullName: profile.fullName,
    industry: profile.industry,
    introduction: profile.introduction,
    legalPerson: profile.legalPerson,
    phone: profile.phone,
    platformName: profile.platformName,
    province: profile.province,
    scale: profile.scale,
    shortName: profile.shortName,
    slogan: profile.slogan,
    website: profile.website,
  };
}

function releaseLogoObjectUrl() {
  if (logoUrl.value.startsWith('blob:')) URL.revokeObjectURL(logoUrl.value);
}

async function loadLogo(signal?: AbortSignal) {
  try {
    const logo = await api.getLogo(signal);
    releaseLogoObjectUrl();
    logoUrl.value = logo ? URL.createObjectURL(logo) : DEFAULT_LOGO_URL;
  } catch (error) {
    if (!(error instanceof Error && error.name === 'AbortError')) {
      emit('notify', error instanceof Error ? error.message : '企业 Logo 读取失败，请重试。', 'error');
    }
  }
}

async function loadCompanyProfile() {
  if (!canManage.value) return;
  loadRequest?.abort();
  const controller = new AbortController();
  loadRequest = controller;
  loading.value = true;
  loadError.value = '';
  try {
    const profile = await api.get(controller.signal);
    const nextCompany = toCompanyInfo(profile);
    Object.assign(company, nextCompany);
    Object.assign(draft, nextCompany);
    await loadLogo(controller.signal);
  } catch (error) {
    if (error instanceof Error && error.name === 'AbortError') return;
    loadError.value = error instanceof Error ? error.message : '企业资料读取失败，请刷新重试。';
    emit('notify', loadError.value, 'error');
  } finally {
    if (loadRequest === controller) loading.value = false;
  }
}

onMounted(() => void loadCompanyProfile());
onBeforeUnmount(() => {
  loadRequest?.abort();
  releaseLogoObjectUrl();
});
</script>

<template>
  <SettingsPanel
    title="企业基本信息"
    :description="
      canManage ? '企业资料按当前租户保存，平台名称使用系统名称配置' : '需要平台配置管理权限才能查看或维护企业资料'
    "
    min-height="tall"
  >
    <template #actions>
      <template v-if="editing && canManage">
        <UiButton
          size="sm"
          @click="cancelEdit"
        >
          <template #leading
            ><X
              :size="14"
              :stroke-width="1.8"
          /></template>
          取消
        </UiButton>
        <UiButton
          variant="primary"
          size="sm"
          :disabled="saving"
          @click="save"
        >
          <template #leading
            ><Check
              :size="14"
              :stroke-width="1.8"
          /></template>
          保存信息
        </UiButton>
      </template>
      <UiButton
        v-else-if="canManage"
        ref="editButton"
        variant="primary"
        size="sm"
        :disabled="loading || saving"
        @click="startEdit"
      >
        <template #leading
          ><Pencil
            :size="14"
            :stroke-width="1.8"
        /></template>
        编辑信息
      </UiButton>
    </template>

    <div
      v-if="!canManage"
      role="status"
    >
      当前账号没有查看和维护企业资料的权限。
    </div>
    <div
      v-else-if="loading"
      role="status"
    >
      正在读取企业资料…
    </div>
    <div
      v-else-if="loadError"
      role="alert"
    >
      {{ loadError }}
    </div>
    <template v-else>
      <section :class="$style.verificationCard">
        <span :class="$style.verificationIcon"
          ><ShieldCheck
            :size="20"
            :stroke-width="1.8"
        /></span>
        <div :class="$style.verificationContent">
          <strong :class="$style.verificationTitle">
            企业未认证
            <span :class="$style.verificationBadge">ⓘ 未认证</span>
          </strong>
          <p :class="$style.verificationDescription">
            完成企业实名认证可获得「金 V」标识，享受发票、合同、对公转账快速通道
          </p>
        </div>
        <UiButton
          variant="warning"
          size="sm"
          @click="emit('notify', '企业认证服务尚未接入', 'warning')"
        >
          <template #leading
            ><ShieldCheck
              :size="13"
              :stroke-width="1.8"
          /></template>
          开始认证
        </UiButton>
      </section>

      <section :class="$style.logoCard">
        <strong :class="$style.sectionLabel">企业 Logo</strong>
        <div :class="$style.logoContent">
          <button
            :class="$style.logoButton"
            :disabled="!editing"
            @click="logoInput?.click()"
          >
            <img
              :src="logoUrl"
              alt="企业 Logo"
              :class="$style.logoImage"
            />
            <span
              v-if="editing"
              :class="$style.logoOverlay"
              ><Upload
                :size="18"
                :stroke-width="1.8"
            /></span>
          </button>
          <input
            ref="logoInput"
            type="file"
            accept="image/png,image/jpeg"
            :class="$style.fileInput"
            @change="changeLogo"
          />
          <div :class="$style.brandDetails">
            <strong :class="$style.brandName">{{ editing ? draft.shortName : company.shortName }}</strong>
            <p :class="$style.platformName">{{ editing ? draft.platformName : company.platformName }}</p>
            <p :class="$style.slogan">{{ editing ? draft.slogan : company.slogan }}</p>
            <small :class="$style.uploadHint">支持 PNG / JPG，建议尺寸 200×200px，不超过 2MB</small>
          </div>
        </div>
      </section>

      <section :class="$style.formGrid">
        <UiFormField
          v-for="field in fields"
          :key="field.key"
          :label="field.label"
          :wide="field.full"
        >
          <UiTextarea
            v-if="field.key === 'introduction' && editing"
            v-model="draft[field.key]"
            rows="3"
          />
          <UiInput
            v-else-if="editing"
            v-model="draft[field.key]"
          />
          <span
            v-else
            :class="$style.readonlyValue"
            >{{ company[field.key] }}</span
          >
        </UiFormField>
      </section>
    </template>
  </SettingsPanel>
</template>

<style module>
@reference "../../styles/index.css";
.verificationCard {
  @apply mt-5 flex items-center rounded-lg border p-4;
  border-color: #ffe380;
  background: var(--ui-warning-soft);
}
.verificationIcon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border bg-white;
  color: var(--ui-warning);
  border-color: #ffd580;
}
.verificationContent {
  @apply ml-3 min-w-0 flex-1;
}
.verificationTitle {
  @apply text-[14px];
}
.verificationBadge {
  @apply ml-2 rounded border px-2 py-0.5 text-[10px] font-normal;
  color: var(--ui-warning);
  border-color: #ffab00;
}
.verificationDescription {
  @apply mt-1 text-[12px];
  color: var(--ui-text-subtle);
}
.logoCard {
  @apply mt-4 rounded border p-4;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.sectionLabel {
  @apply text-[12px];
  color: #5e6c84;
}
.logoContent {
  @apply mt-3 flex items-center;
}
.logoButton {
  @apply relative flex h-[84px] w-[84px] items-center justify-center overflow-hidden rounded-lg border-2 border-dashed bg-white disabled:cursor-default;
  border-color: var(--ui-border);
}
.logoImage {
  @apply w-[70px];
}
.logoOverlay {
  @apply absolute inset-0 hidden items-center justify-center text-white;
  background: color-mix(in srgb, var(--app-primary-dark) 60%, transparent);
}
.logoButton:hover .logoOverlay {
  @apply flex;
}
.fileInput {
  @apply hidden;
}
.brandDetails {
  @apply ml-5;
}
.brandName {
  @apply text-[13px];
}
.platformName {
  @apply mt-1 text-[12px];
  color: var(--ui-text-subtle);
}
.slogan {
  @apply mt-1 text-[11px] italic;
  color: #97a0af;
}
.uploadHint {
  @apply mt-2 block text-[10px];
  color: #97a0af;
}
.formGrid {
  @apply mt-5 grid grid-cols-2 gap-x-6 gap-y-4;
}
.readonlyValue {
  @apply block min-h-[34px] rounded px-3 py-2 text-[13px];
  background: var(--ui-surface-subtle);
}
</style>
