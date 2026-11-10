<script setup lang="ts">
import { Building2, ChevronRight, Info, Pencil, Plus, Trash2 } from 'lucide-vue-next';
import { computed, onMounted, reactive, ref } from 'vue';
import { UiAlertDialog, UiButton, UiDialog, UiFormField, UiInput } from '@/components/ui';
import {
  createBusinessLegalEntityApi,
  deleteBusinessLegalEntityApi,
  getBusinessLegalEntitiesApi,
  setDefaultBusinessLegalEntityApi,
  updateBusinessLegalEntityApi,
} from '@/api/core/business-legal-entities';
import type { BusinessLegalEntityApi } from '@/api/core/business-legal-entities';
import { hasAccessCode } from '@/session/access-store';
import SettingsPanel from './SettingsPanel.vue';

type CompanyHeader = BusinessLegalEntityApi.Item;

type NotificationTone = 'success' | 'warning' | 'error' | 'info';

const emit = defineEmits<{ notify: [message: string, tone?: NotificationTone] }>();
const headers = reactive<CompanyHeader[]>([]);
const canView = computed(() => hasAccessCode('Business.LegalEntity.View'));
const canManage = computed(() => canView.value && hasAccessCode('Business.LegalEntity.Manage'));
const canViewSensitiveFields = computed(() => hasAccessCode('Business.LegalEntity.Sensitive'));
const loading = ref(true);
const saving = ref(false);

const expandedId = ref<string | null>(null);
const editingId = ref<string | null>(null);
const deleteId = ref<string | null>(null);
const modalOpen = ref(false);
const deleteDialogOpen = ref(false);
const draft = reactive({ name: '', shortName: '', taxId: '', address: '', bank: '', account: '' });
const defaultHeader = computed(() => headers.find((item) => item.isDefault));
const deleteTarget = computed(() => headers.find((item) => item.id === deleteId.value));
const editingHeader = computed(() => headers.find((item) => item.id === editingId.value));
const fields: { key: keyof typeof draft; label: string; wide?: boolean; required?: boolean }[] = [
  { key: 'name', label: '公司全称', wide: true, required: true },
  { key: 'shortName', label: '公司简称' },
  { key: 'taxId', label: '统一社会信用代码', required: true },
  { key: 'address', label: '注册地址', wide: true },
  { key: 'bank', label: '开户银行' },
  { key: 'account', label: '银行账号' },
];

onMounted(() => {
  void loadHeaders();
});

async function loadHeaders(): Promise<boolean> {
  loading.value = true;
  if (!canView.value) {
    loading.value = false;
    return false;
  }

  try {
    headers.splice(0, headers.length, ...(await getBusinessLegalEntitiesApi()));
    return true;
  } catch (cause) {
    emit('notify', cause instanceof Error ? cause.message : '经营主体加载失败，请重试。', 'error');
    return false;
  } finally {
    loading.value = false;
  }
}

function openForm(item?: CompanyHeader) {
  if (!canManage.value || saving.value) return;
  editingId.value = item?.id ?? null;
  Object.assign(
    draft,
    item
      ? {
          name: item.name,
          shortName: item.shortName,
          taxId: item.taxId,
          address: item.address,
          bank: item.bankName,
          account: canViewSensitiveFields.value ? (item.bankAccount ?? '') : '',
        }
      : { name: '', shortName: '', taxId: '', address: '', bank: '', account: '' },
  );
  modalOpen.value = true;
}

async function saveHeader() {
  if (!canManage.value || saving.value) return;
  if (!draft.name.trim() || !draft.taxId.trim()) {
    emit('notify', '请填写公司名称和统一社会信用代码', 'warning');
    return;
  }

  saving.value = true;
  try {
    const input: BusinessLegalEntityApi.SaveInput = {
      address: draft.address.trim(),
      bankName: draft.bank.trim(),
      name: draft.name.trim(),
      shortName: draft.shortName.trim(),
      taxId: draft.taxId.trim(),
    };
    if (canViewSensitiveFields.value) input.bankAccount = draft.account.trim();

    const existing = editingId.value;
    if (existing) await updateBusinessLegalEntityApi(existing, input);
    else await createBusinessLegalEntityApi(input);

    modalOpen.value = false;
    const loaded = await loadHeaders();
    emit(
      'notify',
      loaded ? (existing ? '抬头信息已更新' : '已新增公司抬头') : '保存成功，但列表刷新失败。',
      loaded ? 'success' : 'warning',
    );
  } catch (cause) {
    emit('notify', cause instanceof Error ? cause.message : '经营主体保存失败，请重试。', 'error');
  } finally {
    saving.value = false;
  }
}

async function setDefault(id: string) {
  if (!canManage.value || saving.value) return;
  saving.value = true;
  try {
    await setDefaultBusinessLegalEntityApi(id);
    await loadHeaders();
    emit('notify', '默认抬头已更新', 'success');
  } catch (cause) {
    emit('notify', cause instanceof Error ? cause.message : '默认主体更新失败，请重试。', 'error');
  } finally {
    saving.value = false;
  }
}

function requestRemoveHeader(item: CompanyHeader) {
  if (!canManage.value || saving.value) return;
  if (item.isDefault) {
    emit('notify', '默认抬头不能删除', 'warning');
    return;
  }

  deleteId.value = item.id;
  deleteDialogOpen.value = true;
}

async function confirmRemoveHeader() {
  const id = deleteId.value;
  if (!id || !canManage.value || saving.value) return;

  saving.value = true;
  try {
    await deleteBusinessLegalEntityApi(id);
    deleteDialogOpen.value = false;
    deleteId.value = null;
    const loaded = await loadHeaders();
    emit('notify', loaded ? '抬头已删除' : '删除成功，但列表刷新失败。', loaded ? 'success' : 'warning');
  } catch (cause) {
    emit('notify', cause instanceof Error ? cause.message : '经营主体删除失败，请重试。', 'error');
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <SettingsPanel
    title="多抬头管理"
    description="管理不同公司主体的开票信息和签约抬头"
  >
    <template #actions>
      <UiButton
        v-if="canManage"
        variant="primary"
        size="sm"
        @click="openForm()"
      >
        <template #leading
          ><Plus
            :size="15"
            :stroke-width="1.8"
        /></template>
        新增抬头
      </UiButton>
    </template>

    <p
      v-if="!canView"
      role="status"
    >
      当前账号没有经营主体查看权限。
    </p>
    <p
      v-else-if="loading"
      role="status"
    >
      正在加载公司主体…
    </p>
    <p
      v-else-if="headers.length === 0"
      role="status"
    >
      当前租户还没有公司主体。
    </p>

    <section
      v-if="canView && !loading"
      :class="$style.summaryGrid"
      aria-label="抬头统计"
    >
      <div :class="$style.summaryCard">
        <span :class="$style.summaryLabel">抬头总数</span>
        <strong :class="$style.summaryValue">{{ headers.length }}</strong>
      </div>
      <div :class="$style.summaryCard">
        <span :class="$style.summaryLabel">默认抬头</span>
        <strong :class="$style.defaultSummary">{{ defaultHeader?.shortName || '—' }}</strong>
      </div>
      <div :class="$style.summaryCard">
        <span :class="$style.summaryLabel">可用抬头</span>
        <strong :class="$style.availableSummary">{{ headers.length }}</strong>
      </div>
    </section>

    <div
      v-if="canView && !loading"
      :class="$style.headerList"
    >
      <section
        v-for="item in headers"
        :key="item.id"
        :class="[$style.headerCard, item.isDefault && $style.defaultCard]"
        data-company-header-card
        :data-company-header-id="item.id"
        :data-company-header-name="item.name"
        :data-company-header-default="item.isDefault ? 'true' : 'false'"
      >
        <div :class="$style.headerRow">
          <button
            :class="$style.expandButton"
            :aria-label="expandedId === item.id ? '收起抬头' : '展开抬头'"
            @click="expandedId = expandedId === item.id ? null : item.id"
          >
            <ChevronRight
              :size="14"
              :stroke-width="1.8"
              :class="[$style.chevron, expandedId === item.id && $style.chevronExpanded]"
            />
          </button>
          <span :class="$style.companyIcon"
            ><Building2
              :size="19"
              :stroke-width="1.8"
          /></span>
          <div :class="$style.companyIdentity">
            <div :class="$style.companyHeading">
              <strong :class="$style.companyName">{{ item.name }}</strong>
              <span
                v-if="item.isDefault"
                :class="$style.defaultBadge"
                >★ 默认</span
              >
            </div>
            <p :class="$style.companyMeta">{{ item.shortName }} | 税号: {{ item.taxId }}</p>
          </div>
          <div
            v-if="canManage"
            :class="$style.actions"
          >
            <UiButton
              v-if="!item.isDefault"
              variant="secondary"
              size="xs"
              @click="setDefault(item.id)"
            >
              设为默认
            </UiButton>
            <UiButton
              aria-label="编辑抬头"
              title="编辑抬头"
              variant="ghost"
              size="icon-sm"
              @click="openForm(item)"
            >
              <Pencil
                :size="14"
                :stroke-width="1.8"
              />
            </UiButton>
            <UiButton
              :class="$style.deleteAction"
              aria-label="删除抬头"
              title="删除抬头"
              variant="ghost"
              size="icon-sm"
              @click="requestRemoveHeader(item)"
            >
              <Trash2
                :size="14"
                :stroke-width="1.8"
              />
            </UiButton>
          </div>
        </div>

        <dl
          v-if="expandedId === item.id"
          :class="$style.companyDetails"
        >
          <div>
            <dt>注册地址</dt>
            <dd>{{ item.address }}</dd>
          </div>
          <div>
            <dt>开户银行</dt>
            <dd>{{ item.bankName || '—' }}</dd>
          </div>
          <div>
            <dt>银行账号</dt>
            <dd>{{ canViewSensitiveFields ? item.bankAccount || '未设置' : item.bankAccountMask || '未设置' }}</dd>
          </div>
          <div>
            <dt>统一社会信用代码</dt>
            <dd>{{ item.taxId }}</dd>
          </div>
        </dl>
      </section>
    </div>

    <aside :class="$style.infoPanel">
      <Info
        :size="14"
        :stroke-width="1.8"
        :class="$style.infoIcon"
      />
      <p>
        <strong :class="$style.infoTitle">抬头说明</strong>
        公司主体用于项目经营中的合同和开票记录。新增记录默认使用默认抬头，也可按权限选择其他主体；银行账号仅向获授权用户显示。
      </p>
    </aside>

    <UiDialog
      v-model:open="modalOpen"
      :title="editingId ? '编辑抬头' : '新增抬头'"
      description="抬头信息将用于合同和发票记录"
      size="md"
    >
      <div :class="$style.formGrid">
        <UiFormField
          v-for="field in fields"
          :key="field.key"
          :label="field.label"
          :wide="field.wide"
          :required="field.required"
        >
          <UiInput
            v-model="draft[field.key]"
            :disabled="saving || (field.key === 'account' && !canViewSensitiveFields)"
            :placeholder="
              field.key === 'account' && !canViewSensitiveFields
                ? editingHeader?.bankAccountMask || '无权查看'
                : undefined
            "
          />
        </UiFormField>
      </div>
      <template #footer="{ close }">
        <UiButton
          variant="secondary"
          @click="close"
          >取消</UiButton
        >
        <UiButton
          variant="primary"
          :disabled="saving"
          @click="saveHeader"
          >{{ saving ? '正在保存…' : '保存抬头' }}</UiButton
        >
      </template>
    </UiDialog>

    <UiAlertDialog
      v-model:open="deleteDialogOpen"
      title="删除公司抬头"
      :description="
        deleteTarget ? `确定删除“${deleteTarget.name}”吗？删除后，该抬头将无法用于新的合同、报价单和发票申请。` : ''
      "
      :confirm-label="saving ? '正在删除…' : '删除抬头'"
      tone="danger"
      @confirm="confirmRemoveHeader"
    />
  </SettingsPanel>
</template>

<style module>
@reference "../../styles/index.css";
.summaryGrid {
  @apply mt-5 grid grid-cols-3 gap-3;
}
.summaryCard {
  @apply rounded border p-3;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
.summaryLabel {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.summaryValue,
.availableSummary {
  @apply mt-1 block text-[21px];
}
.defaultSummary {
  @apply mt-1 block text-[13px];
  color: var(--app-primary);
}
.availableSummary {
  color: var(--ui-success);
}
.headerList {
  @apply mt-4 space-y-3;
}
.headerCard {
  @apply overflow-hidden rounded border;
  border-color: var(--ui-border);
}
.defaultCard {
  border-color: color-mix(in srgb, var(--app-primary) 35%, white);
}
.headerRow {
  @apply flex h-[66px] items-center px-3;
}
.expandButton {
  @apply flex h-5 w-5 items-center justify-center;
  color: var(--ui-text-subtle);
}
.chevron {
  @apply transition-transform;
}
.chevronExpanded {
  @apply rotate-90;
}
.companyIcon {
  @apply ml-2 flex h-9 w-9 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.companyIdentity {
  @apply ml-3 min-w-0;
}
.companyHeading {
  @apply flex items-center gap-2;
}
.companyName {
  @apply text-[14px];
}
.defaultBadge {
  @apply rounded px-2 py-0.5 text-[9px] font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.companyMeta {
  @apply mt-1 text-[11px];
  color: #97a0af;
}
.actions {
  @apply ml-auto flex items-center gap-1 opacity-0 transition-opacity;
}
.headerCard:hover .actions,
.headerCard:focus-within .actions {
  @apply opacity-100;
}
.deleteAction:hover {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.companyDetails {
  @apply grid grid-cols-2 gap-x-6 gap-y-3 border-t px-12 py-4 text-[11px];
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
.companyDetails dt {
  color: #97a0af;
}
.companyDetails dd {
  @apply mt-1 font-medium;
}
.infoPanel {
  @apply mt-4 flex items-start gap-2 rounded border p-3 text-[11px] leading-5;
  color: var(--ui-text-muted);
  border-color: color-mix(in srgb, var(--app-primary) 35%, white);
  background: color-mix(in srgb, var(--app-primary-soft) 30%, transparent);
}
.infoIcon {
  @apply mt-0.5 shrink-0;
  color: var(--app-primary);
}
.infoTitle {
  @apply block text-[12px];
}
.formGrid {
  @apply grid grid-cols-2 gap-4 p-5;
}
</style>
